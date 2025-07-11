#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class CreateSymbolicLinkTests
{
	[Theory]
	[AutoData]
	public async Task CreateSymbolicLink_ShouldCreateSymbolicLink(
		string path, string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);

		FileSystem.Directory.CreateSymbolicLink(path, pathToTarget);

		await That(FileSystem.DirectoryInfo.New(path).Attributes)
			.HasFlag(FileAttributes.ReparsePoint);
	}

	[Theory]
	[AutoData]
	public async Task CreateSymbolicLink_SourceDirectoryAlreadyExists_ShouldThrowIOException(
		string path, string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateDirectory(path);

		void Act()
		{
			FileSystem.Directory.CreateSymbolicLink(path, pathToTarget);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<IOException>()
				.WithMessageContaining($"'{path}'").And
				.WithHResult(-2147024713);
		}
		else
		{
			await That(Act).Throws<IOException>()
				.WithMessageContaining($"'{path}'").And
				.WithHResult(17);
		}
	}

	[Theory]
	[AutoData]
	public async Task CreateSymbolicLink_TargetDirectoryMissing_ShouldNotThrowException(
		string path, string pathToTarget)
	{
		void Act()
		{
			FileSystem.Directory.CreateSymbolicLink(path, pathToTarget);
		}

		await That(Act).DoesNotThrow();
	}

	[Theory]
	[AutoData]
	public async Task CreateSymbolicLink_WithIllegalCharactersInTarget_ShouldThrowIOException(
		string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory(path);

		void Act()
		{
			FileSystem.Directory.CreateSymbolicLink(path, "bar_?_");
		}

		await That(Act).Throws<IOException>().WithHResult(-2147024713);
	}

	[Theory]
	[AutoData]
	public async Task CreateSymbolicLink_WithIllegalPath_ShouldThrowArgumentException_OnWindows(
		string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);

		void Act()
		{
			FileSystem.Directory.CreateSymbolicLink(" ", pathToTarget);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<ArgumentException>().WithParamName("path");
		}
		else
		{
			await That(Act).DoesNotThrow();
		}
	}

	[Theory]
	[AutoData]
	public async Task CreateSymbolicLink_WithIllegalTarget_ShouldNotThrowException(string path)
	{
		void Act()
		{
			FileSystem.Directory.CreateSymbolicLink(path, " ");
		}

		await That(Act).DoesNotThrow();
	}
}
#endif
