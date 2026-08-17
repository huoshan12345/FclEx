# FclEx.Core 源码审查报告（2026-08-16）

## 范围与结论

本轮在已阅读 `issues` 下 2026-08-14 与 2026-08-15 两份记录、并避开其中已登记事项后，继续审查 `src/FclEx.Core`，不包括 `Combinatorics` 目录。以下按设计与资源/并发契约、公共 API 与命名、最后是实现细节的顺序排列。

本轮发现 51 项，因此按约定在 251 号停止；没有为凑数重复前两轮的 1–200 项。

## 问题清单（201–222：设计、生命周期与并发契约）

201. **[P3][已修复] README 仍把已移除的 object-memory comparer 当作现有能力。**
   - 位置：`src/FclEx.Core/README.md` 的 Comparers 段。
   - 说明：代码已不再提供该种比较器，README 却继续宣传它，消费者会据此寻找不存在或不应使用的 API。
   - 建议：删去该条，改为实际仍受支持的比较器及其适用边界。

202. **[P1][已修复] `JsonHelper.ClearCache` 与并发 `GetOptions` 的竞态可能重新缓存旧 resolver。**
   - 位置：`System/Text/Json/JsonHelper.cs` 的 `GetOptions`、resolver 访问、静态 options 属性与 `ClearCache`。
   - 修复：上述路径现通过同一把锁同步；`ClearCache` 返回后，缓存中不会再保留由旧 resolver 创建的 options。`DefaultEx` 与 `WebEx` 也会随缓存重建。
   - 边界：调用方在清理前取得，或通过 `CreateOptions` 自行创建的 options 实例不会被修改或强制释放；这一点已在 `ClearCache` 文档中说明。

203. **[P2][已修复] `ITypeSerializer<TTarget>` 无法表达声明类型，不能可靠处理 `null` 与多态序列化。**
   - 位置：`Utils/~Serialization/ITypeSerializer.cs`。
   - 说明：`Serialize(object?)` 只接收运行时对象；当值为 `null` 时没有任何类型信息，基类/接口声明类型也会丢失。`Deserialize` 却要求类型，接口的两个方向并不对称。
   - 建议：采用 `Serialize(object? value, Type declaredType)`，或以泛型 `Serialize<T>`/`Deserialize<T>` 为主 API；旧的 object 入口只能作为明确的 runtime-type 便利方法。

204. **[P1][已接受，不修改] `OperationResult<T>` 的两个隐式转换在 `T=string` 时相互冲突。**
   - 位置：`Utils/~Operation/OperationResult.cs`。
   - 说明：同时存在从 `string`（错误）和从 `T`（成功）的隐式转换；构造 `OperationResult<string>` 时，`"value"` 无法表达是成功值还是错误消息，并会导致歧义。
   - 建议：移除至少一个隐式转换，使用 `Success(value)`、`Failure(message)` 这类具名工厂；这也是更容易读懂的错误/成功边界。

205. **[P2][已修复] `RepeatUntil` 把负延迟和负超时静默解释成“无限制”。**
   - 位置：`Actions/ActionExtensions.cs` 的 `RepeatUntil` 两组重载。
   - 说明：`timeout <= TimeSpan.Zero` 被转换为 `null`，负 delay 又会绕过延迟；错误配置可能变成紧凑无限循环，调用者很难诊断。
   - 建议：`null` 是唯一的“未设置超时”表示；显式验证 `delay >= 0`、`timeout > 0`，并让秒数重载保持同一规则。

206. **[P2][已处理（文档说明）] `TaskHelper.Repeat(Func<Task<T>>, …)` 在委托同步抛错时不会返回 faulted task。**
   - 位置：`Helpers/TaskHelper.cs` 的 `Repeat`。
   - 说明：构造 `IEnumerable` 时立即调用委托，异常在到达 `Task.WhenAll` 前同步逸出；这与同组异步 API 的“返回可 await 的失败任务”契约不一致。
   - 建议：统一通过一个捕获同步异常的 task factory 调用委托，或明确拆出同步/异步两套 API。

207. **[P1][已修复] 超时的 `TaskHelper.RunAsync` 允许底层任务在调用者离开后以未观察异常失败。**
   - 位置：`Helpers/TaskHelper.cs` 的 `RunAsync(..., TimeSpan?)`。
   - 说明：超时只取消等待，传入的 operation 可能忽略 token 并继续运行；其后续异常没有观察者。`Operation.ExecuteAsync` 又以忽略 token 的委托调用它，放大了该风险。
   - 建议：把“超时取消工作”与“仅停止等待”拆成显式 API；后一种必须为遗留任务附加异常观察，并在文档说明工作仍会继续。

208. **[P2][已修复] `NextDateTime` 不定义不同 `DateTimeKind` 边界的语义。**
   - 位置：`Extensions/~System/RandomExtensions.cs` 的 `NextDateTime`。
   - 说明：它直接比较 ticks、最后套用 `minValue.Kind`；相同 ticks 的 Local、Utc、Unspecified 代表的时间语义不同，结果也随参数顺序改变。
   - 建议：要求两个边界 Kind 相同，或改用/转为 `DateTimeOffset` 后按 instant 取样；不要静默选择一端的 Kind。

209. **[P1][已修复] `NextDateTimeOffset` 在范围边缘可随机抛出越界异常。**
   - 位置：`Extensions/~System/RandomExtensions.cs` 的 `NextDateTimeOffset`。
   - 说明：实现先在 UTC ticks 上取样，再强行套用 `minValue.Offset`。靠近 `DateTimeOffset.MinValue/MaxValue` 时，该 offset 的本地钟面值可能超界，导致某些随机结果才失败。
   - 建议：返回 UTC offset 的结果，或在取样前收窄为该 offset 可表示的 instant 范围，并测试两端边界。

210. **[P0][已修复] `NextMarshalable<T>` 会把随机指针字节当作托管对象结构解组。**
   - 位置：`Extensions/~System/RandomExtensions.cs` 的 `NextMarshalable`，以及 `TypeExtensions.Unmanaged.EnsureMarshalable`。
   - 说明：`EnsureMarshalable` 因任何 `[MarshalAs]` 就放行，包括 `LPStr`、`LPArray` 等指针形字段；`Marshal.PtrToStructure<T>` 随后可能解引用随机地址，造成访问违例或进程崩溃。
   - 建议：随机生成仅允许无引用、内联布局（或明确验证仅 `ByValArray`/`ByValTStr`）的类型；把可含指针的 interop marshal 与随机值生成彻底分开。

211. **[P1][已处理（文档说明）] 名称为“Bytes”的 marshal API 输出的是悬挂/进程内指针，而不是可安全传输的字节。**
   - 位置：`Helpers/ObjectHelper.cs` 与 `Extensions/~System/BytesExtensions.cs` 的 `MarshalArrayToBytes`。
   - 说明：对含指针字段的结构调用 `StructureToPtr` 后得到的字节含地址；清理结构后地址可能已失效，持久化或跨进程使用更没有意义。当前 API 名称和返回类型暗示了可用的二进制表示。
   - 建议：限制为 `unmanaged`/验证过的内联结构；若必须保留 interop 快照，另设明确命名且说明地址生命周期的 API。

212. **[P0][已修复] `UnsafeHelper` 的按字节读写允许复制含 GC 引用的结构并越过缓冲区边界。**
   - 位置：`Helpers/UnsafeHelper.cs` 的 `WriteTo`、`ReadFrom`、`Reinterpret`。
   - 说明：约束仅为 `struct`，但实现通过 `__makeref` 和调用方给定的 `sizeOfT` 复制原始内存；这会破坏含引用字段的 GC 不变量，任意长度还可越界读写。
   - 建议：公共 API 至少要求 `unmanaged` 并自行计算大小；把真正的裸指针版本设为 internal，或要求 `Span<byte>` 以便进行边界检查。

213. **[P0][已修复] `UnsafeHelper.GetValue<T>(IntPtr)` 可把任意地址伪造为托管引用。**
   - 位置：`Helpers/UnsafeHelper.cs` 的泛型 `GetValue`。
   - 说明：`T` 没有 unmanaged 限制；对 `string` 或含引用结构调用会让 GC 看见来自任意地址的“对象引用”，这是内存安全边界，而不只是普通 unsafe 性能工具。
   - 建议：删除这个公共泛型入口，或只允许 `unmanaged` 并以 `ref readonly`/值类型方式返回；托管对象地址不应由 `IntPtr` API 解释。

214. **[P1][已处理（文档说明）] 自定义 delegate 工厂在不可回收动态程序集持续定义类型。**
   - 位置：`Helpers/DelegateHelper.cs` 的 `MakeNewCustomDelegate`，其调用方包括 `IntPtrExtensions` 与 interface invocation。
   - 说明：每个签名都会 `DefineType`，没有缓存和卸载路径；若签名包含来自可卸载上下文的类型，会把它们和生成的动态类型永久保留。
   - 建议：标准签名优先使用 `Expression.GetDelegateType`；其余签名建立有界、可说明生命周期的缓存，并拒绝或特别处理 collectible 类型。

215. **[P1][已处理（文档说明）] 公开修改 `Exception` 私有字段的 API 不具备跨运行时契约。**
   - 位置：`Extensions/~System/ExceptionExtensions.cs` 的 `SetMessage`、`GetMessage`、`SetStackTrace`、`GetStackTrace`。
   - 说明：实现依赖运行时内部字段名和布局，裁剪、AOT、不同 CLR 版本都可能失效；修改异常对象还可能令消息、远程栈和 HResult 等元数据彼此矛盾。
   - 建议：从核心公共 API 移除这类 mutation，改为包装异常/格式化输出；若保留诊断工具，应移入受限包并明确“不支持生产/AOT”。

216. **[P2][已修复] `RetryHelper` 在异常筛选器中执行用户重试谓词，会吞掉谓词自身异常。**
   - 位置：`Helpers/RetryHelper.cs`。
   - 说明：C# 异常筛选器抛出的异常被运行时视为筛选失败，最终重新抛出的是原操作异常；配置/分类器故障完全不可见。
   - 建议：先在普通控制流中调用 `shouldRetry`，让其异常正常传播或明确包装；不要在 `catch when` 中运行用户代码。

217. **[P2][已修复] `ExpiringLazy<T>` 用墙上时钟实现有效期，且允许首次访问时溢出。**
   - 位置：`Utils/~Lazy/ExpiringLazy.cs`。
   - 说明：系统时间回拨/跳跃会延长或缩短缓存寿命；`DateTime.UtcNow.Add(lifetime)` 对过大的 `TimeSpan` 还会在运行时抛出。
   - 建议：验证可表示范围，并以 `Stopwatch` 的单调时间戳计算期限；若语义确实是绝对时间，应在命名和文档中说明。

218. **[P1][已修复] batch consumer 把正在重试的内部数组作为 `IReadOnlyList<T>` 通知给外部。**
   - 位置：`Utils/~Consumers/RetryingBatchConsumer.cs` 与 `ConsumerNotifications.cs` 的 `BatchConsumptionFailure<T>`。
   - 说明：数组可被订阅者向下转型并修改，而同一个数组随后可能进入 retry/split 队列；这突破了 consumer 的所有权边界，导致重试内容被外部篡改。
   - 建议：通知前复制为不可变快照，且内部工作项永不暴露其数组实例。

219. **[P2][已处理（文档说明）] consumer 的观察事件在消费循环线程同步执行，监听器实际控制吞吐和停机。**
   - 位置：`Utils/~Consumers/RetryingConsumer.cs`、`RetryingBatchConsumer.cs` 的 `Notify`。
   - 说明：文档只说监听器“失败不影响消费”，但慢监听器会阻塞消费；监听器若同步等待 `StopAsync` 还会等待当前循环，形成死锁风险。
   - 建议：明确事件属于同步、不可阻塞回调，或改为受控的异步通知/独立 dispatch；两种模型不能混在一个无说明的 event 上。

220. **[P2][已修复] `NameValues` 构建把任意值 `ToString()` 成协议数据，却没有格式化策略。**
   - 位置：`Utils/~Collections/INameValuesBuilder.cs`、`NameValuesExtensions.cs`。
   - 说明：数字、日期等会使用当前文化，生成的 URI/请求参数会随服务器区域设置变化；这不是一般显示文本，而是跨边界协议值。
   - 建议：选用 `IFormattable.ToString(null, CultureInfo.InvariantCulture)`，或把 `IFormatProvider`/值格式化器作为 options 的一部分。

221. **[P1][已修复] `WhenDefault` 对值类型永远可能判断失败。**
   - 位置：`Utils/~Collections/INameValuesBuilder.cs:105` 的 `DefaultNameValuesBuilder.Build`。
   - 说明：`value == type.DefaultValue()` 比较的是两个装箱后的 `object` 引用；例如 `0`、`false`、枚举默认值通常不是同一对象，因而不会被省略。
   - 建议：以 `EqualityComparer<T>.Default` 的等价逻辑比较实际值，或通过类型化 accessor 取得 default 后使用 `object.Equals`。

222. **[P2][已修复] `NameValueOmitOption.Never` 与可组合 flags 的模型矛盾。**
   - 位置：`Utils/~Collections/NameValueOmitOption.cs:16`。
   - 说明：`Never` 是普通 bit，`Never | WhenNull` 同时成立，而 builder 仍按 `WhenNull` 省略；“Never”并不能覆盖其他选项。
   - 建议：把“继承/从不省略”建模为非 flags 的独立值，或在 options 解析时拒绝冲突组合并定义优先级。

## 问题清单（223–251：公共 API、命名与实现细节）

223. **[P3][已修复] `ConsoleTable.AddRow` 以通用 `Exception` 报告可预期的参数/状态错误。**
   - 位置：`Utils/~ConsoleTable/ConsoleTable.cs:22`。
   - 建议：无列时抛 `InvalidOperationException`，单元格数不匹配时抛带 `values` 参数名的 `ArgumentException`；调用方才能正确处理。

224. **[P2][已修复] `TreeNode<T>` 允许 null value，却把层序遍历声明为非空 `IEnumerable<T>`。**
   - 位置：`Utils/TreeNode.cs:132`。
   - 说明：遍历用 null-forgiving 返回 `Value`，调用方获得的 NRT 承诺不真实。
   - 建议：限制 `T : notnull`，或将所有相关返回值改为 `IEnumerable<T?>`；二者选其一并保持一致。

225. **[P2][已修复] `NameIdentifier<T>` 的构造函数接受 null，却无法维持自身不变量。**
   - 位置：`Utils/NameIdentifier.cs:28`。
   - 说明：直接构造 null 后，`GetHashCode`/`ToString` 可失败；工厂路径的约束与公开构造路径不一致。
   - 建议：在主构造函数验证 `Name` 非空，或将成员和所有派生语义完整改为可空。

226. **[P1][已修复] `Clamp` 未验证下界不大于上界。**
   - 位置：`Extensions/~System/ObjectExtensions.cs:15`。
   - 说明：`Clamp(10, 5, 1)` 会给出看似合法但没有定义基础的结果，和 `Math.Clamp` 的契约不同。
   - 建议：在比较前验证 `min <= max` 并抛 `ArgumentException`，或直接委托到对应 BCL API。

227. **[P2][已修复] `CastTo<T>` 实际是动态绑定转换，不是名称所示的普通 cast。**
   - 位置：`Extensions/~System/ObjectExtensions.cs:7`。
   - 说明：它会运行用户定义转换并依赖 dynamic binder，在 AOT/裁剪环境也带来额外要求；调用者只从名称无法预期这些行为。
   - 建议：普通 API 用 `(T)value` 的类型转换；若要保留转换运算符支持，应命名为 `DynamicConvertTo` 并隔离到可选包。

228. **[P1][已修复] `DictionaryExtensions.Add` 的两个泛型重载对常见“字典到集合”形状形成歧义。**
   - 位置：`Extensions/~System/~Collections/~Generic/DictionaryExtensions.cs:25`、`:58`。
   - 说明：`Dictionary<TKey, List<TValue>>` 同时满足两组 `ICollection<T>` 约束，泛型参数顺序也相反；调用 `Add(key, value)` 难以稳定绑定。
   - 建议：保留单一明确重载，或改名为 `AddToCollection` 并固定 `TKey, TValue, TCollection` 的顺序。

229. **[P2][已修复] `CrossJoin` 假定输入可重复枚举，却接受任意 `IEnumerable`。**
   - 位置：`Extensions/~System/~Collections/~Generic/EnumerableExtensions.cs:188`。
   - 说明：右序列会为每个左元素重枚举；one-shot iterator、网络流或有副作用的 enumerable 会得到不完整/错误的笛卡尔积。
   - 建议：在入口快照需要重用的一侧，或把参数限制为 `IReadOnlyCollection` 并在文档中声明枚举要求。

230. **[P2][已修复] 名为 `SelectMany` 的两序列扩展实际执行自笛卡尔积。**
   - 位置：`Extensions/~System/~Collections/~Generic/EnumerableExtensions.cs:290`。
   - 说明：该名称与 LINQ `SelectMany` 的嵌套展平语义冲突，而库内已有 `CrossJoin`；调用点很容易误解并引入 O(n²) 工作。
   - 建议：移除或重命名为 `CrossJoin`/`SelfCrossJoin`，避免与 BCL LINQ 方法同名但语义不同。

231. **[P1][已修复] `AnyContainsAny`、`AnyContainsAll` 等方法会重复消费输入 `IEnumerable<string>`。**
   - 位置：`Extensions/~System/~Collections/~Generic/EnumerableExtensions.String.cs:8`。
   - 说明：嵌套 `Any`/`All` 会为每个 `values` 元素重新枚举 `enumerable`；一次性序列会在第一个值的比较后耗尽，结果取决于枚举器状态而非字符串关系。
   - 建议：物化较小/需要重用的一侧，或改用集合参数；不能把“可重复枚举”作为未声明前提。

232. **[P2][已修复] 串行 `ToOperationIOPairsSerially` 在最后一项后仍等待 interval。**
   - 位置：`Extensions/~System/~Collections/~Generic/EnumerableExtensions.IOPair.cs:125`。
   - 说明：interval 的自然语义是两次操作之间的间隔，当前实现却把完成时间额外推迟一个 interval。
   - 建议：仅在确认后面还有下一项时延迟，或把现有行为改名为“每项完成后延迟”。

233. **[P2][已修复] `Average(IEnumerable<TimeSpan>)` 经由 `double` 会丢失大 ticks 值的精度。**
   - 位置：`Extensions/~System/~Collections/~Generic/EnumerableExtensions.cs:123`。
   - 说明：`long` ticks 大于 2^53 时不能由 double 精确表示，平均值可能在没有溢出的情况下错误。
   - 建议：用整数累计（必要时 `decimal`/溢出检测）计算商和余数，或清晰返回 `double` 秒数而非伪精确 `TimeSpan`。

234. **[P2][已修复] `AddWeeks` 先执行 int 乘法，极端输入会悄然溢出。**
   - 位置：`Extensions/~System/DateTimeExtensions.cs:63`。
   - 说明：`numberOfWeeks * 7` 在传入 `int` 时先以 32 位计算，随后把已损坏的天数交给 `AddDays`。
   - 建议：使用 `checked((long)numberOfWeeks * 7)`，再明确转换/报告超出 `DateTime` 可表示范围的情况。

235. **[P2] `AsyncEventHandlerExtensions.GetInvocationList<T>` 用 `Unsafe.As` 谎称返回了 `T[]`。**
   - 位置：`Extensions/~System/AsyncEventHandlerExtensions.cs:6`。
   - 说明：运行时对象实际是 `Delegate[]`；读似乎可行，但调用方按声明类型写回数组会产生运行时数组类型问题，API 的数组契约已被破坏。
   - 建议：使用 `.Cast<T>().ToArray()` 返回真实的 `T[]`，或只暴露 `IReadOnlyList<T>`。

236. **[P2][已修复] `ReadOnlyListExtensions.TryGet` 的 `[NotNullWhen(true)]` 承诺不成立。**
   - 位置：`Extensions/~System/~Collections/~Generic/ReadOnlyListExtensions.cs:5`。
   - 说明：列表可以合法包含 null；索引有效时方法返回 true 仍可能把 null 写入 `out value`。
   - 建议：移除该 attribute，或只对 `T : notnull` 的 API 使用它。

237. **[P2][已修复] 带 group index 的 `RegexExtensions.TryMatch` 没有遵守 Try 模式。**
   - 位置：`Extensions/~System/~Text/~RegularExpressions/RegexExtensions.cs:48`。
   - 说明：匹配成功但 index 越界时直接索引 `Groups[groupIndex]` 并抛异常；调用者无法从 `bool` 判断失败。
   - 建议：验证 index 并返回 false，或把参数错误与匹配失败拆成 `GetRequiredGroup` 等具名方法。

238. **[P2][已修复] span split 枚举器在非法 `Current` 状态返回 default 或陈旧数据。**
   - 位置：`Extensions/~System/ReadOnlySpanExtensions.cs:208` 的 split enumerator。
   - 说明：在首次 `MoveNext` 前及其返回 false 后读取 `Current` 不会按 .NET 枚举器约定抛异常，容易掩盖错误循环。
   - 建议：跟踪有效状态并抛 `InvalidOperationException`，或明确它是非常规 ref-struct iterator 并改名。

239. **[P3][已修复] `ToBytes(ReadOnlySpan<bool>)` 隐藏了 bit packing 的位序。**
   - 位置：`Extensions/~System/ReadOnlySpanExtensions.cs:58`。
   - 说明：它把八个 bool 压成一个字节且采用特定 LSB-first 顺序；`ToBytes` 容易被理解为一个 bool 一个字节，跨系统协议会错位。
   - 建议：重命名为 `PackBits`，在 API/文档声明位序，并提供对应 `UnpackBits`。

240. **[P2][已修复] `BuildType`/`IsDebug`/`IsRelease` 把 JIT 标志误报为编译配置。**
   - 位置：`Extensions/~System/~Reflection/AssemblyExtensions.cs:66`、`:95`、`:100`。
   - 说明：`DebuggableAttribute` 与 JIT 优化不能可靠还原 MSBuild 的 Debug/Release 配置；当前二元 enum 还强迫任何程序集属于其中之一。
   - 建议：只公开可观察到的 `IsJitOptimized`（最好命名为 best-effort），或把配置检测设为 nullable/未知而非事实断言。

241. **[P3] `MemberInfoExtensions.IsDefined<T>` 缺少 `where T : Attribute`。**
   - 位置：`Extensions/~System/~Reflection/MemberInfoExtensions.cs:5`。
   - 说明：签名允许传入任意类型，直到运行时才失败；相邻 attribute API 已有正确约束。
   - 建议：添加 `where T : Attribute`，使不合法调用在编译期被拒绝。

242. **[P2] `MethodInfoExtensions.GetSignature` 生成的字符串并不唯一。**
   - 位置：`Extensions/~System/~Reflection/MethodInfoExtensions.cs:14`。
   - 说明：它漏掉泛型参数个数/实参、返回类型以及 ref/out 语义；重载泛型方法可得到相同“signature”，不能用于日志关联、缓存键或查找。
   - 建议：要么命名为 `GetDisplayName`，要么依据 metadata 完整编码 generic arity、参数修饰符和返回类型。

243. **[P2] `TypeExtensions.CreateObject` 把 null 的 params 数组变成“一个 null 参数”。**
   - 位置：`Extensions/~System/TypeExtensions.cs:25`。
   - 说明：`args ??= [null]` 让 `CreateObject(type, (object?[]?)null)` 的含义与空参数列表不同且极不直观，可能意外选择可空单参构造函数。
   - 建议：拒绝 null params 数组，或将其等同空数组；一个 null 实参应由调用者显式传 `new object?[] { null }`。

244. **[P2] `TypeExtensions.Implements` 对接口类型自身返回 false，名称却没有表达此例外。**
   - 位置：`Extensions/~System/TypeExtensions.cs:181`。
   - 说明：`typeof(IDisposable).Implements(typeof(IDisposable))` 返回 false；大多数调用者会把“implements”理解为 assignability/接口关系而不是仅“继承来的接口”。
   - 建议：改为包含自身的 `ImplementsOrIs`/`IsAssignableTo` 语义，或将现有方法重命名为 `ImplementsIndirectly`。

245. **[P2] `ReflectionHelper.AccessorAccessesField` 逐字节扫描 IL，会把操作数误识别为 opcode。**
   - 位置：`Helpers/ReflectionHelper.cs:91`。
   - 说明：循环没有按 IL operand 长度解码，任意 operand byte 都可能恰好是 `ldfld`/`stfld`；方法名承诺的字段访问结论会出现假阳性，也漏掉 `ldflda` 等合法访问。
   - 建议：用完整 IL decoder，或删除这个不可靠的通用判断并只在受控模式下使用。

246. **[P2] `TimerLifetime.BeginDispose` 在首个释放路径抛异常时可能让并发释放者自旋不止。**
   - 位置：`Utils/~Threading/TimerLifetime.cs:14`。
   - 说明：第一个线程已将 `_timer` 交换为 null、尚未写入 `_disposeTask`；若 `DisposeAndWaitAsync` 抛出，其他线程会一直等 `_disposeTask` 被写入。
   - 建议：先发布一个 `TaskCompletionSource`，所有路径（包括异常）都完成它；不要以无限自旋等待一个可能永远不会发布的 task。

247. **[P2] `CancellationTokenSource.TryCancel` 无差别吞掉回调异常和对象生命周期错误。**
   - 位置：`Extensions/~System/~Threading/CancellationTokenSourceExtensions.cs:5`。
   - 说明：名字像“竞态安全地尝试取消”，实现却隐去了取消回调失败等真正需要诊断的问题；调用者无法区分已取消、已释放和回调失败。
   - 建议：只处理预期的 `ObjectDisposedException`（并返回 bool），其他异常照常传播；或命名为明确的 `CancelIgnoringExceptions`。

248. **[P2] `UriParams.Render` 不能保留空键的查询参数语义。**
   - 位置：`Utils/~Net/UriParams.cs:34`。
   - 说明：空键且有值时输出 `value`，不是 `=value`；再次解析会把 value 当作键，破坏 `Parse`/`Render` 往返。现有文档还把这一行为描述成正确。
   - 建议：始终写出 `=`，或明确不支持空键并在 Add/Parse 时拒绝它。

249. **[P2] `StreamExtensions.ReadAllTextAsync` 默认关闭由调用方提供的 stream。**
   - 位置：`Extensions/~System/~IO/StreamExtensions.cs:24`。
   - 说明：扩展方法读取现有 stream 时通常不应取得所有权；默认 `leaveOpen=false` 让简单读取意外关闭网络流、压缩流或复用的内存流。
   - 建议：默认 `leaveOpen=true`，或采用 `ReadAllTextAndDisposeAsync` 这类明确所有权的名称。

250. **[P3] `Check.NotEmpty(IEnumerable<T>)` 在验证阶段消费序列，API 未揭示这一副作用。**
   - 位置：`Check.cs:155`。
   - 说明：对一次性/有副作用 enumerable，检查本身就可能启动 I/O 或改变后续结果；同名的 collection 重载没有这种行为。
   - 建议：优先接受 `IReadOnlyCollection<T>`，或返回一个可安全继续枚举的物化结果，并在名称中体现枚举行为。

251. **[P3] `SemaphoreSlimExtensions.IsEmpty` 将瞬时快照包装成状态判断。**
   - 位置：`Extensions/~System/~Threading/SemaphoreSlimExtensions.cs:43`。
   - 说明：`CurrentCount == 0` 在返回后立即可能变化，`IsEmpty` 这个集合式名称易被误用于“可以据此安全决策”的检查。
   - 建议：改名为 `HasNoAvailablePermitsSnapshot` 并补充文档，或不提供该竞争敏感的便利 API。

## 建议处理顺序

1. 先处理 235、245 与 246：它们涉及类型安全、反射结果可靠性与并发释放。
2. 接着统一 241–244 的公共 API 契约、命名、异常类型和边界行为。
3. 最后处理 247–251 的取消、URI、流所有权和瞬时状态 API，并为每个已确认的行为补充针对性测试。
