#if FEATURE_FILESYSTEM_LINK
using System.Globalization;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemInfo;

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
	public async Task ResolveLinkTarget_FileWithDifferentCase_ShouldReturnPathToMissingFile(
		string path, string pathToTarget, string contents)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.File.WriteAllText(pathToTarget, "foo");
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		fileInfo.CreateAsSymbolicLink(pathToTarget);
		FileSystem.File.Delete(pathToTarget);
		FileSystem.File.WriteAllText(pathToTarget.ToUpper(CultureInfo.InvariantCulture), contents);

		IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

		await That(target!.FullName).IsEqualTo(targetFullPath);
		if (!Test.RunsOnLinux)
		{
			await That(target.Exists).IsTrue();
			await That(FileSystem.File.ReadAllText(target.FullName)).IsEqualTo(contents);
		}
		else
		{
			await That(target.Exists).IsFalse();
		}
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
			IFileInfo linkFileInfo = FileSystem.FileInfo.New(newPath);
			linkFileInfo.CreateAsSymbolicLink(previousPath);
			previousPath = newPath;
		}

		IFileInfo fileInfo = FileSystem.FileInfo.New(previousPath);

		IFileSystemInfo? target = fileInfo.ResolveLinkTarget(true);

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
	public async Task ResolveLinkTarget_MissingFileInLinkChain_ShouldReturnPathToMissingFile(
		string path, string pathToFinalTarget, string pathToMissingFile)
	{
		FileSystem.File.WriteAllText(pathToFinalTarget, null);
		IFileInfo linkFileInfo1 = FileSystem.FileInfo.New(pathToMissingFile);
		linkFileInfo1.CreateAsSymbolicLink(pathToFinalTarget);
		IFileInfo linkFileInfo2 = FileSystem.FileInfo.New(path);
		linkFileInfo2.CreateAsSymbolicLink(pathToMissingFile);
		linkFileInfo1.Delete();

		IFileSystemInfo? target = linkFileInfo2.ResolveLinkTarget(true);

		await That(target!.FullName).IsEqualTo(FileSystem.Path.GetFullPath(pathToMissingFile));
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_NormalDirectory_ShouldReturnNull(
		string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo fileInfo = FileSystem.DirectoryInfo.New(path);

		IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

		await That(target).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_NormalFile_ShouldReturnNull(
		string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

		await That(target).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task ResolveLinkTarget_ShouldFollowSymbolicLink(
		string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.File.WriteAllText(pathToTarget, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		fileInfo.CreateAsSymbolicLink(pathToTarget);

		IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

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
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		fileInfo.CreateAsSymbolicLink(pathToTarget);
		FileSystem.File.Delete(pathToTarget);

		IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

		await That(target!.FullName).IsEqualTo(targetFullPath);
		await That(target.Exists).IsFalse();
	}
}
#endif
