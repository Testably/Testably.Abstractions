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

		void Act()
		{
			FileSystem.DirectoryInfo.New(path).CreateAsSymbolicLink(pathToTarget);
		}

		await That(Act).Throws<IOException>()
			.WithMessageContaining($"'{path}'").And
			.WithHResult(Test.RunsOnWindows ? -2147024713 : 17);
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_TargetDirectoryMissing_ShouldNotThrowException(
		string path, string pathToTarget)
	{
		void Act()
		{
			FileSystem.DirectoryInfo.New(path).CreateAsSymbolicLink(pathToTarget);
		}

		await That(Act).DoesNotThrow();
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_WithIllegalCharactersInTarget_ShouldThrowIOException(
		string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory(path);

		void Act()
		{
			FileSystem.DirectoryInfo.New(path).CreateAsSymbolicLink("bar_?_");
		}

		await That(Act).Throws<IOException>().WithHResult(-2147024713);
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_WithIllegalTarget_ShouldNotThrowException(string path)
	{
		void Act()
		{
			FileSystem.DirectoryInfo.New(path).CreateAsSymbolicLink(" ");
		}

		await That(Act).DoesNotThrow();
	}
}
#endif
