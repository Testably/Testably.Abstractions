using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class RemoveCurrentDirectoryTests
{
	[Fact]
	public async Task MoveCurrentDirectory_ShouldThrowIOException_WithoutPath()
	{
		Skip.IfNot(Test.RunsOnWindows);

		IDirectoryInfo directoryInfo
			= FileSystem.DirectoryInfo.New(FileSystem.Directory.GetCurrentDirectory());

		await That(() => directoryInfo.MoveTo("new")).ThrowsExactly<IOException>().Which.HasMessage(
			"The process cannot access the file because it is being used by another process."
		);
	}

	[Fact]
	public async Task DeleteCurrentDirectory_ShouldThrowIOException_WithPath()
	{
		Skip.IfNot(Test.RunsOnWindows);

		IDirectoryInfo directoryInfo
			= FileSystem.DirectoryInfo.New(FileSystem.Directory.GetCurrentDirectory());

		await That(() => directoryInfo.Delete()).ThrowsExactly<IOException>().Which.HasMessage(
			$"The process cannot access the file '{directoryInfo.FullName}' because it is being used by another process."
		);
	}
}
