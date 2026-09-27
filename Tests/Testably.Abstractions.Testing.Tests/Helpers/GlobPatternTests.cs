using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public sealed class GlobPatternTests
{
	[Test]
	[Arguments("*", "", true)]
	[Arguments("*", "foo", true)]
	[Arguments("*", "a/b", false)]
	[Arguments("f*o", "foo", true)]
	[Arguments("f*o", "fo", true)]
	[Arguments("f*o", "fooa", false)]
	[Arguments("*.txt", "a.txt", true)]
	[Arguments("*.txt", "a.txt.bak", false)]
	[Arguments("*o*o*", "foobar", true)]
	[Arguments("*o*o*", "fobar", false)]
	[Arguments("f?o", "foo", true)]
	[Arguments("f?o", "fo", false)]
	[Arguments("f?o", "f/o", false)]
	[Arguments("foo", "foo", true)]
	[Arguments("foo", "bar", false)]
	[Arguments("foo", "foobar", false)]
	[Arguments("", "", true)]
	[Arguments("", "a", false)]
	public async Task IsMatch_Wildcards_ShouldMatchWithinSegment(
		string pattern, string text, bool expected)
	{
		bool result = GlobPattern.Parse(pattern, false).IsMatch(text);

		await That(result).IsEqualTo(expected);
	}

	[Test]
	[Arguments("a/*", "a/b", true)]
	[Arguments("a/*", "a/b/c", false)]
	[Arguments("a/b", "a\\b", true)]
	[Arguments("a\\b", "a/b", true)]
	[Arguments("a/b", "a/c", false)]
	[Arguments("/a/b", "/a/b", true)]
	[Arguments("C:/a/*.txt", "C:/a/b.txt", true)]
	public async Task IsMatch_Separators_ShouldTreatSlashAndBackslashAsSeparator(
		string pattern, string text, bool expected)
	{
		bool result = GlobPattern.Parse(pattern, false).IsMatch(text);

		await That(result).IsEqualTo(expected);
	}

	[Test]
	[Arguments("**", "a", true)]
	[Arguments("**", "a/b", true)]
	[Arguments("**/foo", "foo", true)]
	[Arguments("**/foo", "a/b/foo", true)]
	[Arguments("**/foo", "/a/foo", true)]
	[Arguments("**/foo", "C:/a/foo", true)]
	[Arguments("**/foo", "a/foobar", false)]
	[Arguments("**/f*o", "C:/bar/foo", true)]
	[Arguments("a/**/b", "a/b", true)]
	[Arguments("a/**/b", "a/x/y/b", true)]
	[Arguments("a/**/b", "a/x/c", false)]
	[Arguments("a/**/b", "x/a/b", false)]
	[Arguments("a/**", "a/b/c", true)]
	[Arguments("a/**", "b/c", false)]
	[Arguments("a/**", "a", false)]
	[Arguments("a**b", "axb", true)]
	[Arguments("a**b", "ax/yb", false)]
	public async Task IsMatch_DirectoryWildcard_ShouldMatchAnyNumberOfDirectories(
		string pattern, string text, bool expected)
	{
		bool result = GlobPattern.Parse(pattern, false).IsMatch(text);

		await That(result).IsEqualTo(expected);
	}

	[Test]
	[Arguments("[abc]x", "bx", true)]
	[Arguments("[abc]x", "dx", false)]
	[Arguments("[a-c]x", "bx", true)]
	[Arguments("[a-c]x", "dx", false)]
	[Arguments("[!abc]x", "dx", true)]
	[Arguments("[!abc]x", "ax", false)]
	[Arguments("[!a-c]x", "dx", true)]
	[Arguments("[!a-c]x", "bx", false)]
	[Arguments("a[!b]c", "a/c", true)]
	[Arguments("[a-cx-z]", "y", true)]
	[Arguments("[a-cx-z]", "m", false)]
	[Arguments("[*]", "*", true)]
	[Arguments("[*]", "a", false)]
	[Arguments("[?]", "?", true)]
	[Arguments("[?]", "a", false)]
	public async Task IsMatch_CharacterClasses_ShouldMatchSingleCharacter(
		string pattern, string text, bool expected)
	{
		bool result = GlobPattern.Parse(pattern, false).IsMatch(text);

		await That(result).IsEqualTo(expected);
	}

	[Test]
	[Arguments("FOO", "foo", true)]
	[Arguments("f*O", "FoO", true)]
	[Arguments("[A-C]x", "bX", true)]
	[Arguments("[abc]", "B", true)]
	[Arguments("[!A-C]x", "bx", false)]
	public async Task IsMatch_IgnoreCase_ShouldMatchCaseInsensitive(
		string pattern, string text, bool expected)
	{
		bool result = GlobPattern.Parse(pattern, true).IsMatch(text);

		await That(result).IsEqualTo(expected);
	}

	[Test]
	[Arguments("FOO", "foo", false)]
	[Arguments("f*O", "FoO", false)]
	[Arguments("[A-C]x", "bx", false)]
	[Arguments("[abc]", "B", false)]
	[Arguments("[!A-C]x", "bx", true)]
	public async Task IsMatch_CaseSensitive_ShouldMatchCaseSensitive(
		string pattern, string text, bool expected)
	{
		bool result = GlobPattern.Parse(pattern, false).IsMatch(text);

		await That(result).IsEqualTo(expected);
	}
}
