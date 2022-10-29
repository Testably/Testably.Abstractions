using System.IO;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Internal;

public class FilePlatformIndependenceExtensionsTests
{
	[Fact]
	public void NormalizePath_Null_ShouldReturnNull()
	{
		string? path = null;

		path = path!.NormalizePath();

		path.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void NormalizePath_Unix_RootedPath_ShouldRemoveDriveInfo(string part1)
	{
		Skip.If(Test.RunsOnWindows);

		string path = "C:/" + part1;
		string expectedPath = part1.PrefixRoot();
		path = path.NormalizePath();

		path.Should().Be(expectedPath);
	}

	[SkippableTheory]
	[AutoData]
	public void NormalizePath_Unix_ShouldReplaceAltDirectorySeparatorChar(
		string part1, string part2)
	{
		Skip.If(Test.RunsOnWindows);

		char[] separatorChars =
		{
			Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar
		};
		foreach (char separatorChar in separatorChars)
		{
			string path = part1 + separatorChar + part2;
			string expectedPath = part1 + Path.DirectorySeparatorChar + part2;
			path = path.NormalizePath();

			path.Should().Be(expectedPath);
		}
	}

	[SkippableTheory]
	[AutoData]
	public void NormalizePath_Windows_ShouldAlsoKeepAltDirectorySeparatorChar(
		string part1, string part2)
	{
		Skip.IfNot(Test.RunsOnWindows);

		char[] separatorChars =
		{
			Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar
		};
		foreach (char separatorChar in separatorChars)
		{
			string path = part1 + separatorChar + part2;
			path = path.NormalizePath();

			path.Should().Be(path);
		}
	}

	[Fact]
	public void PrefixRoot_Null_ShouldReturnNull()
	{
		string? path = null;

		string? result = path!.PrefixRoot();

		result.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void PrefixRoot_RootedPath_ShouldReturnPath(string path)
	{
		path = path.PrefixRoot();

		string result = path.PrefixRoot();

		result.Should().Be(path);
	}

	[Theory]
	[AutoData]
	public void PrefixRoot_UnRootedPath_ShouldPrefixRoot(string path)
	{
		string result = path.PrefixRoot();

		result.Should().NotBe(path);
		result.Should().EndWith(path);
	}
}