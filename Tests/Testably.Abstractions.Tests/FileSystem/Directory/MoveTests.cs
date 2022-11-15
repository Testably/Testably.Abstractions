using System.IO;
using Testably.Abstractions.Testing.FileSystemInitializer;
#if !NETFRAMEWORK
#endif

namespace Testably.Abstractions.Tests.FileSystem.Directory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class MoveTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Move_CaseOnlyChange_ShouldMoveDirectoryWithContent(string path)
	{
		Skip.If(Test.IsNetFramework);

		string source = path.ToLowerInvariant();
		string destination = path.ToUpperInvariant();
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(source).Initialized(s => s
					.WithAFile()
					.WithASubdirectory().Initialized(t => t
						.WithAFile()
						.WithASubdirectory()));

		FileSystem.Directory.Move(source, destination);

		FileSystem.Directory.Exists(source).Should().Be(!Test.RunsOnLinux);
		FileSystem.Directory.Exists(destination).Should().BeTrue();
		FileSystem.Directory.GetDirectories(".").Should()
			.ContainSingle(d => d.Contains(destination));
		FileSystem.Directory.GetFiles(destination, initialized[1].Name)
			.Should().ContainSingle();
		FileSystem.Directory.GetDirectories(destination, initialized[2].Name)
			.Should().ContainSingle();
		FileSystem.Directory.GetFiles(destination, initialized[3].Name,
				SearchOption.AllDirectories)
			.Should().ContainSingle();
		FileSystem.Directory.GetDirectories(destination, initialized[4].Name,
				SearchOption.AllDirectories)
			.Should().ContainSingle();
	}

	[SkippableTheory]
	[AutoData]
	public void Move_CaseOnlyChange_ShouldThrowIOException_OnNetFramework(string path)
	{
		Skip.IfNot(Test.IsNetFramework);

		string source = path.ToLowerInvariant();
		string destination = path.ToUpperInvariant();
		FileSystem.Initialize()
			.WithSubdirectory(source);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.Move(source, destination);
		});

		exception.Should().BeException<IOException>(hResult: -2146232800);
	}

	[SkippableTheory]
	[AutoData]
	public void Move_DestinationDoesNotExist_ShouldThrowDirectoryNotFoundException(
		string source)
	{
		FileSystem.InitializeIn(source)
			.WithAFile();
		string destination = FileTestHelper.RootDrive("not-existing/path");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.Move(source, destination);
		});

		exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
	}

	[SkippableTheory]
	[AutoData]
	public void Move_ShouldMoveAttributes(string source, string destination)
	{
		FileSystem.Initialize()
			.WithSubdirectory(source);
		FileSystem.DirectoryInfo.New(source).Attributes |= FileAttributes.System;
		FileAttributes expectedAttributes =
			FileSystem.DirectoryInfo.New(source).Attributes;

		FileSystem.Directory.Move(source, destination);

		FileSystem.DirectoryInfo.New(destination).Attributes
			.Should().Be(expectedAttributes);
	}

	[SkippableTheory]
	[AutoData]
	public void Move_ShouldMoveDirectoryWithContent(string source, string destination)
	{
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(source).Initialized(s => s
					.WithAFile()
					.WithASubdirectory().Initialized(t => t
						.WithAFile()
						.WithASubdirectory()));

		FileSystem.Directory.Move(source, destination);

		FileSystem.Directory.Exists(source).Should().BeFalse();
		FileSystem.Directory.Exists(destination).Should().BeTrue();
		FileSystem.Directory.GetFiles(destination, initialized[1].Name)
			.Should().ContainSingle();
		FileSystem.Directory.GetDirectories(destination, initialized[2].Name)
			.Should().ContainSingle();
		FileSystem.Directory.GetFiles(destination, initialized[3].Name,
				SearchOption.AllDirectories)
			.Should().ContainSingle();
		FileSystem.Directory.GetDirectories(destination, initialized[4].Name,
				SearchOption.AllDirectories)
			.Should().ContainSingle();
	}

	[SkippableTheory]
	[AutoData]
	public void Move_ShouldNotAdjustTimes(string source, string destination)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.Initialize()
			.WithSubdirectory(source).Initialized(s => s
				.WithAFile()
				.WithASubdirectory().Initialized(t => t
					.WithAFile()
					.WithASubdirectory()));
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);

		FileSystem.Directory.Move(source, destination);

		DateTime creationTime = FileSystem.Directory.GetCreationTimeUtc(destination);
		DateTime lastAccessTime = FileSystem.Directory.GetLastAccessTimeUtc(destination);
		DateTime lastWriteTime = FileSystem.Directory.GetLastWriteTimeUtc(destination);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
				.BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
				.BeOnOrBefore(creationTimeEnd);
		}

		lastAccessTime.Should()
			.BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			.BeOnOrBefore(creationTimeEnd);
		lastWriteTime.Should()
			.BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			.BeOnOrBefore(creationTimeEnd);
	}

	[SkippableTheory]
	[AutoData]
	public void Move_SourceAndDestinationIdentical_ShouldThrowIOException(string path)
	{
		FileSystem.Initialize()
			.WithSubdirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.Move(path, path);
		});

		exception.Should().BeException<IOException>(hResult: -2146232800);
	}

	[SkippableTheory]
	[AutoData]
	public void Move_WithLockedFile_ShouldThrowIOException_AndNotMoveDirectoryAtAll_OnWindows(
		string source, string destination)
	{
		Skip.IfNot(Test.RunsOnWindows);

		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(source).Initialized(s => s
					.WithAFile()
					.WithASubdirectory().Initialized(t => t
						.WithAFile()
						.WithASubdirectory()));
		using FileSystemStream stream = FileSystem.File.Open(initialized[3].FullName,
			FileMode.Open,
			FileAccess.Read,
			FileShare.Read);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.Move(source, destination);
		});

		exception.Should().BeException<IOException>(hResult: -2147024891);
		FileSystem.Directory.Exists(source).Should().BeTrue();
		FileSystem.Directory.Exists(destination).Should().BeFalse();
		IDirectoryInfo sourceDirectory =
			FileSystem.DirectoryInfo.New(source);
		sourceDirectory.GetFiles(initialized[1].Name)
			.Should().ContainSingle();
		sourceDirectory.GetDirectories(initialized[2].Name)
			.Should().ContainSingle();
		sourceDirectory.GetFiles(initialized[3].Name, SearchOption.AllDirectories)
			.Should().ContainSingle();
		sourceDirectory
			.GetDirectories(initialized[4].Name, SearchOption.AllDirectories)
			.Should().ContainSingle();
	}

	[SkippableTheory]
	[AutoData]
	public void Move_WithLockedFile_ShouldStillMoveDirectory_NotOnWindows(
		string source, string destination)
	{
		Skip.If(Test.RunsOnWindows);

		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(source).Initialized(s => s
					.WithAFile()
					.WithASubdirectory().Initialized(t => t
						.WithAFile()
						.WithASubdirectory()));
		using FileSystemStream stream = FileSystem.File.Open(initialized[3].FullName,
			FileMode.Open,
			FileAccess.Read,
			FileShare.Read);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.Move(source, destination);
		});

		exception.Should().BeNull();
		FileSystem.Directory.Exists(source).Should().BeFalse();
		FileSystem.Directory.Exists(destination).Should().BeTrue();
		IDirectoryInfo destinationDirectory =
			FileSystem.DirectoryInfo.New(destination);
		destinationDirectory.GetFiles(initialized[1].Name)
			.Should().ContainSingle();
		destinationDirectory.GetDirectories(initialized[2].Name)
			.Should().ContainSingle();
		destinationDirectory
			.GetFiles(initialized[3].Name, SearchOption.AllDirectories)
			.Should().ContainSingle();
		destinationDirectory
			.GetDirectories(initialized[4].Name, SearchOption.AllDirectories)
			.Should().ContainSingle();
	}

	[SkippableTheory]
	[AutoData]
	public void Move_WithReadOnlyFile_ShouldMoveDirectoryWithContent(
		string source, string destination)
	{
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(source).Initialized(s => s
					.WithAFile()
					.WithASubdirectory().Initialized(t => t
						.WithAFile()
						.WithASubdirectory()));
		initialized[3].Attributes = FileAttributes.ReadOnly;

		FileSystem.Directory.Move(source, destination);

		FileSystem.Directory.Exists(source).Should().BeFalse();
		FileSystem.Directory.Exists(destination).Should().BeTrue();
		IDirectoryInfo destinationDirectory =
			FileSystem.DirectoryInfo.New(destination);
		destinationDirectory.GetFiles(initialized[1].Name)
			.Should().ContainSingle();
		destinationDirectory.GetDirectories(initialized[2].Name)
			.Should().ContainSingle();
		destinationDirectory.GetFiles(initialized[3].Name, SearchOption.AllDirectories)
			.Should().ContainSingle().Which.Attributes.Should()
			.HaveFlag(FileAttributes.ReadOnly);
		destinationDirectory
			.GetDirectories(initialized[4].Name, SearchOption.AllDirectories)
			.Should().ContainSingle();
	}
}
