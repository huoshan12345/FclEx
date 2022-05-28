using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Actions;

partial class Extensions
{
    public static IAction<(T Cur, TNext Next)> Union<T, TNext>(this IAction<T> action, Func<T, IAction<TNext>> next)
    {
        return new UnionAction<T, TNext>(action, next);
    }

    public static IAction<(T Cur, TNext Next)> Union<T, TNext>(this IAction<T> action, Func<T, OperateResult<TNext>> next)
    {
        return new UnionAction<T, TNext>(action, m => CommonAction.Create(t => next(m), excuteSafely: false));
    }

    public static IAction<(T1, T2, TNext)> Union<T1, T2, TNext>(this IAction<(T1, T2)> action, Func<T1, T2, IAction<TNext>> next)
    {
        return new UnionAction<(T1, T2), TNext>(action, m => next(m.Item1, m.Item2)).Map(m => (m.Item1.Item1, m.Item1.Item2, m.Item2));
    }
}