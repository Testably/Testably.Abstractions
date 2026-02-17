using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class RemoveCurrentDirectoryTests
{
	[Fact]
	public async Task MoveCurrentDirectory_ShouldThrowIOException_WithoutPath()
	{
		Skip.IfNot(Test.RunsOnWindows);

		string directory = FileSystem.Directory.GetCurrentDirectory();

		await That(() => FileSystem.Directory.Move(directory, "new")).ThrowsExactly<IOException>()
			.Which.HasMessage(
				"The process cannot access the file because it is being used by another process."
			);
	}

	[Fact]
	public async Task DeleteCurrentDirectory_ShouldThrowIOException_WithPath()
	{
		Skip.IfNot(Test.RunsOnWindows);

		string directory = FileSystem.Directory.GetCurrentDirectory();

		await That(() => FileSystem.Directory.Delete(directory)).ThrowsExactly<IOException>().Which
			.HasMessage(
				$"The process cannot access the file '{directory}' because it is being used by another process."
			);
	}
}
