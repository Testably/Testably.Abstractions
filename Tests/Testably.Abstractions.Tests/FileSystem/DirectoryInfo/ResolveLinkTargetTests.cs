#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class ResolveLinkTargetTests
{
	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_ShouldThrow(string path)
	{
		IFileSystemInfo link = FileSystem.Directory.CreateSymbolicLink(path, path + "-start");

		// UNIX allows 43 and Windows 63 nesting, so 70 is plenty to force the exception
		for (int i = 0; i < 70; i++)
		{
			link = FileSystem.Directory.CreateSymbolicLink($"{path}{i}", link.Name);
		}

		await That(() => link.ResolveLinkTarget(true)).Throws<IOException>();
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_ShouldReturnNull(string path)
	{
		IDirectoryInfo targetDir = FileSystem.DirectoryInfo.New(path);
		targetDir.Create();

		IFileSystemInfo? resolvedTarget = targetDir.ResolveLinkTarget(false);

		await That(resolvedTarget).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_WithChainedLink_ShouldReturnNull(
		string path,
		string pathToLink,
		string pathToTarget
	)
	{
		IFileSystemInfo innerLink
			= FileSystem.Directory.CreateSymbolicLink(pathToLink, pathToTarget);

		IFileSystemInfo outerLink = FileSystem.Directory.CreateSymbolicLink(path, pathToLink);

		IFileSystemInfo? resolvedTarget = outerLink.ResolveLinkTarget(true);

		await That(resolvedTarget?.Name).IsEqualTo(innerLink.LinkTarget);
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_ShouldReturnImmediateFile(
		string path,
		string pathToTarget
	)
	{
		IDirectoryInfo targetDir = FileSystem.DirectoryInfo.New(pathToTarget);
		targetDir.Create();

		IFileSystemInfo symbolicLink
			= FileSystem.Directory.CreateSymbolicLink(path, targetDir.FullName);

		IFileSystemInfo? resolvedTarget = symbolicLink.ResolveLinkTarget(false);

		await That(resolvedTarget?.FullName).IsEqualTo(targetDir.FullName);
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_WithChainedLink_ShouldReturnImmediateLink(
		string path,
		string pathToLink,
		string pathToTarget
	)
	{
		FileSystem.DirectoryInfo.New(pathToTarget).Create();

		IFileSystemInfo innerLink
			= FileSystem.Directory.CreateSymbolicLink(pathToLink, pathToTarget);

		IFileSystemInfo outerLink = FileSystem.Directory.CreateSymbolicLink(path, pathToLink);

		IFileSystemInfo? resolvedTarget = outerLink.ResolveLinkTarget(false);

		await That(resolvedTarget?.FullName).IsEqualTo(innerLink.FullName);
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_ShouldReturnFinalFile(string path, string pathToTarget)
	{
		IDirectoryInfo targetDir = FileSystem.DirectoryInfo.New(pathToTarget);
		targetDir.Create();

		IFileSystemInfo symbolicLink
			= FileSystem.Directory.CreateSymbolicLink(path, targetDir.FullName);

		IFileSystemInfo? resolvedTarget = symbolicLink.ResolveLinkTarget(true);

		await That(resolvedTarget?.FullName).IsEqualTo(targetDir.FullName);
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_WithChainedLink_ShouldReturnFinalFile(
		string path,
		string pathToLink,
		string pathToTarget
	)
	{
		IDirectoryInfo targetDir = FileSystem.DirectoryInfo.New(pathToTarget);
		targetDir.Create();

		IFileSystemInfo innerLink
			= FileSystem.Directory.CreateSymbolicLink(pathToLink, targetDir.FullName);

		IFileSystemInfo outerLink
			= FileSystem.Directory.CreateSymbolicLink(path, innerLink.FullName);

		IFileSystemInfo? resolvedTarget = outerLink.ResolveLinkTarget(true);

		await That(resolvedTarget?.FullName).IsEqualTo(targetDir.FullName);
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_OfDifferentTypes_ShouldThrow(string directoryName, string fileLinkName, string directoryLinkName)
	{
		IDirectoryInfo targetDirectory = FileSystem.Directory.CreateDirectory(directoryName);

		IFileSystemInfo fileSymLink = FileSystem.File.CreateSymbolicLink(fileLinkName, targetDirectory.FullName);

		IFileSystemInfo dirSymLink = FileSystem.Directory.CreateSymbolicLink(directoryLinkName, fileSymLink.FullName);

		if(Test.RunsOnWindows)
		{
			await That(() => dirSymLink.ResolveLinkTarget(true))
				.Throws<IOException>().Which
				.Satisfies(x => x.Message.Contains(dirSymLink.FullName, StringComparison.Ordinal));
		}
		else
		{
			await That(() => dirSymLink.ResolveLinkTarget(true))
				.DoesNotThrow<IOException>();
		}
	}
}
#endif
