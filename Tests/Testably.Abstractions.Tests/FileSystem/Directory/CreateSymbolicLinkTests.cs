#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class CreateSymbolicLinkTests
{
	[Theory]
	[AutoData]
	public void CreateSymbolicLink_ShouldCreateSymbolicLink(
		string path, string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);

		FileSystem.Directory.CreateSymbolicLink(path, pathToTarget);

		FileSystem.DirectoryInfo.New(path).Attributes
			.HasFlag(FileAttributes.ReparsePoint)
			.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void CreateSymbolicLink_SourceDirectoryAlreadyExists_ShouldThrowIOException(
		string path, string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateSymbolicLink(path, pathToTarget);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<IOException>($"'{path}'", hResult: -2147024713);
		}
		else
		{
			exception.Should().BeException<IOException>($"'{path}'", hResult: 17);
		}
	}

	[Theory]
	[AutoData]
	public async Task CreateSymbolicLink_TargetDirectoryMissing_ShouldNotThrowException(
		string path, string pathToTarget)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateSymbolicLink(path, pathToTarget);
		});

		await That(exception).IsNull();
	}

	[Theory]
	[AutoData]
	public void CreateSymbolicLink_WithIllegalCharactersInTarget_ShouldThrowIOException(
		string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateSymbolicLink(path, "bar_?_");
		});

		exception.Should().BeException<IOException>(hResult: -2147024713);
	}

	[Theory]
	[AutoData]
	public async Task CreateSymbolicLink_WithIllegalPath_ShouldThrowArgumentException_OnWindows(
		string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateSymbolicLink(" ", pathToTarget);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<ArgumentException>(paramName: "path");
		}
		else
		{
			await That(exception).IsNull();
		}
	}

	[Theory]
	[AutoData]
	public async Task CreateSymbolicLink_WithIllegalTarget_ShouldNotThrowException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateSymbolicLink(path, " ");
		});

		await That(exception).IsNull();
	}
}
#endif
