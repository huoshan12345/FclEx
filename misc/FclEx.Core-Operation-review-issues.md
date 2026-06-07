# FclEx.Core Operation Review Issues

This file records improvement suggestions from the review of `src/FclEx.Core/FclEx/Utils/~Operation`.

Reviewed on 2026-06-05. The review focused on naming, public API shape, implementation behavior, and test coverage.

## Verification

1. `dotnet build src\FclEx.Core\FclEx.Core.csproj -c Release --no-restore`: passed for `net472`, `netstandard2.0`, `net8.0`, `net9.0`, and `net10.0`.

2. `dotnet test test\FclEx.Core.Tests\FclEx.Core.Tests.csproj -c Release --no-restore --filter FullyQualifiedName~Operation`: passed 347 tests for `net472`, `net8.0`, `net9.0`, and `net10.0`.

## Issues

1. [No change] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResult.cs`: `OperationResult<T>` has implicit conversions from `T`, `string?`, and `Exception`. This is convenient for common result construction, and callers can use explicit factories such as `Operation.Success(value)` when `T` itself is `string`, `Exception`, or a derived exception type.

2. [No change] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResult.cs` and `IOperationResult.cs`: nullable annotations intentionally optimize for the normal convention that successful results should carry non-null values. Callers should avoid `Operation.Success<string?>(null)` and similar null-success cases; removing the annotations would create more NRT noise without removing the need for domain-level null checks.

3. [No change] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResult.cs`: `default(OperationResult<T>)` represents success because `Exception` is null. Adding a status field or making the default state an error would make the result type heavier or less natural for current usage, so callers should avoid relying on default-created results.

4. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.cs`, `Operation.Async.cs`, and `OperationResultExtensions.*.cs`: `Execute(Func<OperationResult>)`, `Execute<T>(Func<OperationResult<T>>)`, and their async equivalents now unwrap nested results while applying the outer measured elapsed time. Covered by `Execute_OperationResult_UsesOuterElapsed`, `Execute_OperationResult_T_UsesOuterElapsed`, and `ExecuteAsync_OperationResult_UsesOuterElapsed`.

5. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Async.cs`: `ExecuteValueAsync<T>(Func<ValueTask<OperationResult<T>>> action, TimeSpan? timeout = null)` now passes the timeout through. Covered by `ExecuteValueAsync_OperationResult_Timeout_Test`.

6. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.Async.cs`: async `WhenResult` now executes the callback only when the condition is true, matching the synchronous overload. Covered by `WhenResult_InvokesCallbackOnlyWhenConditionMatches`.

7. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.Async.cs`: async `OnFaulted<T>(Func<OperationResult<T>, Task>)` now uses `m.IsFaulted()`, so cancellations are not treated as faults. Covered by `OnFaulted_DoesNotRunForCanceledResult` and `OnFaulted_RunsForNonCanceledError`.

8. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.cs` and `OperationResultExtensions.*.Async.cs`: async result-chain methods now consistently route throwing or faulted `next` delegates through `Operation.ExecuteAsync`, producing error results instead of faulting the returned task. Covered by thrown/faulted-task tests for `Then` and thrown-delegate tests for `ThenResult`.

9. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.Async.cs`: the `~Operation` async helpers now use `async`/`await` and `NoCapture()` instead of raw `ContinueWith`. Remaining `ContinueWith` usages found by repository search are outside the reviewed `~Operation` code path.

10. [Documented] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Async.cs`: timeout support is intentionally a wait timeout, matching `Task.WaitAsync` behavior. A timeout returns an error result but does not cancel or stop the underlying task/delegate.

11. [Documented] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Async.cs` and `Operation.Action.cs`: synchronous delegates passed to async operation/action factories are intentionally executed through `Task.Run`. This keeps timeout behavior aligned with `Task.WaitAsync`: a timeout stops waiting for the work but does not stop the underlying delegate. Avoid these overloads for thread-affine work.

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
