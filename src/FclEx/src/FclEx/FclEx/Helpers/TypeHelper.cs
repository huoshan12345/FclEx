using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FclEx.Extensions;

namespace FclEx.Helpers;

public static class TypeHelper
{
    public static readonly HashSet<Type> ActionTypes = AssemblyHelper.AssemblyOfAction
        .GetExportedTypes()
        .Where(m => m.SimpleName() == nameof(Action))
        .ToHashSet();

    public static readonly Dictionary<int, Type> ActionTypeDic = ActionTypes.ToDictionary(m => m.GetTypeInfo().GenericTypeParameters.Length);

    public static readonly HashSet<Type> FuncTypes = AssemblyHelper.AssemblyOfFunc
        .GetExportedTypes()
        .Where(m => m.SimpleName() == nameof(Func<int>))
        .ToHashSet();

    public static readonly Dictionary<int, Type> FuncTypeDic = FuncTypes.ToDictionary(m => m.GetTypeInfo().GenericTypeParameters.Length);
}