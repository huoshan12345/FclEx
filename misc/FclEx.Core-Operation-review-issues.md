# FclEx.Core Operation Review Issues

This file records improvement suggestions from the review of `src/FclEx.Core/FclEx/Utils/~Operation`.

Reviewed on 2026-06-05. The review focused on naming, public API shape, implementation behavior, and test coverage.

## Verification

1. `dotnet build src\FclEx.Core\FclEx.Core.csproj -c Release --no-restore`: passed for `net472`, `netstandard2.0`, `net8.0`, `net9.0`, and `net10.0`.

2. `dotnet test test\FclEx.Core.Tests\FclEx.Core.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~Operation|FullyQualifiedName~Actions"`: passed 433 tests for `net472`, `net8.0`, `net9.0`, and `net10.0`.

3. `dotnet test test\FclEx.Core.Tests\FclEx.Core.Tests.csproj -c Release --no-restore --no-build`: passed for `net472` with 9987 passed and 3 skipped tests, and for `net8.0`, `net9.0`, and `net10.0` with 10031 passed and 3 skipped tests each.

4. `dotnet test test\FclEx.Core.Tests\FclEx.Core.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~Operation"`: passed 363 tests for `net472`, `net8.0`, `net9.0`, and `net10.0`.

## Issues

1. [No change] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResult.cs`: `OperationResult<T>` has implicit conversions from `T`, `string?`, and `Exception`. This is convenient for common result construction, and callers can use explicit factories such as `Operation.Success(value)` when `T` itself is `string`, `Exception`, or a derived exception type.

2. [No change] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResult.cs` and `IOperationResult.cs`: nullable annotations intentionally optimize for the normal convention that successful results should carry non-null values. Callers should avoid `Operation.Success<string?>(null)` and similar null-success cases; removing the annotations would create more NRT noise without removing the need for domain-level null checks.

3. [No change] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResult.cs`: `default(OperationResult<T>)` represents success because `Exception` is null. Adding a status field or making the default state an error would make the result type heavier or less natural for current usage, so callers should avoid relying on default-created results.

4. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.cs`, `Operation.Async.cs`, and `OperationResultExtensions.*.cs`: `Execute(Func<OperationResult>)`, `Execute<T>(Func<OperationResult<T>>)`, and their async equivalents now unwrap nested results while applying the outer measured elapsed time. Covered by `Execute_OperationResult_UsesOuterElapsed`, `Execute_OperationResult_T_UsesOuterElapsed`, and `ExecuteAsync_OperationResult_UsesOuterElapsed`.

5. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Async.cs`: `ExecuteValueAsync<T>(Func<ValueTask<OperationResult<T>>> action, TimeSpan? timeout = null)` now passes the timeout through. Covered by `ExecuteValueAsync_OperationResult_Timeout_Test`.

6. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.Async.cs`: async `WhenResult` now executes the callback only when the condition is true, matching the synchronous overload. Covered by `WhenResult_InvokesCallbackOnlyWhenConditionMatches`.

7. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.Async.cs`: async `OnFaulted<T>(Func<OperationResult<T>, Task>)` now uses `m.IsFaulted()`, so cancellations are not treated as faults. Covered by `OnFaulted_DoesNotRunForCanceledResult` and `OnFaulted_RunsForNonCanceledError`.

8. [Documented] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.cs` and `OperationResultExtensions.*.Async.cs`: result-returning delegates passed to `Then`/`ThenResult` are treated as trusted result factories. These combinators do not catch delegate exceptions into error results; use `Operation.Execute`, `Operation.ExecuteAsync`, or `Operation.Action` at exception boundaries when arbitrary throwing code should be converted into `OperationResult`.

9. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.Async.cs`: the `~Operation` async helpers now use `async`/`await` and `NoCapture()` instead of raw `ContinueWith`. Remaining `ContinueWith` usages found by repository search are outside the reviewed `~Operation` code path.

10. [Documented] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Async.cs`: timeout support is intentionally a wait timeout, matching `Task.WaitAsync` behavior. A timeout returns an error result but does not cancel or stop the underlying task/delegate.

11. [Documented] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Async.cs` and `Operation.Action.cs`: synchronous delegates passed to async operation/action factories are intentionally executed through `Task.Run`. This keeps timeout behavior aligned with `Task.WaitAsync`: a timeout stops waiting for the work but does not stop the underlying delegate. Avoid these overloads for thread-affine work.

12. [No change] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Create.*.cs`: `Cancel<T>(Exception ex, ...)` intentionally creates a canceled result by ensuring the stored exception is an `OperationCanceledException`. Result state is classified by exception type: cancellation exceptions mean canceled, other exceptions mean faulted.

13. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Create*.cs`: error-message factories now use non-null `string` parameters.

14. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResult.cs`: `Cast<TTarget>()` now returns an error result when value conversion fails instead of throwing from the successful result path. It preserves successful nulls for nullable/reference targets and reports the runtime source type in cast failure messages.

15. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation` and `src/FclEx.Core/FclEx/Actions`: public operation factories and result/action combinators now validate null delegate parameters more consistently with `Check.NotNull`. Covered by `Execute_RejectsNullAction`, `Action_RejectsNullExecute`, `OperationResultExtensions_RejectNullDelegates`, `MapError_RejectsNullMapper`, and `Then_RejectsNullNextDelegates`.

16. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.cs`: value extraction keeps the Rust-inspired `Unwrap` name, default fallback is now `UnwrapOr`, and nested result flattening is now `Flatten`.

17. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions.*.cs`: `FallBack` has been renamed to `Fallback`.

18. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Action.cs`, `Operation.Create*.cs`, `OperationResult.cs`, and related action classes: public parameter names such as `obj`, `func`, `timeSpan`, `paras`, `item`, and `ex` have been renamed to clearer names such as `value`, `execute`, `elapsed`, `tuple`, and `exception`.

19. [No change] `src/FclEx.Core/FclEx/Utils/~Operation/Operation.Action.cs`: `Operation.Action(...)` is kept because it is already scoped under the `Operation` static class and matches the nearby concise factory style such as `Operation.Success(...)` and `Operation.Error(...)`.

20. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions*.cs`: task-based merge now applies wall-clock elapsed measured around the awaited task rather than blindly using the sum of child result elapsed times.

21. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions*.cs`: `Merge` now uses concrete `OperationResult` inputs instead of `IOperationResult`, keeping the interface reserved for covariance or overload reduction cases and avoiding the previous null-element concern.

22. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/OperationIOPairs.cs`: `OperationIOPairs` keeps the existing `IO` abbreviation to stay aligned with `IOPair`, and the partition properties have been renamed from `Success`/`Failure` to `Succeeded`/`Failed`.

23. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/OperationIOPairs.cs`: `Succeeded` and `Failed` now fall back to empty lists, and the `+` operator handles default-created values.

24. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/Operation*.cs`, `OperationResultExtensions*.cs`, and `OperationIOPairs.cs`: partial type declarations now spell the full intended modifiers consistently.

25. [Resolved] `src/FclEx.Core/FclEx/Utils/~Operation/OperationResultExtensions*.cs`: `Then`, `ThenResult`, and `ThenWith` now treat result-returning delegates as serial composition and add elapsed time from both the source result and next result. The async `Normalize` helper exists only to normalize a source `Task<OperationResult<T>>` before chaining: successful tasks keep the inner result's elapsed time, while faulted or canceled source tasks are converted to error results measured with the outer wait elapsed time.

26. [Open] `src/FclEx.Core/FclEx/Utils/~Operation`: XML documentation is uneven across public methods. `OperationResult<T>` and `IOperationResult` have summaries, but many public extension and factory methods are undocumented or have placeholder generic docs. Because this is package surface area, add concise XML docs for behavior, exception/cancellation semantics, elapsed-time semantics, and null-value behavior.

27. [Open] `test/FclEx.Core.Tests/FclEx/Utils/OperationResult`: tests currently cover only a small slice of the public API. Add focused tests for null success values, `string`/`Exception` success-value ambiguity, default result behavior, `ValueTask<OperationResult<T>>` timeout, async `WhenResult`, async `OnFaulted` cancellation behavior, thrown exceptions from async chain delegates, `Merge` empty/null-element cases, and `OperationIOPairs` default/operator behavior.
