using System;
using System.Collections.Generic;
using System.Threading;
using FclEx.Extensions;

namespace FclEx.Utils;

public readonly struct QueryTerms
{
    public QueryTerms(DateTimeOffset? minTime = null, DateTimeOffset? maxTime = null,
        int? minCount = null, int? querySizePerTime = null,
        CancellationToken cancellationToken = default)
    {
        CancellationToken = cancellationToken;
        QuerySizePerTime = querySizePerTime;
        MinTime = minTime;
        MaxTime = maxTime;
        MinCount = minCount;
    }

    public readonly DateTimeOffset? MinTime;
    public readonly DateTimeOffset? MaxTime;
    public readonly int? MinCount;
    public readonly int? QuerySizePerTime;
    public readonly CancellationToken CancellationToken;
}

public static class QueryTermsExtensions
{
    public static IEnumerable<T> InOpenInterval<T>(this IEnumerable<T> source, Func<T, DateTimeOffset> timeSelector, QueryTerms query)
    {
        return source
            .WhereIf(m => timeSelector(m) > query.MinTime, query.MinTime.HasValue)
            .WhereIf(m => timeSelector(m) < query.MaxTime, query.MaxTime.HasValue);
    }

    public static IEnumerable<T> InClosedInterval<T>(this IEnumerable<T> source, Func<T, DateTimeOffset> timeSelector, QueryTerms query)
    {
        return source
            .WhereIf(m => timeSelector(m) >= query.MinTime, query.MinTime.HasValue)
            .WhereIf(m => timeSelector(m) <= query.MaxTime, query.MaxTime.HasValue);
    }
}