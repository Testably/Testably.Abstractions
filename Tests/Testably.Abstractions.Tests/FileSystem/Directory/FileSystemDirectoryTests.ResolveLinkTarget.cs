#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	/// <summary>
	///     The maximum number of symbolic links that are followed.<br />
	///     <see href="https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.resolvelinktarget?view=net-6.0#remarks" />
	/// </summary>
	private static int MaxResolveLinks =>
		Test.RunsOnWindows ? 63 : 40;

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.ResolveLinkTarget))]
	public void ResolveLinkTarget_AbsolutePath_ShouldFollowSymbolicLink(
		string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateSymbolicLink(path, targetFullPath);

		IFileSystem.IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		target!.FullName.Should().Be(targetFullPath);
		target.Exists.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.ResolveLinkTarget))]
	public void
		ResolveLinkTarget_FileWithDifferentCase_ShouldReturnPathToMissingDirectory(
			string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateSymbolicLink(path, targetFullPath);
		FileSystem.Directory.Delete(pathToTarget);
		FileSystem.Directory.CreateDirectory(pathToTarget.ToUpper());

		IFileSystem.IFileSystemInfo? target =
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
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.ResolveLinkTarget))]
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

		IFileSystem.IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(previousPath, true);

		target!.FullName.Should().Be(FileSystem.Path.GetFullPath(pathToFinalTarget));
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.ResolveLinkTarget))]
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
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.ResolveLinkTarget))]
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

		IFileSystem.IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, true);

		target!.FullName.Should().Be(FileSystem.Path.GetFullPath(pathToMissingDirectory));
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.ResolveLinkTarget))]
	public void ResolveLinkTarget_NormalDirectory_ShouldReturnNull(
		string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		IFileSystem.IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		target.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.ResolveLinkTarget))]
	public void ResolveLinkTarget_NormalFile_ShouldReturnNull(
		string path)
	{
		FileSystem.File.WriteAllText(path, null);

		IFileSystem.IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		target.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.ResolveLinkTarget))]
	public void ResolveLinkTarget_RelativePath_ShouldFollowSymbolicLinkUnderWindows(
		string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateSymbolicLink(path, targetFullPath);

		IFileSystem.IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		target!.FullName.Should().Be(targetFullPath);
		target.Exists.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.ResolveLinkTarget))]
	public void ResolveLinkTarget_TargetDeletedAfterLinkCreation_ShouldReturnNull(
		string path, string pathToTarget)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.Directory.CreateSymbolicLink(path, targetFullPath);
		FileSystem.Directory.Delete(pathToTarget);

		IFileSystem.IFileSystemInfo? target =
			FileSystem.Directory.ResolveLinkTarget(path, false);

		target!.FullName.Should().Be(targetFullPath);

		target.Exists.Should().BeFalse();
	}
}
#endif