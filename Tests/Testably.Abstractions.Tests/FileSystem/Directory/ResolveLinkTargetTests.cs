#if FEATURE_FILESYSTEM_LINK
using System.Globalization;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class ResolveLinkTargetTests
{
	#region Test Setup

	/// <summary>
	///     The maximum number of symbolic links that are followed.<br />
	///     <see href="https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.resolvelinktarget?view=net-6.0#remarks" />
	/// </summary>
	private int MaxResolveLinks =>
		Test.RunsOnWindows ? 63 : 40;

	#endregion

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_AbsolutePath_ShouldFollowSymbolicLink(
		string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateSymbolicLink(path, targetFullPath);

		IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		await That(target!.FullName).IsEqualTo(targetFullPath);
		await That(target.Exists).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_FileWithDifferentCase_ShouldReturnPathToMissingDirectory(
			string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateSymbolicLink(path, targetFullPath);
		FileSystem.Directory.Delete(pathToTarget);
		FileSystem.Directory.CreateDirectory(pathToTarget.ToUpper(CultureInfo.InvariantCulture));

		IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		if (!Test.RunsOnLinux)
		{
			await That(target!.FullName).IsEqualTo(targetFullPath);
			await That(target.Exists).IsTrue();
		}
		else
		{
			await That(target!.FullName).IsEqualTo(targetFullPath);
			await That(target.Exists).IsFalse();
		}
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_FinalTarget_ShouldFollowSymbolicLinkToFinalTarget(
		string path, string pathToFinalTarget)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		int maxLinks = MaxResolveLinks;

		FileSystem.Directory.CreateDirectory(pathToFinalTarget);
		string previousPath = pathToFinalTarget;
		for (int i = 0; i < maxLinks; i++)
		{
			string newPath = $"{path}-{i}";
			FileSystem.Directory.CreateSymbolicLink(newPath,
				FileSystem.Path.Combine(BasePath, previousPath));
			previousPath = newPath;
		}

		IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(previousPath, true);

		await That(target!.FullName).IsEqualTo(FileSystem.Path.GetFullPath(pathToFinalTarget));
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_FinalTargetWithTooManyLevels_ShouldThrowIOException(
		string path, string pathToFinalTarget)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		int maxLinks = MaxResolveLinks + 1;
		FileSystem.Directory.CreateDirectory(pathToFinalTarget);
		string previousPath = pathToFinalTarget;
		for (int i = 0; i < maxLinks; i++)
		{
			string newPath = $"{path}-{i}";
			FileSystem.Directory.CreateSymbolicLink(newPath,
				FileSystem.Path.Combine(BasePath, previousPath));
			previousPath = newPath;
		}

		void Act()
		{
			_ = FileSystem.Directory.ResolveLinkTarget(previousPath, true);
		}

		await That(Act).Throws<IOException>()
			.WithMessageContaining($"'{previousPath}'").And
			.WithHResult(Test.RunsOnWindows ? -2147022975 : -2146232800);
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_MissingDirectoryInLinkChain_ShouldReturnPathToMissingDirectory(
			string path, string pathToFinalTarget, string pathToMissingDirectory)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Directory.CreateDirectory(pathToFinalTarget);
		FileSystem.Directory.CreateSymbolicLink(pathToMissingDirectory,
			FileSystem.Path.Combine(BasePath, pathToFinalTarget));
		FileSystem.Directory.CreateSymbolicLink(path,
			FileSystem.Path.Combine(BasePath, pathToMissingDirectory));
		FileSystem.Directory.Delete(pathToMissingDirectory);

		IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, true);

		await That(target!.FullName).IsEqualTo(FileSystem.Path.GetFullPath(pathToMissingDirectory));
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_NormalDirectory_ShouldReturnNull(
		string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		await That(target).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_NormalFile_ShouldReturnNull(
		string path)
	{
		FileSystem.File.WriteAllText(path, null);

		IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		await That(target).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_RelativePath_ShouldFollowSymbolicLinkUnderWindows(
		string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateSymbolicLink(path, targetFullPath);

		IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		await That(target!.FullName).IsEqualTo(targetFullPath);
		await That(target.Exists).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_TargetDeletedAfterLinkCreation_ShouldReturnNull(
		string path, string pathToTarget)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateSymbolicLink(path, targetFullPath);
		FileSystem.Directory.Delete(pathToTarget);

		IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		await That(target!.FullName).IsEqualTo(targetFullPath);

		await That(target.Exists).IsFalse();
	}
}
#endif
