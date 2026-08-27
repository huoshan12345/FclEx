# FclEx.Dapper 源码审查报告（2026-08-22）

## 范围与结论

本轮审查了 `src/FclEx.Dapper` 的全部生产代码、项目文件和包级 README，并对照 `test/FclEx.Dapper.Tests` 检查了现有覆盖。`issues` 目录此前只有 FclEx.Core 的审查记录，没有可复用或需要避开的 FclEx.Dapper issue。

`FclEx.Dapper` 的定位是轻量 Dapper/ADO.NET 扩展，重点封装重复的 CRUD 操作，并缓存实体映射生成的 SQL。实体元数据只应覆盖现有操作需要的表、列、键和值生成信息。Dapper 全局配置、多连接事务协调和 ORM 式数据模型不应进入核心 CRUD 路径。后续改进以包级 [Design Principles](../src/FclEx.Dapper/DESIGN.md) 为边界。

本轮初次登记 45 项。2026-08-24 按包级设计原则复核后移除 1 项缺少当前支持目标依据的 Native AOT 议题，保留 44 项。为避免已有讨论和处理记录错指问题，保留原 issue 编号。优先级含义：P0 为可能破坏数据一致性的设计；P1 为高概率错误、资源泄漏或主要公共契约缺陷；P2 为中等风险 API/兼容性问题；P3 为命名、文档与可维护性问题。

初次验证状态：执行 `dotnet build src/FclEx.Dapper/FclEx.Dapper.csproj -c Release --no-restore`，`netstandard2.0`、`net472`、`net8.0`、`net9.0`、`net10.0` 全部构建成功，0 warning、0 error。初次审查以源码和契约为主，当时未运行依赖外部数据库的完整测试集，也未修改生产代码。

后续处理（2026-08-23）：issue 1、2 已按下述记录修改，同时解决了与 issue 2 重叠的 issue 6、37。生产项目五个目标框架和测试项目四个目标框架均构建成功；不依赖外部数据库的 `FclExDapperConfigurationTests`、`DbConnectionExtensionsApiTests`、`SqliteMigrationTests` 在 `net472`、`net8.0`、`net9.0`、`net10.0` 共运行 36 个测试实例，全部通过。外部 provider 测试仍未运行。

后续处理（2026-08-24）：新增独立 `IEntityMappingSource` 契约并让 CRUD SQL、SQL 缓存和表/列解析共用 `EntityMapping`，解决 issue 3 以及重叠的 issue 19、21、26、27、28、29、30、38。`EntityDefinition`、`FieldDefinition` 和 `GetEntityDefinition` 已删除，属于有意的 breaking change。生产项目五个目标框架构建成功；当时的 `EntityMappingTests`、`FclExDapperConfigurationTests`、`SqliteMigrationTests` 在四个测试目标框架共运行 48 个测试实例，全部通过。外部 provider 测试仍未运行。

架构收敛（2026-08-24）：删除 `FclExDapperConfigurationBuilder`、`FclExDapperRegistration`、`DapperRegistrationConflictBehavior` 及全部 FclEx-owned `SqlMapper.SetTypeMap` 状态，测试 fixture 不再扫描程序集或安装全局映射。生产与测试项目的所有目标框架构建通过，0 warning、0 error；`EntityMappingTests` 和 `SqliteMigrationTests` 共 24 个测试实例通过，raw Dapper 无全局映射回归用例在 `net10.0` 运行 24 个实例通过。完整外部 provider 测试运行 492 个实例，其中 65 个因既有异常文本断言、跨目标共享数据库的主键冲突及 MySQL 连接上限失败，本次未修改这些独立问题。

## 问题清单：整体设计、职责与生命周期

1. **[P0][已修复 2026-08-23] 多连接 `DoTransactionAsync` 暗示原子事务，但顺序提交必然允许部分提交。**
   - 位置：`FclEx/Dapper/~Extensions/DbConnectionExtensions.Dapper.cs:44-61`。
   - 说明：多个本地事务先同时创建，随后逐个 `CommitAsync`。如果第一个提交成功、第二个提交失败，前者已经无法回滚；后续对全部事务调用 rollback 不能恢复原子性。方法名称和单连接重载让调用方很容易把它当作跨连接事务边界。
   - 建议：删除该重载，或改名为明确的 best-effort coordination API 并返回每个连接的提交结果。真正的原子需求应使用 `TransactionScope`/provider 支持的分布式事务，或由业务实现 outbox/saga；不能用顺序 commit 模拟。
   - 处理：已直接删除 `IReadOnlyList<DbConnection>` 多连接重载，保留两个单连接本地事务重载。该修改会使现有多连接调用无法编译，属于有意的 breaking change；未用 `TransactionScope` 制造 provider 均支持分布式事务的错误承诺。

2. **[P1][已修复 2026-08-23] 包初始化会静默修改进程级 Dapper 状态，生命周期和所有权不可控。**
   - 位置：`DapperHelper.cs:9-12,47-57,88-101`。
   - 说明：首次触碰 `DapperHelper` 就注册全局 `GuidTypeHandler`、扫描程序集并调用 `SqlMapper.SetTypeMap`；这可能覆盖宿主已注册的 type map/handler，也没有撤销或冲突检测。一个工具方法不应隐式重配整个进程的 Dapper。
   - 建议：删除核心 CRUD 路径中的隐式全局配置。raw Dapper 如需 type map 或 handler，由应用通过 Dapper API 显式配置，并且不自动扫描或覆盖宿主配置。
   - 处理：已删除静态构造、无参/程序集 `Initialize`、`RegisterColumnMapping` 以及后续过渡期的 `CreateConfiguration` builder/registration。核心包不再扫描 `AppDomain`、注册 `GuidTypeHandler` 或调用 `SqlMapper.SetTypeMap`；raw Dapper 的全局配置由应用负责。

3. **[P1][已修复 2026-08-24] CRUD 映射模型借用了 DataAnnotations，却只实现其中一部分语义。**
   - 位置：`EntityDefinition.cs:23-47`、`FieldDefinition.cs:3-11`。
   - 说明：实现读取 `[Table]`、`[Column]`、`[Key]` 和部分 `[DatabaseGenerated]`，因此 API 看起来遵守 DataAnnotations 映射契约；实际上 schema、`[NotMapped]`、computed、只读/索引器等均未正确处理。消费者无法知道哪些约定可信。
   - 建议：定义只覆盖现有 CRUD SQL 所需元数据的独立映射契约，或明确记录支持的 DataAnnotations 子集；不要引入关系、跟踪等未被当前操作消费的 ORM 元数据。
   - 处理：新增 `IEntityMappingSource`、不可变 `EntityMapping`/`PropertyMapping` 和 `DatabaseValueGeneration`。CRUD 通过 `CommandOptions.EntityMappingSource` 接收自定义 source，默认使用 `DataAnnotationsEntityMappingSource`；后者明确支持 `Table`、`Column`、`Key`、`NotMapped` 和全部 `DatabaseGeneratedOption`，并采用可验证的 persistent scalar property 规则。SQL 缓存按 mapping identity 隔离，自定义 source 必须为同一实体返回稳定映射实例。

4. **[P1][已修复 2026-08-26] `ISqlAdapter` 把 provider 方言简化成少量字符串，无法可靠表达生成键、批量写入和能力差异。**
   - 位置：`SqlAdapters/ISqlAdapter.cs:3-10`。
   - 说明：接口只有引用名称、参数创建、schema 布尔值和一段 `SelectIdentitySql`，但调用方实际需要表达 `RETURNING`/`OUTPUT`、参数上限、默认值插入、identity override、批次大小等能力。当前抽象迫使通用 CRUD 层拼接并不通用的 SQL。
   - 建议：保持 `ISqlAdapter` 轻量，只补充现有 Insert/BulkInsert/Get/Delete 确实需要的能力，例如返回键语法、安全批次大小和 explicit identity 行为；不要扩张成完整 provider 框架。
   - 处理：删除 `SelectIdentitySql`，新增统一服务单行和多行 INSERT 的 `BuildInsertCommandText`、`GetMaxInsertBatchSize` 以及显式 identity scope。通用层继续负责映射、参数、拆批、执行和最终 SQL 缓存；adapter 只表达实际方言差异。

5. **[P1][已修复 2026-08-26] adapter 注册按连接类型的 `FullName` 精确匹配，包装连接、派生连接和同名类型均不可靠。**
   - 位置：`DapperHelper.cs:14-21,104-111`。
   - 说明：连接查找忽略 assembly identity，只比较字符串；代理/重试包装器和 provider 派生类型不会命中，两个程序集中的同名类型又会冲突。这与公开的可扩展 adapter 模型不匹配。
   - 建议：以 `Type` 为键并按可赋值关系解析；无法识别的包装连接由调用方显式传入 adapter，不要把类型身份降级为字符串。
   - 处理：显式注册改为以 `Type` 为键的 `RegisteredAdapters`，解析时优先精确注册，再选择最具体的可赋值注册；多个不可比较注册同时命中时明确报错。内置 provider 以 assembly simple name 和完整类型名共同识别，并沿实际连接的基类链匹配，因此无需引用可选 provider 包也能支持派生连接。组合 wrapper 仍要求显式注册或通过 `CommandOptions.SqlAdapter` 指定。解析结果不另设缓存，注册替换会立即生效。

6. **[P1][已修复 2026-08-23] 自动程序集扫描既脆弱又依赖加载顺序。**
   - 位置：`DapperHelper.cs:60-101`。
   - 说明：静态初始化只扫描当时已加载的程序集，后来加载的插件不会自动映射；`assembly.ExportedTypes` 的异常还可能把 `DapperHelper` 静态构造永久置于失败状态。针对 `Microsoft.TestPlatform.*` 的硬编码跳过进一步说明该模型不稳健。
   - 建议：移除 AppDomain 全扫描，要求调用方显式传入实体类型/程序集；扫描失败应产生可诊断的逐程序集结果，不能从静态构造函数传播。
   - 处理：已移除 AppDomain 全扫描、静态构造路径和后续过渡期的显式程序集扫描 API。

7. **[P2][已修复 2026-08-26] 部分静态 SQL 缓存的键空间无界，可能长期持有动态输入。**
   - 位置：`DapperHelper.cs:22-23,38`、`DbConnectionExtensions.cs:18-22`。
   - 说明：静态缓存符合避免重复创建 SQL 字符串的目标，但表名和 CRUD SQL 缓存会强引用任意 schema 字符串及 adapter 实例；`ParaNames` 还会为每个列名/行号永久增长。动态 schema 和任意批次数量会持续扩大键空间。
   - 建议：继续为稳定的 operation、mapping 和 adapter 组合使用静态缓存；对动态 schema、批次大小等开放输入采用有界缓存或直接生成，并确保 cache key 包含所有影响 SQL 的值。
   - 处理：完整 CRUD SQL 只在 canonical path（未提供调用级 schema/adapter override）进入进程级缓存，key 由稳定的 adapter、mapping、操作选项和受 500 行上限约束的批次行数组成。调用级 override 直接生成 SQL；bulk 在一次调用内以批次形状局部复用，因此重复 full batch 不会重复构造字符串。删除独立表名缓存，参数名改为有界位置 key（column index、row index）；替换已注册 adapter 时移除旧实例对应的 SQL 条目。

8. **[P1][已修复 2026-08-26] 数据库异步 API 没有端到端取消契约。**
   - 位置：`DbConnectionExtensions.cs:37-84,211-245,268-295`、`DbConnectionExtensions.Dapper.cs:5-60`、`DbTransactionExtensions.cs:63-113`。
   - 说明：插入、批量插入、查询、删除、事务回调和 commit/rollback 均不接收 `CancellationToken`；只有底层 `TryOpenAsync` 和 `IDbCommand` 扩展孤立地支持 token，调用者无法取消真实工作。
   - 建议：把 token 放入每个异步公共签名或明确的 command options，并传到 open、execute、commit、rollback 和用户回调；不要只取消连接打开。
   - 处理：`CommandOptions.CancellationToken` 现在统一控制 connection 和 transaction CRUD 的 open 与 command execution。事务 helper 新增接收 `(DbTransaction, CancellationToken)` 的回调 overload，并将 token 传到 open、begin、callback 和 commit；回调完成后再次检查取消以避免在已取消时提交。rollback 在 token 尚可用时接收该 token，取消已经发生时则作为不可取消的 cleanup 执行，防止跳过回滚。explicit identity setup 同样接收 token，而 cleanup 始终执行。

9. **[P1][已修复 2026-08-26] `BulkInsertAsync` 生成一个无限增长的多值 INSERT，没有 provider 批次策略。**
   - 位置：`DbConnectionExtensions.cs:67-117,142-173`。
   - 说明：参数数等于“行数 × 插入列数”，SQL 和参数列表全部一次性分配。SQL Server 的参数容量为 2,100；SQLite 默认 host parameter 上限在新版本为 32,766、旧版本为 999。当前 API 会在正常大集合上突然失败或造成大额分配。
   - 建议：adapter 暴露安全批次大小并分批执行；对真正的高吞吐场景使用 `SqlBulkCopy`、COPY 等 provider 能力。参考：[SQL Server capacity](https://learn.microsoft.com/en-us/sql/sql-server/maximum-capacity-specifications-for-sql-server)、[SQLite limits](https://www.sqlite.org/limits.html)。
   - 处理：`BulkInsertAsync` 现在根据 adapter 的安全批次大小和内部批次上限执行有界多行 INSERT，每批复用按行数缓存的完整 SQL。SQL Server 同时考虑 2,100 参数和 1,000 行 `VALUES` 限制，SQLite 使用保守的 999 参数限制；批量操作不会静默退化成逐行命令。

10. **[P2][已修复 2026-08-26] 连接所有权不一致：方法会隐式打开调用方连接，却从不恢复原状态。**
    - 位置：`DbConnectionExtensions.cs:273-279`、`DbConnectionExtensions.Dapper.cs:5-35,64-69`。
    - 说明：关闭的连接会被自动打开并留在 Open；原本已打开的连接也保持 Open。API 没有说明谁负责关闭，使短生命周期调用泄漏连接，而调用方仅从方法名看不出状态会变化。
    - 建议：记录初始状态，并只关闭由本方法打开的连接；或要求调用方传入已打开连接并在入口验证。两种模型应选一并写入文档。
    - 处理：FclEx 自己打开连接的 Insert、BulkInsert 和 transaction helper 现在记录入口状态，并在 `finally` 中仅把原本为 Closed 的连接恢复为 Closed；原本已打开的连接保持打开。Get/Delete 使用的 Dapper async execution 本身已采用相同的 `wasClosed` 所有权模型。测试覆盖成功、异常和取消路径。

11. **[P3][已修复 2026-08-26] `SqlAdapterBase<TSelf>.Instance` 的命名和具体 adapter 的继承边界不清楚。**
    - 位置：`SqlAdapterBase.cs`、各具体 adapter 类型。
    - 说明：issue 7 后 SQL cache 已统一按 adapter 实例区分，公开 shared instance 和仅用于创建它的 CRTP 泛型、`new()` 约束不再表达必要的设计约束。注册后进入 SQL cache 的 adapter 若改变影响 SQL 生成的状态，仍会得到陈旧 SQL。
    - 处理：删除公开 `Instance`，将基类简化为非泛型 `SqlAdapterBase`，并由 `DapperHelper` 在私有只读映射中持有内置 adapter 实例。自定义 adapter 可直接实例化和注册；`ISqlAdapter` 契约同时明确，注册期间所有影响 SQL 生成的行为必须保持稳定。

## 问题清单：公共 API、签名与命名

13. **[P1][已修复 2026-08-26] `InsertAsync` 返回 `dynamic?`，调用方无法获得编译期生成键类型检查。**
    - 位置：`DbConnectionExtensions.cs:37-54`、`DbTransactionExtensions.cs:63-66`。
    - 说明：调用方只有在运行时才能发现 provider 返回的是 `decimal`、`long` 还是其他类型；注释所谓“先转 dynamic 以便转换”只是把转换失败推迟到调用点。
    - 建议：提供 `InsertAsync<TEntity, TKey>`/`InsertAndGetKeyAsync<TKey>`，由扩展将结果转换为明确的 `TKey`；无返回键的路径不应继续依赖 dynamic 或 scalar result。
    - 处理：connection 和 transaction API 均改为 `InsertAsync<TEntity, TKey>`，返回 `Task<TKey?>`；标量结果在扩展内部按目标键类型完成数值和枚举转换，不再向调用方暴露 `dynamic`。

14. **[P2][已修复 2026-08-26] `returnId` 与 `includeAutoKey` 两个相邻布尔参数形成难读且存在非法组合的 API。**
    - 位置：`DbConnectionExtensions.InsertAsync`、`DbTransactionExtensions.InsertAsync`。
    - 说明：位置调用如 `InsertAsync(entity, schema, false, true)` 无法自解释；`includeAutoKey=true` 时 `returnId=true` 又会被静默忽略。
    - 建议：拆成具名操作（普通 insert、insert explicit identity、insert and return key），或以枚举/options 表达互斥策略并在入口验证。
    - 处理：保留默认返回 `long` 的便利重载和 `InsertAsync<TEntity, TKey>`；`returnId` 重命名为 `returnGeneratedKey`，显式写入数据库生成键的路径拆为 `InsertWithExplicitGeneratedKeysAsync`。connection 和 transaction API 保持对应，不再公开两个可形成非法组合的布尔参数。

15. **[P2][已修复 2026-08-26] `CommandInfo` 名称过于宽泛，且把执行选项固化为公共 positional record。**
    - 位置：`DbConnectionExtensions.cs:13`。
    - 说明：它实际是 FclEx CRUD command options；positional constructor 随属性增加而变得脆弱，`TimeoutSeconds` 不验证负值，`Transaction` 与连接是否匹配也未验证。
    - 建议：采用准确的 options 命名、具名 init properties 和集中验证，避免每个 CRUD 方法重复实现相同约束。
    - 处理：类型重命名为 `CommandOptions`，改为带 init properties 的 `readonly record struct`，并公开 `ValidateFor(DbConnection)`。验证统一拒绝负 timeout、已脱离 connection 的 transaction，以及属于其他 connection 的 transaction；connection CRUD 的共享执行入口和独立 Get/Delete/Bulk 路径调用该方法，transaction 重载只负责构造 options 并转发。

16. **[P2][已修复 2026-08-27] `DoTransactionAsync` 名称不自然，默认 `ReadUncommitted` 又偏离常见安全默认值。**
    - 位置：`DbConnectionExtensions.Dapper.cs:5,25`。
    - 说明：`DoTransactionAsync` 没表达“在事务中执行回调”；保留的两个单连接重载默认 dirty-read 隔离级别，而 `CreateAsyncTransactionScope` 默认 `ReadCommitted`，同一包内部也不一致。
    - 建议：使用 `ExecuteInTransactionAsync`，默认采用 provider/ADO.NET 默认隔离级别或 `ReadCommitted`；非默认隔离必须由调用方显式选择。
    - 处理：四个单连接重载统一重命名为 `ExecuteInTransactionAsync`，默认隔离级别改为 `ReadCommitted`；调用方仍可通过 `level` 显式选择其他隔离级别。

18. **[P2][已修复 2026-08-27] connection 与 transaction CRUD 重载形状重复且已经发生能力漂移。**
    - 位置：`DbConnectionExtensions.cs:37-245`、`DbTransactionExtensions.cs:50-114`。
    - 说明：transaction 重载把 `CommandOptions` 拆成 timeout/adapter 参数；后续增加 cancellation 或其他选项必须维护两套签名，调用体验也不一致。
    - 建议：让 transaction 重载接收同一 options 类型并仅注入 transaction，或抽象一个内部 command context 后由薄重载转发。
    - 处理：全部 transaction CRUD 重载改为接收与 connection CRUD 相同的 `CommandOptions`，不再拆分 timeout、adapter 和 cancellation 参数，并补齐 `EntityMappingSource` 等能力。`CommandOptions.BindTransaction(DbTransaction)` 公开提供经过校验的绑定操作：拒绝 null、已脱离 connection 的 transaction，以及把已绑定 options 改绑到其他 transaction；transaction 重载通过该方法注入 receiver transaction 后转发到 connection 实现。

19. **[P2][已修复 2026-08-24] `ISqlAdapter.EnableIdentityInsertAsync<T>` 把泛型实体、命令和 scope 生命周期混在方言接口中。**
    - 位置：`ISqlAdapter.cs:10`、`SqlAdapterBase.cs:49-51`、`SqlServerAdapter.cs:17-22`。
    - 说明：方法并不操作传入 command，只借其 connection；泛型 `T` 也只是取表名。返回 `IAsyncDisposable` 表示隐式 ON/OFF scope，但失败和 cleanup 异常契约没有表达。
    - 建议：传入明确的 quoted table identifier 和执行 context，或让 adapter 生成 before/after commands；以专门 scope 类型记录 cleanup 行为和异常策略。
    - 处理：移除无意义泛型和 schema/entity lookup；新签名接收已经由 mapping source 解析并引用的完整表名以及 command。返回 scope 的 cleanup 设计仍保留，异常策略可在后续 adapter 方言重构中继续收敛。

20. **[P2][已修复 2026-08-27] `CreateAsyncTransactionScope` 的名称只表达 async flow，却隐藏“使用机器最大超时”的策略。**
    - 位置：`DapperHelper.cs:150-157`。
    - 说明：该方法没有异步工作；`Async` 仅指 `TransactionScopeAsyncFlowOption.Enabled`。它把 timeout 固定成 `TransactionManager.MaximumTimeout`，可能让挂起事务远超调用方预期。
    - 建议：改名为 `CreateTransactionScopeWithAsyncFlow`，接受显式 timeout/option，默认遵循平台默认事务配置。
    - 处理：方法重命名为 `CreateTransactionScope`；`timeout` 改为必填的非空 `TimeSpan`，不再隐式使用 `TransactionManager.MaximumTimeout`。创建的 scope 仍显式启用 async flow，隔离级别默认保持 `ReadCommitted`。

21. **[P2][已修复 2026-08-24] 元数据命名没有准确表达数据库概念。**
    - 位置：`EntityDefinition.cs:15-21,53-62`、`FieldDefinition.cs:3-11`。
    - 说明：`Alias` 实际是 table/column name override，`FieldDefinition` 实际描述 property-to-column mapping，`FieldName` 实际是 column name，`AutoKeys` 实际是 identity keys，`IsGenerated` 却只代表 Identity；`InsertFields` 同时是属性和扩展方法名。
    - 建议：采用 `TableName`/`ColumnName`、`ColumnDefinition`、`IdentityKeys`、`IsIdentity`、`GetInsertableColumns` 等准确术语。
    - 处理：删除旧 metadata 类型；新契约使用 `EntityMapping.TableName/Schema/Properties/Keys/GeneratedKeys/InsertProperties`、`PropertyMapping.ColumnName/ValueGeneration/StoreTypeName` 和 `GetInsertProperties`，不再使用 Alias、Field 或语义不完整的 IsGenerated。

22. **[P3][已修复 2026-08-27] 多个公共参数使用内部式缩写，降低 API 可读性。**
    - 位置：`DbConnectionExtensions.cs` 的 `con`/`paras`，`DbTransactionExtensions.cs` 的 `tran`，`DbConnectionExtensions.Dapper.cs` 的 `con`。
    - 说明：这些名字会进入 IntelliSense 和生成文档，不符合 `connection`、`parameters`、`transaction` 的公共 API 习惯。
    - 建议：完整命名所有公开参数；内部局部变量可另行决定是否精简。
    - 处理：保留内部局部变量命名不变；所有公开 extension 参数中的 `con`、`paras` 和 `tran` 分别改为 `connection`、`parameters` 和 `transaction`。

23. **[P3] 大部分公共 API 没有消费者可用的 XML 文档。**
    - 位置：`src/FclEx.Dapper` 全部公共类型；现有 CRUD 文档的 `<param>`/`<returns>` 基本为空。
    - 说明：生成键类型、连接状态、schema 解释、映射属性、异常、批量限制、全局注册副作用均未说明；当前 0 warning 只是项目没有把缺失文档当作错误。
    - 建议：先确定公共面，再为保留 API 补齐行为、参数、返回、连接所有权、失败模式和 provider 差异；实现细节类型应改为 internal 而不是补表面文档。

24. **[P2][已修复 2026-08-27] `SqlConnectionHelper.ParseEndpoint` 名称和解析能力均不足以表达 SQL Server data source。**
    - 位置：`SqlConnectionHelper.cs:3-11`。
    - 说明：默认 1433 使其实际面向 SQL Server，但只按第一个逗号切分；`tcp:` 前缀、named instance、IPv6、LocalDB 和错误端口均没有明确语义，非法端口还会静默回退到 1433。
    - 建议：若只支持 `host[,port]`，重命名并严格验证；若目标是 SQL Server connection string，则使用 provider 的 connection-string builder/官方解析能力，不自行猜测。
    - 处理：直接删除 `SqlConnectionHelper` 和 `ParseEndpoint`，不再由该通用 Dapper 扩展包维护不完整的 SQL Server data source 解析规则。

## 问题清单：实现正确性与 provider 行为

25. **[P1][已修复 2026-08-27] insert/bulk 内部创建的 `DbCommand` 从未释放。**
    - 位置：`DbConnectionExtensions.cs:273-279`。
    - 说明：`CreateCommand` 后直接 await 执行并返回，没有 `using`/`await using`；command 及其 provider 参数可能持有 native handle、连接引用和缓冲区，异常路径同样泄漏。
    - 建议：在内部执行方法中以跨目标兼容方式保证 command 始终释放，并增加 fake command 回归测试验证成功、open 失败和 execute 失败路径。
    - 处理：bulk 路径和单实体 insert/get/delete 共用的内部 `ExecuteAsync` 均在 command 的完整生命周期使用 `using`；回归测试验证成功、连接打开失败和命令执行失败时都会释放 command。

26. **[P1][已修复 2026-08-24] 表达式版 `GetQuotedColumnName` 对带 `[Column]` 的属性必然按错误名字查找。**
    - 位置：`DapperHelper.cs:130-147`。
    - 说明：表达式得到 CLR property name，随后却在 `Fields` 中用 `FieldName == columnName` 查找；`FieldName` 在有 alias 时是数据库列名。因此 `x => x.Json` 对 `[Column("json_string")]` 会抛“Column 'Json' not found”。
    - 建议：字符串重载明确区分 property name 与 column name；表达式重载按 `PropertyInfo`/property name 找 definition，再返回其 `ColumnName`。
    - 处理：`EntityMapping.FindProperty` 可按 CLR property name 或 database column name 无歧义解析；表达式重载按 property name 获取 `PropertyMapping.ColumnName`。自定义 source + SQLite 测试验证 alias 属性返回正确引用列名。

27. **[P1][已修复 2026-08-24] `[Table(..., Schema=...)]` 的 schema 被忽略。**
    - 位置：`EntityDefinition.cs:25-29`、`DapperHelper.cs:114-127`。
    - 说明：元数据只读取 `TableAttribute.Name`，所有 CRUD 都要求另传 schema；标准属性明确把 `Schema` 定义为映射表的 schema。实体声明与实际 SQL 会分离。
    - 建议：把 schema 纳入 `EntityDefinition`，显式参数只作为有清楚优先级的 override。参考：[TableAttribute.Schema](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.schema.tableattribute.schema)。
    - 处理：`EntityMapping.Schema` 保存声明的 schema；CRUD 的显式非空 schema 参数优先，否则使用 mapping schema；不支持 schema 的 adapter 明确忽略两者。

28. **[P1][已修复 2026-08-24] `[NotMapped]` 和 `DatabaseGeneratedOption.Computed` 仍会进入 INSERT。**
    - 位置：`EntityDefinition.cs:31-46`。
    - 说明：所有属性先加入 `Fields`；只有 Identity 被标记为 generated 并排除。`[NotMapped]` 本应从数据库映射排除，Computed 也不应由普通 insert 写入。
    - 建议：过滤 `[NotMapped]`，完整处理 `DatabaseGeneratedOption.Identity/Computed/None`，并为组合属性增加 SQLite 内存回归测试。参考：[NotMappedAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.schema.notmappedattribute)。
    - 处理：默认 source 排除 `NotMapped`，把 Identity 映射为 `OnInsert`、Computed 映射为 `OnInsertOrUpdate`、None 映射为普通写入；insert property 集合只包含允许应用提供值的属性，显式 identity insert 仅额外加入 generated key。

29. **[P1][已修复 2026-08-24] 元数据把所有 public property 当作可插入列，包括静态、索引器、只读和导航属性。**
    - 位置：`EntityDefinition.cs:31-45`、`DbConnectionExtensions.cs:100-107`。
    - 说明：无参数的 `GetProperties()` 返回 public instance/static 属性并包含索引器；代码没有验证 getter、setter、index parameters 或标量类型。`PropertyInfo.GetValue(item)` 会对索引器失败，导航对象则会被错误创建成参数。
    - 建议：建立明确的 persistent scalar property 筛选规则，排除 static/indexer/non-readable/navigation；允许调用方通过 map 显式包含特殊属性。
    - 处理：DataAnnotations source 只约定 public instance、非索引器、可读写的 scalar 属性；非 scalar 属性必须显式声明映射属性，`NotMapped` 始终优先。`EntityMapping` 自身拒绝静态、索引器及不可读写属性，自定义 source 可显式包含可持久化的特殊 CLR 类型。

30. **[P1][已修复 2026-08-24] `[Column(TypeName=...)]` 被当成 provider enum 名称解析，误用了标准属性语义。**
    - 位置：`EntityDefinition.cs:32-40`、`SqlAdapterBase.cs:70-77`。
    - 说明：`TypeName` 是 provider-specific 数据库类型名，合法值可为 `nvarchar(max)`、`decimal(18,2)` 等；实现却用 `Enum.Parse` 转成 `SqlDbType`/`NpgsqlDbType`/`SqliteType`，很多合法映射会在运行时失败。
    - 建议：不要复用 `ColumnAttribute.TypeName` 传参数 enum；定义 adapter-specific parameter type map，或根据 CLR 值让 provider 推断。参考：[ColumnAttribute.TypeName](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.schema.columnattribute.typename)。
    - 处理：映射契约将其保存为语义准确的 `PropertyMapping.StoreTypeName`，adapter 仅在名称可识别为自身 provider enum 时显式设置参数类型；`varchar(200)` 等非 enum store type 不再抛出 `Enum.Parse` 异常，而是让 provider 根据 CLR 值推断。测试覆盖 SQLite 对未识别 store type 的推断路径。

31. **[P1][已修复 2026-08-26] PostgreSQL 用 `LASTVAL()` 返回生成键，可能得到其他 sequence 的值。**
    - 位置：`SqlAdapters/NpgsqlAdapter.cs:8`、`DbConnectionExtensions.cs:176-194`。
    - 说明：`lastval()` 返回当前 session 最近一次 `nextval` 的值，不绑定当前表或列；INSERT trigger 若调用另一条 sequence，返回值就不是实体主键。
    - 建议：为 PostgreSQL 生成 `INSERT ... RETURNING <quoted key column>`，不要追加 session-global 查询。参考：[PostgreSQL sequence functions](https://www.postgresql.org/docs/current/functions-sequence.html)。
    - 处理：`NpgsqlAdapter.BuildInsertCommandText` 将映射中的 quoted generated-key column 作为 `RETURNING` 子句附加到 INSERT，不再调用 `LASTVAL()`。

32. **[P1][已修复 2026-08-27] 显式插入 identity key 不维护 provider sequence，后续自动键可能冲突。**
    - 位置：`DbConnectionExtensions.cs:40-50,73-83`；测试侧 `DapperTestsFixture.cs:87-102`。
    - 说明：单实体 `InsertWithExplicitGeneratedKeysAsync` 和批量插入的 `includeAutoKey` 路径对非 SQL Server provider 不做序列修正；现有测试不得不在 PostgreSQL 上额外执行 `setval`，说明公共操作没有封装其自身后置条件。
    - 建议：把 explicit identity insert 作为 provider 能力实现并明确 sequence 行为；不支持安全维护的 provider 应拒绝该选项，而不是要求调用方猜测补救步骤。
    - 处理：API 重命名为 `InsertWithExplicitGeneratedKeysAsync`，明确表达只显式写入 database-generated key；单实体和 `BulkInsertAsync(..., includeAutoKey: true)` 的 XML 文档及 README 均声明不会推进或重置 identity/sequence/auto-increment，调用者必须维护该状态并避免后续自动键冲突。

33. **[P1][已修复 2026-08-26] 只有 generated 列的实体会生成无效 INSERT。**
    - 位置：`DbConnectionExtensions.cs:119-173`。
    - 说明：当可插入列为空时，代码生成 `INSERT INTO table () values` 和空参数 tuple；不同 provider 的正确形式通常是 `DEFAULT VALUES` 或专用语法。
    - 建议：adapter 提供 default-row insert 语法；bulk 情况需定义是否支持多行默认值并添加边界测试。
    - 处理：SQL Server、PostgreSQL 和 SQLite 生成 `DEFAULT VALUES`，MySQL adapters 生成 `() VALUES ()`。单实体路径支持返回生成键；多个 default-only 实体的 bulk 路径明确抛出 `NotSupportedException`，不会逐行执行。

34. **[P2][已修复 2026-08-26] 不请求返回键时仍使用 `ExecuteScalarAsync`。**
    - 位置：`DbConnectionExtensions.cs:37-53,176-198`。
    - 说明：不请求生成键或映射没有 generated key 时，SQL 只有 INSERT，却仍走 scalar execute 并返回 null，依赖 provider 对无结果 scalar 的行为。
    - 建议：无返回值路径使用 `ExecuteNonQueryAsync`；单行插入无需公开通常恒为 1 且可能受 provider 配置影响的 affected rows，数据库失败直接抛出异常。
    - 处理：只有实际请求且映射恰好包含一个 generated key 时使用 `ExecuteScalarAsync`；其余单行插入和 `InsertWithExplicitGeneratedKeysAsync` 均使用 `ExecuteNonQueryAsync`，并不公开 affected rows。

35. **[P2] identifier quoting 只包围名称，不转义结束符。**
    - 位置：`SqlAdapterBase.cs:22-38`、`DapperHelper.cs:114-121`。
    - 说明：表/列/schema 中若含 `]`、`"` 或反引号，会生成非法 SQL；schema 又是每次公共调用传入的字符串，若来自外部输入还可能扩大为 SQL 注入边界。
    - 建议：每个 adapter 正确 escape identifier 结束符，并明确 schema/table name 只能来自可信配置；不要把任意用户输入当 identifier。

36. **[P2][已修复 2026-08-26] 表名缓存只按 adapter 类型区分，却在 factory 中捕获具体 adapter 实例。**
    - 位置：`DapperHelper.cs:23,114-122`。
    - 说明：两个同类型但配置不同的自定义 adapter 会共享首个实例生成的引用结果；这与公开允许传入任意 `ISqlAdapter` 实例的 `CommandOptions` 冲突。
    - 建议：若 adapter 实例决定行为，缓存键使用稳定的 adapter identity/configuration key；若类型决定行为，则禁止有状态实例并在接口契约中声明。
    - 处理：删除独立表名缓存；canonical CRUD SQL cache 以 adapter 实例作为 key 的一部分，调用级 adapter override 不进入全局缓存，因此不会跨实例错误复用引用结果或永久保留临时实例。

37. **[P2][已修复 2026-08-23] `_isDapperInitialized` 的 check-then-set 不是线程安全的一次初始化。**
    - 位置：`DapperHelper.cs:7,88-101`。
    - 说明：`volatile` 只保证可见性，两个线程仍可同时看到 false、同时写 true 并重复执行 handler 注册和程序集循环。
    - 建议：使用 `Lazy<T>`、静态构造的单一初始化路径或 `Interlocked.CompareExchange`；显式配置后则可直接删除这组全局状态。
    - 处理：已删除 `_isDapperInitialized` 及所有 FclEx-owned type map 注册和恢复状态。

38. **[P2][已修复 2026-08-24] column mapping 注释声称大小写不敏感，代码却使用大小写敏感比较。**
    - 位置：`DapperHelper.cs:45-56`。
    - 说明：`p.FieldName == name` 是 ordinal case-sensitive；provider 返回不同 casing 时找不到属性，与注释和常见数据库行为不一致。
    - 建议：采用 `StringComparer.OrdinalIgnoreCase`，并在同名不同大小写产生歧义时明确报错；添加 alias 与 casing 组合测试。
    - 处理：`EntityMapping` 以 `StringComparer.OrdinalIgnoreCase` 同时索引 property/column identifiers，构造时拒绝跨属性歧义；CRUD SQL 和 helper 共用该解析，测试覆盖大写 alias 查询。

39. **[P1][已修复 2026-08-27] 单连接事务在 rollback 失败时会丢失原始业务/commit 异常。**
    - 位置：`DbConnectionExtensions.Dapper.cs:12-21,32-40`。
    - 说明：catch 中先 await `TryRollbackAsync`；如果 rollback 自身抛错，后面的 bare `throw` 不会执行，调用方只看到 rollback 异常。
    - 建议：分别捕获原始异常与 rollback 异常；无 rollback 错误时用 `ExceptionDispatchInfo` 保留原异常，有两者时抛包含两者的 `AggregateException`/专用异常。
    - 处理：rollback 成功时继续用 bare `throw` 保留原始异常和堆栈；rollback 也失败时抛出按顺序包含 operation/commit 异常与 rollback 异常的 `AggregateException`，并以 fake transaction 回归测试验证两者均可见。

40. **[P2][已修复 2026-08-27] `IDbCommand.Execute*Async` 对非 `DbCommand` 实现同步阻塞，Async 名称没有真实保证。**
    - 位置：`DbCommandExtensions.cs:5-16`。
    - 说明：fallback 在调用线程直接执行 `ExecuteScalar`/`ExecuteNonQuery`；现在会在同步调用开始前检查 cancellation token，但执行开始后仍无法取消，调用者也无法从 Async 签名判断会阻塞。
    - 建议：把扩展限定为 `DbCommand`；对只有 `IDbCommand` 的实现提供明确命名的同步兼容方法，不要用 `Task.FromResult` 包装同步 I/O 冒充异步。
    - 处理：保留 `IDbCommand` 兼容面，但非 `DbCommand` fallback 改为在线程池执行，不再阻塞调用线程；文档明确只有 `DbCommand` 路径使用 provider 原生异步，fallback 的 cancellation 只能在同步 I/O 开始前生效。回归测试验证调用会先返回未完成的 task，而不是同步等待命令结束。

41. **[P2][已修复 2026-08-27] `DateTimeHandler` 的名称掩盖了“把原 ticks 重新解释为 UTC”的破坏性语义。**
    - 位置：`DateTimeHandler.cs:3-15`、`DapperHelper.cs:95-96`。
    - 说明：`DateTime.SpecifyKind` 不做时区转换，只改 `Kind`；如果 provider 返回 Local 值，代表的 instant 会改变。类型名只说 DateTime handler，未表达 `AssumeUtc`，而该公开类型目前又没有被默认注册，消费者无法判断它是支持能力还是遗留代码。
    - 建议：若确需该策略，重命名为 `AssumeUtcDateTimeTypeHandler` 并严格限定输入 Kind；否则删除。README 应明确默认是否注册以及时间语义。
    - 处理：类型移至 `Dapper` namespace 并重命名为 opt-in 的 `AssumeUtcDateTimeTypeHandler`；Local 值通过 `ToUniversalTime` 保持 instant，Unspecified 值才保留 ticks 并标记为 UTC，README 不再引用旧名称。测试覆盖 Local 转换语义。

42. **[P2][已修复 2026-08-27] `GuidTypeHandler` 把 `null` 映射为 `Guid.Empty`，会混淆缺失值与真实空 GUID。**
    - 位置：`GuidTypeHandler.cs:5-19`。
    - 说明：非 nullable Guid 的数据库 NULL 应是映射错误，而不是合法的全零 GUID；同时 ADO.NET 常用 `DBNull.Value` 表示数据库 NULL，该分支又不会覆盖它，行为不一致。
    - 建议：让 null/`DBNull` 明确失败；nullable Guid 交给 nullable 映射处理。若保留宽松转换，应使用显式命名和 opt-in 注册。
    - 处理：`GuidTypeHandler` 对 null/`DBNull` 明确抛出 `InvalidCastException`；删除冗余的 `NullableGuidTypeHandler`，nullable Guid 继续使用 Dapper 自身的 nullable 映射。Dapper 会把 value-type handler 同时注册给该类型及其 nullable counterpart，因此不提供两套相互覆盖的 handler。

43. **[P2][已修复 2026-08-27] `RegisterSqlAdapter` 实际是无条件全局替换，却没有明确的冲突契约。**
    - 位置：`DapperHelper.cs:104-111`。
    - 说明：同一连接类型的现有 adapter 会被静默覆盖，方法没有返回旧值或注册结果，并发宿主模块无法明确协调注册所有权。替换现在会移除旧 adapter 对应的 CRUD SQL cache，缓存失效已不再是本 issue 的未解决部分。
    - 建议：区分 `TryAddSqlAdapter` 与 `ReplaceSqlAdapter`，或通过返回值明确告知调用方是新增还是替换以及被替换的 adapter。
    - 处理：`RegisterSqlAdapter` 保持无条件新增或替换，并继续清理被替换 adapter 的 SQL cache；新增 `TryRegisterSqlAdapter` 的 Type 与泛型重载，只在 exact connection type 尚未注册时返回 `true` 并添加，冲突时返回 `false` 且保留原 adapter。

## 问题清单：测试与消费者文档

44. **[P2][已修复 2026-08-27] provider 测试通过 early return 伪装成成功，SQLite 主路径基本未执行。**
    - 位置：`DapperTestsFixture.cs:40-57`、`DbConnectionExtensionsTests.CustomDbType.cs:8-13,39-44,69-74,100-105`。
    - 说明：`DbDrivers` 默认不含 SQLite；custom-type 测试在 provider 不可用时直接 `return`，测试报告仍显示 passed。MySQL 测试还只检查 `DbDriver.MySql`，即使 case 是 MySqlConnector 也可能整体退出。
    - 建议：在 MemberData 构造阶段只生成可用 provider case，或使用 xUnit 明确 Skip；利用已加入的 FluentMigrator + SQLite memory fixture 覆盖可移植 CRUD 边界，让“passed”确实表示执行过断言。
    - 处理：不可用的外部 provider case 改为 xUnit 显式 Skip；SQLite BLOB 测试改用 FluentMigrator 在独立的 shared-memory SQLite 数据库中按需建表，并改为普通 `Fact`，不再依赖本地预建数据库。

45. **[P3][已修复 2026-08-27] README 没有说明包最重要的运行时契约和限制。**
    - 位置：`src/FclEx.Dapper/README.md`。
    - 说明：文档只列能力名称，没有 provider 支持/安装方式、adapter 注册、DataAnnotations 子集、单键限制、全局 Dapper mutation、连接所有权、批量上限或 identity 行为；Description 还宣传 type handlers，却未说明 `DateTimeHandler` 并未注册。
    - 建议：整体设计收敛后补一套最小可运行示例和兼容性表，并明确副作用、限制及 provider-specific 行为；根 README、包 README 和项目 Description 同步更新。
    - 处理：包 README 增加安装和最小示例、内置 provider/adapter 表、映射与单键限制、连接及事务所有权、批量与 SQL cache、显式生成键、全局 Dapper 状态和自定义 adapter 契约；项目 Description 同步收敛到 cached CRUD、bounded multi-row insert、local transaction、mapping 与 adapter。按本次明确范围未修改根 README。

## 建议的处理顺序

1. 先决定删除/替换多连接事务（issue 1），并修复 command 释放、rollback 异常和生成键正确性（25、31、39）。
2. 再确定 CRUD 扩展的长期边界：最小且稳定的 metadata contract、SQL 缓存键、adapter 能力和取消/连接生命周期（2–11）。
3. 在该设计上重塑返回键、options、事务及 adapter API（13–24），避免为即将替换的签名补兼容性包袱。
4. 最后处理具体映射/SQL 缺陷和命名文档，并把 SQLite memory 测试扩展为不依赖外部服务的回归层（26–45）。
