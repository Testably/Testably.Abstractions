#if FEATURE_FILESYSTEM_LINK
using System.IO;
#if NET8_0_OR_GREATER
using System.Runtime.InteropServices;
#endif
using System.Text.RegularExpressions;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

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
		FileSystem.Directory.CreateDirectory(pathToFinalTarget);
		string previousPath = pathToFinalTarget;

		for (int i = 0; i < maxLinks; i++)
		{
			string newPath = $"{path}-{i}";
			IDirectoryInfo linkDirectoryInfo = FileSystem.DirectoryInfo.New(newPath);
			linkDirectoryInfo.CreateAsSymbolicLink(previousPath);
			previousPath = newPath;
		}

		IDirectoryInfo directoryInfo = FileSystem.DirectoryInfo.New(previousPath);

		void Act()
		{
			_ = directoryInfo.ResolveLinkTarget(true);
		}

		await That(Act).Throws<IOException>()
			.WithHResult(Test.RunsOnWindows ? -2147022975 : -2146232800).And
			.WithMessageContaining($"'{directoryInfo.FullName}'");
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
	public async Task ResolveLinkTarget_ShouldReturnImmediateFile(string path, string pathToTarget)
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
	public async Task ResolveLinkTarget_OfDifferentTypes_ShouldThrow(
		string directoryName,
		string fileLinkName,
		string directoryLinkName
	)
	{
		IDirectoryInfo targetDirectory = FileSystem.Directory.CreateDirectory(directoryName);

		IFileSystemInfo fileSymLink = FileSystem.File.CreateSymbolicLink(
			fileLinkName, targetDirectory.FullName
		);

		IFileSystemInfo dirSymLink = FileSystem.Directory.CreateSymbolicLink(
			directoryLinkName, fileSymLink.FullName
		);

		if (Test.RunsOnWindows)
		{
			const int errorCode = 267; // Magic number, error code was discovered via debugger
			Marshal.SetLastPInvokeError(errorCode);

			string errorMessage =
#if NET8_0_OR_GREATER
					Marshal.GetPInvokeErrorMessage(errorCode)
#else
						"The directory name is invalid." // Marshal.GetPInvokeErrorMessage is only available for .NET 7 and above
#endif
				;
			await That(() => dirSymLink.ResolveLinkTarget(true)).Throws<IOException>()
				.WithMessage(
					$@"^{Regex.Escape(errorMessage)} \: \'{Regex.Escape(dirSymLink.FullName)}\'\.?$"
				).AsRegex().And.WithHResult(-2147024629);
		}
		else
		{
			await That(dirSymLink.ResolveLinkTarget(true)?.FullName)
				.IsEqualTo(targetDirectory.FullName);
		}
	}
}
#endif
