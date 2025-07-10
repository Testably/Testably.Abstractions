#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemInfo;

[FileSystemTests]
public partial class CreateAsSymbolicLinkTests
{
	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_ShouldCreateSymbolicLink(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		fileInfo.CreateAsSymbolicLink(pathToTarget);

		await That(FileSystem.File.GetAttributes(path))
			.HasFlag(FileAttributes.ReparsePoint);
	}

	[Theory]
	[AutoData]
	public async Task CreateAsSymbolicLink_SourceFileAlreadyExists_ShouldThrowIOException(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.WriteAllText(path, "foo");
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		void Act()
		{
			fileInfo.CreateAsSymbolicLink(pathToTarget);
		}

		await That(Act).Throws<IOException>()
			.WithMessageContaining($"'{path}'").And
			.WithHResult(Test.RunsOnWindows ? -2147024713 : 17);
	}
}
#endif
