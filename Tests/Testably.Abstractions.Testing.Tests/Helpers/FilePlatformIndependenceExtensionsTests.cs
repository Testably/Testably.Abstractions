using System.IO;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class FilePlatformIndependenceExtensionsTests
{
	[Fact]
	public void NormalizePath_Null_ShouldReturnNull()
	{
		string? path = null;

		path = path!.NormalizePath(new MockFileSystem());

		path.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void NormalizePath_Unix_RootedPath_ShouldRemoveDriveInfo(string part1)
	{
		Skip.If(Test.RunsOnWindows);

		MockFileSystem fileSystem = new();

		string path = fileSystem.GetDefaultDrive().Name.Replace("\\", "/") + part1;
		string expectedPath = part1.PrefixRoot(fileSystem);
		path = path.NormalizePath(fileSystem);

		path.Should().Be(expectedPath);
	}

	[SkippableTheory]
	[AutoData]
	public void NormalizePath_Unix_ShouldReplaceAltDirectorySeparatorChar(
		string part1, string part2)
	{
		Skip.If(Test.RunsOnWindows);

		char[] separatorChars =
		[
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar
		];
		foreach (char separatorChar in separatorChars)
		{
			string path = part1 + separatorChar + part2;
			string expectedPath = part1 + Path.DirectorySeparatorChar + part2;
			path = path.NormalizePath(new MockFileSystem());

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
		[
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar
		];
		foreach (char separatorChar in separatorChars)
		{
			string path = part1 + separatorChar + part2;
			path = path.NormalizePath(new MockFileSystem());

			path.Should().Be(path);
		}
	}

	[Fact]
	public void PrefixRoot_Null_ShouldReturnNull()
	{
		string? path = null;

		string result = path!.PrefixRoot(new MockFileSystem());

		result.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void PrefixRoot_RootedPath_ShouldReturnPath(string path)
	{
		path = path.PrefixRoot(new MockFileSystem());

		string result = path.PrefixRoot(new MockFileSystem());

		result.Should().Be(path);
	}

	[Theory]
	[AutoData]
	public void PrefixRoot_UnRootedPath_ShouldPrefixRoot(string path)
	{
		string result = path.PrefixRoot(new MockFileSystem());

		result.Should().NotBe(path);
		result.Should().EndWith(path);
	}
}
