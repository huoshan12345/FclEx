namespace FclEx.Extensions;

public static class MethodBaseExtensions
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
}