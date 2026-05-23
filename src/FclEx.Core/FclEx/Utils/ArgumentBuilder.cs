namespace FclEx.Utils;

public class ArgumentBuilder
{
    private readonly List<(object? value, Type type)> _args = [];

    public ArgumentBuilder AddArgument<T>(T value)
    {
        _args.Add((value, typeof(T)));
        return this;
    }

    public object CreateObject(Type type)
    {
        Check.NotNull(type);

        var ctors = type.GetConstructors()
            .Select(m => (Ctor: m, Paras: m.GetParameters()))
            .ToList();

        var args = _args.Select((m, i) => new ArgumentInfo(m.value, m.type, i)).ToList();

        var allMatches = ctors.SelectMany(m => GetMatches(m.Paras, args), (x, y) => (x.Ctor, x.Paras, Match: y))
            .OrderByDescending(m => m.Paras.Length)
            .ThenByDescending(m => m.Match.EqualMatchCount)
            .ThenByDescending(m => m.Match.InheritMatchCount)
            .ThenByDescending(m => m.Match.UseDefaultCount)
            .ToList();

        if (allMatches.IsEmpty())
            throw new MissingMethodException(type.ShortName(), "ctor");


        var (ctor, _, match) = allMatches.First();
        var obj = ctor.Invoke(match.Args.ToArray());
        return obj;
    }

    private static List<ArgumentMatchResult> GetMatches(IReadOnlyList<ParameterInfo> paras, IReadOnlyList<ArgumentInfo> args)
    {
        if (paras.Count == 0)
        {
            return [new ArgumentMatchResult()];
        }

        var leafNodes = new List<TreeNode<ArgumentMatchInfo>>();
        var root = new TreeNode<ArgumentMatchInfo>(new ArgumentMatchInfo(default, default, -1, new HashSet<int>(Enumerable.Range(0, args.Count))));
        var queue = new Queue<TreeNode<ArgumentMatchInfo>>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            var value = cur.Value!;
            var nextIdx = value.ParaIndex + 1;
            if (nextIdx >= paras.Count)
            {
                leafNodes.Add(cur);
                continue;
            }
            var para = paras[nextIdx];
            foreach (var arg in args.Where(m => value.RemainArgIndexes.Contains(m.Index)))
            {
                var matchType = ArgumentMatchType.NoMatch;
                if (para.ParameterType == arg.Type)
                    matchType = ArgumentMatchType.EqualMatch;
                else if (para.ParameterType.IsAssignableFrom(arg.Type))
                    matchType = ArgumentMatchType.InheritMatch;

                if (matchType != ArgumentMatchType.NoMatch)
                {
                    var remainArgIndexes = new HashSet<int>(value.RemainArgIndexes.Where(m => m != arg.Index));
                    var matchInfo = new ArgumentMatchInfo(arg, matchType, nextIdx, remainArgIndexes);
                    var child = cur.AddChild(matchInfo);
                    queue.Enqueue(child);
                }
            }
            if (para.HasDefaultValue)
            {
                var defaultArg = new ArgumentInfo(para.DefaultValue, para.ParameterType, -1);
                var matchInfo = new ArgumentMatchInfo(defaultArg, ArgumentMatchType.UseDefault, nextIdx, value.RemainArgIndexes);
                var child = cur.AddChild(matchInfo);
                queue.Enqueue(child);
            }
        }

        var paths = leafNodes.Select(m => m.GetPathToRoot().Where(p => p.Parent != null).Reverse())
            .Select(m => new ArgumentMatchResult(m.Select(x => x.Value!)))
            .ToList();

        return paths;
    }

    private class ArgumentMatchResult
    {
        public ArgumentMatchResult() { }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public ArgumentMatchResult(IEnumerable<ArgumentMatchInfo> matchInfos)
        {
            Check.NotNull(matchInfos);
            foreach (var matchInfo in matchInfos)
            {
                Args.Add(matchInfo.Argument.Value);
                switch (matchInfo.MatchType)
                {
                    case ArgumentMatchType.EqualMatch:
                        ++EqualMatchCount;
                        break;
                    case ArgumentMatchType.InheritMatch:
                        InheritMatchCount++;
                        break;
                    case ArgumentMatchType.UseDefault:
                        UseDefaultCount++;
                        break;
                }
            }
        }

        public List<object?> Args { get; } = [];
        public int EqualMatchCount { get; }
        public int InheritMatchCount { get; }
        public int UseDefaultCount { get; }

    }

    private readonly struct ArgumentInfo
    {
        public ArgumentInfo(object? value, Type type, int index)
        {
            Value = value;
            Type = type;
            Index = index;
        }

        public object? Value { get; }
        public Type Type { get; }
        public int Index { get; }
    }

    private class ArgumentMatchInfo
    {
        public ArgumentMatchInfo(ArgumentInfo argument, ArgumentMatchType matchType, int paraIndex, IEnumerable<int> remainArgIndexes)
        {
            Argument = argument;
            MatchType = matchType;
            RemainArgIndexes = [..remainArgIndexes];
            ParaIndex = paraIndex;
        }

        public ArgumentInfo Argument { get; }
        public ArgumentMatchType MatchType { get; }
        public int ParaIndex { get; }
        public HashSet<int> RemainArgIndexes { get; }
    }

    private enum ArgumentMatchType
    {
        NoMatch = 0,
        EqualMatch = 1,
        InheritMatch = 2,
        UseDefault,
    }
}