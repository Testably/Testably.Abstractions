using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

/// <summary>
///     <see cref="FileShare" /> is a flags enum, so <see cref="FileShare.ReadWrite" /> combined with
///     <see cref="FileShare.Delete" /> shares reading and writing exactly as <see cref="FileShare.ReadWrite" /> does.
/// </summary>
[FileSystemTests]
public class ShareReadWriteWithDeleteTests(FileSystemTestData testData)
	: FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments(FileShare.ReadWrite)]
	[AutoArguments(FileShare.ReadWrite | FileShare.Delete)]
	public async Task Open_ForReadWrite_WhenShareIncludesReadWrite_ShouldBeAllowed(
		FileShare share, string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream first = FileSystem.File.Open(path,
			FileMode.Open, FileAccess.ReadWrite, share);

		void Act()
		{
			using FileSystemStream second = FileSystem.File.Open(path,
				FileMode.Open, FileAccess.ReadWrite, share);
		}

		await That(Act).DoesNotThrow();
	}

	[Test]
	[AutoArguments(FileShare.Read)]
	[AutoArguments(FileShare.Read | FileShare.Delete)]
	public async Task Open_ForReadWrite_WhenShareOmitsWrite_ShouldThrowIOException(
		FileShare share, string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows, "only Windows enforces the file share");

		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream first = FileSystem.File.Open(path,
			FileMode.Open, FileAccess.ReadWrite, share);

		void Act()
		{
			using FileSystemStream second = FileSystem.File.Open(path,
				FileMode.Open, FileAccess.ReadWrite, share);
		}

		await That(Act).Throws<IOException>();
	}
}
