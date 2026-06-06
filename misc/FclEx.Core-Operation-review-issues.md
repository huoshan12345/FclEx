# FclEx.Core Operation Review Issues

This file records improvement suggestions from the review of `src/FclEx.Core/FclEx/Utils/~Operation`.

Reviewed on 2026-06-05. The review focused on naming, public API shape, implementation behavior, and test coverage.

## Verification

1. `dotnet build src\FclEx.Core\FclEx.Core.csproj -c Release --no-restore`: passed for `net472`, `netstandard2.0`, `net8.0`, `net9.0`, and `net10.0`.

2. `dotnet test test\FclEx.Core.Tests\FclEx.Core.Tests.csproj -c Release --no-restore --no-build --filter FullyQualifiedName~OperationResult`: passed 23 tests for `net472`, `net8.0`, `net9.0`, and `net10.0`.

## Issues

1. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResult.cs`: `OperationResult<T>` has implicit conversions from `T`, `string?`, and `Exception`. This makes normal success values ambiguous or impossible to express naturally when `T` is `string`, `Exception`, or a derived exception type. A string-returning operation can look like either a successful value or an error message depending on overload resolution. Prefer explicit factories such as `Operation.Success(value)` and `Operation.Error<T>(message)`, and consider removing at least the public implicit `string?` and `Exception` conversions.

2. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResult.cs` and `IOperationResult.cs`: nullable annotations claim more than the type can guarantee. `IsSuccess` is annotated with `[MemberNotNullWhen(true, nameof(Value))]`, and `TryGetValue` uses `[NotNullWhen(true)]`, but `Operation.Success<string?>(null)` and `default(OperationResult<T>)` are both successful results with a null/default value. Remove the non-null success annotation, split nullable/non-null result APIs, or constrain success factories that promise non-null values.

3. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResult.cs`: `default(OperationResult<T>)` represents success because `Exception` is null. This can hide uninitialized fields, arrays, or locals as successful results with default values. Consider storing an explicit status flag, adding an `IsDefined`/`Status` property, or making the default state an error/undefined state with clear behavior.

4. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.cs`, `Operation.Async.cs`, and `OperationResultExtensions.*.cs`: `Execute(Func<OperationResult>)`, `Execute<T>(Func<OperationResult<T>>)`, and their async equivalents unwrap the inner result and discard the outer measured elapsed time when the delegate returns a result. If the delegate does real work and returns `Operation.Success()` with default elapsed, the public `Execute` result reports zero elapsed. Decide whether `Execute` should measure wrapper execution, preserve inner elapsed, or combine both, and make the behavior explicit.

5. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Async.cs`: `ExecuteValueAsync<T>(Func<ValueTask<OperationResult<T>>> action, TimeSpan? timeout = null)` ignores the `timeout` parameter because it calls `ExecuteValueAsync<OperationResult<T>>(action)` without passing `timeout`. Pass the timeout through and add a test that a delayed `ValueTask<OperationResult<T>>` times out.

6. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.Async.cs`: async `WhenResult` executes the callback when the condition is false, not when it is true. The implementation uses `condition(r) ? r : action(r).Then(() => r)`, which is the opposite of the synchronous `WhenResult` behavior and the method name. Swap the branches and add tests for both true and false conditions.

7. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.Async.cs`: async `OnFaulted<T>(Func<OperationResult<T>, Task>)` uses `m.IsError` while the synchronous overload and the async `Action` overload use `IsFaulted()`. This means the async callback also runs for cancellations. Change the predicate to `m.IsFaulted()` and add cancellation coverage.

8. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.Async.cs`: async chaining has inconsistent exception semantics. `Then(Func<T, Task<TNext>>)` wraps the next step in `Operation.ExecuteAsync`, while `Then(Func<T, Task<OperationResult<TNext>>>)` and `ThenResult(Func<OperationResult<T>, Task<OperationResult<TNext>>>)` let exceptions from `next` fault the returned task. Choose one model: either result-chain methods always convert thrown exceptions into `OperationResult` errors, or names/docs should make the faulting-task behavior clear.

9. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.Async.cs`: many async helpers are implemented with raw `ContinueWith`, nested async lambdas, and checks against the captured `task` variable. This makes scheduler behavior and exception propagation harder to reason about than ordinary `async`/`await`. Prefer `await` with `ConfigureAwait(false)` for library code, or at least use consistent continuation options and inspect the continuation parameter.

10. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Async.cs`: timeout support is a wait timeout, not cooperative cancellation. For synchronous delegates wrapped in `Task.Run`, a timeout returns an error result while the underlying work can keep running. Consider adding `CancellationToken` overloads, documenting that timeout does not stop the delegate, or using delegate shapes that can observe cancellation.

11. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Async.cs` and `Operation.Action.cs`: `ExecuteAsync(Action)`, `ExecuteAsync(Func<T>)`, and `Operation.Action(Func<CancellationToken, T>)` always queue synchronous work through `Task.Run`. That may be surprising for lightweight or thread-affine work and gives callers no way to choose inline execution. Consider separating synchronous capture from background scheduling, for example `ExecuteAsync` for naturally async delegates and `RunAsync`/`ExecuteOnThreadPoolAsync` for forced thread-pool execution.

12. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Create.*.cs`: `Cancel<T>(Exception ex, ...)` turns any non-`OperationCanceledException` into an `OperationCanceledException`. This can misclassify ordinary failures if callers pass the wrong exception. Either require/validate cancellation exception types, rename the method to indicate wrapping, or expose a separate `FromCancellation(Exception)` API.

13. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Create*.cs`: error-message factories accept `string?`, which allows an error result with a null or empty message. For a public result API, a null message is not very diagnostic. Require a non-empty message, normalize to a default message, or keep the nullable overload internal.

14. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResult.cs`: `Cast<TTarget>()` uses `Value.CastTo<TTarget>()`, which relies on `dynamic` conversion and can throw from a successful result path. The name also suggests a CLR cast, while the helper may perform dynamic conversions. Consider renaming to `ConvertValue`, catching conversion failures into an error result, or providing separate throwing and non-throwing APIs.

15. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.cs`: `Then<T, TResult>(OperationResult<T>, Func<T, OperationResult<TResult>>)` and related synchronous chain methods do not validate null delegates. Because these are public APIs, null arguments currently turn into later `NullReferenceException`s. Validate with `Check.NotNull` consistently, as many action APIs already do.

16. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.cs`: method naming around `Unwrap` is overloaded across very different meanings: extracting a value and throwing on error, extracting a value with a default fallback, and flattening nested `OperationResult<OperationResult<T>>`. Split these into clearer names such as `GetValueOrThrow`, `GetValueOrDefault`, and `Flatten`.

17. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.cs`: `FallBack` should be named `Fallback` for normal .NET naming and natural English when used as a method. If the intent is a verb phrase, use `OrElse`, `Recover`, or `RecoverWith`; if the intent is a noun concept, use `Fallback`.

18. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Action.cs`, `Operation.Create*.cs`, and related action classes: public parameter names such as `obj`, `func`, and `timeSpan` are less descriptive than `value`, `execute`, `operation`, or `elapsed`. Since these are public NuGet APIs and appear in IntelliSense and named arguments, rename them for clarity.

19. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Action.cs`: the `Operation.Action(...)` factory name is easy to confuse with `System.Action`, while the return type is `IAction<T>`. Consider `Operation.CreateAction`, `Operation.ToAction`, or static factories on `OperationAction`/`ResultAction` to make the API easier to discover.

20. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions*.cs`: `Merge` sums elapsed time for every result. That is reasonable for serial operations but misleading for parallel work, where wall-clock elapsed is usually the maximum or separately measured duration. Rename to make summed elapsed explicit, or add merge options for sum/max/preserve.

21. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions*.cs`: `Merge` accepts `IEnumerable<IOperationResult>` and `IEnumerable<OperationResult<T>>` but does not protect against null elements for the interface overload. Add null validation for elements or document that null entries are invalid.

22. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/OperationIOPairs.cs`: `OperationIOPairs` is an opaque abbreviation-heavy public name, and the `Success`/`Failure` properties use different output shapes. Consider a clearer name such as `OperationInputOutputPairs`, `OperationBatchResult<TInput, TOutput>`, or `OperationPartition<TInput, TOutput>`, and consider exposing more intention-revealing property names such as `Succeeded` and `Failed`.

23. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/OperationIOPairs.cs`: the record struct can be default-constructed with null `Success` and `Failure` lists, and the `+` operator assumes both sides are initialized. Use empty arrays for defaults where possible, validate constructor inputs, or make this a reference type if an always-initialized invariant matters.

24. [Open] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.cs`, `Operation.Async.cs`, `Operation.Action.cs`, and `OperationResultExtensions*.cs`: partial type declarations are inconsistent. Some parts spell `public static partial class`, while others omit `public` and/or `static`. The project builds, but the style makes source review unnecessarily confusing. Spell the full intended modifiers on every partial declaration.

25. [Open] `src/FclEx.Core/FclEx/Utils/~Operation`: XML documentation is uneven across public methods. `OperationResult<T>` and `IOperationResult` have summaries, but many public extension and factory methods are undocumented or have placeholder generic docs. Because this is package surface area, add concise XML docs for behavior, exception/cancellation semantics, elapsed-time semantics, and null-value behavior.

26. [Open] `test/FclEx.Core.Tests/FclEx/Utils/OperationResult`: tests currently cover only a small slice of the public API. Add focused tests for null success values, `string`/`Exception` success-value ambiguity, default result behavior, `ValueTask<OperationResult<T>>` timeout, async `WhenResult`, async `OnFaulted` cancellation behavior, thrown exceptions from async chain delegates, `Merge` empty/null-element cases, and `OperationIOPairs` default/operator behavior.
