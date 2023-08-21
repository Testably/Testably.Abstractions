using System.Collections.Generic;
using System.Linq;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class FileSystemExtensibilityTests
{
	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void Directory_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IDirectory sut = fileSystem.Directory;

		IFileSystem result = sut.FileSystem;

		result.Should().BeSameAs(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void DirectoryInfo_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IDirectoryInfo sut = fileSystem.DirectoryInfo.New("foo");

		IFileSystem result = sut.FileSystem;

		result.Should().BeSameAs(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void DirectoryInfoFactory_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IDirectoryInfoFactory sut = fileSystem.DirectoryInfo;

		IFileSystem result = sut.FileSystem;

		result.Should().BeSameAs(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void DriveInfo_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IDriveInfo sut = fileSystem.DriveInfo.GetDrives().First();

		IFileSystem result = sut.FileSystem;

		result.Should().BeSameAs(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void DriveInfoFactory_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IDriveInfoFactory sut = fileSystem.DriveInfo;

		IFileSystem result = sut.FileSystem;

		result.Should().BeSameAs(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void File_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFile sut = fileSystem.File;

		IFileSystem result = sut.FileSystem;

		result.Should().BeSameAs(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void FileInfo_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileInfo sut = fileSystem.FileInfo.New("foo");

		IFileSystem result = sut.FileSystem;

		result.Should().BeSameAs(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void FileInfoFactory_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileInfoFactory sut = fileSystem.FileInfo;

		IFileSystem result = sut.FileSystem;

		result.Should().BeSameAs(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void FileStreamFactory_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileStreamFactory sut = fileSystem.FileStream;

		IFileSystem result = sut.FileSystem;

		result.Should().BeSameAs(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void FileSystemInfo_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileSystemInfo sut = fileSystem.FileInfo.New("foo");

		IFileSystem result = sut.FileSystem;

		result.Should().BeSameAs(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void FileSystemWatcher_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileSystemWatcher sut = fileSystem.FileSystemWatcher.New();

		IFileSystem result = sut.FileSystem;

		result.Should().BeSameAs(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void FileSystemWatcherFactory_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileSystemWatcherFactory sut = fileSystem.FileSystemWatcher;

		IFileSystem result = sut.FileSystem;

		result.Should().BeSameAs(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void Path_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IPath sut = fileSystem.Path;

		IFileSystem result = sut.FileSystem;

		result.Should().BeSameAs(fileSystem);
	}

	public static IEnumerable<object[]> GetFileSystems =>
		new List<object[]>
		{
			new object[]
			{
				new RealFileSystem()
			},
			new object[]
			{
				new MockFileSystem()
			},
		};
}
