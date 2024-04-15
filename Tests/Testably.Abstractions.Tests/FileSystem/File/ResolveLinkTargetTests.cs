#if FEATURE_FILESYSTEM_LINK
using System.Globalization;

namespace Testably.Abstractions.Tests.FileSystem.File;

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
	private int MaxResolveLinks =>
		Test.RunsOnWindows ? 63 : 40;

	#endregion

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_AbsolutePath_ShouldFollowSymbolicLink(
		string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.CreateSymbolicLink(path, targetFullPath);

		IFileSystemInfo? target =
			FileSystem.File.ResolveLinkTarget(path, false);

		target!.FullName.Should().Be(targetFullPath);
		target.Should().Exist();
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_FileWithDifferentCase_ShouldReturnPathToMissingFile(
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
			target!.FullName.Should().Be(targetFullPath);
			target.Should().Exist();
			FileSystem.File.ReadAllText(target.FullName).Should().Be(contents);
		}
		else
		{
			target!.FullName.Should().Be(targetFullPath);
			target.Should().NotExist();
		}
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_FinalTarget_MultipleSteps_ShouldFollowSymbolicLinkToFinalTarget(
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

		target.Should().NotBeNull();
		target!.FullName.Should().Be(FileSystem.Path.GetFullPath(pathToFinalTarget));
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
			FileSystem.File.CreateSymbolicLink(newPath,
				FileSystem.Path.Combine(BasePath, previousPath));
			previousPath = newPath;
		}

		IFileSystemInfo? target =
			FileSystem.File.ResolveLinkTarget(previousPath, true);

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
			FileSystem.File.CreateSymbolicLink(newPath,
				FileSystem.Path.Combine(BasePath, previousPath));
			previousPath = newPath;
		}

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.File.ResolveLinkTarget(previousPath, true);
		});

		exception.Should().BeException<System.IO.IOException>($"'{previousPath}'",
			hResult: Test.RunsOnWindows ? -2147022975 : -2146232800);
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_MissingFileAtBeginningOfLinkChain_ShouldReturnPathToMissingFile(
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

		target!.FullName.Should().Be(FileSystem.Path.GetFullPath(pathToMissingFile));
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_MissingFileInLinkChain_ShouldReturnPathToMissingFile(
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

		target!.FullName.Should().Be(FileSystem.Path.GetFullPath(pathToMissingFile));
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_NormalDirectory_ShouldReturnNull(
		string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		IFileSystemInfo? target =
			FileSystem.File.ResolveLinkTarget(path, false);

		target.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_NormalFile_ShouldReturnNull(
		string path)
	{
		FileSystem.File.WriteAllText(path, null);

		IFileSystemInfo? target =
			FileSystem.File.ResolveLinkTarget(path, false);

		target.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_RelativePath_ShouldFollowSymbolicLinkUnderWindows(
		string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.CreateSymbolicLink(path, targetFullPath);

		IFileSystemInfo? target =
			FileSystem.File.ResolveLinkTarget(path, false);

		target!.FullName.Should().Be(targetFullPath);
		target.Should().Exist();
	}

	[SkippableTheory]
	[AutoData]
	public void ResolveLinkTarget_TargetDeletedAfterLinkCreation_ShouldReturnNull(
		string path, string pathToTarget)
	{
		string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.CreateSymbolicLink(path, targetFullPath);
		FileSystem.File.Delete(pathToTarget);

		IFileSystemInfo? target =
			FileSystem.File.ResolveLinkTarget(path, false);

		target!.FullName.Should().Be(targetFullPath);

		target.Should().NotExist();
	}
}
#endif
