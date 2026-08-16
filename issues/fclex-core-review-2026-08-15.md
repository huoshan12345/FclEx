# FclEx.Core 源码审查报告（2026-08-15）

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

132. **[P1][已修复] `StreamReaderExtensions` 在 `StreamWriter` 类型上查找读取方法**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/StreamReaderExtensions.cs:5-7,29-30`。两个反射字段都对 `typeof(StreamWriter)` 查找 `ReadToEndAsync`/`ReadLineAsync`，结果必为 null；旧目标程序集即使运行在提供原生 cancellation overload 的新 runtime 上也永远走 fallback，取消只停止等待而不中断底层读取。应改为 `typeof(StreamReader)`，并为反射返回类型分别验证 `Task<string>`/`ValueTask<string?>`。
    修复：反射目标已改为 `StreamReader`，使 netstandard2.0 程序集可在支持对应 overload 的较新 runtime 上调用原生 cancellation-aware API。

133. **[P1][不修改] 旧目标的异步文本写入宣称支持取消，但主体写入完全不观察 token**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/FileExtensions.cs:17-25`、`StreamWriterExtensions.cs:17-26`。`WriteAllTextAsync` 用无 token 的 `sw.WriteAsync(content)` 写完全部内容后才在 flush 检查取消；`FlushAsync(token)` fallback 又直接忽略 token。大文本操作可能在取消后继续长时间写盘却仍最终报告取消。应分块写入并在块间观察 token；无法取消底层 flush 时至少取消等待并在文档中说明 I/O 可能继续。
    处理决定：低目标框架的 `StreamWriter.WriteAsync` 不接受 `CancellationToken`。保留当前实现；取消不能中断正在进行的底层写入。

134. **[P1][已修复] `TextWriter.SetConsole` 用可嵌套 scope 包装进程级全局状态**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/TextWriterExtensions.cs:3-10`。`Console.Out` 是进程全局属性；并发或非 LIFO Dispose 的两个 scope 会互相覆盖，并把过期 writer 恢复回来，没有任何同步或所有权检查。该能力不适合作为通用 `IDisposable` 扩展；应由应用启动层集中设置，测试重定向则使用串行 fixture/显式全局锁。
    修复：已移除 `TextWriter.SetConsole`，不再为进程级全局状态提供看似局部的 scope 抽象。

135. **[P0][已修复] `PhysicalAddress.AddressBytes` 公开了可替换/可修改的私有后备数组**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Net/~NetworkInformation/PhysicalAddressExtensions.cs:5-32,52-56`。NET8+ 甚至返回 `ref byte[]`，调用方可替换 `_address`；旧目标也返回同一可变数组。对象可在作为 dictionary key 后被修改，破坏 equality/hash，不同 TFM 的返回签名还不一致，并依赖私有字段名。应删除该公共 API，格式化直接使用官方 `GetAddressBytes()` 的副本；确需零复制只能限于 internal、只读且目标受控的实现。
    修复：返回类型改为 `ReadOnlySpan<byte>`，保留零复制读取能力，同时不再以可转型、可写的对象形式公开底层数组。

136. **[P1][已修复] 旧目标回填的 `HttpRequestException.StatusCode` 与官方 nullability 契约不同**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Net/~Http/HttpRequestExceptionExtensions.cs:7-18,27-33`。官方属性是 `HttpStatusCode?`，这里却返回非 nullable enum，并用数值 0 表示不存在；调用方跨 TFM 编译时既看到不同签名，也无法区分“无状态码”和非法/默认值。回填 API 应精确匹配官方 nullable 类型与缺失语义。
    修复：旧目标回填属性改为 `HttpStatusCode?`；工厂方法也接受 nullable status code，缺失状态码保持为 null。

137. **[P1][不修改] `IsIPv6UniqueLocal` 为简单位判断依赖 `IPAddress` 私有字段布局**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Net/IPAddressExtensions.cs:48-54`、`src/FclEx.Core/FclEx/FieldInfos.cs:16-22`。旧目标实现读取 `_numbers`，字段名、元素顺序和存在性都不是 runtime 契约，在不同 Mono/.NET Framework 实现、裁剪或未来 runtime 上会失效。该判断只需官方 `GetAddressBytes()` 的首字节满足 `(b & 0xFE) == 0xFC`，没有使用反射的合理性。
    处理决定：为避免 `GetAddressBytes()` 的小数组复制，保留当前内部字段访问，并接受其与 runtime 私有布局耦合的风险。

138. **[P1][已修复] embedded resource 的后缀匹配会在重名时任意选择第一个资源**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Reflection/AssemblyExtensions.cs:11-18`、`src/FclEx.Core/FclEx/Helpers/ResourceHelper.cs:7-16`。两个入口都对 manifest names 做无 `StringComparison` 的 `EndsWith(name)` 并取 `FirstOrDefault`；不同命名空间含同名资源时结果依赖枚举顺序，文化比较也不适合标识符。应优先要求完整资源名；若支持短名，使用 ordinal 比较并在多个候选时抛 ambiguity error。
    修复：两个入口统一使用内部 resolver：先做 ordinal 完整名称匹配，再允许唯一的 ordinal 后缀匹配；无匹配仍按原入口语义处理，多个后缀匹配则抛明确的 `ArgumentException`。测试使用两个同后缀嵌入资源覆盖完整名称和歧义路径。

139. **[P1][已修复] `ReflectionHelper` 的全局 Type cache 会阻止 collectible assembly 卸载**

    位置：`src/FclEx.Core/FclEx/Helpers/ReflectionHelper.cs:16-44`。静态 `ConcurrentDictionary<Type,...>` 强引用每个见过的 Type 及其 `MemberInfo`，插件/脚本通过 collectible `AssemblyLoadContext` 加载的程序集将永远被该 Core helper 固定。应使用 `ConditionalWeakTable<Type,...>` 或让缓存归属调用方/可卸载上下文。
    修复：cache 改为 `ConditionalWeakTable<Type, IReadOnlyList<DataMemberInfo>>`，不再由缓存的 key 强引用 Type；同时保留同步初始化以避免重复创建值。

140. **[P0][已验证] `AccessorAccessesField` 没有解码 IL 指令，可能误判或解析任意 operand 为 metadata token**

    位置：`src/FclEx.Core/FclEx/Helpers/ReflectionHelper.cs:83-127`。循环逐字节寻找 `0x7B/0x7D/0x7E/0x80`，这些字节也可能出现在其他指令的 operand 中；随后把后四字节交给 `ResolveField`，可抛出 metadata 异常或碰巧命中错误字段。必须按 opcode 和 operand 宽度完整解码 IL（含双字节 opcode），并只对真实 field 指令解析 token；否则应删除这一启发式公共 API。
    验证：新增动态 IL 用例，将 field token 仅嵌入 `ldc.i4` operand；当前实现把其中的 `0x7B` 当作 `ldfld` 并返回 true。该失败用例按真实 defect 保留。

141. **[P0][已修复] `HashAlgorithm.Hash` 把空输入的摘要定义成空数组**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Security/~Cryptography/HashAlgorithmExtensions.cs:5-25`。空消息有标准且非空的 cryptographic digest，实现却对 null/空数组返回 `[]`，使所有算法在空输入上产生相同结果；offset/count overload 还因此跳过本应发生的范围校验。应对空数组调用 `ComputeHash`，对 null 明确抛 `ArgumentNullException` 或单独定义 nullable 语义。
    修复：所有 overload 直接委托 `ComputeHash`，空输入产生算法规定的 digest，null 和范围错误由 BCL 参数校验报告。

142. **[P1][已修复] `IAsyncEnumerable` materializer 没有取消入口**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/AsyncEnumerableExtensions.cs:3-19`。`ToListAsync`/`ToArrayAsync` 只能无 token 枚举，面对无限流、慢 I/O 或调用方取消时无法通过 API 传播 cancellation；这对异步 materializer 是核心签名缺失。应增加 `CancellationToken` 并使用 `source.WithCancellation(token).ConfigureAwait(false)`，保持与常见 async LINQ 约定一致。
    修复：两个 materializer 新增可选 `CancellationToken`，并通过 `WithCancellation` 传入枚举器；测试覆盖取消传播和正常 array materialization。

143. **[P1][已修复] `Exception.ForEach` 的实现并不遍历“每个异常”，且共享节点会重复执行**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/ExceptionExtensions.cs:37-86`。`action` 只在没有 inner 的叶节点调用，root `AggregateException` 和所有中间包装异常都被跳过，与文档相反；`handled` 又只在叶节点处理后写入，两个分支共享同一 inner 时会先重复入队并重复执行。应采用统一的 visited set，在 dequeue 时去重，并明确对每个节点（含 aggregate/container）执行一次还是只遍历 leaves，名称和文档须与选择一致。
    修复：移除语义模糊的 `ForEach`，改为 `Enumerate()`（完整异常树）和 `EnumerateLeaves()`（仅叶异常）。两者按广度优先、引用身份去重；共享子树在入队时即去重。测试覆盖完整树、叶节点、共享子树和普通 inner 链。

144. **[P1][已修复] `Enum.Info/GetAttribute` 对未命名值和复合 flags 值抛 `NullReferenceException`**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/EnumExtensions.cs:7-18,100-105`。`enumValue.ToString()` 对 `A | B` 返回 `"A, B"`，对未知数值返回数字字符串；`GetField(...)` 为 null 后被 `!` 掩盖并立即调用扩展。attribute 查询应在没有对应声明字段时返回 null，`Info` 还需定义复合值是按组成成员合并信息还是只提供格式化名称。
    修复：`GetAttribute` 在不存在声明字段时返回 null；未声明的 enum 值直接创建信息而不进入 cache，因此复合 flags 和未知数值不会抛异常。

145. **[P1][已修复] `TryToInteger<TEnum,TInteger>` 实际接受任意同尺寸 unmanaged 类型并做位重解释**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/EnumExtensions.cs:32-57`。`TInteger : unmanaged` 允许 `float`、`Guid`、用户 struct 等；只要 `Unsafe.SizeOf` 相同就返回 true，并非“转换为整数”。应为受支持的整型提供明确 overload/运行时类型检查并定义溢出策略；若保留位重解释，应改名为 `BitCast`，其目标不应伪装成 integer。
    修复：在相同尺寸外额外要求 `TInteger` 为整数类型；其他 unmanaged 类型现在返回 false，`ToInteger` 相应抛出 `InvalidCastException`。

146. **[P2][已修复] `Enum.Info` 的展示值受当前文化影响，缓存又会无界保存任意枚举数值**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/EnumExtensions.cs:5-18`。`ToLower()`/`ToUpper()` 使用当前文化，identifier 在土耳其语等文化下产生不同结果；cache 以 boxed `Enum` 值为 key，flags 任意组合和强转出的未知数值都会永久增长。应使用 invariant casing，并优先按 enum type/已声明 member 缓存元数据；动态组合值不应全局 intern。
    修复：大小写改为 invariant，且只有声明的 enum 值进入 cache；未声明的 flags 组合和数值不会造成无界增长。

147. **[P1][已修复] `NameValues<TSelf>` 的 CRTP 基类可被直接实例化并在首次修改时强转失败**

    位置：`src/FclEx.Core/FclEx/Utils/~Collections/NameValues.cs:8,29-32,71-76`。只要存在 `Foo : NameValues<Foo>`，调用方仍可合法创建 `new NameValues<Foo>(comparer)`；`Add` 随后执行 `(TSelf)this` 并抛 `InvalidCastException`。self-typed 基类应为 abstract 且构造器 protected，确保实际实例确实是 `TSelf`；或删除 CRTP，普通方法返回基类/void。
    修复：CRTP 基类现在为 abstract，构造器为 protected，只有正确的派生 `TSelf` 可以实例化。

148. **[P0][已修复] `EncodingHelper.GetEncoding` 的 BOM 判断会漏掉绝大多数 UTF-16/UTF-32 文件**

    位置：`src/FclEx.Core/FclEx/Helpers/EncodingHelper.cs:22-45`。实现只读 3 字节，并要求 UTF-16 BE 的第三字节恰为 `0x00`、UTF-16 LE 的第三字节恰为 `0x41`，把正文首字节错误地当作 BOM；UTF-32 BOM 需要 4 字节也无法正确识别。应复用已经存在的 `TryDetectEncoding`，读取足够的最大 preamble 长度并按完整 BOM 匹配。
    修复：按完整 preamble 检测 UTF-8、UTF-16 BE/LE、UTF-32 BE/LE，并循环读取前缀以处理短读；测试覆盖这些 BOM。

149. **[P0][已修复] `EncodingHelper.IsUtf8` 既拒绝位于文件末尾的合法多字节字符，也接受非法 continuation byte**

    位置：`src/FclEx.Core/FclEx/Helpers/EncodingHelper.cs:52-97`。读取 lead byte 后使用 `Position < Length - N`，刚好剩余 N 个 continuation bytes 时条件为 false，所以任何以非 ASCII 字符结尾的文件都可能被判非 UTF-8；continuation 只检查最高位为 1，`11xxxxxx` 也被接受，并未排除 overlong、surrogate 或超过 U+10FFFF。应使用严格的 `UTF8Encoding(false, true)` decoder（流式处理跨 buffer 序列），不要维护不完整的手写验证器。
    修复：删除仅用于 `GetEncoding` 的 `IsUtf8` 启发式；无 BOM 内容不再被不可靠地猜测为 UTF-8，而是返回调用方指定的 default encoding。

150. **[P1][已修复] `EncodingHelper.GetEncoding(Stream)` 未声明却夺取并重置/关闭调用方 stream**

    位置：`src/FclEx.Core/FclEx/Helpers/EncodingHelper.cs:22-27,42-57`。方法要求 `Length`/`Seek`，把位置无条件重置到 0 而非原位置；`IsUtf8` 中 `using new BinaryReader(stream)` 又会在返回时关闭外部传入的 stream。应明确支持能力和所有权：通常保存并恢复原 position、使用 `leaveOpen: true`，对不可 seek stream 采用前缀缓冲或明确拒绝且文档说明。
    修复：方法现在要求可读、可 seek 的 stream，在 finally 中恢复原 position，且不会关闭调用方 stream；文档和测试明确这一契约。

## 继续审查（151–200）

说明：本轮继续排除 `Combinatorics` 目录，并先审查类型职责、生命周期、并发模型和公共 API，再看实现与命名。确认第 50 个新增问题（编号 200）后停止；以下问题未因兼容性顾虑降级或省略。

151. **[P1][已修复] `CompositeDisposable<T>` 没有完整的 disposed 生命周期，重复释放、释放后添加和异常清理都未定义好**

    位置：`src/FclEx.Core/FclEx/Utils/~Disposables/CompositeDisposable.cs:24-44`。`Dispose` 后仍保留原列表，第二次调用会再次释放全部子对象；`Add` 在释放后仍成功，新增对象永远不会被容器释放；任一子对象抛异常又会中止后续清理。这不是一个可靠的资源所有者。应原子地进入 disposed 状态、拒绝或立即释放后续 `Add`，并在尝试释放全部元素后聚合异常；若需并发使用，还要同步 `Add` 与 `Dispose`。
    修复：以可空资源列表表示 disposed 状态；重复释放幂等，释放后添加抛 `ObjectDisposedException`，所有子对象均会得到释放机会并在最后聚合异常。该容器明确不保证线程安全，因此不引入同步开销。测试覆盖重复释放、释放后添加和异常聚合。

152. **[P1][已删除] `ListEnumerator` 拒绝合法的空列表和空尾区间**

    位置：`src/FclEx.Core/System/Collections/Generic/ListEnumerator.cs:10-22`。构造器把 `start` 限制到 `0..Count-1`，因此默认构造空列表时连 `start = 0, length = 0` 都会抛异常，也不允许标准的 `start == Count, length == 0`。应采用 BCL 的 offset/count 规则：分别检查非负，并用 `start <= Count && length <= Count - start` 避免溢出。
    处理决定：删除未被使用的 `ListEnumerator`，不再维护该公共枚举器。

153. **[P1][已删除] `ListEnumerator.Current` 对非零起点使用了错误的有效范围**

    位置：`src/FclEx.Core/System/Collections/Generic/ListEnumerator.cs:24-42`。`_length` 保存的是区间元素数，`_i` 也是区间内索引，但校验上限写成 `_length - _start - 1`；例如 `start = 5, length = 3` 时第一次读取就失败。校验应为 `0.._length-1`，并保持枚举结束后 `Current` 的异常语义与 `IEnumerator<T>` 约定一致。
    处理决定：随问题 152 一并删除 `ListEnumerator`。

154. **[P1][已修复] `KeyValuePairEqualityComparer` 的相等与哈希使用了不同的 comparer**

    位置：`src/FclEx.Core/System/Collections/Generic/KeyValuePairEqualityComparer.cs:7-27`。`Equals` 使用构造器传入的 key/value comparer，`GetHashCode` 却调用 `HashCode.Combine(obj.Key, obj.Value)`，回到了默认 comparer。使用大小写不敏感等自定义 comparer 时会出现“相等对象哈希不同”，直接破坏哈希集合契约。应分别调用 `_keyComparer.GetHashCode` 和 `_valueComparer.GetHashCode` 后组合，并定义 null 值处理。
    修复：key/value 的哈希分别由对应自定义 comparer 生成并组合，null 通过 `GetHashCodeOrDefault` 处理。测试使用大小写不敏感 comparer 验证相等对象具有相同哈希且在 `HashSet` 中只保留一项。

155. **[P1][已修复] `OrderedIndex.RangeByRank` 的整数溢出会把有限区间变成几乎无限的枚举**

    位置：`src/FclEx.Core/System/Collections/Generic/OrderedIndex.cs:435-459,738-762`。`start + count` 使用 unchecked `int`；溢出后 `end - start` 可为负数，而 `RankRangeEnumerator.MoveNext` 只在 `_remaining == 0` 时停止，负数会继续递减并遍历到链表末尾。应先用 `count >= _count - start` 判断或使用更宽类型，并将停止条件写成 `_remaining <= 0` 作为防御。
    修复：以 `long` 执行 `start + count` 后再与 `_count` 取最小值，避免 `int` 溢出；测试覆盖 `start = 1, count = int.MaxValue` 并验证返回剩余全部元素。

156. **[P1][已修复] `OrderedIndex` 的 range 枚举器绕过了主枚举器的版本一致性模型**

    位置：`src/FclEx.Core/System/Collections/Generic/OrderedIndex.cs:435-488,738-836`。`RankRangeEnumerator` 和 `ScoreRangeEnumerator` 只保存内部 `Node` 引用，不捕获 `_version`，也不在 `MoveNext`/`Current` 检查修改。取得 range 后执行 remove/clear 可能继续沿已脱链节点返回已删除数据；这与同一类型主枚举器的 fail-fast 行为不一致。range 应保存 owner 与版本并统一检查，或明确返回快照。
    修复：两个 range enumerator 均保存 owner 和创建时版本，并在 `MoveNext`/`Reset` 时执行与主枚举器一致的版本检查。检查过程中发现空 range 返回的默认 struct 没有 owner，初版版本检查会触发 `NullReferenceException`；现已让默认 enumerator 保持空枚举语义。测试覆盖空 range，以及 rank range 与 score range 创建后的结构修改。

157. **[P1][已修复] `MultiValueDictionary.Add` 在值集合添加失败时会留下空 key**

    位置：`src/FclEx.Core/System/Collections/Generic/MultiValueDictionary.cs:649-660`。新 key 路径先把空 `InnerCollectionView` 发布到字典，再调用可能由用户提供、可能抛异常的 `AddValue`。一旦添加失败，字典就残留一个与“每个 key 至少一个 value”设计不一致的空项。应先创建并填充集合，成功后再发布；或在异常路径回滚字典和版本。
    修复：新 key 路径先创建集合并成功加入 value，之后才把集合发布到内部 dictionary。测试使用拒绝添加的自定义 collection，验证异常后不会残留 key。

158. **[P1][保留] `MultiValueDictionary.Create` 通过丢弃一次 factory 结果来“验证”后续集合**

    位置：`src/FclEx.Core/System/Collections/Generic/MultiValueDictionary.cs:417-426,453-464` 及其他 factory overload。每个入口先调用一次 `collectionFactory()` 检查 `IsReadOnly`，然后丢弃该实例；这会触发无意义的副作用或资源泄漏，也不能保证后续返回值非 null、可写且彼此独立。应在每次实际创建集合时验证结果，并避免探测性构造；文档还把异常条件写反成“IsReadOnly 为 true”。
    处理决定：为了在创建 `MultiValueDictionary` 时尽早报告只读 collection factory，保留当前探测行为；文档中的异常条件已修正。factory 应被视为可重复调用且无外部资源所有权的构造函数。

159. **[P2][API][已修复] `MultiValueDictionary` 关闭 nullable 分析，向 nullable-enabled NuGet API 暴露 oblivious 契约**

    位置：`src/FclEx.Core/System/Collections/Generic/MultiValueDictionary.cs:4-64` 及整个文件。`#nullable disable` 使 key、factory、comparer 和 `TryGetValue` 输出都无法表达真实的 null 契约，且 `TKey` 没有 `notnull` 约束；同一包的其他 API 已启用 nullable，消费者会得到不一致的编译期保证。应恢复 nullable，标注可选 comparer、`MaybeNullWhen(false)` 等，并给 `TKey` 加上合适约束。
    修复：移除 `#nullable disable`，为 `TKey` 增加 `notnull` 约束，并补齐 comparer 等入口的 nullable 签名。

160. **[P1][已验证] `JsonValidator` 把 Unicode whitespace 和 digit 当成 JSON 词法字符**

    位置：`src/FclEx.Core/FclEx/Utils/~Text/~Json/JsonValidator.cs:13-38,153-200`。JSON whitespace 只有空格、TAB、CR、LF，数字也只能是 ASCII `0-9`；当前 `char.IsWhiteSpace`/`char.IsDigit` 会接受 NBSP、阿拉伯数字等无效 JSON。此类标准解析不应维护第二套近似语法，建议直接基于 `Utf8JsonReader`/`JsonDocument`，并把该 API 的目标明确为“完整验证”还是“快速预检”。
    方向确认：该类型定位为不依赖具体 JSON parser 的快速校验器，因此不改为 `Utf8JsonReader`/`JsonDocument`；后续应在此定位下补齐 JSON 的 ASCII whitespace/digit、转义、数值和深度等词法/结构规则。
    验证：新增 65 个有效、无效和深度边界用例；四个目标框架均有相同的 10 个失败，分别覆盖 5 种 JSON 不允许的 Unicode whitespace 和 5 种非 ASCII 数字位置。失败 reproducer 按真实 defect 保留，其余结构、转义、数值及深度用例通过。

161. **[P1][安全/可靠性][已修复] 递归 `JsonValidator` 没有最大深度，攻击者可触发进程级栈溢出**

    位置：`src/FclEx.Core/FclEx/Utils/~Text/~Json/JsonValidator.cs:13-151`。对象和数组递归调用 `ParseValue`，没有深度预算；足够深的外部输入可触发不可正常捕获的 `StackOverflowException`。应使用带 `MaxDepth` 的官方解析器，或改为显式栈并要求调用方配置上限。该问题是解析器整体设计风险，不只是一个边界判断。
    修复：新增 `MaxDepth = 64`，只为现有递归传递和检查 object/array 嵌套深度，不改变其他校验规则。测试覆盖 array/object 的第 64 层成功和第 65 层失败，并在四个目标框架通过。

162. **[P1][安全/API][文档已说明] `TypeJsonConverter` 允许 JSON 任意解析 assembly-qualified type name**

    位置：`src/FclEx.Core/System/Text/Json/Serialization/TypeJsonConverter.cs:3-35`。读取路径调用 `Type.GetType(typeName, true, true)`，把不受信任字符串直接交给运行时类型/程序集解析；这扩大程序集加载面，也把持久化格式绑死在程序集名、版本和忽略大小写匹配上。公共 converter 应要求 allowlist/binder 或稳定的逻辑 type id；不应提供“任意 CLR Type”作为安全无感知的默认格式。
    处理决定：保留当前行为，并在 XML 文档中明确 assembly-qualified wire format 的版本耦合、任意类型解析/程序集加载风险、仅用于可信 JSON，以及不可信输入应改用 allowlist 和稳定逻辑名称。

163. **[P1][已修复] `FileSystemInfoJsonConverter.CanConvert` 与 `CreateConverter` 的支持集合不一致**

    位置：`src/FclEx.Core/System/Text/Json/Serialization/FileSystemInfoJsonConverter.cs:7-21`。`CanConvert` 对所有 `FileSystemInfo` 派生类返回 true，工厂却只接受精确的 `FileInfo` 和 `DirectoryInfo`，导致 serializer 选择该 factory 后再抛 `NotSupportedException`。应把 `CanConvert` 限制为两个精确类型，或真正为派生类型创建兼容 converter。
    修复：`CanConvert` 仅对精确的 `FileInfo`/`DirectoryInfo` 返回 true；测试补充验证 `FileSystemInfo` 基类返回 false。

164. **[P1][保留] `IgnoreJsonConverterImpl<T>` 在根值写入时不输出任何 JSON token**

    位置：`src/FclEx.Core/System/Text/Json/Serialization/IgnoreJsonConverter.cs:23-39`。`writer.CurrentDepth == 0` 时直接返回，根对象序列化会产生空输出而不是一个合法 JSON value；converter 的 `Write` 必须恰好写一个值。无论层级都应写 `null`，是否忽略属性应由 `JsonIgnoreAttribute`/ignore condition 决定，而不应靠 writer depth 猜测。
    处理决定：根值产生空 payload 是该 placeholder converter 的预期语义，由 `Serialize_AsRootValue_ShouldWriteEmptyPayload` 锁定；嵌套值仍写 `null`。XML 文档已明确两种行为。

165. **[P2][命名/API][已修复] `JsonNode.GetOrAdd<TNode>` 会静默覆盖类型不同的已有属性**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Text/~Json/JsonNodeExtensions.cs:19-30`。已有 key 的节点不是 `TNode` 时，方法创建新节点并赋值，原 JSON 子树被无提示丢弃；这违反 `GetOrAdd` 通常“不覆盖已有值”的语义。类型不匹配应抛清楚的异常，或者把有意替换的 API 命名为 `GetOrReplace`/`Set`。
    修复：已有非 null 节点类型不匹配时抛 `InvalidOperationException`，保留原节点且不调用 creator；缺失或 JSON null 仍创建新节点。XML 文档和测试同步覆盖这三种路径。

166. **[P1][生命周期][已修复] 修复 `ReflectionHelper` 后，其他全局 `Type` cache 仍会固定 collectible assembly**

    位置包括 `src/FclEx.Core/FclEx/Extensions/~System/TypeExtensions.TypeInfoEx.cs:5-20`、`TypeExtensions.Member.cs:214-230`、`src/FclEx.Core/FclEx/Helpers/UnsafeHelper.cs:6-8`、`TaskHelper.cs:206-208`、`System/Xml/XmlHelper.cs:8` 等。多个静态 `ConcurrentDictionary<Type,...>` 仍强引用插件/脚本上下文中的 Type 及反射产物，因此问题 139 的根因只在一个入口被修复。应系统盘点 Type-keyed cache，统一改为弱键、closed-generic cache 或显式限定“不支持 collectible assembly”，而不是逐个漏修。
    修复：所有 Type-keyed cache 现在以 `Type` 为 `ConditionalWeakTable` 的外层弱 key；需要第二维 key 的 cache 在 value 中使用普通并发字典。复查发现第一版把临时 `(Type, string)` `Tuple` 本身用作弱 key，虽然不再泄漏 Type，却会让 key 在调用后立即失去强引用、缓存形同失效；现已改为稳定的两层结构。测试在强制 GC 后验证 `LambdaHelper` 仍复用同一类型/属性的缓存条目。

167. **[P2][设计/命名][已删除] `Type.IsDynamic()` 无法从运行时 `Type` 回答它声称的问题**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/TypeExtensions.cs:183-192`。`dynamic` 是使用点上的编译期元数据，运行时与 `object` 是同一个 `Type`；仅检查 Type 本身的 `DynamicAttribute` 既不能区分 `typeof(object)`，也覆盖不了成员、参数和嵌套 generic type-use。应删除该扩展，或把输入改成 `ParameterInfo`/member+attribute context 并按真实能力重新命名。
    处理：已删除该扩展；源码和测试中均无残留调用。

168. **[P2][API][已修复] `EnumerableElementType` 对实现多个 `IEnumerable<T>` 的类型返回任意一个 T**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/TypeExtensions.TypeInfoEx.cs:92-110,251-253`。实现使用 `FirstOrDefault`，但一个类型可通过多个接口暴露不同的 `IEnumerable<T>`；反射接口顺序不是这个 singular API 的选择契约。应返回全部候选、在歧义时抛异常，或要求调用方指定目标 enumerable interface。
    修复：新增 `EnumerableElementTypes()` 返回全部候选；singular `EnumerableElementType()` 在多个候选时抛 `AmbiguousMatchException`。测试覆盖同时实现 `IEnumerable<int>` 与 `IEnumerable<string>` 的接口。

169. **[P2][命名/正确性][已修复] `ShortName`/`LongName` 会重复打印嵌套泛型的外层参数**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/TypeExtensions.TypeInfoEx.cs:124-167`。内层类型使用全部 `GenericTypeArguments`，同时 prefix 又格式化 declaring type；对 `Outer<T>.Inner<U>` 会把外层参数在 inner 部分再次输出。应按当前类型名反引号后的自身 arity 只消费新增参数，并补充 open generic、嵌套多层和数组组合测试。
    修复：按每一级 metadata arity 分配泛型参数，`ShortName` 只格式化当前类型声明的参数，`LongName` 逐级格式化 declaring type；同时补齐数组 shape。开放泛型测试还发现泛型参数与 declaring type 会经 `TypeInfoEx` cache 相互递归并最终栈溢出，现改为在泛型签名中直接使用参数 metadata name。测试覆盖闭合/开放泛型、多层嵌套、仅外层泛型、框架嵌套类型及二维数组。

170. **[P2][设计/API][已修复] 公共 positional `record TypeInfoEx` 可以被构造或 `with` 成自相矛盾的元数据**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/TypeExtensions.TypeInfoEx.cs:336-372`。调用方可任意构造该 record；更严重的是 `with { EnumerableElementType = ... }` 只改变 primary property，不会重新执行 `IsEnumerable`/`IsNumeric` 的属性初始化器，产生互相矛盾的值。它应是由 `Type` 唯一推导的不可伪造对象：使用 sealed immutable class/internal constructor，或把派生属性改为实时计算。
    修复：改为 sealed class，构造函数只接收 `Type` 并一次性推导全部 public readonly 字段，不能再通过 object initializer 或 `with` 制造矛盾状态；测试验证全部公共实例字段 readonly 且类型 sealed。

171. **[P2][命名/API][已修复] `IsFloatingPoint` 把 `decimal` 算作浮点，`IsNumeric` 又漏掉新数值类型**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/TypeExtensions.TypeInfoEx.cs:170-190,295-323,347-372`。这与 .NET 的数值分类不一致：`decimal` 不是 binary floating point，而 `Half`、`BigInteger`、`Int128`/`UInt128` 等在可用目标上又被排除。应把名字限定到明确列举的 primitive 集合，或按 generic math 接口建立可解释的分类；文档不能把自定义集合称作通用 numeric/floating-point 判断。
    修复：`IsFloatingPoint` 只包含 `float`、`double` 和可用目标上的 `Half`；`IsNumeric` 另包含 `decimal`、`BigInteger` 和扩展后的整数集合。检查时补上了遗漏的 `decimal?`/`BigInteger?`，并同步修正文档及 nullable 测试。

172. **[P1][已修复] `ExpressionHelper.GetMember(expression, type)` 错误拒绝接口成员**

    位置：`src/FclEx.Core/FclEx/Helpers/ExpressionHelper.cs:69-83`。校验使用 `type.IsSubclassOf(reflectedType)`；类实现接口并不属于 `IsSubclassOf(interface)`，因此合法的接口成员 selector 会被判为“not from type”。应使用 `reflectedType.IsAssignableFrom(type)`，并明确选择 `DeclaringType` 还是 `ReflectedType` 作为契约。
    修复：检查时发现初次修改把 `IsAssignableFrom` 方向写反，现已改为 `reflectedType.IsAssignableFrom(type)`；测试覆盖实现类型通过转换选择接口属性。

173. **[P2][API][已修复] `GetDataMembers<T>` 没有确认返回的 member 是 T 的直接成员**

    位置：`src/FclEx.Core/FclEx/Helpers/ExpressionHelper.cs:209-236`。该入口允许 `x => x.Child.Name`，甚至捕获对象或静态对象的 member，并直接返回最末端字段/属性；这与方法名和其他 `GetDataMemberInfo` 的“禁止 nested”规则不一致。应验证表达式根是 selector 参数且只允许一级访问，或把方法明确命名为路径提取并返回完整 member path。
    修复：保留单个直接成员和 `new { x.A, x.B }` 多成员形式；每个成员都必须直接以 selector 参数为接收者，仅允许中间存在类型转换。测试覆盖多成员、nested member、匿名对象中的 nested member、captured member 以及接口转换后的直接成员。

174. **[P2][命名/副作用][已修复] `ExpressionExtensions.GetArgumentValues` 实际会编译并执行任意表达式**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Linq/~Expressions/ExpressionExtensions.cs:56-65`。名字像读取常量，实现在非 constant 情况下会 `Compile().Invoke()`，因此可能执行方法调用、产生副作用或异常，并对未绑定参数失败。应只支持 constant/closure field 读取，或改名 `EvaluateArguments` 并明确这是执行代码的 API。
    修复：`Evaluate` 比原名准确，但作为 `IEnumerable<Expression>` 扩展仍过宽，最终命名为 `EvaluateArguments`，同步两个调用点；XML 文档明确延迟枚举会编译执行表达式、可能有副作用或抛异常。测试覆盖 constant 与计算表达式。

175. **[P2][命名/签名][设计确认] `TryGetSingleNonNull` 在两个输入都为 null 时抛异常，违反 Try-pattern**

    位置：`src/FclEx.Core/FclEx/Check.cs:176-220`。文档一方面说“otherwise false”，另一方面又为 `(null, null)` 抛 `ArgumentNullException`；调用方不能把 `Try...` 当作完整的非抛分支。应在两个不满足“恰好一个”的情况都返回 false，或改名 `GetSingleNonNull` 并用显式结果/异常区分零个与两个。
    设计确认：把双 null 视为调用前置条件错误，因此保留异常；单 null 返回 true 和非 null `result`，双非 null 返回 false。当前 `[NotNullWhen(true)] out result` 与两个输入上的 `[NotNullWhen(false)]` 已精确表达两个正常返回分支，是该语义下最充分的 NRT flow contract。新增行为及编译期流分析用例，并修正 `ArgumentNullException.ParamName`。

176. **[P1][未修复] `ActionExtensions.Chain<T>` 对 reference type 连非空 action 序列也无法构造**

    位置：`src/FclEx.Core/FclEx/Actions/ActionExtensions.cs:398-408`、`SuccessAction.cs:23-38`。`Aggregate` 的 seed 是 `new SuccessAction<T>(default!)`，而 `SuccessAction<T>` 明确拒绝 null；因此 `T` 为 reference type 时在枚举 actions 前就抛异常。应要求序列非空并以第一个 action 为 seed，或用 `Unit`/`Optional<T>` 表达空 chain，不能靠违反成功结果非 null invariant 的伪值。
    复查：改为 `SuccessAction<T>.Default` 只把 null 检查从构造器延后到执行期；`OperationResult<T>` 同样拒绝成功 null，因此 non-empty `IAction<string>` chain 仍会在第一个真实 action 前抛 `ArgumentNullException`。已保留 reference type chain 的 failing reproducer；需决定改为拒绝空序列并以首个 action 为 seed，还是重新设计空 chain 的返回类型/语义。

177. **[P2][已修复] `ThenWithAction` 只在成功路径累计两个 action 的 elapsed**

    位置：`src/FclEx.Core/FclEx/Actions/ThenWithAction.cs:25-42`。next 成功时返回 `result.Elapsed + nextResult.Elapsed`，next 失败时直接 cast 后者，丢失第一阶段时间；同一组合操作的耗时含义随结果状态变化。失败路径也应附加前序耗时，或统一规定 elapsed 是单阶段还是端到端并让所有 combinator 遵守。
    修复：next failure 路径也以 `AddElapsed(result.Elapsed)` 累计前序耗时。测试覆盖三个串联 action 的最终失败，验证返回 error 且 elapsed 为三段耗时之和。

178. **[P1][签名][已修复] action retry API 不验证 `retryCount`，并在 `int.MaxValue` 时溢出**

    位置：`src/FclEx.Core/FclEx/Actions/ActionExtensions.cs:441-479`。`Math.Max(1, retryCount + 1)` 会把负数静默当成一次执行；`int.MaxValue + 1` 溢出后也意外变成一次。应明确要求 `retryCount >= 0` 并抛 `ArgumentOutOfRangeException`，循环条件直接按“首次 + 最多 N 次重试”表达，避免加一溢出。
    修复：负数现在抛 `ArgumentOutOfRangeException`；循环以从 0 开始的 attempt 与 `retryCount` 比较，不再计算可能溢出的 `retryCount + 1`。即使 `retryCount == int.MaxValue`，最后一次会在自增前返回。测试覆盖负数输入。

179. **[P2][已修复] retry loop 在最后一次失败后仍调用 delay provider 并等待**

    位置：`src/FclEx.Core/FclEx/Actions/ActionExtensions.cs:483-504`。即使 `i == executeCount`、已经不会再执行下一次，代码仍调用 `sleepDurationProvider(i)` 并 `Task.Delay`，让最终失败无意义地延迟返回，也可能在操作已结束时转成 cancellation。只有确实存在下一次尝试时才应计算和等待 backoff。
    修复：最后一次失败先返回，只有还会重试时才调用 condition 和 delay provider；delay provider 仍以 1 作为第一次 retry 的索引。测试覆盖 `retryCount = 0` 的失败路径，验证 provider 不被调用。

180. **[P1][不成立][设计确认] `IAction<T>` 的 result-based 错误模型没有封闭异常边界**

    位置：`src/FclEx.Core/FclEx/Actions/IAction.cs:3-10`、`OperationAction.cs:3-24` 以及各 action combinator。公共契约返回 `OperationResult<T>`，但 `OperationAction` 原样调用任意 delegate，combinator 也直接 await；同步抛出或 faulted task 会越过 `OperationResult`，使相同 pipeline 有两套失败通道。应在唯一执行边界统一捕获/规范化异常，或明确 IAction 本来就允许 throw 并重新评估 result wrapper 的职责。
    复审结论：不成立。`OperationAction<T>` 的职责是忠实执行一个已经返回 `Task<OperationResult<T>>` 的委托；`Operation.Action(...)` 工厂才负责通过 `Operation.ExecuteAsync(...)` 将普通 value/task delegate 的异常转换为 result。直接构造 `OperationAction<T>` 或自行实现 `IAction<T>` 的代码需遵守该委托/result 契约，抛出的异常按普通 .NET async 调用传播。因而 result 是受控 operation 工厂提供的失败通道，不是要求每个 `IAction<T>` 实现都吞掉异常的封闭异常模型。

181. **[P2][异步设计] `Operation.ExecuteAsync(Func<Task...>)` 又把 naturally-async delegate 包进 `Task.Run`**

    位置：`src/FclEx.Core/FclEx/Utils/~Operation/Operation.Async.cs:35-48,95-121`。`Task.Run(action, token)` 增加一次 thread-pool 调度，并改变 delegate 启动所在的 execution/synchronization context；它并不会让异步 I/O 更异步。应直接调用并 await delegate，再在外层应用 timeout；只有同步 overload 才需要明确的 thread-pool offload。

182. **[P1][签名] `Queryable.OrderByIf` 在 condition=false 时伪造 `IOrderedQueryable<T>`**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Linq/QueryableExtensions.OrderBy.cs:20-31`。false 分支把任意 `IQueryable<T>` 强转成 `IOrderedQueryable<T>`；普通 provider 可立即抛 `InvalidCastException`，即便碰巧实现接口，也没有真实排序可供后续 `ThenBy` 追加。返回类型应是 `IQueryable<T>`，或 API 必须接收已经排序的 source/使用可选 ordering composer。

183. **[P1][取消设计] `ToOperationIOPairs` 取消后仍遍历并物化整个剩余 source**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.IOPair.cs:28-99`。selector 不接收 token；检测取消后，batch 版本和 serial 版本继续枚举每个剩余元素并记录 cancellation，长序列代价巨大，无限序列永不返回。应提供 token-aware selector，把 token 传播给实际任务，并在取消时停止枚举；若业务确实需要为剩余输入生成结果，必须要求有限、可计数输入并明确命名。

184. **[P2][命名] `TryGetFirstOfDiffSet` 返回的是有方向的 `right \ left`，不是通常意义的 diff set**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.Get.cs:5-26`。结果还取决于 right 的枚举顺序；名称既没表达方向，也容易被理解为对称差。应改名 `TryGetFirstExcept`/`TryGetFirstMissingFrom` 并明确 comparer，或真正实现并返回对称差集合。

185. **[P2][命名/API] `IEnumerable<string>.ContainsAny/ContainsAll` 实际做元素与 substring 的两层包含**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/EnumerableExtensions.String.cs:5-18`。集合 API 的 `ContainsAny` 通常表示元素相等，这里却调用 `m.Contains(n, comparison)`，语义是“任一/全部元素包含某些 substring”，而名称未透露方向；同时会反复枚举 values。应使用完整描述性名称，例如 `AnyElementContainsAnySubstring`，并按需要一次物化 patterns。

186. **[P1][路径安全] `DirectoryInfo.Rename` 允许 rooted/path-containing name 逃离父目录**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/DirectoryInfoExtensions.cs:105-128`。该方法只检查非空，然后 `Path.Combine(parent, name)`；绝对路径或包含目录分隔符的 name 可以把“重命名”变成移动到任意位置。应复用已经用于直接子项的名称校验，仅允许单个文件名；如果要支持移动，应提供另一个明确命名和授权边界的 API。

187. **[P1][多目标兼容] 旧目标 `File.WriteAllTextAsync` 默认 overload 会写 UTF-8 BOM**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~IO/FileExtensions.cs:9-28`。回填 overload 把默认编码传为 `Encoding.UTF8`；在 .NET Framework 上该实例会发出 BOM，而现代官方 `File.WriteAllTextAsync(path, contents, token)` 使用无 BOM UTF-8。相同源码跨 TFM 会生成不同字节。应使用 `new UTF8Encoding(false)` 并用字节级测试锁定官方行为。

188. **[P2][包边界/解析] `CookieHelper` 把 HTTP cookie 语义留在 Core，且会丢弃含 `=` 的合法值**

    位置：`src/FclEx.Core/FclEx/Helpers/CookieHelper.cs:3-10`。按 `;` 后再对整段 `Split('=')`，base64/padding 等常见值因产生多个片段被静默忽略；同时 API 未说明解析的是 `Cookie` 还是 `Set-Cookie`，两者语法不同。HTTP 集成应移到 `FclEx.Http`，使用对应 header parser；若只保留简单 pair parser，至少按第一个 `=` 分割并用准确名称/documentation。

189. **[P1][兼容设计] `DelegateHelper` 把 runtime 私有 `AssemblyGen.DefineDelegateType` 当成公共依赖**

    位置：`src/FclEx.Core/FclEx/Helpers/DelegateHelper.cs:3-22`。类型初始化通过反射查找 `System.Linq.Expressions.Compiler.AssemblyGen` 私有成员，runtime 更新、裁剪或 AOT 都可能让整个 helper 以 `TypeInitializationException` 失效。应使用 `Expression.GetDelegateType` 等公共 API，或在确有必要时自行管理 `Reflection.Emit`，不能把实现细节暴露为库的稳定能力。

190. **[P2][命名/API] `Dictionary.Get` 把“key 存在且 value 为 null”当成 key 不存在**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/DictionaryExtensions.cs:5-28`。`TryGetValue` 成功后还要求 `value is not null` 才返回，因而会调用 fallback factory；这抹掉了 dictionary 对 present-null 与 absent 的重要区分，而普通名字 `Get` 没有提示。应只根据 `TryGetValue` 判断存在，或改名 `GetNonNullOrDefault` 并统一 selector overload 的契约。

191. **[P2][命名/所有权] `AsReadOnlyDictionary` 经常原样返回可变 `Dictionary`**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/DictionaryExtensions.cs:96-104`。`Dictionary<TKey,TValue>` 本身实现 `IReadOnlyDictionary`，因此分支直接返回原对象，调用方可向下转换并修改；名称容易被理解为获得只读 wrapper。若目标是阻止通过返回值修改，应始终包装可变 `IDictionary`；若只是接口视图，应改名 `AsReadOnlyDictionaryView` 并明确不提供 immutability。

192. **[P2][运算符设计] `LinkedList` 的 `+` 同时存在纯函数和原地修改两套所有权语义**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/LinkedListExtensions.cs:7-65`。`list + list` 返回新实例，而 `list + item`、`list + IEnumerable` 及反向形式修改某个 operand 并返回同一实例；这与已修复的问题 79 是同类公共 API 不一致。`+` 应全部为纯运算，原地修改只由 `+=`/`Add`/`AddRange` 表达，或删除这些非直观 operator。

193. **[P2][命名/求值时机] `Queue.Dequeue(chunkSize)` 返回 deferred enumerable，调用时并不 dequeue**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Generic/QueueExtensions.cs:14-22`。只有枚举结果时才逐项删除，部分枚举只删除一部分，重复枚举还会继续删除后续项；负数又静默得到空序列。具有破坏性动作的 `Dequeue` 应立即执行并返回 array/list，同时验证 `chunkSize >= 0`；否则必须用 `EnumerateAndDequeueUpTo` 一类名称显式暴露 deferred side effect。

194. **[P3][签名] `ConcurrentDictionary.Remove` 丢弃了原子移除结果**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/~Collections/~Concurrent/ConcurrentDictionaryExtensions.cs:3-8`。扩展返回 `void`，内部忽略 `TryRemove` 的 bool 和 value；调用方无法知道 key 是否存在，也容易把它误认成常见 `Remove` 契约。该薄封装应删除并直接使用 `TryRemove`，或至少返回 bool/被移除值。

195. **[P1][并发/生命周期] `Timer<T>` 和 `StatelessTimer` 的 disposed 状态有数据竞争，也不表达 callback 仍可能运行**

    位置：`src/FclEx.Core/System/Threading/Timer.cs:5-24`、`StatelessTimer.cs:3-22`。`_timer` 的读写未同步，多个线程可同时看到非 null 并重复执行释放，`Available` 也不是可靠状态；底层 `Timer.Dispose()` 返回时已有 callback 仍可能运行，但 API 没有等待或说明。应以 `Interlocked.Exchange` 完成一次性 ownership 转移，并在需要强停止保证时提供等待 callback 的 async dispose/显式文档。

196. **[P2] `ReadOnlyList<T>.ToString` 跳过 null 元素，却按原索引决定分隔符**

    位置：`src/FclEx.Core/System/Collections/Generic/ReadOnlyList.cs:17-31`。例如 `[a, null]` 输出 `[a, ]`，中间或开头 null 又产生不同形态；字符串不再可预测地表示元素数量。应明确把 null 输出为 `null`/空标记，或先过滤后 `Join`，不能在保留原 `isLast` 的同时跳过元素。

197. **[P2][设计] `SocketEndpoint` 允许构造长期无效状态，默认值也必然无效**

    位置：`src/FclEx.Core/System/Net/SocketEndpoint.cs:3-18`。positional record struct 接受 null/空 Host 和任意 int Port，隐式转换到 `DnsEndPoint` 才延迟抛异常；`default(SocketEndpoint)` 也是公开可产生的无效值。应通过验证构造器/factory 建立 invariant，移除掩盖失败点的隐式转换，或直接使用已有 `DnsEndPoint`/`IPEndPoint`。

198. **[P1][并发设计] `LockHelper.DoubleCheckAndDo` 无法为任意 condition 提供正确的 double-checked locking**

    位置：`src/FclEx.Core/FclEx/Helpers/LockHelper.cs:3-18`。第一次读取发生在 lock 外，helper 对 condition 所读状态没有 volatile/内存顺序契约；把任意 delegate 包起来并不能保证发布与可见性，反而鼓励调用方写出看似安全的竞态代码。状态、同步原语和初始化动作应由同一类型共同拥有；通用 helper 应删除，或至少只保留一次受锁检查而不声称 double-check 正确性。

199. **[P2][命名/时间语义] `DateTime.ToUtc` 对 `Unspecified` 只改 Kind，不做时区转换**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/DateTimeExtensions.cs:179-188`。`SpecifyKind(Utc)` 保留 ticks，含义是“假定该墙上时间原本就是 UTC”；这与方法名暗示的“转换到 UTC”不同，并可能造成数小时偏差。该行为应命名为 `AssumeUtc`；真正转换必须要求/使用明确 `TimeZoneInfo`，不能替调用方猜测 unspecified 的来源时区。

200. **[P2][命名/契约] `String.Truncate(maxLength)` 默认会返回超过 maxLength 的字符串**

    位置：`src/FclEx.Core/FclEx/Extensions/~System/StringExtensions.cs:37-49`。截取 `maxLength` 后再追加 `...`，最终长度是 `maxLength + 3`；参数和常见 truncate 语义通常把 max 当作最终输出上限。应把省略号计入预算（过小上限也要定义），或把参数改名 `maxContentLength` 并在文档明确输出还会增长。

## 建议处理顺序

1. 先处理可能违反内存、线程和安全基础契约的 112、114、129、135、140、141、148、149。
2. 再确定需要重塑或删除的公共设计：102–104、109、115–124、128、134、139、142、147；破坏性升级不应阻止合理设计。
3. 随后修复解析/校验、跨目标兼容和异常语义问题，并为每项增加最窄范围的回归测试。
4. 解决条目后继续在对应标题增加 `[已修复]`（或明确的保留决定），并在正文追加处理说明；历史条目不因已修复而删除。
5. 对 151–200，优先处理会破坏资源、哈希、枚举、解析安全、路径边界和异常模型的 151–164、166、172、176、178、180、182、183、186、187、189、195、198；再统一决定 Type 元数据、Action pipeline、集合 operator 与命名 API 的破坏性重塑方向。
