#if FEATURE_FILESYSTEM_LINK
using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ResolveLinkTargetTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	#region Test Setup

	/// <summary>
	///     The maximum number of symbolic links that are followed.<br />
	///     <see href="https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.resolvelinktarget?view=net-6.0#remarks" />
	/// </summary>
	private static int MaxResolveLinks =>
		Test.RunsOnWindows ? 63 : 40;

	#endregion

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_AbsolutePath_ShouldFollowSymbolicLink(
		string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateSymbolicLink(path, targetFullPath);

		IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		target!.FullName.Should().Be(targetFullPath);
		target.Exists.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void
		ResolveLinkTarget_FileWithDifferentCase_ShouldReturnPathToMissingDirectory(
			string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateSymbolicLink(path, targetFullPath);
		FileSystem.Directory.Delete(pathToTarget);
		FileSystem.Directory.CreateDirectory(pathToTarget.ToUpper());

		IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		if (!Test.RunsOnLinux)
		{
			target!.FullName.Should().Be(targetFullPath);
			target.Exists.Should().BeTrue();
		}
		else
		{
			target!.FullName.Should().Be(targetFullPath);
			target.Exists.Should().BeFalse();
		}
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_FinalTarget_ShouldFollowSymbolicLinkToFinalTarget(
		string path, string pathToFinalTarget)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		int maxLinks = MaxResolveLinks;

		FileSystem.Directory.CreateDirectory(pathToFinalTarget);
		string previousPath = pathToFinalTarget;
		for (int i = 0; i < maxLinks; i++)
		{
			string newPath = $"{path}-{i}";
			FileSystem.Directory.CreateSymbolicLink(newPath,
				System.IO.Path.Combine(BasePath, previousPath));
			previousPath = newPath;
		}

		IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(previousPath, true);

		target!.FullName.Should().Be(FileSystem.Path.GetFullPath(pathToFinalTarget));
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_FinalTargetWithTooManyLevels_ShouldThrowIOException(
		string path, string pathToFinalTarget)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		int maxLinks = MaxResolveLinks + 1;
		FileSystem.Directory.CreateDirectory(pathToFinalTarget);
		string previousPath = pathToFinalTarget;
		for (int i = 0; i < maxLinks; i++)
		{
			string newPath = $"{path}-{i}";
			FileSystem.Directory.CreateSymbolicLink(newPath,
				System.IO.Path.Combine(BasePath, previousPath));
			previousPath = newPath;
		}

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.Directory.ResolveLinkTarget(previousPath, true);
		});

		exception.Should().BeOfType<IOException>()
		   .Which.Message.Should().Contain($"'{previousPath}'");
		exception.Should().BeOfType<IOException>()
		   .Which.HResult.Should().Be(-2147022975);
	}

	[SkippableTheory]
	[AutoData]
	public void
		ResolveLinkTarget_MissingDirectoryInLinkChain_ShouldReturnPathToMissingDirectory(
			string path, string pathToFinalTarget, string pathToMissingDirectory)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		FileSystem.Directory.CreateDirectory(pathToFinalTarget);
		FileSystem.Directory.CreateSymbolicLink(pathToMissingDirectory,
			System.IO.Path.Combine(BasePath, pathToFinalTarget));
		FileSystem.Directory.CreateSymbolicLink(path,
			System.IO.Path.Combine(BasePath, pathToMissingDirectory));
		FileSystem.Directory.Delete(pathToMissingDirectory);

		IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, true);

		target!.FullName.Should().Be(FileSystem.Path.GetFullPath(pathToMissingDirectory));
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_NormalDirectory_ShouldReturnNull(
		string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		target.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_NormalFile_ShouldReturnNull(
		string path)
	{
		FileSystem.File.WriteAllText(path, null);

		IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		target.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_RelativePath_ShouldFollowSymbolicLinkUnderWindows(
		string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateSymbolicLink(path, targetFullPath);

		IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		target!.FullName.Should().Be(targetFullPath);
		target.Exists.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_TargetDeletedAfterLinkCreation_ShouldReturnNull(
		string path, string pathToTarget)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateSymbolicLink(path, targetFullPath);
		FileSystem.Directory.Delete(pathToTarget);

		IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		target!.FullName.Should().Be(targetFullPath);

		target.Exists.Should().BeFalse();
	}
}
#endif