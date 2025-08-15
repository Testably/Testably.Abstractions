#if FEATURE_FILESYSTEM_LINK
using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class ResolveLinkTargetTests
{
	#region Test Setup

	/// <summary>
	///     The maximum number of symbolic links that are followed.<br />
	///     <see href="https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.resolvelinktarget?view=net-6.0#remarks" />
	/// </summary>
	private int MaxResolveLinks => Test.RunsOnWindows ? 63 : 40;

	#endregion

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_FinalTargetWithTooManyLevels_ShouldThrowIOException(
		string path,
		string pathToFinalTarget
	)
	{
		int maxLinks = MaxResolveLinks + 1;

		await FileSystem.File.WriteAllTextAsync(
			pathToFinalTarget, string.Empty, CancellationToken.None
		);

		string previousPath = pathToFinalTarget;

		for (int i = 0; i < maxLinks; i++)
		{
			string newPath = $"{path}-{i}";
			IFileInfo linkFileInfo = FileSystem.FileInfo.New(newPath);
			linkFileInfo.CreateAsSymbolicLink(previousPath);
			previousPath = newPath;
		}

		IFileInfo fileInfo = FileSystem.FileInfo.New(previousPath);

		void Act()
		{
			_ = fileInfo.ResolveLinkTarget(true);
		}

		await That(Act).Throws<IOException>()
			.WithHResult(Test.RunsOnWindows ? -2147022975 : -2146232800).And
			.WithMessageContaining($"'{fileInfo.FullName}'");
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_ShouldReturnNull(string path)
	{
		IFileInfo targetFile = FileSystem.FileInfo.New(path);
		await targetFile.Create().DisposeAsync();

		IFileSystemInfo? resolvedTarget = targetFile.ResolveLinkTarget(false);

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
		IFileSystemInfo innerLink = FileSystem.File.CreateSymbolicLink(pathToLink, pathToTarget);
		IFileSystemInfo outerLink = FileSystem.File.CreateSymbolicLink(path, pathToLink);

		IFileSystemInfo? resolvedTarget = outerLink.ResolveLinkTarget(true);

		await That(resolvedTarget?.Name).IsEqualTo(innerLink.LinkTarget);
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_ShouldReturnImmediateFile(string path, string pathToTarget)
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
	public async Task ResolveLinkTarget_WithChainedLink_ShouldReturnImmediateLink(
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
	public async Task ResolveLinkTarget_ShouldReturnFinalFile(string path, string pathToTarget)
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
	public async Task ResolveLinkTarget_WithChainedLink_ShouldReturnFinalFile(
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

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_OfDifferentTypes_ShouldThrow(
		string fileName,
		string directoryName,
		string fileLinkName
	)
	{
		IFileInfo targetFile = FileSystem.FileInfo.New(fileName);
		await targetFile.Create().DisposeAsync();

		IFileSystemInfo dirSymLink
			= FileSystem.Directory.CreateSymbolicLink(directoryName, targetFile.FullName);

		IFileSystemInfo fileSymLink
			= FileSystem.File.CreateSymbolicLink(fileLinkName, dirSymLink.FullName);

		string? Act()
		{
			return fileSymLink.ResolveLinkTarget(true)?.FullName;
		}

		if (Test.RunsOnWindows)
		{
			await That(Act)
				.Throws<UnauthorizedAccessException>().WithMessage(
					$"Access to the path '{fileSymLink.FullName}' is denied."
				);
		}
		else
		{
			await That(Act()).IsEqualTo(targetFile.FullName);
		}
	}
}
#endif
