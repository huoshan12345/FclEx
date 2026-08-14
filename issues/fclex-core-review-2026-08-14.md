# FclEx.Core 源码审查报告

## 范围与结论

- 审查范围：`src/FclEx.Core` 下的 C# 源码，明确排除任何 `Combinatorics` 目录。
- 规模：424 个 C# 文件，约 34,179 行。
- 方法：结合编译、现有测试、公共 API/命名审查和重点静态分析；优先检查并发、异步、资源释放、集合不变量、I/O、安全、序列化和多目标框架兼容性。
- 停止条件：候选问题已经超过 50 个。按要求停止继续扩展，本报告只保留证据最强、优先级最高的 50 条；未把剩余候选写入报告。
- 基线验证：`dotnet build src/FclEx.Core/FclEx.Core.csproj -c Release --no-restore` 在 `net472`、`netstandard2.0`、`net8.0`、`net9.0`、`net10.0` 上均成功，0 warning、0 error。
- 测试验证：`dotnet test test/FclEx.Core.Tests/FclEx.Core.Tests.csproj -c Release --no-restore --no-build /nr:false` 全部通过；`net472` 通过 10,138 条、跳过 3 条，其他三个测试目标各通过 10,186 条、跳过 3 条。

严重性约定：P0 为安全或数据破坏风险；P1 为较高概率的错误、死锁、竞态或公共契约破坏；P2 为边界错误、资源/兼容性风险或明显的行为歧义；P3 为命名和可维护性问题。

## 问题清单

1. **[P0][已修复] ZIP 解压允许路径穿越（Zip Slip）**  
   位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/~Compression/ZipArchiveEntryExtensions.cs:26-33`。`ExtractToDirAsync` 直接将不可信的 `entry.FullName` 与目标目录组合；诸如 `../../outside.txt` 或绝对路径可逃出 `dir`。建议在 `Path.GetFullPath` 后验证结果仍位于规范化的目标目录内，同时拒绝 rooted path、父目录跳转及平台相关的替代分隔符，并增加恶意 ZIP 测试。
   修复：规范化目标目录和输出路径，并在写入前验证输出路径仍位于目标目录内；已增加路径穿越回归测试。

2. **[P1][已修复] `LruCache.TryGetValue` 在读锁下修改链表**  
   位置：`src/FclEx.Core/FclEx/Utils/~Caching/LruCache.cs:116-126`。方法取得 `LockRead` 后调用 `UpdateInternal`，后者会 `Remove`/`AddFirst` 修改 `_list`。多个读者可同时写同一链表，造成结构损坏或随机异常。应使用可升级读锁并在写锁内更新 LRU 顺序，或将查找与触碰顺序合并到写锁中。
   修复：查找改用可升级读锁，并在写锁内更新 LRU 链表；已增加并发读取回归测试。

3. **[P1][已修复] `LfuCache.TryGetValue` 同样在读锁下修改频率和链表**  
   位置：`src/FclEx.Core/FclEx/Utils/~Caching/LfuCache.cs:41-51`。`UpdateInternal` 会改写节点计数、交换节点值并更新字典，但调用方只持有读锁。应提升为写锁，并补充高并发 `TryGetValue` 后校验字典、链表和频率顺序一致性的压力测试。
   修复：查找改用可升级读锁，并在写锁内更新频率、链表和字典；已增加并发读取回归测试。

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

8. **[P1][待讨论] `ObjectMemoryEqualityComparer` 在未固定对象时创建原始内存 Span**

   位置：`src/FclEx.Core/System/Collections/Generic/~EqualityComparers/ObjectMemoryEqualityComparer.cs:17-60`。从托管引用计算地址后直接构造 `Span<byte>`，期间 GC 可以移动对象，使地址失效并读取任意旧内存；引用类型布局算法也依赖 CLR 内部实现。即使保留该低层 API，也应限制为明确可安全处理的 unmanaged 值类型；否则必须在读取期间 pin 对象，并在文档中标为运行时相关、不适合作为通用 comparer。
   状态：`GCHandle.Alloc(instance, GCHandleType.Pinned)` 不能统一固定任意对象，而且临时固定根对象仍不能使原始内存比较成为稳定的相等/哈希契约；等待设计讨论后再决定是否移除或收窄 API。

9. **[P1][已修复] `FileHelper.AreFilesEqual` 会比较未初始化的栈内存**

   位置：`src/FclEx.Core/FclEx/Helpers/FileHelper.cs:55-83`。`NET5_0_OR_GREATER` 分支分配两个 4096 字节的 `stackalloc` 缓冲区，却对整个缓冲区做 `SequenceEqual`，而短文件或最后一块之后的区域未必初始化；相同文件可能被误判为不同。还应处理 `Read` 短读，而不是预先按请求长度递减。建议只比较 `buf[..i]`/`buf[..j]`，按实际读取数推进，并循环填满或正确处理短读。
   修复：改为循环读取至填满当前逻辑块，只比较有效范围，并在完整读取后推进长度；已覆盖空文件、短文件、块边界及跨块文件。

10. **[P1][已修复] 取消 `ProcessInvoker` 只停止等待，不终止子进程**

    位置：`src/FclEx.Core/FclEx/Utils/~Diagnostics/ProcessInvoker.cs:34-46`。`WaitForExitAsync` 因 token 取消后，`Process` 被 dispose，但操作系统进程通常继续运行；对于 PowerShell/WSL 命令会遗留后台进程和副作用。建议明确取消语义，并在取消时按需 `Kill(entireProcessTree: true)`（旧框架使用兼容实现），随后等待退出并保留原取消异常。
    修复：取消时现代 .NET 终止整个进程树，旧目标终止所启动的进程，等待退出后重新抛出取消异常；已增加真实进程终止测试。

11. **[P1] `ObjectHelper.TrySet` 的委托缓存键缺少泛型签名**  
    位置：`src/FclEx.Core/FclEx/Helpers/ObjectHelper.cs:62-86`。缓存只以 `MemberInfo` 为键；若同一个基类成员先通过 `Derived` selector 使用，再通过 `Base` selector 使用，缓存中的 `Func<Derived, TMember>` 会被强制转换为 `Func<Base, TMember>` 并抛 `InvalidCastException`，setter 同理。缓存键应包含成员、目标类型和成员类型，或按封闭泛型类型拆分缓存。

12. **[P1] Marshal 辅助方法泄漏非 blittable 结构的嵌套非托管内存**  
    位置：`src/FclEx.Core/FclEx/Helpers/ObjectHelper.cs:43-53`、`src/FclEx.Core/FclEx/Extensions/~System/BytesExtensions.cs:88-108`。`Marshal.StructureToPtr(..., false)` 可能为字符串、数组等字段分配嵌套内存，但代码只 `FreeHGlobal` 外层块，从不 `Marshal.DestroyStructure`；数组版本还反复复用同一块内存。应限定 `unmanaged`/blittable 类型，或在每次成功 marshal 后可靠调用 `DestroyStructure<T>`，并在异常路径清理。

13. **[P1] `OperationResult<T>` 的 nullability 属性向编译器提供了错误保证**  
    位置：`src/FclEx.Core/FclEx/Utils/~Operation/OperationResult.cs:18-27`。`IsSuccess == true` 被标注为 `Value` 非 null，但 `FromSuccess(default)`、隐式转换以及成功构造函数都允许 null；消费方会因此消除必要的 null 检查并可能触发 NRE。应移除关于 `Value` 的 `MemberNotNullWhen`，或从类型设计上禁止成功的 null 值。

14. **[P1] Consumer 的 `Add` 与 `CompleteAdding` 存在竞态**  
    位置：`src/FclEx.Core/FclEx/Utils/~Consumers/ConsumerBase.cs:98-132`。`Add` 无锁检查 `_isAddingCompleted`，随后另一个线程可完成添加，再由当前线程把项目放入队列；消费循环可能已经按“完成且空”退出，留下永不处理的项目。应使用 `BlockingCollection.CompleteAdding`/`Add` 的原生原子语义，或在同一锁内检查状态并入队。

15. **[P1] Consumer 的停止和释放未与工作任务完成同步**  
    位置：`src/FclEx.Core/FclEx/Utils/~Consumers/ConsumerBase.cs:63-76,104-160`。`Dispose` 不持有生命周期锁，取消后立即 drain 并 dispose `_items`/`_cts`，而 `ProcessAsync` 可能仍在取项或执行 handler；`Stop` 也在持锁时调用外部 `CancellationHandler`，重入时可能死锁。建议维护单一运行任务，先取消、在锁外等待其结束，再释放资源；所有外部回调都应在锁外调用。

16. **[P1] `RepeatUntil` 创建了超时 token，却没有传给实际操作**  
    位置：`src/FclEx.Core/FclEx/Actions/ActionExtensions.cs:333-352`。循环检查 `cts.IsCancellationRequested`，但执行 action 和 delay 时都传原 token `t`，因此一次长时间 action 或 delay 可以无限超过总 timeout。应把 `cts.Token` 传给两者，并区分调用方取消与内部超时生成的结果。

17. **[P1] `WhenAnySuccess` 和 `WhenAllOrError` 对空序列永久不完成**  
    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.Task.cs:114-204`。三个实现都依赖 continuation 增加完成数；当任务数为 0 时没有 continuation，返回的 TCS 永远 pending。应在 materialize 后立即处理空集合：按 API 契约返回默认值、成功完成或抛出明确异常。

18. **[P1] 最后一个任务成功时，`WhenAnySuccess` 会在 continuation 中再次 `SetException`**  
    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.Task.cs:127-174`。成功分支先 `TrySetResult`，随后完成计数达到总数又调用 `SetException`；若成功发生在最后一个任务，第二次完成 TCS 会抛异常并成为未观察的 continuation 异常。所有终结路径应使用 `TrySet*`，并仅在尚无成功结果且所有任务结束时设置失败。

19. **[P1] `WhenAnySuccess` 的 predicate/default factory 抛异常时，返回任务可能永久 pending**  
    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.Task.cs:128-153`。用户委托在 `ContinueWith` 内无保护执行；异常只 fault continuation，不会完成对外返回的 TCS，而且完成计数也可能少一次。应捕获用户委托异常并 `TrySetException`，或使用一个可读性更高的 async 协调实现。

20. **[P1] `ActionHelper` 会捕获取消异常并继续重试**  
    位置：`src/FclEx.Core/FclEx/Helpers/ActionHelper.cs:56-107`。异步重试使用 `catch (Exception)`，把 `OperationCanceledException` 当普通失败处理，且 API 没有 cancellation token；取消后的操作可能继续执行多次。建议异步重载接收 token，单独重新抛出与该 token 相关的取消异常，并将 delay 也绑定到 token。

21. **[P1] `Disposable`/`AsyncDisposable` 每次释放都会重复执行回调**  
    位置：`src/FclEx.Core/FclEx/Utils/~Disposables/Disposable.cs:12-16`、`AsyncDisposable.cs:12-16`。这些公共资源包装器没有 disposed 状态，重复 `Dispose`/`DisposeAsync` 会重复释放底层资源；这违反常见 IDisposable 幂等约定，也会使 `GCHandle.Free` 等动作抛错。建议用 `Interlocked.Exchange` 原子取走回调，只允许执行一次。

22. **[P1] `DisposableValue` 与 `AsyncDisposableValue` 的释放检查不是原子的**  
    位置：`src/FclEx.Core/FclEx/Utils/~Disposables/DisposableValue.cs:13-30`、`AsyncDisposableValue.cs:21-40`。两个线程都可能看到 `_disposed == false` 并重复释放；回调重入也会重复进入，因为状态在回调完成后才设置。应在调用用户代码前原子完成状态转换，并定义回调失败后对象是否仍视为 disposed。

23. **[P1] `AsyncTimer` 的后台任务不可观察、不可等待也不可释放**  
    位置：`src/FclEx.Core/FclEx/Utils/~Threading/AsyncTimer.cs:3-44`。构造函数立即启动 `_task`，但字段私有且类型不实现 `IDisposable/IAsyncDisposable`；due/period delay 的取消异常位于 try/catch 外，`onException` 自身抛错也会 fault 一个无人观察的任务。建议提供显式 `StartAsync`/`Completion` 和异步停止释放协议，避免构造函数 fire-and-forget。

24. **[P1] LRU/LFU 在写锁内调用公开的 eviction 事件**  
    位置：`LruCache.cs:177-192`、`LfuCache.cs:191-204`。`OnItemCleared` 在内部状态更新中且持有 `ReaderWriterLockSlim` 写锁时执行；handler 重入缓存会触发锁递归异常，handler 抛错还会造成“旧项已删、新项未加”的半完成状态。应先完成内部事务，释放锁后再触发回调，并决定回调异常是否传播。

25. **[P1] LRU/LFU 在写锁内执行调用方的 value factory**  
    位置：`LruCache.cs:35-55`、`LfuCache.cs:67-87`。`activator(key)` 作为 `AddInternal` 参数在写锁作用域内求值；慢 factory 会阻塞全部访问，重入缓存会因 `NoRecursion` 失败。建议在锁外创建值，再在写锁内二次检查；若要求单次创建，应使用 per-key lazy，而不是在全局写锁中运行用户代码。

26. **[P1] `SafeCounter.IncrementToThreshold` 不是一个原子的阈值操作**  
    位置：`src/FclEx.Core/FclEx/Utils/~Threading/SafeCounterExtensions.cs:10-29`。多个线程可同时得到 `>= threshold`，并发执行 action 后相互 reset；期间的新增量还可能被 reset 丢失。名称中的 `Safe` 容易让调用者误认为整个组合操作线程安全。应使用 CAS 状态机/交换固定批次，或明确改名并记录仅单次增减原子。

27. **[P1] `PrecisionDateTimeOffsetComparer` 不能满足 `IEqualityComparer` 契约**  
    位置：`src/FclEx.Core/System/DateTimeOffsetComparers.cs:19-42`。“差值小于容差”不具传递性，例如 A≈B、B≈C 但 A≉C；非零 precision 的 `GetHashCode` 还直接抛错，因此不能用于 `Dictionary`/`HashSet`。建议不要实现 `IEqualityComparer`，改为显式 `IsWithinTolerance`；若确需 comparer，应采用离散 bucket 规则并保证 Equals/GetHashCode 一致，同时拒绝负 precision。

28. **[P1] `ProcessInvoker` 在旧目标框架上可能丢失末尾输出**  
    位置：`src/FclEx.Core/FclEx/Utils/~Diagnostics/ProcessInvoker.cs:35-45`、`FclEx/Extensions/~System/~Diagnostics/ProcessExtensions.cs:5-26`。`net472/netstandard2.0` 的兼容 `WaitForExitAsync` 只等待 Exited 事件，没有等待异步 stdout/stderr reader 的 EOF；进程退出后立即读取队列可能漏掉尾部行。应等待两个流的完成信号，或在进程退出后执行兼容的 drain/`WaitForExit` 收尾。

29. **[P1] `ZipArchive.BuildTree` 用局部目录名作为父节点键，无法表示一般 ZIP 树**  
    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/~Compression/ZipArchiveEntryExtensions.cs:52-80`、`System/IO/Compression/ZipArchiveEntryInfo.cs:18-20`。父键只是上一段目录名，每层还重建字典；不同分支出现同名目录会冲突，而且 ZIP 未显式包含目录 entry 时会找不到父节点。应以规范化完整路径为键，并在遇到文件时按需创建缺失祖先目录。

30. **[P2] `CopyToAsync(..., AutoRename)` 返回错误的目标 `FileInfo`**  
    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/FileInfoExtensions.cs:118-130`。递归复制到 `newDest` 后忽略其返回值，最终仍返回原本冲突的 `dest`，调用者无法知道真实文件名。应直接 `return await file.CopyToAsync(newDest, ...)`，并测试连续多个冲突后的最终路径。

31. **[P2] `ActionHelper.TryAsync` 的 `delaySeconds` 实际按毫秒解释**  
    位置：`src/FclEx.Core/FclEx/Helpers/ActionHelper.cs:56-74`。非泛型异步重载调用 `Task.Delay(delaySeconds)`，而同步重载和泛型异步重载都按秒延迟。应统一使用 `TimeSpan`，或调用 `Task.Delay(TimeSpan.FromSeconds(delaySeconds))`；公共参数最好避免用整数表达时间单位。

32. **[P2] `ActionHelper` 重新抛出时丢失原始调用栈，且最后一次失败后仍延迟**  
    位置：`src/FclEx.Core/FclEx/Helpers/ActionHelper.cs:13-31,38-52,61-83,91-106`。`throw lastEx` 把栈顶重置到 helper；每个 catch（包括最后一次）都 sleep/delay，导致已确定失败的返回被无意义推迟。建议只在还有下一次尝试时延迟，并通过 `ExceptionDispatchInfo.Capture(lastEx).Throw()` 保留原栈。

33. **[P2] `readBufferTimeout` 同时限制写入操作**  
    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/StreamExtensions.cs:46-64`。同一个带超时的 CTS 先用于 read，随后也用于 `dest.WriteAsync`；慢目标流会被名为“读取超时”的参数取消，而且写入消耗的是 read 剩余时间。应让 read timeout 只包围 read，写操作仅使用调用方 token，或把参数改成明确的 per-iteration timeout。

34. **[P2] `LastTickOfMonth` 暴露的时间参数完全无效**  
    位置：`src/FclEx.Core/FclEx/Extensions/~System/DateTimeExtensions.cs:92-96`。`hour/minute/second` 从未传给 `EndOfMonth`，任何参数值都返回当月最后一刻。应删除这些参数，或按明确语义应用它们；当前 API 会让调用者误以为可控制基准时间。

35. **[P2] 多个 DateTime 日历辅助方法丢失 `Kind`**  
    位置：`src/FclEx.Core/FclEx/Extensions/~System/DateTimeExtensions.cs:46-90`。`Today`、`ThisYear`、`ThisMonth`、`StartOfMonth`、`EndOfMonth` 使用不带 kind 的构造函数，输入即使是 Local/Utc，输出也变为 Unspecified。应使用包含 `dt.Kind` 的构造函数，或明确记录并命名为创建 Unspecified 时间。

36. **[P2] `DateTime.ToCnTime` 返回带 `Utc` Kind 的中国本地钟面时间**  
    位置：`src/FclEx.Core/FclEx/Extensions/~System/DateTimeExtensions.cs:120-143`。`ToUtc().AddHours(8)` 保留 `DateTimeKind.Utc`，但数值已经是 UTC+8；后续再转 UTC 会重复偏移。应优先返回 `DateTimeOffset` 并用 `TimeZoneInfo.ConvertTime`，或至少把本地钟面值标记为 Unspecified。

37. **[P2] `DateTimeHelper.FromUnixTime*` 返回 Unspecified Kind**  
    位置：`src/FclEx.Core/FclEx/Helpers/DateTimeHelper.cs:3-9`。使用 `DateTimeOffset.DateTime` 会丢失 offset 并生成 `DateTimeKind.Unspecified`，与 Unix 时间天然为 UTC 的语义不符。应返回 `.UtcDateTime`，或直接以 `DateTimeOffset` 作为公共返回类型。

38. **[P2] `Partition` 的 `Both` 选项实现成了 `None`**  
    位置：`src/FclEx.Core/FclEx/Extensions/~System/StringExtensions.Split.cs:52-82`。文档称 separator 同时包含在左右两部分，但实现返回 `source[..index]` 和 `source[sepEndIndex..]`，两边都排除了 separator。应返回 `(source[..sepEndIndex], source[index..])` 并覆盖左右搜索和多字符 separator。

39. **[P2] `HexToBytes` 拒绝小写 `b` 到 `f`**  
    位置：`src/FclEx.Core/FclEx/Extensions/~System/StringExtensions.cs:126-138`。匹配范围写成 `>= 'a' and <= 'a'`，只有小写 `a` 可通过。应改为 `<= 'f'`，增加 `abcdef`、混合大小写和非法字符测试。

40. **[P2] `IsPossibleHtml` 对任何非空字符串都返回 true**  
    位置：`src/FclEx.Core/FclEx/Extensions/~System/StringExtensions.cs:86-93`。`"not html"` 也被认定为可能的 HTML，API 名称传达了并不存在的判定能力。若只想检查非空应删除/改名；若保留 HTML 探测，应定义最小判据和误判边界。

41. **[P2] 按字符拆行会把 CRLF 当作两个换行符**  
    位置：`src/FclEx.Core/FclEx/Extensions/~System/StringExtensions.Split.cs:29-42`、`FclEx/Helpers/ResourceHelper.cs:3,28-36`。`Split(['\r','\n'], StringSplitOptions.None)` 会在每个 `\r\n` 中间制造空行；默认 RemoveEmpty 又会误删真实空白行。应按 `\r\n|\r|\n` 作为完整分隔序列处理，并分别测试保留/删除空行。

42. **[P2] 子 `ArraySegment` 可以越过父 segment 的边界**  
    位置：`src/FclEx.Core/FclEx/Extensions/~System/ArraySegmentExtensions.cs:24-29`。构造时只依赖底层数组的范围检查，例如父 segment 只有 2 项，但请求 count 10 只要底层数组够大就会成功。应验证 `offset >= 0`、`count >= 0` 且 `offset + count <= segment.Count`。

43. **[P2] 无限并发分支完全忽略 cancellation token**  
    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.Task.cs:46-84`。当 `concurrency is null` 时直接枚举并启动所有 operation，不检查已经取消的 token；同一 API 在分批分支才检查取消。至少应在开始枚举前 `ThrowIfCancellationRequested`，并考虑提供接收 token 的 operation 委托，使取消语义一致。

44. **[P2] Base32 解码静默接受无效的短输入**  
    位置：`src/FclEx.Core/FclEx/Extensions/~System/BytesExtensions.Base32.cs:5-44`。例如单字符 `"A"` 计算出的 `byteCount` 为 0，最终返回空数组而非报告无效编码；padding 长度和被丢弃的尾位也未校验。应验证 RFC 4648 合法长度、padding 位置和尾部零位，或明确提供 tolerant 模式。

45. **[P2] 静态 JSON 成员的值在元数据创建时被永久捕获**  
    位置：`src/FclEx.Core/System/Text/Json/JsonHelper.cs:137-157`。`var value = member.GetValue(null)` 只执行一次，`propertyInfo.Get = _ => value` 使以后修改的静态属性/字段仍序列化旧值；还用首次运行时类型代替声明类型创建 contract。getter 应每次读取 `member.GetValue(null)`，类型应使用 `member.DataMemberType`。

46. **[P2] `ReadAsStringJsonConverter` 会改变数字文本并损失精度**  
    位置：`src/FclEx.Core/System/Text/Json/Serialization/ReadAsStringJsonConverter.cs:20-42`。非 Int64 数字先转 `double` 再格式化，诸如高精度 decimal、超大整数或特定指数表示无法保留原始值。既然目标是字符串表示，应从 reader 的原始 UTF-8 token 获取文本，或通过 `JsonDocument.ParseValue(...).RootElement.GetRawText()` 保真。

47. **[P2] 多值 `NameValues.Set` 只保留最后一个值**  
    位置：`src/FclEx.Core/FclEx/Utils/~Collections/NameValuesExtensions.cs:64-76`。每个 value 都调用 `self.Set(key, value)`，后一次会删除前一次，所以 `IEnumerable<string>` 重载与“多值”类型目的相违。应对每个 key 先 Remove 一次，再逐个 Add；空 values 的行为也需明确定义。

48. **[P2] `FileExtensionEqualityComparer` 把无扩展名的整个文件名当作扩展名**  
    位置：`src/FclEx.Core/System/Collections/Generic/~EqualityComparers/FileExtensionEqualityComparer.cs:7-21`。`SkipUntil(".", untilLast: true)` 找不到点时返回原字符串，因此 `foo` 与 `bar` 被认为扩展名不同；`.gitignore` 等边界也与 `Path.GetExtension` 语义不一致。应基于 `Path.GetExtension`，并明确是否接受完整路径、尾点和 dotfile。

49. **[P2] `SizeCalculator` 把无字段值类型的大小报告为 0**  
    位置：`src/FclEx.Core/FclEx/Utils/~Runtime/SizeCalculator.cs:19-25`。CLR 中空 struct 仍有非零实例大小，返回 0 也会让“引用类型最后一个字段为空 struct”的计算继续低估。值类型应统一通过可靠的 `Unsafe.SizeOf`/生成泛型调用计算，并用空 struct、显式布局、嵌套 struct 做验证。

50. **[P3] 多个公共名称不准确或不符合自然英语，应在发布前统一**  
    位置示例：`FclEx/Helpers/DebuggerHepler.cs:3`（拼写错误，应为 `DebuggerHelper`）；`System/ComponentModel/DataAnnotations/UriAttribute.cs:28`（URI 术语是 scheme，应为 `AllowedSchemes`）；`TaskHelper.cs:70`（`DelayMilli` 应为 `DelayMilliseconds`）；`DateTimeExtensions.cs:136`/`DateTimeOffsetExtensions.cs:5`（`Cn` 含义含混，建议 `ChinaStandardTime`）；`LruCache.cs:18`/`LfuCache.cs:19`（事件只在容量驱逐时触发，`OnItemCleared` 应为 `ItemEvicted`）；`IPEndPointHelper.cs:9`（端口在 socket 释放后不再保留，`NextLocalEndpoint` 应明确为 candidate）。这些都是公共 NuGet API，建议采用新增正确名称、旧名称 `[Obsolete]` 转发、到下一个 major 再删除的兼容迁移方式。

## 建议处理顺序

1. 先修复 1-15，尤其是 ZIP 路径穿越、缓存锁、双向字典不变量、XML comparer、原始内存读取和 Consumer 生命周期。
2. 再处理异步协调与资源释放问题 16-29，并为每个竞态增加可重复的并发测试。
3. 处理 30-49 的边界和契约问题；这些修复中有若干会改变当前错误行为，应在 release notes 中说明。
4. 对第 50 条单独做一次公共 API 命名决策。避免直接删除旧成员，先提供兼容转发层。
