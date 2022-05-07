using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Dawn;
using FclEx.Utils;
using MoreLinq.Extensions;
using Newtonsoft.Json.Linq;

namespace FclEx
{
    using System.Linq;

    public static class NameValueCollectionExtensions
    {
        public static IEnumerable<KeyValuePair<string, string>> ToPair(this NameValueCollection col)
        {
            var q = from k in col.AllKeys.NotNull()
                    from v in col.GetValues(k).Touch()
                    select KvPair.Create(k, v);
            return q;
        }

        public static KeyValuePair<string, string>[] ToPairs(this NameValueCollection col)
        {
            return col.ToPair().ToArray();
        }

        public static Dictionary<string, string> ToDictionary(this NameValueCollection nvc, DupPolicy policy = DupPolicy.OnlyLast)
        {
            if (policy == DupPolicy.Array)
                throw new NotSupportedException();

            var dic = new Dictionary<string, string>(nvc.Count);
            foreach (var (k, v) in nvc.ToPair())
            {
                switch (policy)
                {
                    case DupPolicy.OnlyLast:
                    {
                        dic[k] = v;
                        break;
                    }
                    case DupPolicy.OnlyFirst:
                    {
                        dic.TryAdd(k, v);
                        break;
                    }
                    case DupPolicy.Throw:
                    {
                        if (dic.TryGetValue(k, out var old))
                        {
                            if (old != v)
                                throw new ArgumentException($"duplicate key: {k} with different values: {old},{v}");
                        }
                        else
                        {
                            dic.Add(k, v);
                        }
                        break;
                    }
                    default: throw new ArgumentOutOfRangeException(nameof(policy), policy, null);
                }
            }
            return dic;
        }

        public static JObject ToJObject(this NameValueCollection col, DupPolicy policy = DupPolicy.OnlyLast)
        {
            var obj = new JObject();
            foreach (var k in col.AllKeys.NotNull())
            {
                var values = col.GetValues(k).Touch().ToHashSet();
                if (values.Count > 0)
                    obj.Add(k, values.ToJToken(policy));
            }
            return obj;
        }

        internal static JToken ToJToken(this ISet<string> values, DupPolicy policy)
        {
            Guard.Argument(values, nameof(values)).NotNull().NotEmpty();
            if (values.Count == 1) return JToken.FromObject(values.First());
            switch (policy)
            {
                case DupPolicy.OnlyLast: return JToken.FromObject(values.Last());
                case DupPolicy.OnlyFirst: return JToken.FromObject(values.First());
                case DupPolicy.Array: return JArray.FromObject(values);
                case DupPolicy.Throw: throw new InvalidOperationException("the collection contains more than one value");
                default: throw new ArgumentOutOfRangeException(nameof(policy), policy, null);
            }
        }

    }
}
