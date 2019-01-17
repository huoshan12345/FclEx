using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FclEx
{
    public static class RegexExtensions
    {
        public static string TryGet(this Match m, int index = 0, string defaultValue = default)
        {
            return m.Success && index >= 0 && index < m.Groups.Count
                ? m.Groups[index].Value
                : defaultValue;
        }

        public static int TryGetInt(this Match m, int index = 0, int defaultValue = default)
        {
            var s = TryGet(m, index);
            return s != null && int.TryParse(s, out var i)
                ? i
                : defaultValue;
        }

        public static bool MatchAndDo(this Regex regex, string input, Action<Match> onSuccess, Action onFail = null)
        {
            onSuccess = onSuccess ?? (m => { });
            onFail = onFail ?? (() => { });
            var match = regex.Match(input);
            if (match.Success)
                onSuccess(match);
            else
                onFail();
            return match.Success;
        }

        public static async Task<bool> MatchAndDoAsync(this Regex regex, string input, Func<Match, Task> onSuccess, Func<Task> onFail = null)
        {
            onSuccess = onSuccess ?? (m => Task.CompletedTask);
            onFail = onFail ?? (() => Task.CompletedTask);
            var match = regex.Match(input);
            if (match.Success)
                await onSuccess(match).DonotCapture();
            else
                await onFail().DonotCapture();
            return match.Success;
        }
    }
}
