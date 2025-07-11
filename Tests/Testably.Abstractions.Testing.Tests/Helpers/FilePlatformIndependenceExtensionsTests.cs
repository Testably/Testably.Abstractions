using System.IO;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class FilePlatformIndependenceExtensionsTests
{
	[Fact]
	public async Task NormalizePath_Null_ShouldReturnNull()
	{
		string? path = null;

		path = path!.NormalizePath(new MockFileSystem());

		await That(path).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task NormalizePath_Unix_RootedPath_ShouldRemoveDriveInfo(string part1)
	{
		Skip.If(Test.RunsOnWindows);

		MockFileSystem fileSystem = new();

		string path = fileSystem.GetDefaultDrive().Name.Replace('\\', '/') + part1;
		string expectedPath = part1.PrefixRoot(fileSystem);
		path = path.NormalizePath(fileSystem);

		await That(path).IsEqualTo(expectedPath);
	}

	[Theory]
	[AutoData]
	public async Task NormalizePath_Unix_ShouldReplaceAltDirectorySeparatorChar(
		string part1, string part2)
	{
		Skip.If(Test.RunsOnWindows);

		char[] separatorChars =
		[
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar,
		];
		foreach (char separatorChar in separatorChars)
		{
			string path = part1 + separatorChar + part2;
			string expectedPath = part1 + Path.DirectorySeparatorChar + part2;
			path = path.NormalizePath(new MockFileSystem());

			await That(path).IsEqualTo(expectedPath);
		}
	}

	[Theory]
	[AutoData]
	public async Task NormalizePath_Windows_ShouldAlsoKeepAltDirectorySeparatorChar(
		string part1, string part2)
	{
		Skip.IfNot(Test.RunsOnWindows);

		char[] separatorChars =
		[
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar,
		];
		foreach (char separatorChar in separatorChars)
		{
			string path = part1 + separatorChar + part2;
			path = path.NormalizePath(new MockFileSystem());

			await That(path).IsEqualTo(path);
		}
	}

	[Fact]
	public async Task PrefixRoot_Null_ShouldReturnNull()
	{
		string? path = null;

		string result = path!.PrefixRoot(new MockFileSystem());

		await That(result).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task PrefixRoot_RootedPath_ShouldReturnPath(string path)
	{
		path = path.PrefixRoot(new MockFileSystem());

		string result = path.PrefixRoot(new MockFileSystem());

		await That(result).IsEqualTo(path);
	}

	[Theory]
	[AutoData]
	public async Task PrefixRoot_UnRootedPath_ShouldPrefixRoot(string path)
	{
		string result = path.PrefixRoot(new MockFileSystem());

		await That(result).IsNotEqualTo(path);
		await That(result).EndsWith(path);
	}
}
