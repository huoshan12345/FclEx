using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

using FclEx;
using FclEx.Extensions;

namespace AutoMapper;

public static class Extensions
{
    public static IMappingExpression<TSource, TDestination> MapArrayToStr<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> map, Func<TSource, IEnumerable<string>> sourceMember,
        Expression<Func<TDestination, string>> destinationMember, string separator = "|")
    {
        Check.NotNull(map);
        Check.NotNull(destinationMember);
        Check.NotNull(sourceMember);

        return map.ForMember(destinationMember, o => o.MapFrom(s => sourceMember(s).Touch().JoinWith(separator)));
    }

    public static IMappingExpression<TSource, TDestination> MapStrToArray<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> map, Func<TSource, string> sourceMember,
        Expression<Func<TDestination, IEnumerable<string>>> destinationMember, string separator = "|")
    {
        Check.NotNull(map);
        Check.NotNull(destinationMember);
        Check.NotNull(sourceMember);

        return map.ForMember(destinationMember, o => o.MapFrom(s => sourceMember(s).ToStringOrEmpty().Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)));
    }
}