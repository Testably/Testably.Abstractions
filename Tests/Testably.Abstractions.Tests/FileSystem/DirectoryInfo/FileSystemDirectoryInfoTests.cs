using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

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
	public void MissingFile_Attributes_ShouldAlwaysBeNegativeOne(
		FileAttributes fileAttributes)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
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
	public void MissingFile_CreationTime_ShouldAlwaysBeNullTime(DateTime creationTime)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
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
	public void MissingFile_CreationTimeUtc_ShouldAlwaysBeNullTime(
		DateTime creationTimeUtc)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
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
	public void MissingFile_LastAccessTime_ShouldAlwaysBeNullTime(DateTime lastAccessTime)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
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
	public void MissingFile_LastAccessTimeUtc_ShouldAlwaysBeNullTime(
		DateTime lastAccessTimeUtc)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
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
	public void MissingFile_LastWriteTime_ShouldAlwaysBeNullTime(DateTime lastWriteTime)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
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
	public void MissingFile_LastWriteTimeUtc_ShouldAlwaysBeNullTime(
		DateTime lastWriteTimeUtc)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
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
	public void Parent_ArbitraryPaths_ShouldNotBeNull(string path1,
	                                                  string path2,
	                                                  string path3)
	{
		string path = FileSystem.Path.Combine(path1, path2, path3);

		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		sut.Parent.Should().NotBeNull();
		sut.Parent!.Exists.Should().BeFalse();
		sut.Parent.Parent.Should().NotBeNull();
		sut.Parent.Parent!.Exists.Should().BeFalse();
	}

	[SkippableFact]
	[AutoData]
	public void Parent_Root_ShouldBeNull()
	{
		IDirectoryInfo sut =
			FileSystem.DirectoryInfo.New(FileTestHelper.RootDrive());

		sut.Parent.Should().BeNull();
	}

	[SkippableFact]
	[AutoData]
	public void Root_Name_ShouldBeCorrect()
	{
		string rootName = FileTestHelper.RootDrive();
		IDirectoryInfo sut =
			FileSystem.DirectoryInfo.New(rootName);

		sut.Name.Should().Be(rootName);
	}

	[SkippableTheory]
	[AutoData]
	public void Root_ShouldExist(string path)
	{
		string expectedRoot = FileTestHelper.RootDrive();
		IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);

		result.Root.Exists.Should().BeTrue();
		result.Root.FullName.Should().Be(expectedRoot);
	}
}