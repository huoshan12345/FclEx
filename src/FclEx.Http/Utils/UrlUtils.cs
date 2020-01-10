using System;
using System.Text.RegularExpressions;

namespace FclEx.Utils
{
	/// <summary>
	/// Code from https://github.com/tmenier/Flurl
	/// </summary>
	public static class UrlUtils
    {
		#region static utility methods
		/// <summary>
		/// Basically a Path.Combine for URLs. Ensures exactly one '/' separates each segment,
		/// and exactly on '&amp;' separates each query parameter.
		/// URL-encodes illegal characters but not reserved characters.
		/// </summary>
		/// <param name="parts">URL parts to combine.</param>
		public static string Combine(params string[] parts)
		{
			if (parts == null)
				throw new ArgumentNullException(nameof(parts));

			string result = "";
			bool inQuery = false, inFragment = false;

			string CombineEnsureSingleSeparator(string a, string b, char separator)
			{
				if (string.IsNullOrEmpty(a)) return b;
				if (string.IsNullOrEmpty(b)) return a;
				return a.TrimEnd(separator) + separator + b.TrimStart(separator);
			}

			foreach (var part in parts)
			{
				if (string.IsNullOrEmpty(part))
					continue;

				if (result.EndsWith("?") || part.StartsWith("?"))
					result = CombineEnsureSingleSeparator(result, part, '?');
				else if (result.EndsWith("#") || part.StartsWith("#"))
					result = CombineEnsureSingleSeparator(result, part, '#');
				else if (inFragment)
					result += part;
				else if (inQuery)
					result = CombineEnsureSingleSeparator(result, part, '&');
				else
					result = CombineEnsureSingleSeparator(result, part, '/');

				if (part.Contains("#"))
				{
					inQuery = false;
					inFragment = true;
				}
				else if (!inFragment && part.Contains("?"))
				{
					inQuery = true;
				}
			}
			return EncodeIllegalCharacters(result);
		}

		/// <summary>
		/// Decodes a URL-encoded string.
		/// </summary>
		/// <param name="s">The URL-encoded string.</param>
		/// <param name="interpretPlusAsSpace">If true, any '+' character will be decoded to a space.</param>
		/// <returns></returns>
		public static string Decode(string s, bool interpretPlusAsSpace)
		{
			if (string.IsNullOrEmpty(s))
				return s;

			return Uri.UnescapeDataString(interpretPlusAsSpace ? s.Replace("+", " ") : s);
		}

		private const int MAX_URL_LENGTH = 65519;

		/// <summary>
		/// URL-encodes a string, including reserved characters such as '/' and '?'.
		/// </summary>
		/// <param name="s">The string to encode.</param>
		/// <param name="encodeSpaceAsPlus">If true, spaces will be encoded as + signs. Otherwise, they'll be encoded as %20.</param>
		/// <returns>The encoded URL.</returns>
		public static string Encode(string s, bool encodeSpaceAsPlus = false)
		{
			if (string.IsNullOrEmpty(s))
				return s;

			if (s.Length > MAX_URL_LENGTH)
			{
				// Uri.EscapeDataString is going to throw because the string is "too long", so break it into pieces and concat them
				var parts = new string[(int)Math.Ceiling((double)s.Length / MAX_URL_LENGTH)];
				for (var i = 0; i < parts.Length; i++)
				{
					var start = i * MAX_URL_LENGTH;
					var len = Math.Min(MAX_URL_LENGTH, s.Length - start);
					parts[i] = Uri.EscapeDataString(s.Substring(start, len));
				}
				s = string.Concat(parts);
			}
			else
			{
				s = Uri.EscapeDataString(s);
			}
			return encodeSpaceAsPlus ? s.Replace("%20", "+") : s;
		}

		/// <summary>
		/// URL-encodes characters in a string that are neither reserved nor unreserved. Avoids encoding reserved characters such as '/' and '?'. Avoids encoding '%' if it begins a %-hex-hex sequence (i.e. avoids double-encoding).
		/// </summary>
		/// <param name="s">The string to encode.</param>
		/// <param name="encodeSpaceAsPlus">If true, spaces will be encoded as + signs. Otherwise, they'll be encoded as %20.</param>
		/// <returns>The encoded URL.</returns>
		public static string EncodeIllegalCharacters(string s, bool encodeSpaceAsPlus = false)
		{
			if (string.IsNullOrEmpty(s))
				return s;

			if (encodeSpaceAsPlus)
				s = s.Replace(" ", "+");

			// Uri.EscapeUriString mostly does what we want - encodes illegal characters only - but it has a quirk
			// in that % isn't illegal if it's the start of a %-encoded sequence https://stackoverflow.com/a/47636037/62600

			// no % characters, so avoid the regex overhead
			if (!s.Contains("%"))
				return Uri.EscapeUriString(s);

			// pick out all %-hex-hex matches and avoid double-encoding 
			return Regex.Replace(s, "(.*?)((%[0-9A-Fa-f]{2})|$)", c => {
				var a = c.Groups[1].Value; // group 1 is a sequence with no %-encoding - encode illegal characters
				var b = c.Groups[2].Value; // group 2 is a valid 3-character %-encoded sequence - leave it alone!
				return Uri.EscapeUriString(a) + b;
			});
		}

		/// <summary>
		/// Checks if a string is a well-formed absolute URL.
		/// </summary>
		/// <param name="url">The string to check</param>
		/// <returns>true if the string is a well-formed absolute URL</returns>
		public static bool IsValid(string url) => url != null && Uri.IsWellFormedUriString(url, UriKind.Absolute);
		#endregion
	}
}
