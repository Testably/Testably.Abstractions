#if FEATURE_FILESYSTEM_LINK
using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ResolveLinkTargetTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
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
	public void ResolveLinkTarget_FileWithDifferentCase_ShouldReturnPathToMissingFile(
		string path, string pathToTarget, string contents)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.File.WriteAllText(pathToTarget, "foo");
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		fileInfo.CreateAsSymbolicLink(pathToTarget);
		FileSystem.File.Delete(pathToTarget);
		FileSystem.File.WriteAllText(pathToTarget.ToUpper(), contents);

		IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

		target!.FullName.Should().Be(targetFullPath);
		if (!Test.RunsOnLinux)
		{
			target.Exists.Should().BeTrue();
			FileSystem.File.ReadAllText(target.FullName).Should().Be(contents);
		}
		else
		{
			target.Exists.Should().BeFalse();
		}
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_FinalTarget_ShouldFollowSymbolicLinkToFinalTarget(
		string path, string pathToFinalTarget)
	{
		int maxLinks = MaxResolveLinks;

		FileSystem.File.WriteAllText(pathToFinalTarget, null);
		string previousPath = pathToFinalTarget;
		for (int i = 0; i < maxLinks; i++)
		{
			string newPath = $"{path}-{i}";
			IFileInfo linkFileInfo = FileSystem.FileInfo.New(newPath);
			linkFileInfo.CreateAsSymbolicLink(previousPath);
			previousPath = newPath;
		}

		IFileInfo fileInfo = FileSystem.FileInfo.New(previousPath);

		IFileSystemInfo? target = fileInfo.ResolveLinkTarget(true);

		target!.FullName.Should().Be(FileSystem.Path.GetFullPath(pathToFinalTarget));
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_FinalTargetWithTooManyLevels_ShouldThrowIOException(
		string path, string pathToFinalTarget)
	{
		int maxLinks = MaxResolveLinks + 1;
		FileSystem.File.WriteAllText(pathToFinalTarget, null);
		string previousPath = pathToFinalTarget;
		for (int i = 0; i < maxLinks; i++)
		{
			string newPath = $"{path}-{i}";
			IFileInfo linkFileInfo = FileSystem.FileInfo.New(newPath);
			linkFileInfo.CreateAsSymbolicLink(previousPath);
			previousPath = newPath;
		}

		IFileInfo fileInfo = FileSystem.FileInfo.New(previousPath);

		Exception? exception = Record.Exception(() =>
		{
			_ = fileInfo.ResolveLinkTarget(true);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<IOException>()
			   .Which.HResult.Should().Be(-2147022975);
		}
		else
		{
			exception.Should().BeOfType<IOException>()
			   .Which.HResult.Should().Be(-2146232800);
		}

		exception.Should().BeOfType<IOException>()
		   .Which.Message.Should().Contain($"'{fileInfo.FullName}'");
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_MissingFileInLinkChain_ShouldReturnPathToMissingFile(
		string path, string pathToFinalTarget, string pathToMissingFile)
	{
		FileSystem.File.WriteAllText(pathToFinalTarget, null);
		IFileInfo linkFileInfo1 = FileSystem.FileInfo.New(pathToMissingFile);
		linkFileInfo1.CreateAsSymbolicLink(pathToFinalTarget);
		IFileInfo linkFileInfo2 = FileSystem.FileInfo.New(path);
		linkFileInfo2.CreateAsSymbolicLink(pathToMissingFile);
		linkFileInfo1.Delete();

		IFileSystemInfo? target = linkFileInfo2.ResolveLinkTarget(true);

		target!.FullName.Should().Be(FileSystem.Path.GetFullPath(pathToMissingFile));
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_NormalDirectory_ShouldReturnNull(
		string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo fileInfo = FileSystem.DirectoryInfo.New(path);

		IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

		target.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_NormalFile_ShouldReturnNull(
		string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

		target.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_ShouldFollowSymbolicLink(
		string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.File.WriteAllText(pathToTarget, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		fileInfo.CreateAsSymbolicLink(pathToTarget);

		IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

		target!.FullName.Should().Be(targetFullPath);
		target.Exists.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_TargetDeletedAfterLinkCreation_ShouldReturnNull(
		string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.File.WriteAllText(pathToTarget, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		fileInfo.CreateAsSymbolicLink(pathToTarget);
		FileSystem.File.Delete(pathToTarget);

		IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

		target!.FullName.Should().Be(targetFullPath);
		target.Exists.Should().BeFalse();
	}
}
#endif