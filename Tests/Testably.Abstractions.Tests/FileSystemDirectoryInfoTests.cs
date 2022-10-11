using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected FileSystemDirectoryInfoTests(
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.DirectoryInfo("MissingFile")]
	public void MissingFile_Attributes_ShouldAlwaysBeNegativeOne(
		FileAttributes fileAttributes)
	{
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		sut.Attributes.Should().Be((FileAttributes)(-1));
		Exception? exception = Record.Exception(() =>
		{
			sut.Attributes = fileAttributes;
		});
		exception.Should().BeOfType<FileNotFoundException>();
		sut.Attributes.Should().Be((FileAttributes)(-1));
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.DirectoryInfo("MissingFile")]
	public void MissingFile_CreationTime_ShouldAlwaysBeNullTime(DateTime creationTime)
	{
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		sut.CreationTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
		Exception? exception = Record.Exception(() =>
		{
			sut.CreationTime = creationTime;
		});
		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<FileNotFoundException>();
		}
		else
		{
			exception.Should().BeOfType<DirectoryNotFoundException>();
		}

		sut.CreationTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.DirectoryInfo("MissingFile")]
	public void MissingFile_CreationTimeUtc_ShouldAlwaysBeNullTime(
		DateTime creationTimeUtc)
	{
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		sut.CreationTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
		Exception? exception = Record.Exception(() =>
		{
			sut.CreationTimeUtc = creationTimeUtc;
		});
		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<FileNotFoundException>();
		}
		else
		{
			exception.Should().BeOfType<DirectoryNotFoundException>();
		}

		sut.CreationTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.DirectoryInfo("MissingFile")]
	public void MissingFile_LastAccessTime_ShouldAlwaysBeNullTime(DateTime lastAccessTime)
	{
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		sut.LastAccessTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
		Exception? exception = Record.Exception(() =>
		{
			sut.LastAccessTime = lastAccessTime;
		});
		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<FileNotFoundException>();
		}
		else
		{
			exception.Should().BeOfType<DirectoryNotFoundException>();
		}

		sut.LastAccessTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.DirectoryInfo("MissingFile")]
	public void MissingFile_LastAccessTimeUtc_ShouldAlwaysBeNullTime(
		DateTime lastAccessTimeUtc)
	{
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		sut.LastAccessTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
		Exception? exception = Record.Exception(() =>
		{
			sut.LastAccessTimeUtc = lastAccessTimeUtc;
		});
		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<FileNotFoundException>();
		}
		else
		{
			exception.Should().BeOfType<DirectoryNotFoundException>();
		}

		sut.LastAccessTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.DirectoryInfo("MissingFile")]
	public void MissingFile_LastWriteTime_ShouldAlwaysBeNullTime(DateTime lastWriteTime)
	{
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		sut.LastWriteTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
		Exception? exception = Record.Exception(() =>
		{
			sut.LastWriteTime = lastWriteTime;
		});
		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<FileNotFoundException>();
		}
		else
		{
			exception.Should().BeOfType<DirectoryNotFoundException>();
		}

		sut.LastWriteTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.DirectoryInfo("MissingFile")]
	public void MissingFile_LastWriteTimeUtc_ShouldAlwaysBeNullTime(
		DateTime lastWriteTimeUtc)
	{
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		sut.LastWriteTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
		Exception? exception = Record.Exception(() =>
		{
			sut.LastWriteTimeUtc = lastWriteTimeUtc;
		});
		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<FileNotFoundException>();
		}
		else
		{
			exception.Should().BeOfType<DirectoryNotFoundException>();
		}

		sut.LastWriteTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.DirectoryInfo(nameof(IFileSystem.IDirectoryInfo.Parent))]
	public void Parent_ArbitraryPaths_ShouldNotBeNull(string path1,
	                                                  string path2,
	                                                  string path3)
	{
		string path = FileSystem.Path.Combine(path1, path2, path3);

		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		sut.Parent.Should().NotBeNull();
		sut.Parent!.Exists.Should().BeFalse();
		sut.Parent.Parent.Should().NotBeNull();
		sut.Parent.Parent!.Exists.Should().BeFalse();
	}

	[SkippableFact]
	[AutoData]
	[FileSystemTests.DirectoryInfo(nameof(IFileSystem.IDirectoryInfo.Parent))]
	public void Parent_Root_ShouldBeNull()
	{
		IFileSystem.IDirectoryInfo sut =
			FileSystem.DirectoryInfo.New(FileTestHelper.RootDrive());

		sut.Parent.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.DirectoryInfo(nameof(IFileSystem.IDirectoryInfo.Root))]
	public void Root_ShouldExist(string path)
	{
		string expectedRoot = FileTestHelper.RootDrive();
		IFileSystem.IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);

		result.Root.Exists.Should().BeTrue();
		result.Root.FullName.Should().Be(expectedRoot);
	}
}