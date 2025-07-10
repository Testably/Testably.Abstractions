#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class CreateAsSymbolicLinkTests
{
	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_ShouldCreateAsSymbolicLink(
		string path, string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);

		FileSystem.DirectoryInfo.New(path).CreateAsSymbolicLink(pathToTarget);

		await That(FileSystem.DirectoryInfo.New(path).Attributes)
			.HasFlag(FileAttributes.ReparsePoint);
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_SourceDirectoryAlreadyExists_ShouldThrowIOException(
		string path, string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.DirectoryInfo.New(path).CreateAsSymbolicLink(pathToTarget);
		});

		exception.Should().BeException<IOException>($"'{path}'",
			hResult: Test.RunsOnWindows ? -2147024713 : 17);
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_TargetDirectoryMissing_ShouldNotThrowException(
		string path, string pathToTarget)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.DirectoryInfo.New(path).CreateAsSymbolicLink(pathToTarget);
		});

		await That(exception).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_WithIllegalCharactersInTarget_ShouldThrowIOException(
		string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.DirectoryInfo.New(path).CreateAsSymbolicLink("bar_?_");
		});

		exception.Should().BeException<IOException>(hResult: -2147024713);
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_WithIllegalTarget_ShouldNotThrowException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.DirectoryInfo.New(path).CreateAsSymbolicLink(" ");
		});

		await That(exception).IsNull();
	}
}
#endif
