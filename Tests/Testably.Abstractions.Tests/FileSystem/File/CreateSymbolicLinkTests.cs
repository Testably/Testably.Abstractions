#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class CreateSymbolicLinkTests
{
	[Theory]
	[AutoData]
	public async Task CreateSymbolicLink_ShouldCreateSymbolicLink(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);

		FileSystem.File.CreateSymbolicLink(path, pathToTarget);

		await That(FileSystem.File.GetAttributes(path))
			.HasFlag(FileAttributes.ReparsePoint);
	}

	[Theory]
	[AutoData]
	public async Task CreateSymbolicLink_SourceFileAlreadyExists_ShouldThrowIOException(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.WriteAllText(path, "foo");

		void Act()
		{
			FileSystem.File.CreateSymbolicLink(path, pathToTarget);
		}

		await That(Act).Throws<IOException>()
			.WithMessageContaining($"'{path}'").And
			.WithHResult(Test.RunsOnWindows ? -2147024713 : 17);
	}

	[Theory]
	[AutoData]
	public async Task CreateSymbolicLink_TargetFileMissing_ShouldNotThrowException(
		string path, string pathToTarget)
	{
		void Act()
		{
			FileSystem.File.CreateSymbolicLink(path, pathToTarget);
		}

		await That(Act).DoesNotThrow();
	}
}
#endif
