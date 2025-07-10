#if FEATURE_FILESYSTEM_LINK
using System.Globalization;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

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
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.CreateSymbolicLink(path, targetFullPath);

		IFileSystemInfo? target =
			FileSystem.File.ResolveLinkTarget(path, false);

		await That(target!.FullName).IsEqualTo(targetFullPath);
		await That(target.Exists).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_FileWithDifferentCase_ShouldReturnPathToMissingFile(
		string path, string pathToTarget, string contents)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.File.WriteAllText(pathToTarget, "foo");
		FileSystem.File.CreateSymbolicLink(path, targetFullPath);
		FileSystem.File.Delete(pathToTarget);
		FileSystem.File.WriteAllText(pathToTarget.ToUpper(CultureInfo.InvariantCulture), contents);

		IFileSystemInfo? target =
			FileSystem.File.ResolveLinkTarget(path, false);

		if (!Test.RunsOnLinux)
		{
			await That(target!.FullName).IsEqualTo(targetFullPath);
			await That(target.Exists).IsTrue();
			await That(FileSystem.File.ReadAllText(target.FullName)).IsEqualTo(contents);
		}
		else
		{
			await That(target!.FullName).IsEqualTo(targetFullPath);
			await That(target.Exists).IsFalse();
		}
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_FinalTarget_MultipleSteps_ShouldFollowSymbolicLinkToFinalTarget(
		string path, string pathToFinalTarget)
	{
		int maxLinks = 10;

		FileSystem.File.WriteAllText(pathToFinalTarget, null);
		string previousPath = pathToFinalTarget;
		for (int i = 0; i < maxLinks; i++)
		{
			string newPath = $"{path}-{i}";
			FileSystem.File.CreateSymbolicLink(newPath,
				FileSystem.Path.Combine(BasePath, previousPath));
			previousPath = newPath;
		}

		IFileSystemInfo? target =
			FileSystem.File.ResolveLinkTarget(previousPath, true);

		await That(target).IsNotNull();
		await That(target!.FullName).IsEqualTo(FileSystem.Path.GetFullPath(pathToFinalTarget));
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_FinalTarget_ShouldFollowSymbolicLinkToFinalTarget(
		string path, string pathToFinalTarget)
	{
		int maxLinks = MaxResolveLinks;

		FileSystem.File.WriteAllText(pathToFinalTarget, null);
		string previousPath = pathToFinalTarget;
		for (int i = 0; i < maxLinks; i++)
		{
			string newPath = $"{path}-{i}";
			FileSystem.File.CreateSymbolicLink(newPath,
				FileSystem.Path.Combine(BasePath, previousPath));
			previousPath = newPath;
		}

		IFileSystemInfo? target =
			FileSystem.File.ResolveLinkTarget(previousPath, true);

		await That(target!.FullName).IsEqualTo(FileSystem.Path.GetFullPath(pathToFinalTarget));
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_FinalTargetWithTooManyLevels_ShouldThrowIOException(
		string path, string pathToFinalTarget)
	{
		int maxLinks = MaxResolveLinks + 1;
		FileSystem.File.WriteAllText(pathToFinalTarget, null);
		string previousPath = pathToFinalTarget;
		for (int i = 0; i < maxLinks; i++)
		{
			string newPath = $"{path}-{i}";
			FileSystem.File.CreateSymbolicLink(newPath,
				FileSystem.Path.Combine(BasePath, previousPath));
			previousPath = newPath;
		}

		void Act()
		{
			_ = FileSystem.File.ResolveLinkTarget(previousPath, true);
		}

		await That(Act).Throws<IOException>()
			.WithMessageContaining($"'{previousPath}'").And
			.WithHResult(Test.RunsOnWindows ? -2147022975 : -2146232800);
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_MissingFileAtBeginningOfLinkChain_ShouldReturnPathToMissingFile(
		string path, string pathToFinalTarget, string pathToMissingFile)
	{
		FileSystem.File.WriteAllText(pathToFinalTarget, null);
		FileSystem.File.CreateSymbolicLink(pathToMissingFile,
			FileSystem.Path.Combine(BasePath, pathToFinalTarget));
		FileSystem.File.CreateSymbolicLink(path,
			FileSystem.Path.Combine(BasePath, pathToMissingFile));
		FileSystem.File.Delete(pathToMissingFile);

		IFileSystemInfo? target =
			FileSystem.File.ResolveLinkTarget(path, true);

		await That(target!.FullName).IsEqualTo(FileSystem.Path.GetFullPath(pathToMissingFile));
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_MissingFileInLinkChain_ShouldReturnPathToMissingFile(
		string path,
		string pathToIntermediateTarget,
		string pathToFinalTarget,
		string pathToMissingFile)
	{
		FileSystem.File.WriteAllText(pathToFinalTarget, null);
		FileSystem.File.CreateSymbolicLink(pathToMissingFile,
			FileSystem.Path.Combine(BasePath, pathToFinalTarget));
		FileSystem.File.CreateSymbolicLink(pathToIntermediateTarget,
			FileSystem.Path.Combine(BasePath, pathToMissingFile));
		FileSystem.File.CreateSymbolicLink(path,
			FileSystem.Path.Combine(BasePath, pathToIntermediateTarget));
		FileSystem.File.Delete(pathToMissingFile);

		IFileSystemInfo? target =
			FileSystem.File.ResolveLinkTarget(path, true);

		await That(target!.FullName).IsEqualTo(FileSystem.Path.GetFullPath(pathToMissingFile));
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_NormalDirectory_ShouldReturnNull(
		string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		IFileSystemInfo? target =
			FileSystem.File.ResolveLinkTarget(path, false);

		await That(target).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_NormalFile_ShouldReturnNull(
		string path)
	{
		FileSystem.File.WriteAllText(path, null);

		IFileSystemInfo? target =
			FileSystem.File.ResolveLinkTarget(path, false);

		await That(target).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_RelativePath_ShouldFollowSymbolicLinkUnderWindows(
		string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.CreateSymbolicLink(path, targetFullPath);

		IFileSystemInfo? target =
			FileSystem.File.ResolveLinkTarget(path, false);

		await That(target!.FullName).IsEqualTo(targetFullPath);
		await That(target.Exists).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_TargetDeletedAfterLinkCreation_ShouldReturnNull(
		string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.CreateSymbolicLink(path, targetFullPath);
		FileSystem.File.Delete(pathToTarget);

		IFileSystemInfo? target =
			FileSystem.File.ResolveLinkTarget(path, false);

		await That(target!.FullName).IsEqualTo(targetFullPath);

		await That(target.Exists).IsFalse();
	}
}
#endif
