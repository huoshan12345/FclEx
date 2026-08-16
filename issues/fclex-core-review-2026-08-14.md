# FclEx.Core 源码审查报告

## 范围与结论

- 审查范围：`src/FclEx.Core` 下的 C# 源码，明确排除任何 `Combinatorics` 目录。
- 当前规模：418 个纳入范围的 C# 文件，约 30,741 行（不计 `obj` 生成文件）。
- 方法：采用自顶向下的顺序，先判断功能目的、包归属、职责边界、数据/并发模型和公共抽象是否合理；整体设计成立后，再检查命名、签名、内部实现和边界条件。结合编译、现有测试和重点静态分析，优先检查并发、异步、资源释放、集合不变量、I/O、安全、序列化和多目标框架兼容性。
- 停止条件：第一轮保留问题 1–50，其中第 31 条因后续重构不再适用而删除；本轮继续审查并在确认 50 个新问题后停止，记录为 51–100。
- 此前基线验证：`dotnet build src/FclEx.Core/FclEx.Core.csproj -c Release --no-restore` 在 `net472`、`netstandard2.0`、`net8.0`、`net9.0`、`net10.0` 上均成功，0 warning、0 error。
- 此前测试验证：`dotnet test test/FclEx.Core.Tests/FclEx.Core.Tests.csproj -c Release --no-restore --no-build /nr:false` 全部通过；`net472` 通过 10,138 条、跳过 3 条，其他四个测试目标各通过 10,186 条、跳过 3 条。

严重性约定：P0 为安全或数据破坏风险；P1 为较高概率的错误、死锁、竞态或公共契约破坏；P2 为边界错误、资源/兼容性风险或明显的行为歧义；P3 为命名和可维护性问题。

复审说明：未修复的第 31–50 条已按“整体设计优先”重新检查。第 31 条所指的旧异步重载已经被 token-aware、`TimeSpan` 形式的实现替换，因此删除；第 32、36、37、40、42、43、49、50 条按当前代码重新表述为设计层问题；其余未修复条目的用途和总体方向成立，原问题仍适用。

第二轮说明：问题 51–100 基于当前源码重新审查，仍排除 `Combinatorics`。本轮只做静态审查和记录，没有修改生产代码；条目优先保留整体用途、公共 API、正确性、并发、资源生命周期、安全和多目标兼容性问题，未把单纯格式偏好计入数量。

## 问题清单

1. **[P0][已修复] ZIP 解压允许路径穿越（Zip Slip）**  
   位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/~Compression/ZipArchiveEntryExtensions.cs:26-33`。`ExtractToDirAsync` 直接将不可信的 `entry.FullName` 与目标目录组合；诸如 `../../outside.txt` 或绝对路径可逃出 `dir`。建议在 `Path.GetFullPath` 后验证结果仍位于规范化的目标目录内，同时拒绝 rooted path、父目录跳转及平台相关的替代分隔符，并增加恶意 ZIP 测试。
   修复：规范化目标目录和输出路径，并在写入前验证输出路径仍位于目标目录内；已增加路径穿越回归测试。

2. **[P1][已修复] `LruCache.TryGetValue` 在读锁下修改链表**  
   位置：`src/FclEx.Core/FclEx/Utils/~Caching/LruCache.cs:116-126`。方法取得 `LockRead` 后调用 `UpdateInternal`，后者会 `Remove`/`AddFirst` 修改 `_list`。多个读者可同时写同一链表，造成结构损坏或随机异常。应使用可升级读锁并在写锁内更新 LRU 顺序，或将查找与触碰顺序合并到写锁中。
   修复：后续整体重构中改用普通互斥锁，将字典查找与 LRU 链表触碰合并为一次原子操作；快照枚举不持锁，并增加并发访问回归测试。

3. **[P1][已修复] `LfuCache.TryGetValue` 同样在读锁下修改频率和链表**  
   位置：`src/FclEx.Core/FclEx/Utils/~Caching/LfuCache.cs:41-51`。`UpdateInternal` 会改写节点计数、交换节点值并更新字典，但调用方只持有读锁。应提升为写锁，并补充高并发 `TryGetValue` 后校验字典、链表和频率顺序一致性的压力测试。
   修复：后续整体重构中改用普通互斥锁，并以“频率桶 + 桶内 LRU”维护 LFU 状态；命中时的频率和节点迁移在同一临界区完成，并增加并发访问回归测试。

4. **[P1][已修复] `BiDictionary` 的写操作不是原子的，会破坏双射不变量**  
   位置：`src/FclEx.Core/System/Collections/Generic/BiDictionary.cs:54-88`。`Add` 先写 `_dic1`，若值已存在，第二次 `Add` 抛出后 `_dic1` 已残留新项；两个索引器还会用另一个键已占用的值直接覆盖反向映射，却保留旧的正向映射。建议所有写操作预先检查键和值冲突，或在失败时回滚，并统一实现为一个维护双向映射的原子内部方法。
   修复：新增统一的 `AddMapping` 和 `SetMapping` 私有方法，冲突时拒绝更新，异常时回滚已写入的映射；已覆盖两个索引器方向和 `Add` 的回归测试。

5. **[P1][已修复] 同步版 `TaskHelper.Repeat` 实际只执行委托一次**  
   位置：`src/FclEx.Core/FclEx/Helpers/TaskHelper.cs:29-44`。`Enumerable.Repeat(Task.Run(action), times)` 复制的是同一个已经创建的 `Task`，并不会重复调用 `action`；返回数组也只是重复同一结果。应改为重复委托后逐次调用，例如 `Enumerable.Range(...).Select(_ => Task.Run(action))`，并测试调用次数和结果实例。
   修复：改为按次数分别创建并启动任务；已验证 `Action` 和 `Func<TResult>` 的调用次数及返回结果。

6. **[P1][已修复] `XElementEqualityComparer.Equals` 忽略元素名和子树结构，并违反哈希契约**

   位置：`src/FclEx.Core/System/Xml/Linq/XElementEqualityComparer.cs:9-25`。`Equals` 只比较聚合后的 `Value` 和属性，因此 `<a><b>x</b></a>` 可能等于 `<c>x</c>`；但 `GetHashCode` 又包含 `obj.Name`，导致“相等对象必须有相同哈希码”的契约被破坏。建议明确需要结构相等还是浅层相等；结构相等可基于 `XNode.DeepEquals`，并由同一组字段生成哈希码。
   修复：按决定直接移除了 `XElementEqualityComparer` 公共类型；仓库内没有调用方。

7. **[P1][已修复] 多属性元素可能使 `XElementEqualityComparer` 直接抛异常**

   位置：`src/FclEx.Core/System/Xml/Linq/XElementEqualityComparer.cs:15-16`。属性使用 `OrderBy(m => m.Name)`，而 `XName` 只实现 `IEquatable<XName>`，未实现 `IComparable`；两个或更多不同属性名会触发“至少一个对象必须实现 IComparable”。应提供明确比较器，例如按 namespace URI、local name 做 ordinal 排序，或改用按名称查找的无序比较。
   修复：随问题 6 一并移除了 `XElementEqualityComparer`。

8. **[P1][已修复] `ObjectMemoryEqualityComparer` 在未固定对象时创建原始内存 Span**

   位置：`src/FclEx.Core/System/Collections/Generic/~EqualityComparers/ObjectMemoryEqualityComparer.cs:17-60`。从托管引用计算地址后直接构造 `Span<byte>`，期间 GC 可以移动对象，使地址失效并读取任意旧内存；引用类型布局算法也依赖 CLR 内部实现。即使保留该低层 API，也应限制为明确可安全处理的 unmanaged 值类型；否则必须在读取期间 pin 对象，并在文档中标为运行时相关、不适合作为通用 comparer。
   修复：删除了 `ObjectMemoryEqualityComparer<T>`，替换为受 `where T : unmanaged` 约束的 `BitwiseEqualityComparer<T>`；比较和哈希均基于完整内存表示，并明确记录 padding、架构和端序语义。

9. **[P1][已修复] `FileHelper.AreFilesEqual` 会比较未初始化的栈内存**

   位置：`src/FclEx.Core/FclEx/Helpers/FileHelper.cs:55-83`。`NET5_0_OR_GREATER` 分支分配两个 4096 字节的 `stackalloc` 缓冲区，却对整个缓冲区做 `SequenceEqual`，而短文件或最后一块之后的区域未必初始化；相同文件可能被误判为不同。还应处理 `Read` 短读，而不是预先按请求长度递减。建议只比较 `buf[..i]`/`buf[..j]`，按实际读取数推进，并循环填满或正确处理短读。
   修复：改为循环读取至填满当前逻辑块，只比较有效范围，并在完整读取后推进长度；已覆盖空文件、短文件、块边界及跨块文件。

10. **[P1][已修复] 取消 `ProcessInvoker` 只停止等待，不终止子进程**

    位置：`src/FclEx.Core/FclEx/Utils/~Diagnostics/ProcessInvoker.cs:34-46`。`WaitForExitAsync` 因 token 取消后，`Process` 被 dispose，但操作系统进程通常继续运行；对于 PowerShell/WSL 命令会遗留后台进程和副作用。建议明确取消语义，并在取消时按需 `Kill(entireProcessTree: true)`（旧框架使用兼容实现），随后等待退出并保留原取消异常。
    修复：取消时现代 .NET 终止整个进程树，旧目标终止所启动的进程，等待退出后重新抛出取消异常；已增加真实进程终止测试。

11. **[P1][已修复] `ObjectHelper.TrySet` 的委托缓存键缺少泛型签名**
    位置：`src/FclEx.Core/FclEx/Helpers/ObjectHelper.cs:62-86`。缓存只以 `MemberInfo` 为键；若同一个基类成员先通过 `Derived` selector 使用，再通过 `Base` selector 使用，缓存中的 `Func<Derived, TMember>` 会被强制转换为 `Func<Base, TMember>` 并抛 `InvalidCastException`，setter 同理。缓存键应包含成员、目标类型和成员类型，或按封闭泛型类型拆分缓存。
    修复：缓存按封闭的 `T, TMember` 泛型类型拆分，并改为强类型 getter/setter 字典；已增加同一基类成员分别通过派生类型和基类型 selector 使用的回归测试。

12. **[P1][已修复] Marshal 辅助方法泄漏非 blittable 结构的嵌套非托管内存**
    位置：`src/FclEx.Core/FclEx/Helpers/ObjectHelper.cs:43-53`、`src/FclEx.Core/FclEx/Extensions/~System/BytesExtensions.cs:88-108`。`Marshal.StructureToPtr(..., false)` 可能为字符串、数组等字段分配嵌套内存，但代码只 `FreeHGlobal` 外层块，从不 `Marshal.DestroyStructure`；数组版本还反复复用同一块内存。应限定 `unmanaged`/blittable 类型，或在每次成功 marshal 后可靠调用 `DestroyStructure<T>`，并在异常路径清理。
    修复：每次 `StructureToPtr` 成功后都在 `finally` 中调用 `DestroyStructure<T>`；数组转换会先销毁当前元素的嵌套非托管数据，再复用外层缓冲区。

13. **[P1][已修复] `OperationResult<T>` 的 nullability 属性向编译器提供了错误保证**
    位置：`src/FclEx.Core/FclEx/Utils/~Operation/OperationResult.cs:18-27`。`IsSuccess == true` 被标注为 `Value` 非 null，但 `FromSuccess(default)`、隐式转换以及成功构造函数都允许 null；消费方会因此消除必要的 null 检查并可能触发 NRE。应移除关于 `Value` 的 `MemberNotNullWhen`，或从类型设计上禁止成功的 null 值。
    修复：成功构造函数统一拒绝 null，所有成功工厂和隐式转换均经由该入口。由于值类型的默认值无法经过构造函数，XML 文档明确规定 `default(OperationResult<T>)` 不是有效结果；已增加正常构造路径拒绝 null 以及默认值例外的回归测试。

14. **[P1][已修复] Consumer 的 `Add` 与 `CompleteAdding` 存在竞态**
    位置：`src/FclEx.Core/FclEx/Utils/~Consumers/ConsumerBase.cs:98-132`。`Add` 无锁检查 `_isAddingCompleted`，随后另一个线程可完成添加，再由当前线程把项目放入队列；消费循环可能已经按“完成且空”退出，留下永不处理的项目。应使用 `BlockingCollection.CompleteAdding`/`Add` 的原生原子语义，或在同一锁内检查状态并入队。
    修复：Consumer 子系统重建后，两个消费者都在同一同步边界内完成 producer enqueue 与 `CompleteAdding` 状态转换；内部重试拥有独立的私有入队路径，不再暴露可绕过完成状态的公共 API。已增加并发 enqueue/complete 回归测试，确保所有成功接收的项目都会被处理。

15. **[P1][已修复] Consumer 的停止和释放未与工作任务完成同步**
    位置：`src/FclEx.Core/FclEx/Utils/~Consumers/ConsumerBase.cs:63-76,104-160`。`Dispose` 不持有生命周期锁，取消后立即 drain 并 dispose `_items`/`_cts`，而 `ProcessAsync` 可能仍在取项或执行 handler；`Stop` 也在持锁时调用外部 `CancellationHandler`，重入时可能死锁。建议维护单一运行任务，先取消、在锁外等待其结束，再释放资源；所有外部回调都应在锁外调用。
    修复：Consumer 改为单次运行的异步生命周期，使用 `StopAsync` 和 `IAsyncDisposable`；锁内只转换状态，锁外执行取消、等待、通知和资源释放。公开类型命名统一为 `RetryingConsumer` 和 `RetryingBatchConsumer`；批量版本不再组合两个后台 Consumer，而是由单一工作循环串行处理新批次与重试分片。已增加停止、取消、零次重试、批次间隔及重试分片优先级等回归测试。

16. **[P1][已修复] `RepeatUntil` 创建了超时 token，却没有传给实际操作**
    位置：`src/FclEx.Core/FclEx/Actions/ActionExtensions.cs:333-352`。循环检查 `cts.IsCancellationRequested`，但执行 action 和 delay 时都传原 token `t`，因此一次长时间 action 或 delay 可以无限超过总 timeout。应把 `cts.Token` 传给两者，并区分调用方取消与内部超时生成的结果。
    修复：action 与重试间隔现在都使用链接 token；调用方取消返回带 `OperationCanceledException` 的取消结果，内部总超时返回 `TimeoutException`，不再把两种终止原因混为一谈。已覆盖活动 action 和重试 delay 的超时取消，以及调用方取消。

17. **[P1][已修复] `WhenAnySuccess` 和 `WhenAllOrError` 对空序列永久不完成**
    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.Task.cs:114-204`。三个实现都依赖 continuation 增加完成数；当任务数为 0 时没有 continuation，返回的 TCS 永远 pending。应在 materialize 后立即处理空集合：按 API 契约返回默认值、成功完成或抛出明确异常。
    修复：空序列在带默认工厂的 `WhenAnySuccess` 中调用工厂，在无默认值的泛型及非泛型重载中抛出明确的 `InvalidOperationException`，在 `WhenAllOrError` 中立即成功完成。

18. **[P1][已修复] 最后一个任务成功时，`WhenAnySuccess` 会在 continuation 中再次 `SetException`**
    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.Task.cs:127-174`。成功分支先 `TrySetResult`，随后完成计数达到总数又调用 `SetException`；若成功发生在最后一个任务，第二次完成 TCS 会抛异常并成为未观察的 continuation 异常。所有终结路径应使用 `TrySet*`，并仅在尚无成功结果且所有任务结束时设置失败。
    修复：移除 continuation/TCS 计数实现，改为单个 async 协调循环逐个观察已完成任务；一旦出现可接受的成功结果便直接返回，不再存在二次终结或未观察 continuation 异常。

19. **[P1][已修复] `WhenAnySuccess` 的 predicate/default factory 抛异常时，返回任务可能永久 pending**
    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.Task.cs:128-153`。用户委托在 `ContinueWith` 内无保护执行；异常只 fault continuation，不会完成对外返回的 TCS，而且完成计数也可能少一次。应捕获用户委托异常并 `TrySetException`，或使用一个可读性更高的 async 协调实现。
    修复：predicate 与默认结果工厂现在位于对外 async 调用链中执行；其异常会直接 fault 返回任务。已添加两类用户委托异常的回归测试。

20. **[P1][已修复] `ActionHelper` 会捕获取消异常并继续重试**
    位置：`src/FclEx.Core/FclEx/Helpers/ActionHelper.cs:56-107`。异步重试使用 `catch (Exception)`，把 `OperationCanceledException` 当普通失败处理，且 API 没有 cancellation token；取消后的操作可能继续执行多次。建议异步重载接收 token，单独重新抛出与该 token 相关的取消异常，并将 delay 也绑定到 token。
    修复：异步重试 API 最初改为 token-aware delegate，并将 token 同时传给操作和重试延迟；问题 32 的后续重构又以 `RetryHelper` 取代了整个 `ActionHelper`。当前同步和异步执行都传播 `OperationCanceledException`，取消也能中断重试等待。已增加操作内取消和重试延迟取消测试。

21. **[P1][已修复] `Disposable`/`AsyncDisposable` 每次释放都会重复执行回调**
    位置：`src/FclEx.Core/FclEx/Utils/~Disposables/Disposable.cs:12-16`、`AsyncDisposable.cs:12-16`。这些公共资源包装器没有 disposed 状态，重复 `Dispose`/`DisposeAsync` 会重复释放底层资源；这违反常见 IDisposable 幂等约定，也会使 `GCHandle.Free` 等动作抛错。建议用 `Interlocked.Exchange` 原子取走回调，只允许执行一次。
    修复：同步版本通过 `Interlocked.Exchange` 原子取走回调。异步版本只发布一个共享释放任务，所有并发或后续调用都会等待并观察同一次释放结果；释放失败也不会重新执行回调。

22. **[P1][已修复] `DisposableValue` 与 `AsyncDisposableValue` 的释放检查不是原子的**
    位置：`src/FclEx.Core/FclEx/Utils/~Disposables/DisposableValue.cs:13-30`、`AsyncDisposableValue.cs:21-40`。两个线程都可能看到 `_disposed == false` 并重复释放；回调重入也会重复进入，因为状态在回调完成后才设置。应在调用用户代码前原子完成状态转换，并定义回调失败后对象是否仍视为 disposed。
    修复：两个类型都在调用用户释放逻辑前原子进入 disposed 状态；即使回调失败，值仍不可访问且释放不会重试。异步版本的并发调用共享同一个任务，并将原本公开的 `_disposeAction` 收回为私有实现细节。

23. **[P1][已修复] `AsyncTimer` 的后台任务不可观察、不可等待也不可释放**
    位置：`src/FclEx.Core/FclEx/Utils/~Threading/AsyncTimer.cs:3-44`。构造函数立即启动 `_task`，但字段私有且类型不实现 `IDisposable/IAsyncDisposable`；due/period delay 的取消异常位于 try/catch 外，`onException` 自身抛错也会 fault 一个无人观察的任务。建议提供显式 `StartAsync`/`Completion` 和异步停止释放协议，避免构造函数 fire-and-forget。
    修复：`AsyncTimer` 重建为显式启动、单次运行的异步定时器。构造函数不再启动后台任务；`RunAsync` 返回生命周期任务，`Completion` 可再次观察，`StopAsync` 与 `IAsyncDisposable` 会取消并等待活动回调。回调不重叠，period 采用 callback 完成后的 fixed-delay 语义；未处理异常会 fault 运行任务，显式异步异常处理器成功后才继续运行。

24. **[P1][已修复] LRU/LFU 在写锁内调用公开的 eviction 事件**
    位置：`LruCache.cs:177-192`、`LfuCache.cs:191-204`。`OnItemCleared` 在内部状态更新中且持有 `ReaderWriterLockSlim` 写锁时执行；handler 重入缓存会触发锁递归异常，handler 抛错还会造成“旧项已删、新项未加”的半完成状态。应先完成内部事务，释放锁后再触发回调，并决定回调异常是否传播。
    修复：将含糊且只覆盖容量驱逐的 `OnItemCleared` 重建为 `EntryRemoved`，统一报告 `Evicted`、`Removed`、`Replaced` 和 `Cleared`。所有状态变更先在锁内完整提交，通知在锁外执行；即使 handler 失败也会继续通知其余 handler，最后再传播单个异常或聚合异常。`HttpClientService` 已改用统一通知释放 provider，`ClearCache` 不再重复释放。

25. **[P1][已修复] LRU/LFU 在写锁内执行调用方的 value factory**
    位置：`LruCache.cs:35-55`、`LfuCache.cs:67-87`。`activator(key)` 作为 `AddInternal` 参数在写锁作用域内求值；慢 factory 会阻塞全部访问，重入缓存会因 `NoRecursion` 失败。建议在锁外创建值，再在写锁内二次检查；若要求单次创建，应使用 per-key lazy，而不是在全局写锁中运行用户代码。
    修复：`GetOrAdd` 以每 key 的 `Lazy<TValue>` 协调并发创建，同一缺失 key 的并发调用共享一次 factory，factory 始终在缓存锁外执行，不同 key 可并行创建；失败的创建记录会移除并允许后续重试。缓存整体也已重构：公共抽象由容易与 Microsoft 缓存混淆的 `IMemoryCache` 改为语义明确的 `IBoundedCache`，删除无意义的 `IDisposable`；LRU 使用字典与双向链表，LFU 使用频率桶、桶内 LRU，并按可配置访问周期将频率减半以淘汰已经降温的历史热点。

26. **[P1][已修复] `SafeCounter.IncrementToThreshold` 不是一个原子的阈值操作**
    位置：`src/FclEx.Core/FclEx/Utils/~Threading/SafeCounterExtensions.cs:10-29`。多个线程可同时得到 `>= threshold`，并发执行 action 后相互 reset；期间的新增量还可能被 reset 丢失。名称中的 `Safe` 容易让调用者误认为整个组合操作线程安全。应使用 CAS 状态机/交换固定批次，或明确改名并记录仅单次增减原子。
    修复：在 `SafeCounter` 内新增 `IncrementAndResetIfThresholdReached`，通过 CAS 将递增和条件归零合并为一次原子状态转换；只有成功认领完整批次的调用方返回 true，之后的增量属于下一批，不会被回调后的 reset 清除。回调扩展重命名为 `IncrementAndInvokeAtThreshold`/`IncrementAndInvokeAtThresholdAsync`，并明确回调失败不恢复批次、不同批次的回调可以重叠。已增加高并发整批计数、余数保留和回调重入测试。

27. **[P1][已修复] `PrecisionDateTimeOffsetComparer` 不能满足 `IEqualityComparer` 契约**
    位置：`src/FclEx.Core/System/DateTimeOffsetComparers.cs:19-42`。“差值小于容差”不具传递性，例如 A≈B、B≈C 但 A≉C；非零 precision 的 `GetHashCode` 还直接抛错，因此不能用于 `Dictionary`/`HashSet`。建议不要实现 `IEqualityComparer`，改为显式 `IsWithinTolerance`；若确需 comparer，应采用离散 bucket 规则并保证 Equals/GetHashCode 一致，同时拒绝负 precision。
    修复：按决定直接删除 `PrecisionDateTimeOffsetComparer` 及其专用测试；源码中没有其他调用方。

28. **[P1][已修复] `ProcessInvoker` 在旧目标框架上可能丢失末尾输出**
    位置：`src/FclEx.Core/FclEx/Utils/~Diagnostics/ProcessInvoker.cs:35-45`、`FclEx/Extensions/~System/~Diagnostics/ProcessExtensions.cs:5-26`。`net472/netstandard2.0` 的兼容 `WaitForExitAsync` 只等待 Exited 事件，没有等待异步 stdout/stderr reader 的 EOF；进程退出后立即读取队列可能漏掉尾部行。应等待两个流的完成信号，或在进程退出后执行兼容的 drain/`WaitForExit` 收尾。
    修复：分别以 `DataReceived` 的 null 终止事件跟踪 stdout 和 stderr EOF，正常退出及取消终止进程后都会等待两个流完成再读取结果或释放进程。旧框架 `WaitForExitAsync` 同时改为异步延续、正确注销 Exited handler 和 cancellation registration。已增加大量 stdout/stderr 后紧跟尾标记的回归测试，并覆盖 net472。

29. **[P1][已修复] `ZipArchive.BuildTree` 用局部目录名作为父节点键，无法表示一般 ZIP 树**
    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/~Compression/ZipArchiveEntryExtensions.cs:52-80`、`System/IO/Compression/ZipArchiveEntryInfo.cs:18-20`。父键只是上一段目录名，每层还重建字典；不同分支出现同名目录会冲突，而且 ZIP 未显式包含目录 entry 时会找不到父节点。应以规范化完整路径为键，并在遇到文件时按需创建缺失祖先目录。
    修复：树构建改用 `/` 分隔的规范化完整路径索引目录；遇到缺失祖先时按需合成目录节点，因此不同分支下的同名目录互不冲突。`ZipArchiveEntryInfo` 改为只读属性，并通过可空 `Entry` 与 `IsSynthetic` 明确物理 entry 和合成目录的区别；`Parent` 现在保存完整父路径。已增加“省略目录 entry + 两个分支包含同名目录”的测试。

30. **[P2][已修复] `CopyToAsync(..., AutoRename)` 返回错误的目标 `FileInfo`**
    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/FileInfoExtensions.cs:118-130`。递归复制到 `newDest` 后忽略其返回值，最终仍返回原本冲突的 `dest`，调用者无法知道真实文件名。应直接 `return await file.CopyToAsync(newDest, ...)`，并测试连续多个冲突后的最终路径。
    修复：`AutoRename` 分支直接返回递归复制的结果；连续存在 `report.txt`、`report_1.txt` 时会返回实际创建并已刷新的 `report_2.txt`。已增加多次冲突后的路径、内容及原文件未覆盖测试。

32. **[P2][已修复] `ActionHelper` 实际是重试执行器，但同步与异步 API 是两套不一致的模型**
    位置：`src/FclEx.Core/FclEx/Helpers/ActionHelper.cs:5-141`。提供基础重试能力本身合理，但 `ActionHelper.Try` 这个名称没有表达重试；同步重载仍使用 `retryTimes`、整数秒、`onFail`、`throwOnFail` 和可空返回值，异步重载则使用 `maxRetryCount`、`TimeSpan`、token-aware delegate、fallback/defaultValue 和 `throwOnFailure`。同步版本还会在最后一次失败后延迟，并用 `throw lastEx` 丢失原始调用栈。源码中没有同步重载的实际调用方。建议先把它重建为单一、明确的重试抽象（或直接删除未使用的同步 API），统一 attempt/retry 计数、延迟、失败返回和异常传播；不要用布尔参数切换互斥的失败契约。若保留同步实现，应与异步实现共享策略，只在确实还有下一次尝试时等待，并通过 `ExceptionDispatchInfo` 重抛。
    修复：删除 `ActionHelper`，改为职责明确的 `RetryHelper.Execute`/`ExecuteAsync`。同步和异步重载现在统一使用 token-aware operation、`maxRetryCount`、`TimeSpan retryDelay`、可选 `shouldRetry` 谓词和 cancellation token；成功时返回 operation 结果，拒绝重试或耗尽次数时始终原样抛出异常，不再用布尔参数切换失败契约，也不会在最终失败后等待。已覆盖同步/异步重试成功、不可重试异常、最终异常、参数校验以及取消 operation/等待的测试。

33. **[P2][已修复] `readBufferTimeout` 同时限制写入操作**
    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/StreamExtensions.cs:46-64`。同一个带超时的 CTS 先用于 read，随后也用于 `dest.WriteAsync`；慢目标流会被名为“读取超时”的参数取消，而且写入消耗的是 read 剩余时间。应让 read timeout 只包围 read，写操作仅使用调用方 token，或把参数改成明确的 per-iteration timeout。
    修复：保留“每轮读取和写入共享一个超时预算”的既有行为，将 Core 的 `ReadAllBytesAsync`/`CopyToAsync` 及 Http 转发 API 的参数统一改为 `bufferTransferTimeout`，明确它限制的是一次 buffer transfer，而不只是读取。

34. **[P2][已修复] `LastTickOfMonth` 暴露的时间参数完全无效**
    位置：`src/FclEx.Core/FclEx/Extensions/~System/DateTimeExtensions.cs:92-96`。`hour/minute/second` 从未传给 `EndOfMonth`，任何参数值都返回当月最后一刻。应删除这些参数，或按明确语义应用它们；当前 API 会让调用者误以为可控制基准时间。
    修复：`LastTickOfMonth` 删除全部时间参数，并补充语义一致的 `LastTickOfDay`、`LastTickOfWeek`。原 `StartOfWeek`/`EndOfWeek`、`StartOfMonth`/`EndOfMonth` 分别改名为 `FirstDayOfWeek`/`LastDayOfWeek`、`FirstDayOfMonth`/`LastDayOfMonth`，避免把“最后一天的指定时间”误称为 period end；这些日历日期方法现在都支持毫秒参数，周方法继续允许指定一周从星期几开始。原 `GetMaxTimeOfDate` 被含义更准确的 `LastTickOfDay` 取代。

35. **[P2][已修复] 多个 DateTime 日历辅助方法丢失 `Kind`**
    位置：`src/FclEx.Core/FclEx/Extensions/~System/DateTimeExtensions.cs:46-90`。`Today`、`ThisYear`、`ThisMonth`、`StartOfMonth`、`EndOfMonth` 使用不带 kind 的构造函数，输入即使是 Local/Utc，输出也变为 Unspecified。应使用包含 `dt.Kind` 的构造函数，或明确记录并命名为创建 Unspecified 时间。
    修复：所有会重建日期的日历方法都使用带 `DateTimeKind` 的构造函数；`Today`、`Tomorrow`、`Yesterday`、`ThisYear`、`ThisMonth` 以及第一天/最后一天方法均保留输入的 `Kind`，并统一支持毫秒参数。已对 Unspecified、Utc、Local 三种 Kind 添加覆盖。

36. **[P2][已修复] `DateTime.ToCnTime` 的返回类型无法安全表达它声称的时区转换**
    位置：`src/FclEx.Core/FclEx/Extensions/~System/DateTimeExtensions.cs:120-143`、`DateTimeOffsetExtensions.cs:5-20`。问题不只是 `ToUtc().AddHours(8)` 保留了错误的 `Utc` Kind；`DateTime` 本身也不能携带 UTC+8 offset，因而不适合作为时区转换结果。`Cn` 还是含义不清的公共缩写。建议删除 `DateTime.ToCnTime`/`ToCnTimeStr`，只保留以 `DateTimeOffset` 表达 instant 与 offset 的转换，并将 API 命名为 `ToChinaStandardTime` 或直接显式使用 `ToOffset(TimeSpan.FromHours(8))`；字符串格式化应建立在正确的 `DateTimeOffset` 结果之上。
    修复：删除 `DateTime.ToCnTime`/`ToCnTimeStr`，同时删除会保留钟面值却改变 instant 的 `SetOffset`/`SetCnOffset` 以及公开的 `CnTimeZone`。现在只保留 `DateTimeOffset.ToChinaStandardTime`，通过 `ToOffset(+08:00)` 在保留 instant 的前提下返回正确 offset；测试同时验证 offset 和 UTC instant。

37. **[P2][已修复] `DateTimeHelper` 只是对 BCL Unix 时间 API 的有损包装，整体没有保留价值**
    位置：`src/FclEx.Core/FclEx/Helpers/DateTimeHelper.cs:3-9`。两个方法没有调用方，只把 `DateTimeOffset.FromUnixTimeSeconds/Milliseconds` 的结果取 `.DateTime`，丢失 offset 并生成 `DateTimeKind.Unspecified`。既然所有目标框架都已有原生 `DateTimeOffset` API，这个 helper 没有形成更好的抽象。建议直接删除 `DateTimeHelper`；调用方需要 `DateTime` 时应显式选择 `.UtcDateTime` 或 `.LocalDateTime`，而不是由工具方法暗中丢失语义。
    修复：按决定直接删除无调用方的 `DateTimeHelper`，不再包装 BCL API 或暗中选择 `DateTimeKind.Unspecified`。

38. **[P2][已修复] `Partition` 的 `Both` 选项实现成了 `None`**
    位置：`src/FclEx.Core/FclEx/Extensions/~System/StringExtensions.Split.cs:52-82`。文档称 separator 同时包含在左右两部分，但实现返回 `source[..index]` 和 `source[sepEndIndex..]`，两边都排除了 separator。应返回 `(source[..sepEndIndex], source[index..])` 并覆盖左右搜索和多字符 separator。
    修复：`Both` 改为返回 `(source[..sepEndIndex], source[index..])`；已覆盖从左、从右搜索以及多字符 separator。

39. **[P2][已修复] `HexToBytes` 拒绝小写 `b` 到 `f`**
    位置：`src/FclEx.Core/FclEx/Extensions/~System/StringExtensions.cs:126-138`。匹配范围写成 `>= 'a' and <= 'a'`，只有小写 `a` 可通过。应改为 `<= 'f'`，增加 `abcdef`、混合大小写和非法字符测试。
    修复：小写匹配范围改为 `a`–`f`，并以同时覆盖全部小写高位字符和混合大小写的 `aBcDeF` 增强测试；非法字符测试继续保留。

40. **[P2][已修复] `IsPossibleHtml` 没有可成立的判定契约，应删除而不是补一个脆弱启发式**
    位置：`src/FclEx.Core/FclEx/Extensions/~System/StringExtensions.cs:86-93`、`src/FclEx.Http/FclEx/Http/~Actions/DefaultHtmlAction.cs:27-38`。AngleSharp 可以把普通非空文本解析为 HTML 文档，因此当前实现实际上只是非空检查；若改成查找标签，又会错误拒绝合法片段或接受格式错误文本。这个 API 既不能证明有效 HTML，也没有为调用方提供额外信息。建议删除 `IsPossibleHtml`，让 `DefaultHtmlAction.GetHtml` 只负责非空验证，真正的解析和 selector 匹配继续由 HTML parser/context 决定。
    修复：删除 `IsPossibleHtml`。`DefaultHtmlAction.GetHtml` 现在只验证响应文本非空，并删除不再使用的泛型和 action 参数；解析及 selector 匹配仍由后续 context 阶段负责。现有 plain-text 成功和空文本失败测试继续覆盖该契约。

41. **[P2][已修复] 按字符拆行会把 CRLF 当作两个换行符**
    位置：`src/FclEx.Core/FclEx/Extensions/~System/StringExtensions.Split.cs:29-42`、`FclEx/Helpers/ResourceHelper.cs:3,28-36`。`Split(['\r','\n'], StringSplitOptions.None)` 会在每个 `\r\n` 中间制造空行；默认 RemoveEmpty 又会误删真实空白行。应按 `\r\n|\r|\n` 作为完整分隔序列处理，并分别测试保留/删除空行。
    修复：`SplitToLines` 统一按 `"\r\n"`、`"\r"`、`"\n"` 三个完整序列拆分，先 trim 再按需删除空项，并在旧框架自行解释 `TrimEntries` 位；`ResourceHelper.Embedded.ReadLines` 直接复用该实现。测试覆盖三种换行、混合换行、CRLF 空行保留以及 trim/remove 组合。

42. **[P2][已修复] `ArraySegment.ToSegment` 没有遵守“从当前 segment 切片”的契约，名称也未与 BCL 对齐**
    位置：`src/FclEx.Core/FclEx/Extensions/~System/ArraySegmentExtensions.cs:24-29`。构造时只依赖底层数组的范围检查，例如父 segment 只有 2 项，但请求 count 10 只要底层数组够大就会成功。整体用途是创建子 segment，应该优先使用或回填 BCL 的 `ArraySegment<T>.Slice` 语义，而不是另造 `ToSegment` 名称。若目标框架已有 `Slice`，应删除该方法；若确需为旧框架回填，应命名为 `Slice`、只在缺失框架编译，并验证 `offset >= 0`、`count >= 0`、`offset + count <= segment.Count`。
    修复：删除 `ToSegment`，在 `!NET5_0_OR_GREATER` 下提供与 BCL 同名的 `Slice` 回填，并按父 segment 的相对范围验证 offset/count；.NET 5+ 直接绑定 BCL 实现。内部 `Segments` 也统一调用 `Slice`，测试在所有目标上验证相对 offset 和越过父边界时抛错。

43. **[P1][已修复] `Enumerable` 异步执行扩展的并发、取消和部分结果模型整体不一致**
    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.Task.cs:7-116`。`ExecuteInParallelAsync` 的有限并发实际是整批 `Task.WhenAll`，不是持续补充 worker 的最大并行度；`concurrency == null` 会立即枚举并启动全部操作且忽略 token；有限并发和顺序版本在取消时 `break` 并以成功状态返回部分结果；operation 又不接收 token，而间隔调用的 `TaskHelper.Delay` 会吞掉取消异常。`ExecuteAsync(..., bool executeInParallel, ...)` 还用布尔参数把两种执行模型塞进同一签名。建议整体重建这组 API：operation 使用 `Func<T, CancellationToken, ValueTask[<TResult>]>`，取消统一以 canceled task 结束，有限并发使用固定 worker/信号量或现代框架的 `Parallel.ForEachAsync`，明确结果是否保持输入顺序，并删除布尔模式参数和含糊的“null 表示无限并发”约定。
    修复：删除旧的 `ExecuteSequentiallyAsync`、`ExecuteInParallelAsync`、布尔模式 `ExecuteAsync` 以及重复的 `ParallelForEachAsync`/`ForEachAsync` 包装，重建为 `ForEachSequentiallyAsync`、`SelectSequentiallyAsync`、`ForEachConcurrentlyAsync`、`SelectConcurrentlyAsync`。operation 全部接收 cancellation token 并返回 `ValueTask`；并发 API 使用固定数量 worker 懒枚举 source，要求显式的正数 `maxDegreeOfParallelism`，失败或取消后停止领取新项，结果 API 始终保持输入顺序且绝不成功返回部分结果。Http 批量下载同步改为单一 `MaxDegreeOfParallelism` 配置，默认上限为 8，设置为 1 即顺序执行。已覆盖持续并发上限、顺序、结果顺序、token 传递、取消和非法并行度。

44. **[P2][已修复] Base32 解码静默接受无效的短输入**
    位置：`src/FclEx.Core/FclEx/Extensions/~System/BytesExtensions.Base32.cs:5-44`。例如单字符 `"A"` 计算出的 `byteCount` 为 0，最终返回空数组而非报告无效编码；padding 长度和被丢弃的尾位也未校验。应验证 RFC 4648 合法长度、padding 位置和尾部零位，或明确提供 tolerant 模式。
    修复：解码器现在严格验证 RFC 4648 的有效数据长度、padding 只能位于完整 8 字符块末尾、padding 数量以及未使用尾位必须为零；仍接受语义明确的无 padding 形式和大小写字母。已加入 RFC 4648 各余数长度样例及非法短输入、padding、尾位测试。

45. **[P2][已修复] 静态 JSON 成员的值在元数据创建时被永久捕获**
    位置：`src/FclEx.Core/System/Text/Json/JsonHelper.cs:137-157`。`var value = member.GetValue(null)` 只执行一次，`propertyInfo.Get = _ => value` 使以后修改的静态属性/字段仍序列化旧值；还用首次运行时类型代替声明类型创建 contract。getter 应每次读取 `member.GetValue(null)`，类型应使用 `member.DataMemberType`。
    修复：静态成员的 `JsonPropertyInfo` 始终使用声明类型创建，getter 在每次序列化时重新调用 `member.GetValue(null)`。测试使用声明为 `object` 的可变静态属性，验证同一 options/metadata 在值从字符串变为整数后会输出最新值及正确 JSON 类型。

46. **[P2][已修复] `ReadAsStringJsonConverter` 会改变数字文本并损失精度**
    位置：`src/FclEx.Core/System/Text/Json/Serialization/ReadAsStringJsonConverter.cs:20-42`。非 Int64 数字先转 `double` 再格式化，诸如高精度 decimal、超大整数或特定指数表示无法保留原始值。既然目标是字符串表示，应从 reader 的原始 UTF-8 token 获取文本，或通过 `JsonDocument.ParseValue(...).RootElement.GetRawText()` 保真。
    修复：数字和其他复合 JSON 值统一通过 `JsonDocument.ParseValue` 读取原始 JSON 文本，不再经过 `Int64`/`double` 转换。测试覆盖超出 `Int64` 的整数、高精度小数和带尾零的指数表示。

47. **[P2][已修复] 多值 `NameValues.Set` 只保留最后一个值**
    位置：`src/FclEx.Core/FclEx/Utils/~Collections/NameValuesExtensions.cs:64-76`。每个 value 都调用 `self.Set(key, value)`，后一次会删除前一次，所以 `IEnumerable<string>` 重载与“多值”类型目的相违。应对每个 key 先 Remove 一次，再逐个 Add；空 values 的行为也需明确定义。
    修复：每个键只在添加前删除一次，随后保留输入序列中的全部新值；空值序列明确表示移除该键。已覆盖替换多个旧值和空序列两种情况。

48. **[P2][已修复] `FileExtensionEqualityComparer` 把无扩展名的整个文件名当作扩展名**
    位置：`src/FclEx.Core/System/Collections/Generic/~EqualityComparers/FileExtensionEqualityComparer.cs:7-21`。`SkipUntil(".", untilLast: true)` 找不到点时返回原字符串，因此 `foo` 与 `bar` 被认为扩展名不同；`.gitignore` 等边界也与 `Path.GetExtension` 语义不一致。应基于 `Path.GetExtension`，并明确是否接受完整路径、尾点和 dotfile。
    修复：比较和哈希都基于 `Path.GetExtension` 并使用 `OrdinalIgnoreCase`；完整路径、无扩展名、尾点和 dotfile 均遵循 BCL 语义，且相等值具有相同哈希码。

49. **[P2][已修复] `SizeCalculator` 对“托管对象大小”的公共承诺本身不可靠，空 struct 返回 0 只是一个症状**
    位置：`src/FclEx.Core/FclEx/Utils/~Runtime/SizeCalculator.cs`。该类型把 value size、引用类型 shallow instance size、对象头和对齐混成一个 `SizeOf(Type)` 契约，并通过未初始化对象和字段地址差推断 CLR 私有布局；接口被报告为最小对象大小，抽象类/开放泛型却因无法实例化而抛错，行为并不一致。空 struct 返回 0 进一步证明实现不能稳定表达运行时布局。建议先决定真实用途：若只需要值类型的 managed size，应删除引用类型分支并基于受约束泛型 `Unsafe.SizeOf<T>()`；若确实需要诊断当前运行时的 shallow object size，应改成明确的诊断型名称和返回契约，接受实例而不是伪造对象，注明 runtime/architecture 限制，并用空 struct、显式/自动布局、继承、数组和运行时差异验证。不要继续把当前结果描述为通用、精确的对象大小。
    修复：重命名为 `TypeSizeCalculator.GetInstanceFieldStorageSize`，契约收窄为实例字段的浅层存储总和：值类型字段按内联 managed size，引用字段按指针，包含继承字段但不含对象头、引用对象、变长尾部数据、字段间填充和对象对齐。实现不再创建未初始化对象或推断 CLR 私有布局；数组、接口、抽象类型、开放泛型及其他无实例布局类型明确抛出 `ArgumentException`，`string` 按其固定声明字段计算。

50. **[P2][已修复] 剩余名称问题不能作为一次机械改名处理，其中多个 API 应先删除或重新定义用途**
    复审结果：
    1. `FclEx/Helpers/DebuggerHepler.cs` 不仅拼写错误，而且没有调用方，只是薄封装 `Debugger.Log`；应优先删除，确有统一日志入口需求时再设计，而不是直接改成 `DebuggerHelper`。
    2. `System/ComponentModel/DataAnnotations/UriAttribute.cs` 的验证职责成立，但 URI 术语是 scheme，不是 schema；`AllowedSchemas` 应改为 `AllowedSchemes`，错误消息也应同步修正。
    3. `TaskHelper.DelayMilli` 与 BCL `Task.Delay(int, token)` 重复；更重要的是同文件的 `Delay` 会吞掉 `TaskCanceledException`，名称却没有表达“忽略取消”。应删除毫秒包装，并让普通 `Delay` 传播取消；确需吞取消的调用点应显式捕获或使用准确命名的专用方法。
    4. `IPEndPointHelper.NextLocalEndpoint` 无法提供它名字暗示的保证：socket 释放后端口立即可能被其他进程占用。应删除这个 TOCTOU helper，让服务器直接绑定端口 0 后读取实际端口；若只能用于测试，也应把实现放在测试基础设施而不是 Core 公共 API。
    修复：删除无调用方的 `DebuggerHepler`；将 `AllowedSchemas` 改为 `AllowedSchemes` 并修正验证消息；删除含糊的数字 delay 重载，改为只接受 `TimeSpan` 的 `DelayIgnoringCancellationAsync`，名称和文档明确取消会提前结束等待但不取消返回任务；删除 `IPEndPointHelper`，SSH 调用方直接使用 `IPAddress.Loopback`，HTTP 测试服务器则让 Kestrel 绑定 `127.0.0.1:0` 并在启动后读取实际地址，同时在 fixture 结束时释放服务器和 HTTP client。

51. **[P0][已修复] `Span.Create` 把托管引用降级成了不受 GC 跟踪的裸指针**

    位置：`src/FclEx.Core/System/Span.cs:16-30`。`Unsafe.AsPointer(ref reference)` 创建的 `Span<T>` 不再保留托管 byref 的 GC 跟踪语义，对象移动后即可指向旧内存；任意 `length` 还允许越过原对象。`AsBytes<T>` 同时没有 `unmanaged` 约束，对引用类型得到的只是引用槽位字节。建议删除这两个 API；需要从托管引用创建 span 时使用 `MemoryMarshal.CreateSpan`，字节视图则限制为 `unmanaged` 并使用 `MemoryMarshal.AsBytes`。
    修复：最初尝试改成单元素 span，但安全的 managed-byref 构造 API 并不覆盖所有目标框架；最终删除整个 `System.Span` helper。单元素 `StartsWith`/`EndsWith` 直接比较边界元素，`NextUnmanaged` 在现代目标内局部使用 `MemoryMarshal.CreateSpan` 与 `MemoryMarshal.AsBytes`，旧目标继续使用托管 byte array，不再返回裸指针 span。

52. **[P2][部分处理] `StringInfoSpanEnumerator` 同时破坏了枚举器契约并依赖运行时私有布局**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Globalization/StringInfoExtensions.cs:5-43`。显式 `IEnumerator.Current` 永远抛 `NotImplementedException`，泛型 `Current` 在 `MoveNext` 前或结束后会通过无边界检查的 span 访问非法索引；实现还用 `UnsafeAccessor` 读取 `StringInfo.Indexes` 私有属性。建议不要实现无法完整满足的 `IEnumerator<ReadOnlySpan<char>>`，改成纯 pattern-based ref struct 枚举器并验证状态；文本元素边界应基于公开 API，而不是 CLR 私有成员。
    处理：已移除 `IEnumerator<ReadOnlySpan<char>>` 实现，类型现在只是 pattern-based ref struct enumerator。`Current` 的 `_indexes[_index]` 保留正常边界检查，非法状态会抛越界异常，按当前决定可接受；但 `UnsafeAccessor` 对 `StringInfo` 私有成员的运行时耦合仍保留，因此本条未标为完全修复。

53. **[P1][已修复] 批量等待 `SemaphoreSlim` 失败时会永久吞掉已经取得的许可**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Threading/SemaphoreSlimExtensions.cs:5-13`。循环取得部分许可后，后续一次超时会直接返回 `false`，取消则直接抛异常，两条路径都没有释放前面已取得的许可。应记录成功次数并在所有未成功完成的路径中 `Release(acquiredCount)`；同时验证 `count > 0`，并明确 timeout 是整批预算还是每个许可的独立预算。
    修复：记录本次调用已经取得的许可数，超时或取消时在 `finally` 中全部归还；`count` 现在必须为正数。XML 文档明确 timeout 独立应用于每次 permit acquisition，调用方需要总超时时可使用带 `CancelAfter` 的 cancellation token。测试覆盖成功、部分取得后超时、部分取得后取消及非法 count。

54. **[P1][已修复] `LockEnumerator<T>` 用可复制 struct 持有线程关联的读锁**

    位置：`src/FclEx.Core/FclEx/Utils/~Threading/LockEnumerator.cs:8-43`。复制枚举器会复制 `_isDisposed`，两个副本都可能执行 `ExitReadLock`；枚举器若在取得锁以外的线程释放，`ReaderWriterLockSlim` 也会抛异常。建议用不可复制的引用类型 lease，或者先在锁内取得快照再枚举，避免把线程关联锁跨越调用方代码持有。
    修复：仓库内没有调用方；这种抽象也无法通用地约束枚举器复制、线程切换或调用方持锁时长，因此直接删除 `LockEnumerator`/`LockEnumerator<T>`。具体集合需要一致性枚举时应在自身同步边界内创建快照。

55. **[P1][已修复] `ExpiringLazy<T>` 在新值创建成功前就销毁旧值，且没有自身的释放协议**

    位置：`src/FclEx.Core/FclEx/Utils/~Lazy/ExpiringLazy.cs:3-54`。过期后先 `Dispose` 旧值再调用 factory；factory 抛错时字段仍指向已释放对象，下一次访问会再次释放它。类型也不实现 `IDisposable`，最终值和内部 `ReaderWriterLockSlim` 无法在包装器结束使用时释放。建议先成功创建新值，再原子替换并在锁外释放旧值，同时实现幂等的生命周期终止。
    修复：重建为单 factory 协调的刷新状态机：新值和过期时间成功创建后才发布，旧值在锁外释放；factory 失败会保留未释放的旧值供后续重试。类型实现幂等 `IDisposable`，释放后拒绝访问，释放与正在创建的值也有明确交接；移除了不再需要释放的 `ReaderWriterLockSlim`。测试覆盖刷新失败、随后成功替换、旧值/最终值释放和释放后访问。

56. **[P1][已修复] `ReLazy`/`TimerLazy` 的 `Dispose` 并不终止对象生命周期**

    位置：`src/FclEx.Core/FclEx/Utils/~Lazy/ReLazy.cs:20-52`、`TimerLazy.cs:19-24`。重复释放会对同一值重复调用 discard handler，释放后仍可读取 `Value`、`Recreate` 或更换 factory；`TimerLazy` 又先 discard 再停止 timer，回调可并发重建或再次丢弃值。应引入原子的 disposed 状态，先停止并等待定时活动，再只丢弃一次当前值，所有后续操作统一抛 `ObjectDisposedException`。
    修复：`ReLazy` 现在用单一生命周期锁保护 value creation、recreate、factory replacement 和 dispose；释放幂等，discard handler 在内部锁外至多执行一次，释放后的公开操作抛 `ObjectDisposedException`。由于生命周期锁已经统一串行化访问，删除了不再有实际作用的 `isThreadSafe` 参数。`TimerLazy` 先停止 timer，再通过 callback 锁等待/阻止活动回调，最后释放基础 lazy。已增加重复释放、释放后访问和 timer 停止测试。

57. **[P2][已修复] `SecureStringEqualityComparer` 为比较秘密而制造不可清除的明文副本**

    位置：`src/FclEx.Core/System/Collections/Generic/~EqualityComparers/SecureStringEqualityComparer.cs:14-39`。`Equals` 和 `GetHashCode` 都把 BSTR 转成托管 `string`，这些明文只能等待 GC，抵消了 `SecureString` 可确定清理的主要价值。若该 comparer 确有用途，应直接在 BSTR 内存上做 ordinal 比较和哈希，并在 `finally` 中清零释放；否则应删除这个会给调用方错误安全预期的类型。
    修复：比较和哈希都直接遍历 `SecureStringToBSTR` 返回的 UTF-16 内存，不再调用 `PtrToStringBSTR` 或创建托管明文字符串；BSTR 仍由既有 disposable wrapper 在退出时通过 `ZeroFreeBSTR` 清零释放。测试覆盖等值、非等值及 hash contract。

58. **[P1][已修复] `BooleanJsonConverter` 把非 nullable `bool` 的 JSON `null` 静默转换为 `false`**

    位置：`src/FclEx.Core/System/Text/Json/Serialization/BooleanJsonConverter.cs:7-22`。这会把显式的无效/缺失值与合法的 `false` 合并，隐藏上游数据错误。非 nullable converter 应对 `Null` 抛 `JsonException`；需要接受 null 时应由 `bool?` 的 converter 和类型系统表达。
    修复：构造函数新增 `treatNullAsFalse` 参数，并提供语义明确的 `Strict` 与 `NullAsFalse` 实例；默认/strict converter 对 null 抛 `JsonException`。按决定，`JsonOptions.AllowBoolFromString` 启用时使用 `NullAsFalse` 版本；字符串解析改用大小写不敏感的 `bool.TryParse`，不再通过 `ToLower` 创建额外字符串。后续补回真正的公开无参构造函数，使 `[JsonConverter(typeof(BooleanJsonConverter))]` 可以由 System.Text.Json 反射实例化；带 bool 参数的构造函数不再依赖可选参数伪装成无参形式。测试覆盖 strict、显式兼容模式、特性注册及 JsonHelper 配置。

59. **[P2][已确认保留] `GetBuiltInJsonTypeInfo` 对 `System.Text.Json` 私有实现存在版本耦合**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Text/~Json/JsonSerializerOptionsExtensions.cs:5-35`。代码反射调用 `GetBuiltInConverter`、`ExpandConverterFactory` 和 `CreateTypeInfoCore`，这些名称和签名不是兼容性契约，会随 System.Text.Json 更新、裁剪或 NativeAOT 失效。复核结论：截至目前，公开 API 只能取得当前 options 最终选中的 converter，并没有“跳过当前 converter、继续取得默认 converter”的通用能力。复制 options 并移除 converter 只适用于通过 options 注册且能够控制注册方式的局部场景，无法绕过定义在目标类型上的 `[JsonConverter]`；而类型级 converter 复用默认 converter 的需求本身合理且常见。当前没有功能等价且设计更好的公开实现方式，因此不得不保留这层私有反射，并明确接受相应的版本、裁剪和 NativeAOT 兼容性风险。本条不再作为待修复问题；升级 System.Text.Json 时应继续通过兼容性测试确认这些私有入口仍然有效。仓库同时保留公开 API 组合版 `ReadAsArrayJsonConverter` 和依赖该能力、支持类型级特性的 `ReadAsArrayUsingBuiltInJsonConverter`，测试明确覆盖两者的共同契约和差异。

60. **[P1][已修复] `ObjectJsonConverter` 会静默损失合法 JSON 数字的精度**

    位置：`src/FclEx.Core/System/Text/Json/Serialization/ObjectJsonConverter.cs:55-67`。超出 `Int64` 或带高精度小数的数字统一降为 `double`，可能改变值；超出 double 范围但语法合法的 JSON 数字还会被拒绝。通用 object 模型应保留为 `JsonElement`/原始数字文本，或提供明确可配置的 `decimal`、大整数和浮点策略，而不是无提示地选 `double`。
    修复：数字策略改为：优先 `int`/`long`；更大的整数字面量使用 `BigInteger`；非整数字面量仍优先保留可精确表达的 `double`，当转成 double 会损失且 decimal 可表达时改用 `decimal`；decimal/double 都无法承载但能精确解析为整数的指数形式也使用 `BigInteger`。写入时显式把 `BigInteger` 输出为 JSON number，避免默认序列化为对象。测试覆盖 long、超大整数、高精度 decimal 及二者的写入，并保留普通小数和科学计数法的既有 double 行为。

61. **[P2][已修复] 整数随机 API 的参数模型无法生成类型的完整取值域**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/RandomExtensions.cs:58-100`。range API 采用 exclusive `max`，但 `byte`、`ushort`、`uint`、`ulong` 的参数类型无法表示 `MaxValue + 1`，默认值因此永远不会生成各自的 `MaxValue`；`sbyte` 和 `short` 的无参语义还只覆盖非负数。建议把“完整位域随机值”做成无参 overload（直接填充字节），range overload 保持并清楚记录半开区间。
    修复：`NextSByte`、`NextByte`、`NextInt16`、`NextUInt16`、`NextUInt32` 和 `NextUInt64` 的无参 overload 现在直接填充完整位域，range overload 改为必须显式给出两个边界并继续采用 `[min, max)`。`NextInt64()` 保持与 BCL 同名 API 一致的非负语义；需要完整 `long` 位域时使用 `NextUnmanaged<long>()`。测试用全 1 位模式确认各自的负值或 `MaxValue` 可达。

62. **[P1][已修复] `NextSingle` 可能返回声称排除的上界**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/RandomExtensions.cs:116-126`。先把 `[0,1)` 的 double 转成 float，足够接近 1 的值会舍入成 `1f`，最终结果等于 `max`。应使用能保证 `< 1f` 的单精度采样算法（现代目标直接使用 BCL `Random.NextSingle`），并针对相邻浮点值和上界增加性质测试。
    修复：把该 API 收窄为只在 .NET 6 以前提供的 BCL `Random.NextSingle()` 回填，不再混入额外的 range overload；实现从 24 位随机整数构造可精确表示的 `[0, 1)` 单精度值，最大采样也不会舍入到 `1f`。现代目标直接使用 BCL 实例方法，旧目标回归测试覆盖最大采样。

63. **[P2][已确认保留] `Random.Next<T>` 的对象图生成职责不适合放在基础库的通用随机 API 中**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/RandomExtensions.cs:267-432`。实现会调用非公开构造函数、写入私有字段并触发任意构造副作用，仍无法保证对象不变量；接口、抽象类型、readonly/init-only 字段等形状也只能在运行时失败。建议把它移到专门的测试数据包，并以显式 factory/策略、成员选择和深度策略构建；Core 只保留明确、无副作用的标量与集合随机方法。
    处理：按当前决定继续保留在 Core。XML 文档现在明确它只面向任意测试数据，会调用非公开构造函数和写入非公开字段、可能触发任意副作用、不保证对象不变量、递归深度为每条路径上同类型十层，并且接口、抽象类型、readonly 成员及拒绝随机参数的构造函数可能在运行时失败。补充了常见类型、数组、构造函数、递归图、接口失败和相同 seed 对对象图可复现等测试；这是一项明确接受限制的 API，而不是通用有效对象生成器。

64. **[P2][已修复] 传入种子的 `Random` 不能让 `Next<Guid>` 可复现**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/RandomExtensions.cs:339-347`。Guid 分支调用 `Guid.NewGuid()`，完全绕过调用方提供的随机源，因此相同 seed 产生不同对象图。若保留对象生成，应从该 `Random` 填充 16 字节后构造 Guid，并为 seeded generation 建立确定性契约。
    修复：Guid 的 16 字节现在全部来自调用方传入的 `Random`，相同运行时上相同 seed 的 Guid 和包含随机字段的对象图可复现；增加了两项确定性测试。

65. **[P1][已修复] 旧框架的 `Task.WaitAsync(TimeSpan)` 回填与官方超时契约不兼容**

    位置：`src/FclEx.Core/FclEx/Helpers/TaskHelper.cs:96-106`。实现用超时 CTS 调用 cancellation overload，因此超时抛 `OperationCanceledException`；官方同名 API 在 timeout 时抛 `TimeoutException`。仓库规则要求回填 BCL 名称时匹配官方行为，应区分调用方取消与内部 timeout，并补充旧目标回归测试。
    修复：条件编译边界改为 API 实际引入的 .NET 6；timeout overload 现在只把自身 timeout token 导致的取消转换为 `TimeoutException`，原任务自身的 fault/cancellation 保持不变。取消等待时还会继续观察被放弃任务的后续 fault，旧目标测试确认超时异常契约。

66. **[P1][已修复] `TaskHelper.Run` 会因是否设置 timeout 而改变委托的调度方式**

    位置：`src/FclEx.Core/FclEx/Helpers/TaskHelper.cs:109-130`。没有 timeout 时直接调用委托，有 timeout 时却通过 `Task.Run` 强制切到线程池；超时只放弃等待，并不会取消仍在执行的 operation。这个签名把“调用”“调度”和“等待上限”混在一起。建议直接调用 operation，再等待其任务；若要终止工作，operation 必须接收 cancellation token，并把名字改为表达 timeout/cancellation 的 API。
    修复：删除 `Run` overload，重建为 `RunAsync` 和为避免 async lambda 重载歧义而单独命名的 `RunValueTaskAsync`。operation 始终在当前执行上下文直接调用，并接收由调用方取消与 timeout 联合控制的 token；operation 返回 Task 后，等待会在取消或超时时结束，timeout 抛 `TimeoutException`，即使 operation 忽略 token 也不会继续阻塞调用方。同步委托调用本身无法被抢占，因此文档要求及时返回 Task。现有 `Operation` 调用链已同步迁移，测试覆盖调度一致性、timeout token、调用方取消和 ValueTask 结果。

67. **[P1][已修复] `AwaitObject` 会消费 `ValueTask<T>` 两次**

    位置：`src/FclEx.Core/FclEx/Helpers/TaskHelper.cs:182-188,196-245`。代码先对 `value.AsTask()` 的结果执行 await，随后又从原始 boxed `ValueTask<T>.Result` 取值；基于 `IValueTaskSource<T>` 的 ValueTask 通常只允许单次消费。应从 `AsTask()` 返回的同一个 Task 中同时等待并取得结果，不再访问原 ValueTask。
    修复：`AwaitObject` 只调用一次 `AsTask()`，随后从同一个 `Task<T>` 等待并读取结果，不再访问原始 boxed `ValueTask<T>`。新增只允许一次 `GetResult` 的 `IValueTaskSource<T>` 回归测试，确认消费次数严格为一。

68. **[P2][已修复] `DateTime` 转 Unix 时间对 `Unspecified` 输入产生机器相关结果**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/DateTimeExtensions.cs:10-32`。`new DateTimeOffset(dateTime)` 会把 `DateTimeKind.Unspecified` 当作本地时间，同一钟面值在不同时区得到不同 Unix 时间。API 应拒绝 `Unspecified`，或通过参数明确指定 offset/假定 UTC；不能在基础转换中隐式使用运行机器的本地时区。
    修复：按决定保留无 offset 时与 `new DateTimeOffset(DateTime)` 完全一致的行为；`ToDateTimeOffset`、`ToUnixTimeSeconds` 和 `ToUnixTimeMilliseconds` 新增可选 `TimeSpan? offset`。显式 offset 时调用 `new DateTimeOffset(dateTime, offset)` 并保留其 Kind/offset 验证规则，测试覆盖默认兼容行为、Unspecified 的显式 offset 和非法 Utc/offset 组合。

69. **[P1][已修复] `ArrayBasedCollection.AsSpan` 允许调用方绕过派生集合的不变量**

    位置：`src/FclEx.Core/System/Collections/Generic/ArrayBasedCollection.cs:148`。该公共可写 span 被 `Heap<T>` 和 `OrderedList<T>` 继承，调用方可直接打乱堆或排序顺序，且 `_version` 不会变化，现有枚举器也察觉不到。公共基类不应暴露可写后备存储；至少改成 `ReadOnlySpan<T>`，确需危险访问时应放到类型特定、明确命名且记录不变量责任的 API。
    修复：实例 `AsSpan` 改为 internal，公共实例入口改为 `AsReadOnlySpan`；可写访问集中到 `CollectionsMarshal.AsSpan(ArrayBasedCollection<...>)`。文档明确写入会绕过校验与版本更新、调用方必须维护堆序/排序等具体不变量，并说明 count/capacity 改动会使 span 失效。测试覆盖只读入口、marshal 可写入口和 null 行为。

70. **[P1][已修复] `MultiValueDictionary.AddRange` 可以留下没有任何 value 的 key**

    位置：`src/FclEx.Core/System/Collections/Generic/MultiValueDictionary.cs:674-695`。新 key 在枚举 `values` 前就加入内部字典，因此空序列或首次 `MoveNext` 抛异常都会留下空 collection；这与 `ContainsKey` 文档和内部“每个 key 至少一个 value”的不变量冲突。应先成功取得首项再插入，或在零项/异常时回滚新建 key。
    修复：新 key 的内部 value collection 先在未发布状态完整构建，只有至少包含一个 value 时才加入字典；空序列不添加 key，枚举或插入失败也不会留下部分发布的新 key。已有 key 的修改路径在枚举 values 前递增版本，确保循环中的修改和异常路径都会使既有枚举器失效；两个分支通过私有 `AddValues` 复用添加循环。测试覆盖空序列和产生首项后抛异常的枚举器。

71. **[P1][已修复] `Deque<T>` 的空队列操作只靠 `Debug.Assert` 防护**

    位置：`src/FclEx.Core/System/Collections/Generic/Deque.cs:49-95`。Release 下 `Peek`/`Dequeue` 会访问空数组或已分配数组中的默认槽位，后者还能把 `_size` 减成负数并永久损坏队列状态。应像 BCL 集合一样在空队列上抛 `InvalidOperationException`，并提供 `TryPeekHead`/`TryDequeueHead` 等非抛出 API。
    修复：四个 `Peek`/`Dequeue` 操作在空 deque 上统一抛 `InvalidOperationException`；新增 head/tail 两端的 `TryPeek` 与 `TryDequeue`。测试覆盖从未分配和已经分配过后重新变空的两种状态，确认失败操作不会破坏 count、head 或 tail。

72. **[P1][已修复] `Heap<T>` 在 comparer 抛异常时会损坏计数和堆结构**

    位置：`src/FclEx.Core/System/Collections/Generic/Heap.cs:80-87,121-142,233-289`。`Push` 在比较前先增加 `_count`，`Pop` 在下沉比较前先减少计数并清空尾槽；`SiftUp`/`SiftDown` 又边比较边移动元素。自定义 comparer 一旦抛错，集合可能丢元素、出现未初始化项或不再满足堆序。应先计算移动路径再提交，或在异常时恢复原计数、元素和路径。
    修复：上浮和下沉都拆成“只比较并计算目标/路径”与“不再调用 comparer 的提交”两个阶段；`Push` 在比较成功后才扩容和增加 count，`Pop` 在完整算出路径后才移动元素、清槽并减少 count。`PopPush`/`PushPop` 同样复用事务式下沉。测试让 comparer 在不同比较位置抛错，确认元素顺序、count、capacity、version 和后续堆操作均保持有效。

73. **[P1][已修复] `OrderedList` 的公开 bound API 不验证搜索区间**

    位置：`src/FclEx.Core/System/Collections/Generic/OrderedList.cs:339-389`。负 lower、超过 `Count` 的 upper、以及 lower 大于 upper 都会产生随机索引异常、读取容量内未使用槽位或返回无效边界。应统一验证 `0 <= lower <= upper <= Count`，并让两个方法对非法范围抛出一致、参数名正确的异常。
    修复：`LowerBound` 与 `UpperBound` 共用范围验证，统一要求 `0 <= lower <= upper <= Count`；越界 bound 抛参数名对应的 `ArgumentOutOfRangeException`，反向范围对 `upper` 抛 `ArgumentException`，并允许 `[Count, Count)` 空范围。内部有界 equality 判断也限制在指定搜索区间内。

74. **[P1][已修复] `OrderedList.EqualRange` 永远漏掉最后一个匹配项**

    位置：`src/FclEx.Core/System/Collections/Generic/OrderedList.cs:527-537`。`end` 是最后一个匹配项的包含式索引，循环却使用 `i < end`；只有一个匹配项时结果为空，多个匹配项时少一个。应使用半开区间的 `LowerBound`/`UpperBound`，或将循环条件改为包含 end。
    修复：复核发现把原循环改成 `i <= end` 虽能包含最后一个匹配项，但无匹配时 start/end 都为 `-1`，会读取 `_items[-1]`。最终改用标准的 `[LowerBound(item), UpperBound(item))` 半开区间，重复值完整返回，不存在的值返回空序列。

75. **[P1][已修复] `OrderedList.RemoveRange(min, max)` 在反向范围上返回负删除数**

    位置：`src/FclEx.Core/System/Collections/Generic/OrderedList.cs:539-550`。当 `min > max` 时 `end - start` 为负，私有删除方法静默 no-op，公共方法却把负数作为“删除数量”返回。应先验证 comparer 意义上的 `min <= max`，或者明确定义反向范围删除 0 项且返回 0。
    修复：该 API 表达包含式有效范围，因此 comparer 判定 `min > max` 时抛 `ArgumentException` 并保持列表不变；合法范围继续通过 lower/upper bound 删除全部边界重复项并返回非负删除数。

76. **[P1][已修复] `BiDictionary<TKey,TValue>` 在两种类型相同时公共 API 变得不可调用**

    位置：`src/FclEx.Core/System/Collections/Generic/BiDictionary.cs:59-86`。当 `TKey` 与 `TValue` 都是 `string` 等同一类型时，两个 `Remove` 和两个 indexer 的参数签名相同，C# 调用方无法凭返回类型消除歧义。类型约束却允许这种实例化。建议提供 `Forward`/`Reverse` 视图，或使用 `GetByKey`、`GetByValue`、`RemoveKey`、`RemoveValue` 等具名方法。
    修复：增加方向明确的 `GetValue`/`GetKey`、`SetValue`/`SetKey` 和 `RemoveKey`/`RemoveValue`；现有 indexer 与 `Remove` 委托给具名实现以保持单一逻辑。相同 key/value 类型现在可完整查询、更新和删除，测试覆盖 `BiDictionary<string, string>`。

77. **[P1][已修复] `StableSort(list, index, count)` 完全忽略 `index`**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/ListExtensions.cs:33-64`。比较读取 `list[a]`/`list[b]`，回写也使用 `list[i]`，因此请求排序中间区间时实际排序的是前缀。所有读取和写入都应加上 `index`，临时数据也只需复制目标区间；空列表仍应先验证 index/count 契约。
    修复：只复制 `[index, index + count)` 到临时数组，在该数组上稳定排序索引，并写回原区间；前后元素不再受影响。范围验证现在覆盖空列表、负 count、`index == Count` 的空范围和 index/count 越界，比较阶段抛错前也不会修改列表。

78. **[P1][已修复] `List.Items`/`SetCount` 以安全名称公开了破坏 `List<T>` 不变量的能力**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/ListExtensions.cs:96-119`。`Items` 返回整个私有容量数组，`SetCount` 直接写 `_size`/`_version`；调用方可以观察未使用槽位、制造未经初始化的“有效”元素，并依赖跨 runtime 不保证的私有布局。建议删除公共 API；不可替代的低层场景应使用目标框架公开的 `CollectionsMarshal`，并以 `Dangerous` 命名、限制目标框架和记录失效规则。
    修复：按决定保留方法名，但从普通 list 实例扩展移到 `CollectionsMarshal.Items(list)` 与 `CollectionsMarshal.SetCount(list, count)` 的静态扩展边界；现代 .NET 使用自带的 `CollectionsMarshal.SetCount`，旧目标才提供回填。文档明确容量槽位、版本绕过、后备数组替换、未初始化元素和私有布局风险；测试覆盖后备数组访问与显式初始化后扩 count。

79. **[P2][已修复] `List<T>` 的两个 `+` 运算符具有相反的所有权语义**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/ListExtensions.cs:179-196`。`list + otherList` 创建新列表，而 `list + item` 原地修改左操作数并返回同一实例；从相同符号无法判断是否产生副作用。建议让 `+` 始终是纯连接操作，原地修改只通过 `Add`/`AddRange`/`+=` 表达，或直接删除扩展运算符。
    修复：复核确认 `list + item` 已改为创建新列表，`+` 的两个 overload 现在都不修改输入；新增的 `+= item` 与 `+= IEnumerable<T>` 明确执行原地添加，并经测试确认变量仍引用原 list。`list + list` 内部也改为普通 `AddRange`，不再依赖危险后备数组 API。

80. **[P1][已修复] `BitsToInt` 对任何输入都返回 0**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.cs:135-144`。累加器从 0 开始却使用按位与 `&=`，所以任何 bit 都无法被设置。应使用 `|=`，同时明确首项代表最低位还是最高位，并拒绝或定义超过 32 位时的行为。
    修复：改用按位或设置 bit，明确首项是最低有效位，第 32 项对应 `int` 符号位；空序列返回 0，超过 32 项抛 `ArgumentException`，null 输入抛 `ArgumentNullException`。测试覆盖普通位型、符号位和超长序列。

81. **[P1][已修复] `Enumerable.Split(parts)` 的延迟查询捕获了跨枚举共享的可变索引**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.cs:358-365`。`i` 在查询外定义，第二次枚举会从上一次结束值继续，并发枚举还会竞态；`parts <= 0` 也没有验证，错误延迟到枚举时才发生。应在 iterator 每次枚举时创建局部状态，验证正数，并把 round-robin 行为改成更准确的名称，因为它不是连续分块。
    修复：API 重命名为 `DistributeRoundRobin(source, partitionCount)`，参数在调用时立即验证；索引成为 iterator 每次枚举独有的局部状态，重复或并发枚举不再共享进度。文档明确只返回非空 partition，测试覆盖分配顺序、重复枚举和分区数大于元素数。

82. **[P1][已修复] `WhenAllOrError` 提前失败后不再观察剩余任务**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.Task.cs:296-306`。任一已完成任务 fault/cancel 时 await 立即退出，列表中其他任务继续运行；它们随后 fault 时没有任何观察者，可能触发未观察异常。若契约要求 fail-fast，应给剩余任务附加可靠的异常观察并定义取消策略；否则直接使用 `Task.WhenAll` 并保留聚合完成语义。
    修复：保留 fail-fast：第一个 fault/cancel 仍立即传播，不等待剩余任务；枚举任务时为每个任务预先注册只在 fault 时同步执行的异常观察 continuation，因而提前返回后发生的异常也会被观察。任务序列中的 null 现在立即以 `ArgumentException` 拒绝。

83. **[P2][已修复] Span split 在未请求移除空项时仍会丢失空输入和尾部空项**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/ReadOnlySpanExtensions.cs:186-247`。`_remaining.IsEmpty` 会直接结束，因此空 span 不产生一个空项，`"a,"` 也不会产生尾部空项，和 `StringSplitOptions.None` 的预期不一致。枚举器需要单独记录“尚未开始/最后一个分隔符后仍有一个结果”的状态，而不能用 empty span 同时表示数据和完成。
    修复：枚举器以独立 `_hasResult` 状态区分“剩余数据为空”和“枚举结束”；空输入、前导/尾部分隔符及连续分隔符在 `None` 下都保留空项，`RemoveEmptyEntries` 仍会完整移除它们，并覆盖 trim 组合测试。

84. **[P2][已修复] `IntPtr.AbsDiff` 的返回类型和算术都无法表示合法地址差**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/IntPtrExtensions.cs:16-25`。有符号减法可能溢出，`-long.MinValue` 仍是负数；两个 64 位地址之间的距离还可能大于 `long.MaxValue`。应以无符号算术计算并返回 `nuint`/`ulong`，或对不可表示的结果显式抛出 overflow。
    修复：返回类型改为 `nuint`，根据进程位数将地址位型转换为 `uint`/`ulong` 后再以无符号大小关系相减，可表达从 0 到整个平台地址空间最大值的距离；测试覆盖交换操作数和全地址范围。

85. **[P2][已修复] `IsPossibleXml` 没有可供调用方依赖的有效契约**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/StringExtensions.cs:51-83`。三个 regex 不检查标签匹配、嵌套、属性或实体，可接受明显损坏的 XML；同时会拒绝合法的空元素、前导空白等输入。与已移除的 `IsPossibleHtml` 相同，这种“可能是”判断没有稳定用途；应删除并让调用方直接解析，若只需便宜的外形检查则必须以名称和文档明确它不验证 XML。
    修复：按实际用途改为 `CouldBeXmlDocument`，删除 regex，只做合法 XML 文档必需的廉价 envelope 检查（去除外围空白/BOM 后以 `<` 开始、以 `>` 结束）。文档明确 false 才能排除、true 不代表格式正确；若需要确定性验证仍必须交给 XML parser。

86. **[P0][已修复] `MarshalTo<T>` 会把任意字节解释成原生地址并解引用**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/BytesExtensions.cs:34-84`、`ReadOnlySpanExtensions.cs:29-62`。API 对 `T` 无约束，`PtrToStructure<T>` 遇到 string、数组或引用字段时会把输入字节当地址并解引用，可能产生访问冲突或读取非预期内存；span 单值版本在输入长于结构时还因复制整个 span 到较小缓冲区而抛错。应把原始二进制读取限制为 `unmanaged` 并用 `MemoryMarshal.Read`；真正的 interop marshaling 应使用明确命名、受控布局和可信输入，并只复制精确结构长度。
    修复：API 最终改为明确表达 interop marshaling 的 `MarshalReadAs<T>`/`MarshalReadArrayAs<T>`，约束为 `T : struct` 并通过 `Marshal.PtrToStructure` 读取。类型校验要求顺序或显式布局；托管数组只允许带正 `SizeConst` 的 `[MarshalAs(ByValArray)]`，字符串只允许 `[MarshalAs(ByValTStr)]`，其余会把输入解释成外部地址的托管引用在复制前即以 `NotSupportedException` 拒绝。单值只复制精确结构长度，数组拒绝尾部残缺结构。测试覆盖 fixed buffer、原始类型和结构体元素的 `ByValArray`，并在 .NET 8+ 覆盖相同两类元素的 `InlineArray`。

87. **[P1][已修复] `DirectoryInfo.IsSubOf` 的路径判定在不同平台和边界输入上不可靠**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/DirectoryInfoExtensions.cs:45-50`。实现无条件 `OrdinalIgnoreCase`，在大小写敏感文件系统上错误；父路径已有尾分隔符时会拼出双分隔符，无界静态 cache 还会永久保存任意路径。建议用规范化后的 `Path.GetRelativePath` 和平台正确的比较规则，明确“自身是否算子目录”及符号链接策略，并删除这个无收益缓存。
    修复：重命名为 `IsDescendantOf`，删除无界缓存，规范化完整路径并用带目录分隔符的父路径进行边界比较；Windows 使用不区分大小写比较，其他平台区分大小写。自身明确不算 descendant，文档注明这是不解析符号链接的词法判定。

88. **[P1][已修复] `DirectoryInfo.Sub`/`File` 允许 rooted 参数逃出父目录**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/DirectoryInfoExtensions.cs:33-43`。`Path.Combine(parent, rootedName)` 会忽略 parent，安全感很强的子项 API 因而可以返回任意绝对路径。若契约真的是直接子项，应拒绝 rooted path、目录分隔符和 `..`；若只是路径组合，应改用不暗示 containment 的名称。
    修复：两个 API 共用直接子项名称校验，拒绝空值、rooted path、`.`、`..` 以及任意目录分隔符；合法名称才与父目录组合。测试覆盖正常子目录/文件、嵌套路径、上级路径和绝对路径。

89. **[P1][已修复] `CreateNew` 以普通创建名称执行递归删除**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/DirectoryInfoExtensions.cs:25-31`。调用者容易把它理解成“确保新目录存在”，实际却会不可恢复地删除现有目录及全部内容。应删除该 API或改为明确的 `Recreate`/`DeleteAndCreate`，文档突出破坏性、验证精确目标，并优先提供非破坏性的 `CreateIfMissing`。
    修复：破坏性 API 重命名为 `Recreate`，XML 文档直述会递归删除全部内容，并禁止对文件系统根目录执行；原有内部测试调用已迁移，测试确认旧内容会被移除而目录被重新创建。

90. **[P1][已修复] `FileConflictOptions` 把互斥策略建模为可任意组合的 flags**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/FileConflictOptions.cs:6-41`、`FileInfoExtensions.cs:7,110-129`。`ThrowOnConflict | Overwrite` 等组合通过位掩码后没有匹配的 switch case，方法会静默返回且不执行操作。应把 resolution strategy 建模为普通 enum，把 `IgnoreConflictIfDuplicate` 作为独立选项；过渡期至少验证恰好选择一个策略并拒绝未知位。
    修复：移除 flags，新增普通 enum `FileConflictResolution` 与不可变 `FileTransferOptions`，分别承载 resolution、`IgnoreConflictIfDuplicate` 和 copy buffer size，且验证未知策略和非正 buffer。`CancellationToken` 保持每次异步调用的独立参数，避免把调用生命周期塞进可复用配置对象；copy/move/rename API 统一接受该 options，并返回自动重命名后的实际目标。

91. **[P1][已修复] 文件冲突处理存在检查后覆盖的 TOCTOU 竞态**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/FileInfoExtensions.cs:28-58,82-129`。`dest.Exists == false` 后调用的基础复制使用 `File.Create`，会覆盖在检查与创建之间由其他线程/进程新建的文件；`AutoRename` 的递归检查同样不能保证新名字仍空闲。应在非覆盖策略下用 `FileMode.CreateNew` 原子认领目标，AutoRename 遇到 `IOException` 再选择下一名称，只有明确 `Overwrite` 才使用 Create。
    修复：除显式 `Overwrite` 外，copy 先用 `FileMode.CreateNew` 写入目标目录中的唯一 staging file，完成后以非覆盖 move 原子认领最终名称；move 本身也使用非覆盖移动。只有真实存在的冲突才进入 cancel/throw/auto-rename 分支，自动重命名循环重试直至原子成功。失败或取消只清理自己的 staging file，最终路径不会暴露部分内容；并发 copy/move 测试确认两个操作获得不同目标且内容均保留。

92. **[P2][已修复] `ProcessInvoker` 的返回模型丢失了进程结果的关键结构**

    位置：`src/FclEx.Core/FclEx/Utils/~Diagnostics/ProcessInvoker.cs:35-80`、`ProcessInvocation.cs:3-10`。stdout 与 stderr 被并发写入同一队列，跨流顺序不确定且调用方无法分别处理；忽略非零退出码时又只返回字符串，连 exit code 也丢失。建议始终返回 `ProcessResult(ExitCode, StandardOutput, StandardError)`，再由显式策略决定非零退出是否抛异常。

    修复：`ExecuteAsync` 现在始终产生包含退出码、stdout 和 stderr 的 `ProcessResult`；`ProcessExitCodePolicy` 明确控制非零退出是抛出携带完整结果的 `ProcessException`，还是直接返回结果。

93. **[P1][已修复] PowerShell/Pwsh/WSL invoker 的命令参数拼接不能正确承载引号**

    位置：`src/FclEx.Core/FclEx/Utils/~Diagnostics/PowerShellInvoker.cs:18`、`PwshInvoker.cs:17-21`、`WslInvoker.cs:18`。把完整命令插入 `-command "..."` 或 `-c "..."` 而不转义内部引号、反斜杠和换行，合法命令会被外层命令行解析器截断或改写。应使用 `ProcessStartInfo.ArgumentList` 把命令作为单独参数传递，旧框架使用经过平台验证的 quoting helper，复杂脚本则通过 stdin/临时脚本文件传入。

    修复：invoker 现在生成独立参数列表；新目标使用 `ProcessStartInfo.ArgumentList`，旧目标使用集中实现的命令行 quoting。PowerShell/Pwsh 的命令作为 `-Command` 的单独参数传递，bash 命令作为 `-c` 的单独参数传递，并补充了引号、换行以及旧框架测试。

94. **[P1][不修改] 软删除实体用两个独立可写属性表达同一个状态**

    位置：`src/FclEx.Core/FclEx/Domain/~Entities/SoftDeletableEntity.cs:9-15`、`IHasDeletedAt.cs:6-11`、`ISoftDeletable.cs:6-11`。`IsDeleted == false` 同时带任意 `DeletedAt`，或 `IsDeleted == true` 配 `DateTimeOffset.MinValue` 都是合法对象状态。建议以 nullable `DeletedAt` 作为单一事实来源并派生 `IsDeleted`，或者只暴露 `Delete(at)`/`Restore()` 转换以维护不变量。

    决定：按当前需求保留现有软删除状态模型，不修改源码。

95. **[P2][已修复] `EntityChanges<T>` 是带可变 List 的 record，既不是值对象也不是稳定快照**

    位置：`src/FclEx.Core/FclEx/Domain/~Entities/EntityChanges.cs:20-43`。record equality 对三个 List 使用引用相等，两个内容相同的 changes 不相等；对象创建后列表仍可变，`init` 属性也能被显式赋 null，仅靠 nullable warning 阻止。应选择语义：可变工作集用普通 sealed class 和只读暴露，传输/值对象则复制到 immutable/read-only collection 并定义内容相等。

    修复：明确采用稳定快照语义。`EntityChanges<T>` 改为普通 sealed class，构造时防御性复制三个序列并以只读列表暴露；EF Core 的 `ApplyChanges` 在局部列表中累积结果，完成后再创建快照。

96. **[P1][不修改] `GetRequiredAsync` 的 default 参数使“Required”名存实亡**

    位置：`src/FclEx.Core/FclEx/Domain/~Services/IKeyValueService.cs:28-32`。缺少 key 时 `GetAsync` 会返回调用方提供的非 null default，方法因此成功而不是抛出；只有 default 本身为 null 才符合 Required 语义。应删除 defaultValue 参数并直接检测缺失，另设 `GetOrDefaultAsync`/`GetOrElseAsync` 表达 fallback。

    决定：按当前需求保留现有签名与行为，不修改源码。

97. **[P1][已修复] `NString.Equals(object)` 违反相等关系的对称性**

    位置：`src/FclEx.Core/FclEx/Utils/NString.cs:12-32`。`new NString("x").Equals("x")` 为 true，而 `"x".Equals(new NString("x"))` 为 false；跨类型 object equality 不满足 .NET 相等契约，会给集合和通用算法造成异常结果。`Equals(object)` 应只接受 `NString`，字符串便利性保留在显式/隐式转换或具名比较方法中。

    修复：`Equals(object)` 现在只接受 `NString`，并补充跨类型 object equality 的对称性测试；字符串转换和强类型运算符保持不变。

98. **[P1][已修复] `TreeNode.Children` 的公开可变列表绕过了 Parent 和树拓扑约束**

    位置：`src/FclEx.Core/FclEx/Utils/TreeNode.cs:15-31,33-59`。调用方可直接加入已有节点、删除节点、创建环或共享子树，却不会更新 `Parent`；`DeepEquals` 随后假定严格树结构，并在重复节点上 `map.Add` 抛异常。应封装 children，提供维护双向关系的 Add/Remove/Move API，拒绝环和多父节点，并让遍历对非法图有明确行为。

    修复：children 改为只读视图，新增受控的节点 Add、Remove、Detach、Move 和排序 API；所有拓扑变更维护 `Parent`，并拒绝自引用、环与多父节点。`DeepEquals` 也简化为在受保证的树结构上逐对遍历。

99. **[P0][已注明风险] `ObjectAccessor` 返回的托管对象地址在方法返回时就可能失效**

    位置：`src/FclEx.Core/FclEx/Utils/~Accessors/ObjectAccessor.cs:19-45,60-123,126-193`。这些方法把对象、引用槽位和字段的 managed byref 转成 `IntPtr[]` 后返回，期间没有也无法为任意对象建立可持续的 pin；下一次 GC 即可移动对象。`GetAddress(ref T)` 对引用类型返回的还是引用变量槽位地址，并非对象地址。建议删除面向任意托管对象的地址 API；仅对 `unmanaged` 值提供 scoped 操作，或让低层逻辑在受控 callback 内消费 byref，绝不把裸地址跨出生命周期。

    处理：按当前需求保留 API，但在委托、类型和各公开入口的 XML 文档中明确说明地址不带 pin、可能在返回后立即失效、引用类型的 `GetAddress` 返回引用槽位而非对象数据地址，以及任意托管对象并不都能安全 pin。

100. **[P1][已修复] `OrderedIndex.UpdateScore` 先删除再添加，失败时会永久丢失原项**

    位置：`src/FclEx.Core/System/Collections/Generic/OrderedIndex.cs:245-253`。旧节点先从 skip list 和 map 移除，随后 `Add(newScore, value)` 若因 comparer 或其他异常失败，没有任何回滚，公开的“更新”操作变成删除。应在提交前完成所有可能失败的比较/分配，或保存旧 score/sequence 并在异常路径恢复原节点，保证操作成功或集合保持原状。

    修复：插入被拆分为准备与无异常提交阶段；更新先保存完整移除计划，若新 score 的比较、节点分配或字典更新失败则原样恢复旧节点，成功时才链接新节点并推进版本。新增 comparer 抛异常后的内容、rank、score 与枚举器版本保持测试。

## 第三轮审查（101–150）

范围仍为 `FclEx.Core`，排除 `Combinatorics` 文件夹下的源码。本轮按整体设计、公共 API/签名、实现与跨目标兼容性的顺序继续审查；累计 50 个新问题后停止。

101. **[P1][已修复] `JsonStringSerializer` 会在真正解析前拒绝合法 JSON**

    位置：`src/FclEx.Core/FclEx/Utils/~Serialization/JsonStringSerializer.cs:8-12`、`src/FclEx.Core/FclEx/Extensions/~System/~Text/~Json/StringExtensions.cs:5-45`。`Deserialize` 先调用基于首尾字符的 `IsPossibleJson`，因此带合法外围空白的 `" null "`、`" [1] "` 会被拒绝，指数形式等值也受启发式规则限制。序列化器不应以不完备的外形检查覆盖 `JsonSerializer` 的语法判断；应直接反序列化并保留原始 `JsonException`。
    修复：确认现实现已移除 `IsPossibleJson` 预检并直接调用正式 JSON parser；补充外围空白的标量和数组回归测试，合法 JSON 不再被启发式规则拦截。

102. **[P1][已修复] `JsonMemoryBytesSerializer.Instance` 的线格式取决于运行时类型**

    位置：`src/FclEx.Core/FclEx/Utils/~Serialization/JsonMemoryBytesSerializer.cs:3-13`、`StringAsRawSerializer.cs:14-26`。名为 JSON 的默认实例用 `StringAsRawSerializer`：字符串 `abc` 被写成裸字节 `abc`，其他对象才写成 JSON；同一入口因运行时类型改变协议，且与 `JsonBytesSerializer` 的直觉契约不一致。应让 JSON serializer 始终产生 JSON；需要字符串直通时使用另一个明确命名的组合器或显式选项。
    修复：移除两个会掩盖组合语义的旧类型。新增 `StringPassthroughSerializer`，仅负责让 string 原样通过并把其他类型委托给 fallback；新增 `Utf8MemoryBytesSerializer`，只负责 string/UTF-8 memory 适配；`SerializerPresets.StringOrJson` 与 `Utf8StringOrJson` 明确组装“字符串原样、其他类型 JSON”的需求。Redis 与 Messaging 的默认 serializer 已迁移到具名组合，测试覆盖普通文本、看似 JSON 的字符串、非 ASCII 文本和对象 JSON 往返。

103. **[P1][已修复] `PagedList<T>` 允许构造互相矛盾的分页状态**

    位置：`src/FclEx.Core/FclEx/Utils/~PagedList/PagedList.cs:17-41`。构造器只校验参数各自的范围，不校验 `items.Count <= pageSize`、`items.Count <= totalCount`、非空页是否落在 `PageCount` 内等关系；于是可出现 `TotalCount == 0` 但包含元素、`ItemStart > ItemEnd` 或超出末页仍带数据的对象。分页结果应有统一的不变量，并在构造边界拒绝不可能状态。
    修复：页数改用无浮点且不会中间溢出的整数计算；拒绝超出已知页数的 `pageIndex`，并按当前页在总结果中的剩余容量限制 `items.Count`，因此空总数带元素、超过 page size 以及末页元素过多都会失败。`ItemStart/ItemEnd` 现在按实际返回元素计算，空页统一为 0/0。

104. **[P1][已修复] `PagedList<T>` 的元数据是快照，元素集合却仍可在外部变化**

    位置：`src/FclEx.Core/FclEx/Utils/~PagedList/PagedList.cs:11,27-41,56-59`。构造器直接保存调用方的 `IReadOnlyList<T>`；该接口不代表不可变，传入 `List<T>` 后继续增删会改变 `Count` 和索引内容，而 `TotalCount`、`PageCount`、`ItemStart/ItemEnd` 永远不变。应明确采用快照并防御性复制，或把分页元数据改成随底层集合一致变化的 view；当前混合语义不可保持一致。
    修复：构造时把输入复制到私有数组，后续调用方修改原 list/array 不再影响分页内容、Count 或范围元数据；测试覆盖修改和追加原集合后的稳定快照。

105. **[P1][已修复] `UriCreator.SplitUri` 把 fragment 中的问号误判为非法查询顺序**

    位置：`src/FclEx.Core/FclEx/Utils/~Net/UriCreator.cs:185-197`。`a#fragment?x` 是合法相对 URI，`?x` 属于 fragment；实现却因第一个 `?` 位于 `#` 后而抛 `ArgumentException`。应先定位 `#`，只在 fragment 之前寻找 query 分隔符，或交给标准 URI 解析逻辑。
    修复：先从第一个 `#` 拆出完整 fragment，再只在 fragment 之前的 path/query 部分查找 `?`；fragment 内的所有问号均原样保留。测试同时覆盖“只有 fragment 内问号”和“正常 query 加含问号 fragment”。

106. **[P1][不修改] `UriAttribute` 把 `://` 错当成所有绝对 URI 的必要条件**

    位置：`src/FclEx.Core/System/ComponentModel/DataAnnotations/UriAttribute.cs:17-31`。`mailto:user@example.com`、`urn:isbn:...` 等合法绝对 URI 有显式 scheme 但没有 `://`，即使 `AllowedSchemes` 允许也会提前失败；文档所称“任意显式 scheme”与实现不符。应先用 `Uri.TryCreate(..., Absolute)`，再检查 `uri.Scheme`，不要用层次型 URI 的分隔符替代 scheme 判断。
    处理决定：该 attribute 的预期契约就是只接受含 `://` 的 URI，并有意排除 `mailto:`、`urn:` 等不含该分隔符的绝对 URI。实现保持不变，文档已明确这一限制，并增加回归测试固定该行为。

107. **[P2][已修复] `ElementRequiredAttribute.MinLength` 可被设置为负数并使校验失效**

    位置：`src/FclEx.Core/System/ComponentModel/DataAnnotations/ElementRequiredAttribute.cs:6,27-49`。可写属性没有范围验证，负值会让所有字符串和 enumerable 都满足 `count >= MinLength`。attribute 配置应在 setter/构造器中拒绝负数；如果零是默认有效含义，也应在文档中明确。
    修复：`MinLength` setter 现在拒绝负数，XML 文档明确零是有效值且表示不施加最小长度限制；测试覆盖负数和零值边界。

108. **[P1][已修复] `FormattedException` 不是标准异常包装器，且 null 校验不会按文档生效**

    位置：`src/FclEx.Core/FclEx/Utils/~Exceptions/FormattedException.cs:6-23`。基类构造器先读取 `exception.Message`，传 null 得到 `NullReferenceException` 而非声明的 `ArgumentNullException`；正常输入又只保存到自定义 `Exception` 属性，没有作为 `InnerException` 传给基类，标准日志、异常遍历和 `GetBaseException` 看不到因果链。应先通过静态 helper 校验，并调用 `base(message, exception)`；自定义属性通常可直接删除。
    修复：包装异常现在传给基类作为标准 `InnerException`，自定义重复属性已删除，null 输入按文档抛出 `ArgumentNullException`；回归测试覆盖因果链、消息、格式化和 null 校验。

109. **[P1][已修复] `SimpleException` 的“省略栈以提升性能”设计并不能阻止抛出时捕获栈**

    位置：`src/FclEx.Core/FclEx/Utils/~Exceptions/SimpleException.cs:3-53`。覆盖 `StackTrace` 返回 null 只隐藏诊断信息，CLR 在 throw 时仍会记录栈；`[StackTraceHidden]` 也只是影响格式化。反向选项 `noStackTrace == false` 在构造时额外抓取一次栈，但异常真正抛出后又优先返回 `base.StackTrace`，这份成本通常白付。建议删除该性能承诺和栈覆盖，让“仅消息”成为格式化策略而不是异常对象状态。
    修复：保留现有表现形式，但文档不再声称能够避免运行时捕获栈或以性能为主要目标；类型用途改为承载字符串形式的错误（常用于 `OperationResult<T>`），并明确 `noStackTrace` 只控制对外呈现。

110. **[P1][已修复] `DataMemberInfo.CanWrite` 对 readonly field 返回 true**

    位置：`src/FclEx.Core/System/Reflection/DataMemberInfo.cs:6-20,61-75`。所有 field 都被标记 `CanWrite = true`，包括 `IsInitOnly` 字段；调用方按 `CanWrite` 选择 setter 后仍可能失败或绕过类型不变量。`CanWrite` 应表达实际支持的普通写入能力并排除 init-only/literal 字段，危险的反射写入若保留应独立命名。
    修复：field 的 `CanWrite` 和 `HasPublicSetter` 现在都会排除 init-only 与 literal 字段；现有 `UnsafeWrite` flag 仍作为显式选择 readonly 反射写入的独立入口，literal 字段不会被包含。测试覆盖 public/private、static/instance readonly field 以及 public/private constant。

111. **[P1][已修复] `DataMemberInfo` 的统一 getter/setter 签名无法表示 indexer**

    位置：`src/FclEx.Core/System/Reflection/DataMemberInfo.cs:24-40,71-75`。类型会收集 indexer 并设置 `IsIndexer = true`，但只暴露不带索引参数的 `Func<object?, object?>` / `Action<object?, object?>`；对 indexer 调用必然以参数数量错误失败。应在数据成员抽象中排除 indexer，或为它设计包含索引参数的独立访问模型，而不是暴露看似可调用的无效委托。
    修复：为 indexer 增加独立的 `IndexerGetter` 与 `IndexerSetter`，两者均显式接收索引参数；普通 `Getter`/`Setter` 文档也明确只适用于非 indexer 成员。测试覆盖使用 int indexer 的读取与写入。

112. **[P0][已修复] `ReferenceEqualityComparer<T>` 允许值类型，导致相等和哈希契约失真**

    位置：`src/FclEx.Core/System/Collections/Generic/~EqualityComparers/ReferenceEqualityComparer.cs:8-29`。`T` 没有 `class` 约束；值类型参数在 `ReferenceEquals` 和 `RuntimeHelpers.GetHashCode` 中分别装箱，同一个值通常也不相等，哈希还随新装箱对象变化。应加 `where T : class`；值类型不存在可复用的“引用相等”语义。
    修复：`ReferenceEqualityComparer<T>` 现在以 `where T : class` 限制为引用类型，阻止值类型使用无意义的引用相等比较。

113. **[P1][已修复] `ComparerHelper.TryEquals` 在调用自定义 comparer 前强制要求运行时类型相同**

    位置：`src/FclEx.Core/System/Collections/Generic/ComparerHelper.cs:25-43`，并影响 `EnumerableEqualityComparer`、`KeyEqualityComparer`、`DelegateEqualityComparer`、`MemberEqualityComparerBuilder` 等。对于声明为基类或接口的 `T`，比较器本可按成员/序列语义判定两个不同派生类型相等，该 helper 却提前返回 false；例如内容相同的数组和列表无法由序列 comparer 比较相等。helper 只应处理引用相同/null，运行时类型策略应由具体 comparer 决定。
    修复：新增 `requireSameRuntimeType` 参数，默认保持原有严格行为；传入 `false` 时，非 null 的不同运行时类型不会在 helper 中被提前判定，调用方可继续使用自身 comparer 语义。测试覆盖两种分支。

114. **[P0][不修改] `MarshalToBytesEqualityComparer<T>` 的相等与哈希依赖未定义的 padding 和地址值**

    位置：`src/FclEx.Core/System/Collections/Generic/~EqualityComparers/MarshalToBytesEqualityComparer.cs:3-28`、`src/FclEx.Core/FclEx/Helpers/ObjectHelper.cs:42-62`。`Marshal.SizeOf` 包含 padding，而 `StructureToPtr` 不保证覆盖新分配 native buffer 的所有 padding；同一值的字节和哈希可能不稳定。带引用 marshaling 的字段还会把指针/分配结果纳入比较，而不是比较所指内容。该 comparer 无法提供通用 `IEqualityComparer<T>` 契约，应删除，或只为明确的 blittable 布局定义受约束的 bitwise comparer。
    处理决定：`ObjectHelper.MarshalToBytes` 新增默认关闭的 `clearNativeBuffer` 参数；比较器固定启用它，使 native padding 在 marshal 前归零。比较器文档明确说明 pointer-based marshaling（如 `LPStr`、`LPWStr`、`BStr`、`LPArray`、接口指针和 custom marshaler）会产生地址值，因此不能提供可靠的结构相等或可移植表示。`LPStr` struct 的复现测试表明：同一值可因不同临时地址而不等，两个独立值也可因 allocator 复用地址而相等；两项测试均保留并按已知限制标记为 skip，注明原因。不再将该限制作为当前实现的待修复问题。

115. **[P1][已修复] `ThenWithAction` 可把不存在的下一值或失败伪装成成功 tuple**

    位置：`src/FclEx.Core/FclEx/Actions/ThenWithAction.cs:3-54`。`errorWhenNextNull=false` 返回 `(item, default(TNext))` 的成功结果；`prevWhenNextError=true` 还会吞掉 next 的异常并返回同样的成功值。返回类型承诺两个成功值，这两个布尔开关却允许缺值和失败进入成功通道。应以 `Optional<TNext>`/discriminated result 显式表达缺失，失败保留为失败；不要用布尔参数改变结果类型的真实性。
    修复：移除会伪造成功 tuple 的两个参数。next action 缺失时返回错误，next action 的失败保持为失败；测试覆盖缺失 next action 的错误路径。

116. **[P1][不修改] `IPipelineAction<T>` 在不同目标框架上要求实现不同的接口成员**

    位置：`src/FclEx.Core/FclEx/Actions/IPipelineAction.cs:4-51`。NET6+ 通过 default interface implementation 提供 `GetName`、handler 和显式 `IAction.ExecuteAsync`，旧目标却把前三者变成必须实现且没有 `ExecuteAsync` 默认实现；同一消费者源码不能稳定跨该包支持的 TFM 编译。公共接口形状应跨目标一致：把默认逻辑放到抽象基类/组合器，或在所有 TFM 要求相同成员，而不是条件编译实现责任。
    处理决定：保留条件编译的接口默认实现。低版本平台没有该语言/运行时能力；抽象基类不能替代接口，因为使用者可能已经需要继承其他基类。此处接受与 Serilog 类似的跨目标实现差异。

117. **[P1][已修复] 把已经启动的 `Task` 转成 `IAction` 破坏了 action 的延迟和可重复执行语义**

    位置：``src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.`.Async.cs:387-398,504-517``。这些 overload 捕获现成 task；选择 series 并不能让任务串行启动，token 无法传入原操作，同一个 action 多次执行也只会反复观察同一个结果。应接受 `Func<CancellationToken, Task<...>>` 工厂；若只是等待现成任务，就不要包装成可执行 action。
    修复：批量转换 overload 现在接受带 `CancellationToken` 的 selector 工厂，因而在 action 执行前不会启动操作，并且每次 `ExecuteAsync` 都会重新调用工厂。保留后两个 `Task` overload，它们明确只包装既有 task 的等待/合并语义；测试覆盖工厂延迟执行和重复执行。

118. **[P2][已修复] 同名同步/异步 `Fallback` 对 elapsed 的定义相反**

    位置：``src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.`.cs:329-361``、``OperationResultExtensions.`.Async.cs:400-447``。同步 overload 直接返回 fallback 并丢弃源结果耗时，异步 overload 则把源耗时加到 fallback 上。相同概念不应因调用形态改变度量语义；应统一定义为端到端耗时或仅最终分支耗时，并让所有 overload 一致。
    修复：同步 fallback 在执行 fallback 时也累加源结果与 fallback 结果的 elapsed；文档和测试已与异步版本对齐。

119. **[P2][已修复] 同名同步/异步 `Merge` 使用不可比较的 elapsed 语义**

    位置：``src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.`.cs:364-385``、``OperationResultExtensions.`.Async.cs:486-501``。同步 `Merge` 汇总每个 result 的 elapsed，异步版本随后用等待 source task 的墙钟时间覆盖该值；串行、并行以及已完成 task 得到的数字含义完全不同。应明确选择 aggregate work time 或 wall-clock duration，并用不同属性/方法表达两种指标，而不是静默覆盖。
    修复：异步 `Merge` 不再记录等待 source task 的墙钟时间，而与同步版本一样只汇总包含的 result elapsed；文档和延迟 task 回归测试确认此语义。

120. **[P1][已修复] `Optional.Some(null)` 会静默变成 `None`**

    位置：`src/FclEx.Core/FclEx/Utils/Optional.cs:7-53`。`Some<T>(T value)` 允许 nullable `T`，而 `HasValue` 完全由 `Value is not null` 推导，所以 API 名称承诺的“有值”可立即消失。若 null 不算值，应加 `where T : notnull` 并运行时校验；若要支持 `Some(null)`，结构必须保存独立的存在位。
    修复：`Optional.Some` 现在运行时拒绝 null，防止其创建会立刻表现为 `None` 的值；测试覆盖 null 拒绝和非 null 创建。

121. **[P1][不修改] `NameIdentifier<T>` 默认缓存会永久保留所有动态名称**

    位置：`src/FclEx.Core/FclEx/Utils/NameIdentifier.cs:28-51`。每个闭合类型都有无界静态 `ConcurrentDictionary<string,T>`，`GetOrCreate` 又默认启用缓存；用户输入、路径、租户名等高基数名称会被进程永久持有，只能由全局 `ClearCache` 粗粒度清空。interning 不应成为所有 identifier 的默认责任；应由调用方显式提供有界缓存，或仅对已知有限集合启用。
    处理决定：这是按闭合泛型类型划分的 identifier cache；缓存的基数和清理责任由具体 identifier 类型决定，保留现有设计。

122. **[P1][已修复] `NameIdentifier<T>` 没有验证工厂结果与缓存 key 一致**

    位置：`src/FclEx.Core/FclEx/Utils/NameIdentifier.cs:41-46`。`T.Create(name)` 可以规范化、忽略甚至返回另一个 `Name`，但结果仍缓存到原字符串 key；同一逻辑 identifier 因不同 key 出现多个实例，`GetOrCreate(x).Name` 也可能不等于 x。要么把规范化作为显式 key selector 并在查缓存前执行，要么强制校验工厂保持名称不变。
    修复：在缓存和非缓存路径都验证 `T.Create(name)` 的结果非 null，且其 `Name` 与输入以 ordinal 方式完全一致；违反约定时抛出 `ArgumentException`。测试覆盖两个路径。

123. **[P1][已修复] `ScopedSetter<T>` 对值类型看似成功，实际修改的是丢弃的装箱副本**

    位置：`src/FclEx.Core/FclEx/Utils/ScopedSetter.cs:20,27-60`。泛型没有引用类型约束；当 `T` 是 struct 时 `_obj` 已是调用参数副本，反射访问还会再次装箱，临时值不会写回调用方变量。API 应限制 `where T : class`；若要支持 struct，必须改为 scoped `ref T` 设计，不能用当前持有值的 class。
    修复：`ScopedSetter.For<T>` 和 `ScopedSetter<T>` 均添加 `where T : class`，从类型系统阻止无效的值类型用法。

124. **[P1][已修复] `ScopedSetter.Dispose` 既不幂等，也不能保证完整恢复**

    位置：`src/FclEx.Core/FclEx/Utils/ScopedSetter.cs:30-39`。Dispose 后没有清空/交换恢复表，第二次 Dispose 会再次覆盖对象在第一次 Dispose 后的新修改；恢复任一成员抛异常时，后续成员永远不恢复。应原子取走待恢复状态以保证最多执行一次，并在逐项恢复时收集异常或用可靠的 finally 策略完成其余恢复。
    修复：`Dispose` 以原子交换取走恢复表，重复调用不再改变对象；逐项恢复时继续执行并收集全部失败，单个失败保留其原始异常，多个失败以 `AggregateException` 报告。Dispose 后再调用 `Set` 会抛 `ObjectDisposedException`。测试覆盖幂等和恢复失败时仍恢复其余成员。

125. **[P2][已修复] `SourceBuilder.WriteUsings` 让生成源码随当前文化排序**

    位置：`src/FclEx.Core/FclEx/Utils/SourceBuilder.cs:175-185`。无 comparer 的 `OrderBy` 对字符串使用当前文化，生成文件在不同 OS/区域设置下可能顺序不同，造成增量生成和快照结果不稳定。源码生成应使用 `StringComparer.Ordinal`（并考虑去重）保证确定性。
    修复：已使用 `StringComparer.Ordinal` 排序，使输出不受当前文化影响。

126. **[P1][待重构] `ArgumentBuilder` 会静默忽略调用方提供但未被构造器消费的参数**

    位置：`src/FclEx.Core/FclEx/Utils/ArgumentBuilder.cs:39-90`。到达最后一个 parameter 就把路径视为匹配，从不要求 `RemainArgIndexes` 为空；parameterless constructor 更是不论传了多少参数都直接匹配。对象可能由错误构造器创建且调用方无从发现 typo/多余依赖。默认契约应要求每个 supplied argument 恰好消费一次，除非 API 明确提供“允许剩余参数”的模式。
    处理决定：`ArgumentBuilder` 需要整体重构；在确定新模型前，不单独处理该类及相关问题。

127. **[P1][待重构] `ArgumentBuilder` 的构造器排名会偏爱更多默认参数并对歧义做非确定选择**

    位置：`src/FclEx.Core/FclEx/Utils/ArgumentBuilder.cs:17-35,77-83`。候选先按参数总数降序，再按 `UseDefaultCount` 降序，因而较长、更多参数靠默认值的构造器可能压过较短的精确匹配；完全同分时直接取反射枚举顺序的 `First()`。应优先最大化实际消费/精确匹配、最小化默认值，并在最佳候选不唯一时抛明确的 ambiguity error。
    处理决定：随 `ArgumentBuilder` 的整体重构一并处理，暂不作局部修改。

128. **[P1][已修复] `ConsoleTable` 公开可变行列，允许绕过渲染所依赖的列数不变量**

    位置：`src/FclEx.Core/FclEx/Utils/~ConsoleTable/ConsoleTable.cs:3-13,15-86`。`Columns` 数组和 `Rows` 的可变 `List<object?[]>` 都直接公开，调用方可插入长度不足的 row 或修改列数；`GetColumnLength` 随后无条件访问 `row[index]` 并抛越界异常。应私有化存储、复制输入，并只通过校验列数的 `AddRow`/builder API 修改。
    修复：行列存储改为私有；构造器和 `AddRow` 都复制输入，公开成员只提供只读视图/快照，修改只能经过会校验列数的 `AddRow`。测试覆盖输入数组后续修改不会影响表格。

129. **[P0][已修复] `SynchronizationContextScope.RunAsync` 把线程局部状态跨越了 `await`**

    位置：`src/FclEx.Core/FclEx/Utils/~Threading/SynchronizationContextScope.cs:6-11,29-43`。`Enter` 修改当前线程的 `SynchronizationContext`，`RunAsync` 在第一次未完成 await 时就把控制权返回给调用方，原线程仍保持被替换的 context；continuation 还可能在另一线程执行并在那里“恢复”旧 context。线程局部 scope 不能跨异步挂起；应删除两个 `RunAsync`，或通过显式 scheduler/context dispatch 执行 callback，而不是临时修改调用线程。
    修复：删除两个可跨越 `await` 的 `RunAsync` overload，仅保留同步 scope API。

130. **[P1][已修复] `ReaderWriterLockSlim` 的 `IDisposable` lease 可在错误线程释放锁**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Threading/ReaderWriterLockSlimExtensions.cs:3-21`。`ReaderWriterLockSlim` 的 enter/exit 是线程关联的，但返回普通 `IDisposable` 无法阻止 scope 跨 `await`、被传递或在另一线程 Dispose；此时 Exit 抛错并可能让锁永久保持。应至少用无法跨 await 的 `ref struct` lease 并明确仅同步作用域，或改用接受同步 callback 的 API。
    修复：已移除该 `IDisposable` lease 扩展，避免暴露无法保证线程关联的抽象。

131. **[P1][已修复] `TaskCompletionSource.Exception` 擅自把调用方异常替换成 base exception**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Threading/TaskCompletionSourceExtensions.cs:5-10,25-29`。`GetBaseException()` 会剥掉普通包装异常及单 inner 的 `AggregateException`，丢失上层语义、消息和上下文；名为 `Exception(ex)` 的 helper 理应保存传入对象。应直接 `SetException(ex)`，若确需 unwrap 则提供显式命名的独立 API。
    修复：两个 overload 都直接调用 `SetException(ex)`，保留调用方提供的异常对象和其包装语义。

132. **[P1] `StreamReaderExtensions` 在 `StreamWriter` 类型上查找读取方法**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/StreamReaderExtensions.cs:5-7,29-30`。两个反射字段都对 `typeof(StreamWriter)` 查找 `ReadToEndAsync`/`ReadLineAsync`，结果必为 null；旧目标程序集即使运行在提供原生 cancellation overload 的新 runtime 上也永远走 fallback，取消只停止等待而不中断底层读取。应改为 `typeof(StreamReader)`，并为反射返回类型分别验证 `Task<string>`/`ValueTask<string?>`。

133. **[P1] 旧目标的异步文本写入宣称支持取消，但主体写入完全不观察 token**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/FileExtensions.cs:17-25`、`StreamWriterExtensions.cs:17-26`。`WriteAllTextAsync` 用无 token 的 `sw.WriteAsync(content)` 写完全部内容后才在 flush 检查取消；`FlushAsync(token)` fallback 又直接忽略 token。大文本操作可能在取消后继续长时间写盘却仍最终报告取消。应分块写入并在块间观察 token；无法取消底层 flush 时至少取消等待并在文档中说明 I/O 可能继续。

134. **[P1][已修复] `TextWriter.SetConsole` 用可嵌套 scope 包装进程级全局状态**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/TextWriterExtensions.cs:3-10`。`Console.Out` 是进程全局属性；并发或非 LIFO Dispose 的两个 scope 会互相覆盖，并把过期 writer 恢复回来，没有任何同步或所有权检查。该能力不适合作为通用 `IDisposable` 扩展；应由应用启动层集中设置，测试重定向则使用串行 fixture/显式全局锁。
    修复：已移除 `TextWriter.SetConsole`，不再为进程级全局状态提供看似局部的 scope 抽象。

135. **[P0] `PhysicalAddress.AddressBytes` 公开了可替换/可修改的私有后备数组**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Net/~NetworkInformation/PhysicalAddressExtensions.cs:5-32,52-56`。NET8+ 甚至返回 `ref byte[]`，调用方可替换 `_address`；旧目标也返回同一可变数组。对象可在作为 dictionary key 后被修改，破坏 equality/hash，不同 TFM 的返回签名还不一致，并依赖私有字段名。应删除该公共 API，格式化直接使用官方 `GetAddressBytes()` 的副本；确需零复制只能限于 internal、只读且目标受控的实现。

136. **[P1][已修复] 旧目标回填的 `HttpRequestException.StatusCode` 与官方 nullability 契约不同**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Net/~Http/HttpRequestExceptionExtensions.cs:7-18,27-33`。官方属性是 `HttpStatusCode?`，这里却返回非 nullable enum，并用数值 0 表示不存在；调用方跨 TFM 编译时既看到不同签名，也无法区分“无状态码”和非法/默认值。回填 API 应精确匹配官方 nullable 类型与缺失语义。
    修复：旧目标回填属性改为 `HttpStatusCode?`；工厂方法也接受 nullable status code，缺失状态码保持为 null。

137. **[P1] `IsIPv6UniqueLocal` 为简单位判断依赖 `IPAddress` 私有字段布局**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Net/IPAddressExtensions.cs:48-54`、`src/FclEx.Core/FclEx/FieldInfos.cs:16-22`。旧目标实现读取 `_numbers`，字段名、元素顺序和存在性都不是 runtime 契约，在不同 Mono/.NET Framework 实现、裁剪或未来 runtime 上会失效。该判断只需官方 `GetAddressBytes()` 的首字节满足 `(b & 0xFE) == 0xFC`，没有使用反射的合理性。

138. **[P1][已修复] embedded resource 的后缀匹配会在重名时任意选择第一个资源**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Reflection/AssemblyExtensions.cs:11-18`、`src/FclEx.Core/FclEx/Helpers/ResourceHelper.cs:7-16`。两个入口都对 manifest names 做无 `StringComparison` 的 `EndsWith(name)` 并取 `FirstOrDefault`；不同命名空间含同名资源时结果依赖枚举顺序，文化比较也不适合标识符。应优先要求完整资源名；若支持短名，使用 ordinal 比较并在多个候选时抛 ambiguity error。
    修复：两个入口统一使用内部 resolver：先做 ordinal 完整名称匹配，再允许唯一的 ordinal 后缀匹配；无匹配仍按原入口语义处理，多个后缀匹配则抛明确的 `ArgumentException`。测试使用两个同后缀嵌入资源覆盖完整名称和歧义路径。

139. **[P1][已修复] `ReflectionHelper` 的全局 Type cache 会阻止 collectible assembly 卸载**

    位置：`src/FclEx.Core/FclEx/Helpers/ReflectionHelper.cs:16-44`。静态 `ConcurrentDictionary<Type,...>` 强引用每个见过的 Type 及其 `MemberInfo`，插件/脚本通过 collectible `AssemblyLoadContext` 加载的程序集将永远被该 Core helper 固定。应使用 `ConditionalWeakTable<Type,...>` 或让缓存归属调用方/可卸载上下文。
    修复：cache 改为 `ConditionalWeakTable<Type, IReadOnlyList<DataMemberInfo>>`，不再由缓存的 key 强引用 Type；同时保留同步初始化以避免重复创建值。

140. **[P0] `AccessorAccessesField` 没有解码 IL 指令，可能误判或解析任意 operand 为 metadata token**

    位置：`src/FclEx.Core/FclEx/Helpers/ReflectionHelper.cs:83-127`。循环逐字节寻找 `0x7B/0x7D/0x7E/0x80`，这些字节也可能出现在其他指令的 operand 中；随后把后四字节交给 `ResolveField`，可抛出 metadata 异常或碰巧命中错误字段。必须按 opcode 和 operand 宽度完整解码 IL（含双字节 opcode），并只对真实 field 指令解析 token；否则应删除这一启发式公共 API。

141. **[P0] `HashAlgorithm.Hash` 把空输入的摘要定义成空数组**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Security/~Cryptography/HashAlgorithmExtensions.cs:5-25`。空消息有标准且非空的 cryptographic digest，实现却对 null/空数组返回 `[]`，使所有算法在空输入上产生相同结果；offset/count overload 还因此跳过本应发生的范围校验。应对空数组调用 `ComputeHash`，对 null 明确抛 `ArgumentNullException` 或单独定义 nullable 语义。

142. **[P1] `IAsyncEnumerable` materializer 没有取消入口**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/AsyncEnumerableExtensions.cs:3-19`。`ToListAsync`/`ToArrayAsync` 只能无 token 枚举，面对无限流、慢 I/O 或调用方取消时无法通过 API 传播 cancellation；这对异步 materializer 是核心签名缺失。应增加 `CancellationToken` 并使用 `source.WithCancellation(token).ConfigureAwait(false)`，保持与常见 async LINQ 约定一致。

143. **[P1] `Exception.ForEach` 的实现并不遍历“每个异常”，且共享节点会重复执行**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/ExceptionExtensions.cs:37-86`。`action` 只在没有 inner 的叶节点调用，root `AggregateException` 和所有中间包装异常都被跳过，与文档相反；`handled` 又只在叶节点处理后写入，两个分支共享同一 inner 时会先重复入队并重复执行。应采用统一的 visited set，在 dequeue 时去重，并明确对每个节点（含 aggregate/container）执行一次还是只遍历 leaves，名称和文档须与选择一致。

144. **[P1] `Enum.Info/GetAttribute` 对未命名值和复合 flags 值抛 `NullReferenceException`**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/EnumExtensions.cs:7-18,100-105`。`enumValue.ToString()` 对 `A | B` 返回 `"A, B"`，对未知数值返回数字字符串；`GetField(...)` 为 null 后被 `!` 掩盖并立即调用扩展。attribute 查询应在没有对应声明字段时返回 null，`Info` 还需定义复合值是按组成成员合并信息还是只提供格式化名称。

145. **[P1] `TryToInteger<TEnum,TInteger>` 实际接受任意同尺寸 unmanaged 类型并做位重解释**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/EnumExtensions.cs:32-57`。`TInteger : unmanaged` 允许 `float`、`Guid`、用户 struct 等；只要 `Unsafe.SizeOf` 相同就返回 true，并非“转换为整数”。应为受支持的整型提供明确 overload/运行时类型检查并定义溢出策略；若保留位重解释，应改名为 `BitCast`，其目标不应伪装成 integer。

146. **[P2] `Enum.Info` 的展示值受当前文化影响，缓存又会无界保存任意枚举数值**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/EnumExtensions.cs:5-18`。`ToLower()`/`ToUpper()` 使用当前文化，identifier 在土耳其语等文化下产生不同结果；cache 以 boxed `Enum` 值为 key，flags 任意组合和强转出的未知数值都会永久增长。应使用 invariant casing，并优先按 enum type/已声明 member 缓存元数据；动态组合值不应全局 intern。

147. **[P1] `NameValues<TSelf>` 的 CRTP 基类可被直接实例化并在首次修改时强转失败**

    位置：`src/FclEx.Core/FclEx/Utils/~Collections/NameValues.cs:8,29-32,71-76`。只要存在 `Foo : NameValues<Foo>`，调用方仍可合法创建 `new NameValues<Foo>(comparer)`；`Add` 随后执行 `(TSelf)this` 并抛 `InvalidCastException`。self-typed 基类应为 abstract 且构造器 protected，确保实际实例确实是 `TSelf`；或删除 CRTP，普通方法返回基类/void。

148. **[P0] `EncodingHelper.GetEncoding` 的 BOM 判断会漏掉绝大多数 UTF-16/UTF-32 文件**

    位置：`src/FclEx.Core/FclEx/Helpers/EncodingHelper.cs:22-45`。实现只读 3 字节，并要求 UTF-16 BE 的第三字节恰为 `0x00`、UTF-16 LE 的第三字节恰为 `0x41`，把正文首字节错误地当作 BOM；UTF-32 BOM 需要 4 字节也无法正确识别。应复用已经存在的 `TryDetectEncoding`，读取足够的最大 preamble 长度并按完整 BOM 匹配。

149. **[P0] `EncodingHelper.IsUtf8` 既拒绝位于文件末尾的合法多字节字符，也接受非法 continuation byte**

    位置：`src/FclEx.Core/FclEx/Helpers/EncodingHelper.cs:52-97`。读取 lead byte 后使用 `Position < Length - N`，刚好剩余 N 个 continuation bytes 时条件为 false，所以任何以非 ASCII 字符结尾的文件都可能被判非 UTF-8；continuation 只检查最高位为 1，`11xxxxxx` 也被接受，并未排除 overlong、surrogate 或超过 U+10FFFF。应使用严格的 `UTF8Encoding(false, true)` decoder（流式处理跨 buffer 序列），不要维护不完整的手写验证器。

150. **[P1] `EncodingHelper.GetEncoding(Stream)` 未声明却夺取并重置/关闭调用方 stream**

    位置：`src/FclEx.Core/FclEx/Helpers/EncodingHelper.cs:22-27,42-57`。方法要求 `Length`/`Seek`，把位置无条件重置到 0 而非原位置；`IsUtf8` 中 `using new BinaryReader(stream)` 又会在返回时关闭外部传入的 stream。应明确支持能力和所有权：通常保存并恢复原 position、使用 `leaveOpen: true`，对不可 seek stream 采用前缀缓冲或明确拒绝且文档说明。

## 建议处理顺序

1. 先处理可能违反内存、线程和安全基础契约的 112、114、129、135、140、141、148、149。
2. 再确定需要重塑或删除的公共设计：102–104、109、115–124、128、134、139、142、147；破坏性升级不应阻止合理设计。
3. 随后修复解析/校验、跨目标兼容和异常语义问题，并为每项增加最窄范围的回归测试。
4. 解决条目后继续在对应标题增加 `[已修复]`（或明确的保留决定），并在正文追加处理说明；历史条目不因已修复而删除。
