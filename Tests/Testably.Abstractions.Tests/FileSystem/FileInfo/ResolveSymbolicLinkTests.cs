using NSubstitute.ExceptionExtensions;
using System.IO;
#if FEATURE_FILESYSTEM_LINK
namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class ResolveSymbolicLinkTests
{
	[Theory]
	[AutoData]
	public async Task ResolveSymbolicLink_ShouldThrow(string path)
	{
		IFileSystemInfo link = FileSystem.File.CreateSymbolicLink(path, path + "-start");

		// UNIX allows 43 and Windows 63 nesting, so 70 is plenty to force the exception
		for (int i = 0; i < 70; i++)
		{
			link = FileSystem.File.CreateSymbolicLink($"{path}{i}", link.Name);
		}

		await That(() => link.ResolveLinkTarget(true)).Throws<IOException>();
	}

	[Theory]
	[AutoData]
	public async Task ResolveSymbolicLink_ShouldReturnNull(string path)
	{
		IFileInfo targetFile = FileSystem.FileInfo.New(path);
		await targetFile.Create().DisposeAsync();

		IFileSystemInfo? resolvedTarget = targetFile.ResolveLinkTarget(false);

		await That(resolvedTarget).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task ResolveSymbolicLink_WithChainedLink_ShouldReturnNull(
		string path,
		string pathToLink,
		string pathToTarget
	)
	{
		IFileSystemInfo innerLink = FileSystem.File.CreateSymbolicLink(pathToLink, pathToTarget);
		IFileSystemInfo outerLink = FileSystem.File.CreateSymbolicLink(path, pathToLink);

		IFileSystemInfo? resolvedTarget = outerLink.ResolveLinkTarget(true);

		await That(resolvedTarget?.Name).IsEqualTo(innerLink.LinkTarget);
	}

	[Theory]
	[AutoData]
	public async Task ResolveSymbolicLink_ShouldReturnImmediateFile(
		string path,
		string pathToTarget
	)
	{
		IFileInfo targetFile = FileSystem.FileInfo.New(pathToTarget);
		await targetFile.Create().DisposeAsync();

		IFileSystemInfo symbolicLink
			= FileSystem.File.CreateSymbolicLink(path, targetFile.FullName);

		IFileSystemInfo? resolvedTarget = symbolicLink.ResolveLinkTarget(false);

		await That(resolvedTarget?.FullName).IsEqualTo(targetFile.FullName);
	}

	[Theory]
	[AutoData]
	public async Task ResolveSymbolicLink_WithChainedLink_ShouldReturnImmediateLink(
		string path,
		string pathToLink,
		string pathToTarget
	)
	{
		await FileSystem.FileInfo.New(pathToTarget).Create().DisposeAsync();

		IFileSystemInfo innerLink = FileSystem.File.CreateSymbolicLink(pathToLink, pathToTarget);
		IFileSystemInfo outerLink = FileSystem.File.CreateSymbolicLink(path, pathToLink);

		IFileSystemInfo? resolvedTarget = outerLink.ResolveLinkTarget(false);

		await That(resolvedTarget?.FullName).IsEqualTo(innerLink.FullName);
	}

	[Theory]
	[AutoData]
	public async Task ResolveSymbolicLink_ShouldReturnFinalFile(string path, string pathToTarget)
	{
		IFileInfo targetFile = FileSystem.FileInfo.New(pathToTarget);
		await targetFile.Create().DisposeAsync();

		IFileSystemInfo symbolicLink
			= FileSystem.File.CreateSymbolicLink(path, targetFile.FullName);

		IFileSystemInfo? resolvedTarget = symbolicLink.ResolveLinkTarget(true);

		await That(resolvedTarget?.FullName).IsEqualTo(targetFile.FullName);
	}

	[Theory]
	[AutoData]
	public async Task ResolveSymbolicLink_WithChainedLink_ShouldReturnFinalFile(
		string path,
		string pathToLink,
		string pathToTarget
	)
	{
		IFileInfo targetFile = FileSystem.FileInfo.New(pathToTarget);
		await targetFile.Create().DisposeAsync();

		FileSystem.File.CreateSymbolicLink(pathToLink, targetFile.FullName);
		IFileSystemInfo outerLink = FileSystem.File.CreateSymbolicLink(path, pathToLink);

		IFileSystemInfo? resolvedTarget = outerLink.ResolveLinkTarget(true);

		await That(resolvedTarget?.FullName).IsEqualTo(targetFile.FullName);
	}
}
#endif
