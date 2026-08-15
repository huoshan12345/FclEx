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

71. **[P1] `Deque<T>` 的空队列操作只靠 `Debug.Assert` 防护**

    位置：`src/FclEx.Core/System/Collections/Generic/Deque.cs:49-95`。Release 下 `Peek`/`Dequeue` 会访问空数组或已分配数组中的默认槽位，后者还能把 `_size` 减成负数并永久损坏队列状态。应像 BCL 集合一样在空队列上抛 `InvalidOperationException`，并提供 `TryPeekHead`/`TryDequeueHead` 等非抛出 API。

72. **[P1] `Heap<T>` 在 comparer 抛异常时会损坏计数和堆结构**

    位置：`src/FclEx.Core/System/Collections/Generic/Heap.cs:80-87,121-142,233-289`。`Push` 在比较前先增加 `_count`，`Pop` 在下沉比较前先减少计数并清空尾槽；`SiftUp`/`SiftDown` 又边比较边移动元素。自定义 comparer 一旦抛错，集合可能丢元素、出现未初始化项或不再满足堆序。应先计算移动路径再提交，或在异常时恢复原计数、元素和路径。

73. **[P1] `OrderedList` 的公开 bound API 不验证搜索区间**

    位置：`src/FclEx.Core/System/Collections/Generic/OrderedList.cs:339-389`。负 lower、超过 `Count` 的 upper、以及 lower 大于 upper 都会产生随机索引异常、读取容量内未使用槽位或返回无效边界。应统一验证 `0 <= lower <= upper <= Count`，并让两个方法对非法范围抛出一致、参数名正确的异常。

74. **[P1] `OrderedList.EqualRange` 永远漏掉最后一个匹配项**

    位置：`src/FclEx.Core/System/Collections/Generic/OrderedList.cs:527-537`。`end` 是最后一个匹配项的包含式索引，循环却使用 `i < end`；只有一个匹配项时结果为空，多个匹配项时少一个。应使用半开区间的 `LowerBound`/`UpperBound`，或将循环条件改为包含 end。

75. **[P1] `OrderedList.RemoveRange(min, max)` 在反向范围上返回负删除数**

    位置：`src/FclEx.Core/System/Collections/Generic/OrderedList.cs:539-550`。当 `min > max` 时 `end - start` 为负，私有删除方法静默 no-op，公共方法却把负数作为“删除数量”返回。应先验证 comparer 意义上的 `min <= max`，或者明确定义反向范围删除 0 项且返回 0。

76. **[P1] `BiDictionary<TKey,TValue>` 在两种类型相同时公共 API 变得不可调用**

    位置：`src/FclEx.Core/System/Collections/Generic/BiDictionary.cs:59-86`。当 `TKey` 与 `TValue` 都是 `string` 等同一类型时，两个 `Remove` 和两个 indexer 的参数签名相同，C# 调用方无法凭返回类型消除歧义。类型约束却允许这种实例化。建议提供 `Forward`/`Reverse` 视图，或使用 `GetByKey`、`GetByValue`、`RemoveKey`、`RemoveValue` 等具名方法。

77. **[P1] `StableSort(list, index, count)` 完全忽略 `index`**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/ListExtensions.cs:33-64`。比较读取 `list[a]`/`list[b]`，回写也使用 `list[i]`，因此请求排序中间区间时实际排序的是前缀。所有读取和写入都应加上 `index`，临时数据也只需复制目标区间；空列表仍应先验证 index/count 契约。

78. **[P1] `List.Items`/`SetCount` 以安全名称公开了破坏 `List<T>` 不变量的能力**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/ListExtensions.cs:96-119`。`Items` 返回整个私有容量数组，`SetCount` 直接写 `_size`/`_version`；调用方可以观察未使用槽位、制造未经初始化的“有效”元素，并依赖跨 runtime 不保证的私有布局。建议删除公共 API；不可替代的低层场景应使用目标框架公开的 `CollectionsMarshal`，并以 `Dangerous` 命名、限制目标框架和记录失效规则。

79. **[P2] `List<T>` 的两个 `+` 运算符具有相反的所有权语义**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/ListExtensions.cs:179-196`。`list + otherList` 创建新列表，而 `list + item` 原地修改左操作数并返回同一实例；从相同符号无法判断是否产生副作用。建议让 `+` 始终是纯连接操作，原地修改只通过 `Add`/`AddRange`/`+=` 表达，或直接删除扩展运算符。

80. **[P1] `BitsToInt` 对任何输入都返回 0**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.cs:135-144`。累加器从 0 开始却使用按位与 `&=`，所以任何 bit 都无法被设置。应使用 `|=`，同时明确首项代表最低位还是最高位，并拒绝或定义超过 32 位时的行为。

81. **[P1] `Enumerable.Split(parts)` 的延迟查询捕获了跨枚举共享的可变索引**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.cs:358-365`。`i` 在查询外定义，第二次枚举会从上一次结束值继续，并发枚举还会竞态；`parts <= 0` 也没有验证，错误延迟到枚举时才发生。应在 iterator 每次枚举时创建局部状态，验证正数，并把 round-robin 行为改成更准确的名称，因为它不是连续分块。

82. **[P1] `WhenAllOrError` 提前失败后不再观察剩余任务**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.Task.cs:296-306`。任一已完成任务 fault/cancel 时 await 立即退出，列表中其他任务继续运行；它们随后 fault 时没有任何观察者，可能触发未观察异常。若契约要求 fail-fast，应给剩余任务附加可靠的异常观察并定义取消策略；否则直接使用 `Task.WhenAll` 并保留聚合完成语义。

83. **[P2] Span split 在未请求移除空项时仍会丢失空输入和尾部空项**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/ReadOnlySpanExtensions.cs:186-247`。`_remaining.IsEmpty` 会直接结束，因此空 span 不产生一个空项，`"a,"` 也不会产生尾部空项，和 `StringSplitOptions.None` 的预期不一致。枚举器需要单独记录“尚未开始/最后一个分隔符后仍有一个结果”的状态，而不能用 empty span 同时表示数据和完成。

84. **[P2] `IntPtr.AbsDiff` 的返回类型和算术都无法表示合法地址差**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/IntPtrExtensions.cs:16-25`。有符号减法可能溢出，`-long.MinValue` 仍是负数；两个 64 位地址之间的距离还可能大于 `long.MaxValue`。应以无符号算术计算并返回 `nuint`/`ulong`，或对不可表示的结果显式抛出 overflow。

85. **[P2] `IsPossibleXml` 没有可供调用方依赖的有效契约**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/StringExtensions.cs:51-83`。三个 regex 不检查标签匹配、嵌套、属性或实体，可接受明显损坏的 XML；同时会拒绝合法的空元素、前导空白等输入。与已移除的 `IsPossibleHtml` 相同，这种“可能是”判断没有稳定用途；应删除并让调用方直接解析，若只需便宜的外形检查则必须以名称和文档明确它不验证 XML。

86. **[P0] `MarshalTo<T>` 会把任意字节解释成原生地址并解引用**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/BytesExtensions.cs:34-84`、`ReadOnlySpanExtensions.cs:29-62`。API 对 `T` 无约束，`PtrToStructure<T>` 遇到 string、数组或引用字段时会把输入字节当地址并解引用，可能产生访问冲突或读取非预期内存；span 单值版本在输入长于结构时还因复制整个 span 到较小缓冲区而抛错。应把原始二进制读取限制为 `unmanaged` 并用 `MemoryMarshal.Read`；真正的 interop marshaling 应使用明确命名、受控布局和可信输入，并只复制精确结构长度。

87. **[P1] `DirectoryInfo.IsSubOf` 的路径判定在不同平台和边界输入上不可靠**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/DirectoryInfoExtensions.cs:45-50`。实现无条件 `OrdinalIgnoreCase`，在大小写敏感文件系统上错误；父路径已有尾分隔符时会拼出双分隔符，无界静态 cache 还会永久保存任意路径。建议用规范化后的 `Path.GetRelativePath` 和平台正确的比较规则，明确“自身是否算子目录”及符号链接策略，并删除这个无收益缓存。

88. **[P1] `DirectoryInfo.Sub`/`File` 允许 rooted 参数逃出父目录**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/DirectoryInfoExtensions.cs:33-43`。`Path.Combine(parent, rootedName)` 会忽略 parent，安全感很强的子项 API 因而可以返回任意绝对路径。若契约真的是直接子项，应拒绝 rooted path、目录分隔符和 `..`；若只是路径组合，应改用不暗示 containment 的名称。

89. **[P1] `CreateNew` 以普通创建名称执行递归删除**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/DirectoryInfoExtensions.cs:25-31`。调用者容易把它理解成“确保新目录存在”，实际却会不可恢复地删除现有目录及全部内容。应删除该 API或改为明确的 `Recreate`/`DeleteAndCreate`，文档突出破坏性、验证精确目标，并优先提供非破坏性的 `CreateIfMissing`。

90. **[P1] `FileConflictOptions` 把互斥策略建模为可任意组合的 flags**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/FileConflictOptions.cs:6-41`、`FileInfoExtensions.cs:7,110-129`。`ThrowOnConflict | Overwrite` 等组合通过位掩码后没有匹配的 switch case，方法会静默返回且不执行操作。应把 resolution strategy 建模为普通 enum，把 `IgnoreConflictIfDuplicate` 作为独立选项；过渡期至少验证恰好选择一个策略并拒绝未知位。

91. **[P1] 文件冲突处理存在检查后覆盖的 TOCTOU 竞态**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/FileInfoExtensions.cs:28-58,82-129`。`dest.Exists == false` 后调用的基础复制使用 `File.Create`，会覆盖在检查与创建之间由其他线程/进程新建的文件；`AutoRename` 的递归检查同样不能保证新名字仍空闲。应在非覆盖策略下用 `FileMode.CreateNew` 原子认领目标，AutoRename 遇到 `IOException` 再选择下一名称，只有明确 `Overwrite` 才使用 Create。

92. **[P2] `ProcessInvoker` 的返回模型丢失了进程结果的关键结构**

    位置：`src/FclEx.Core/FclEx/Utils/~Diagnostics/ProcessInvoker.cs:35-80`、`ProcessInvocation.cs:3-10`。stdout 与 stderr 被并发写入同一队列，跨流顺序不确定且调用方无法分别处理；忽略非零退出码时又只返回字符串，连 exit code 也丢失。建议始终返回 `ProcessResult(ExitCode, StandardOutput, StandardError)`，再由显式策略决定非零退出是否抛异常。

93. **[P1] PowerShell/Pwsh/WSL invoker 的命令参数拼接不能正确承载引号**

    位置：`src/FclEx.Core/FclEx/Utils/~Diagnostics/PowerShellInvoker.cs:18`、`PwshInvoker.cs:17-21`、`WslInvoker.cs:18`。把完整命令插入 `-command "..."` 或 `-c "..."` 而不转义内部引号、反斜杠和换行，合法命令会被外层命令行解析器截断或改写。应使用 `ProcessStartInfo.ArgumentList` 把命令作为单独参数传递，旧框架使用经过平台验证的 quoting helper，复杂脚本则通过 stdin/临时脚本文件传入。

94. **[P1] 软删除实体用两个独立可写属性表达同一个状态**

    位置：`src/FclEx.Core/FclEx/Domain/~Entities/SoftDeletableEntity.cs:9-15`、`IHasDeletedAt.cs:6-11`、`ISoftDeletable.cs:6-11`。`IsDeleted == false` 同时带任意 `DeletedAt`，或 `IsDeleted == true` 配 `DateTimeOffset.MinValue` 都是合法对象状态。建议以 nullable `DeletedAt` 作为单一事实来源并派生 `IsDeleted`，或者只暴露 `Delete(at)`/`Restore()` 转换以维护不变量。

95. **[P2] `EntityChanges<T>` 是带可变 List 的 record，既不是值对象也不是稳定快照**

    位置：`src/FclEx.Core/FclEx/Domain/~Entities/EntityChanges.cs:20-43`。record equality 对三个 List 使用引用相等，两个内容相同的 changes 不相等；对象创建后列表仍可变，`init` 属性也能被显式赋 null，仅靠 nullable warning 阻止。应选择语义：可变工作集用普通 sealed class 和只读暴露，传输/值对象则复制到 immutable/read-only collection 并定义内容相等。

96. **[P1] `GetRequiredAsync` 的 default 参数使“Required”名存实亡**

    位置：`src/FclEx.Core/FclEx/Domain/~Services/IKeyValueService.cs:28-32`。缺少 key 时 `GetAsync` 会返回调用方提供的非 null default，方法因此成功而不是抛出；只有 default 本身为 null 才符合 Required 语义。应删除 defaultValue 参数并直接检测缺失，另设 `GetOrDefaultAsync`/`GetOrElseAsync` 表达 fallback。

97. **[P1] `NString.Equals(object)` 违反相等关系的对称性**

    位置：`src/FclEx.Core/FclEx/Utils/NString.cs:12-32`。`new NString("x").Equals("x")` 为 true，而 `"x".Equals(new NString("x"))` 为 false；跨类型 object equality 不满足 .NET 相等契约，会给集合和通用算法造成异常结果。`Equals(object)` 应只接受 `NString`，字符串便利性保留在显式/隐式转换或具名比较方法中。

98. **[P1] `TreeNode.Children` 的公开可变列表绕过了 Parent 和树拓扑约束**

    位置：`src/FclEx.Core/FclEx/Utils/TreeNode.cs:15-31,33-59`。调用方可直接加入已有节点、删除节点、创建环或共享子树，却不会更新 `Parent`；`DeepEquals` 随后假定严格树结构，并在重复节点上 `map.Add` 抛异常。应封装 children，提供维护双向关系的 Add/Remove/Move API，拒绝环和多父节点，并让遍历对非法图有明确行为。

99. **[P0] `ObjectAccessor` 返回的托管对象地址在方法返回时就可能失效**

    位置：`src/FclEx.Core/FclEx/Utils/~Accessors/ObjectAccessor.cs:19-45,60-123,126-193`。这些方法把对象、引用槽位和字段的 managed byref 转成 `IntPtr[]` 后返回，期间没有也无法为任意对象建立可持续的 pin；下一次 GC 即可移动对象。`GetAddress(ref T)` 对引用类型返回的还是引用变量槽位地址，并非对象地址。建议删除面向任意托管对象的地址 API；仅对 `unmanaged` 值提供 scoped 操作，或让低层逻辑在受控 callback 内消费 byref，绝不把裸地址跨出生命周期。

100. **[P1] `OrderedIndex.UpdateScore` 先删除再添加，失败时会永久丢失原项**

    位置：`src/FclEx.Core/System/Collections/Generic/OrderedIndex.cs:245-253`。旧节点先从 skip list 和 map 移除，随后 `Add(newScore, value)` 若因 comparer 或其他异常失败，没有任何回滚，公开的“更新”操作变成删除。应在提交前完成所有可能失败的比较/分配，或保存旧 score/sequence 并在异常路径恢复原节点，保证操作成功或集合保持原状。

## 建议处理顺序

1. 先处理可能产生非法内存访问或悬空地址的 51、86、99，以及会泄漏许可或损坏集合状态的 53、70–75、77、80、100。
2. 再决定整体用途或公共模型需要重建的 54–57、59–60、63、76、78–79、85、90、92、94–98；破坏性升级不应阻止合理设计。
3. 随后处理其余边界、异常和多目标兼容问题，并为每项补充最窄范围的回归测试。
4. 已修复的 1–50 继续作为历史记录；后续解决 51–100 时在对应条目下增加 `修复：` 说明和 `[已修复]` 标记。
