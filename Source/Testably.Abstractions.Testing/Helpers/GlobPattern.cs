namespace Testably.Abstractions.Testing.Helpers;

/// <summary>
///     A glob pattern supporting <c>*</c> and <c>?</c> within a path segment, <c>**</c> as a whole segment for any
///     number of directories and character classes like <c>[abc]</c>, <c>[a-z]</c> or <c>[!a-z]</c>.<br />
///     Both <c>/</c> and <c>\</c> are treated as path separators.
/// </summary>
internal sealed class GlobPattern
{
	private readonly bool _ignoreCase;
	private readonly string _pattern;

	private GlobPattern(string pattern, bool ignoreCase)
	{
		_pattern = pattern;
		_ignoreCase = ignoreCase;
	}

	/// <summary>
	///     Checks if the <paramref name="text" /> matches the glob pattern.
	/// </summary>
	public bool IsMatch(string text)
		=> IsMatch(0, text, 0);

	/// <summary>
	///     Creates a <see cref="GlobPattern" /> from the given <paramref name="pattern" />.
	/// </summary>
	public static GlobPattern Parse(string pattern, bool ignoreCase)
		=> new(pattern, ignoreCase);

	private bool CharEquals(char a, char b)
		=> a == b || (_ignoreCase && char.ToUpperInvariant(a) == char.ToUpperInvariant(b));

	private bool IsDirectoryWildcard(int p)
		=> p + 1 < _pattern.Length && _pattern[p + 1] == '*' &&
		   (p == 0 || IsSeparator(_pattern[p - 1])) &&
		   (p + 2 == _pattern.Length || IsSeparator(_pattern[p + 2]));

	private bool IsMatch(int p, string text, int t)
	{
		while (p < _pattern.Length)
		{
			char c = _pattern[p];
			if (c == '*')
			{
				if (IsDirectoryWildcard(p))
				{
					if (p + 2 == _pattern.Length)
					{
						return true;
					}

					for (int i = t; i <= text.Length; i++)
					{
						if ((i == t || IsSeparator(text[i - 1])) && IsMatch(p + 3, text, i))
						{
							return true;
						}
					}

					return false;
				}

				while (p < _pattern.Length && _pattern[p] == '*')
				{
					p++;
				}

				int segmentEnd = t;
				while (segmentEnd < text.Length && !IsSeparator(text[segmentEnd]))
				{
					segmentEnd++;
				}

				for (int i = t; i <= segmentEnd; i++)
				{
					if (IsMatch(p, text, i))
					{
						return true;
					}
				}

				return false;
			}

			if (t == text.Length)
			{
				return false;
			}

			bool isCharacterMatch = c == '[' && TryMatchClass(ref p, text[t], out bool isClassMatch)
				? isClassMatch
				: MatchesCharacter(_pattern[p++], text[t]);
			if (!isCharacterMatch)
			{
				return false;
			}

			t++;
		}

		return t == text.Length;
	}

	private static bool IsSeparator(char c)
		=> c is '/' or '\\';

	private bool MatchesCharacter(char patternChar, char textChar)
	{
		if (IsSeparator(patternChar))
		{
			return IsSeparator(textChar);
		}

		if (patternChar == '?')
		{
			return !IsSeparator(textChar);
		}

		return CharEquals(patternChar, textChar);
	}

	private bool TryMatchClass(ref int p, char c, out bool isMatch)
	{
		int start = p + 1;
		bool isNegated = start < _pattern.Length && _pattern[start] == '!';
		if (isNegated)
		{
			start++;
		}

		int end = _pattern.IndexOf(']', start);
		if (end <= start)
		{
			isMatch = false;
			return false;
		}

		bool isFound = false;
		int i = start;
		while (i < end && !isFound)
		{
			if (i + 2 < end && _pattern[i + 1] == '-')
			{
				isFound = IsInRange(c, _pattern[i], _pattern[i + 2]) ||
				          (_ignoreCase && (IsInRange(char.ToUpperInvariant(c), _pattern[i], _pattern[i + 2]) ||
				                           IsInRange(char.ToLowerInvariant(c), _pattern[i], _pattern[i + 2])));
				i += 3;
			}
			else
			{
				isFound = CharEquals(_pattern[i], c);
				i++;
			}
		}

		p = end + 1;
		isMatch = isFound != isNegated;
		return true;

		static bool IsInRange(char value, char from, char to)
			=> value >= from && value <= to;
	}
}
