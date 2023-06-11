namespace FclEx.Extensions;

public static class MethodExtensions
{
    public static bool ArgumentListMatches(this MethodBase m, Type[] args)
    {
        // If there are less arguments, then it just doesn't matter.
        var pInfo = m.GetParameters();
        if (pInfo.Length < args.Length)
            return false;

        // Now, check compatibility of the first set of arguments.
        return !args.Where((arg, i) => !pInfo[i].ParameterType.IsAssignableFrom(arg)).Any()
               && pInfo.Skip(args.Length).All(p => p.IsOptional); // And make sure the last set of arguments are actually default!
    }

    public static bool IsAsync(this MethodInfo method)
    {
        // Obtain the custom attribute for the method.
        // The value returned contains the StateMachineType property.
        // Null is returned if the attribute isn't present for the method.
        var attrib = method.GetCustomAttribute<AsyncStateMachineAttribute>();
        return (attrib != null);
    }

    public static string GetSignature(this MethodInfo method)
    {
        var paras = method.GetParameters();
        var name = method.GetFullName();
        var paraNames = paras.Select(m => m.ParameterType.ShortName()).JoinWith(",");
        return name + $"({paraNames})";
    }

    public static string GetFullName(this MethodInfo method)
    {
        return method.DeclaringType == null 
            ? method.Name 
            : $"{method.DeclaringType.Namespace}.{method.DeclaringType.ShortName()}.{method.Name}";
    }
}