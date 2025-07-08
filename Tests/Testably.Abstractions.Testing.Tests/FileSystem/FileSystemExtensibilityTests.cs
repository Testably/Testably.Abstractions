using System.Linq;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class FileSystemExtensibilityTests
{
	#region Test Setup

	public static TheoryData<IFileSystem> GetFileSystems
		=> new()
		{
			(IFileSystem)new RealFileSystem(),
			(IFileSystem)new MockFileSystem(),
		};

	#endregion

	[Theory]
	[MemberData(nameof(GetFileSystems))]
	public async Task Directory_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IDirectory sut = fileSystem.Directory;

		IFileSystem result = sut.FileSystem;

		await That(result).IsSameAs(fileSystem);
	}

	[Theory]
	[MemberData(nameof(GetFileSystems))]
	public async Task DirectoryInfo_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IDirectoryInfo sut = fileSystem.DirectoryInfo.New("foo");

		IFileSystem result = sut.FileSystem;

		await That(result).IsSameAs(fileSystem);
	}

	[Theory]
	[MemberData(nameof(GetFileSystems))]
	public async Task DirectoryInfoFactory_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IDirectoryInfoFactory sut = fileSystem.DirectoryInfo;

		IFileSystem result = sut.FileSystem;

		await That(result).IsSameAs(fileSystem);
	}

	[Theory]
	[MemberData(nameof(GetFileSystems))]
	public async Task DriveInfo_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IDriveInfo sut = fileSystem.DriveInfo.GetDrives()[0];

		IFileSystem result = sut.FileSystem;

		await That(result).IsSameAs(fileSystem);
	}

	[Theory]
	[MemberData(nameof(GetFileSystems))]
	public async Task DriveInfoFactory_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IDriveInfoFactory sut = fileSystem.DriveInfo;

		IFileSystem result = sut.FileSystem;

		await That(result).IsSameAs(fileSystem);
	}

	[Theory]
	[MemberData(nameof(GetFileSystems))]
	public async Task File_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFile sut = fileSystem.File;

		IFileSystem result = sut.FileSystem;

		await That(result).IsSameAs(fileSystem);
	}

	[Theory]
	[MemberData(nameof(GetFileSystems))]
	public async Task FileInfo_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileInfo sut = fileSystem.FileInfo.New("foo");

		IFileSystem result = sut.FileSystem;

		await That(result).IsSameAs(fileSystem);
	}

	[Theory]
	[MemberData(nameof(GetFileSystems))]
	public async Task FileInfoFactory_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileInfoFactory sut = fileSystem.FileInfo;

		IFileSystem result = sut.FileSystem;

		await That(result).IsSameAs(fileSystem);
	}

	[Theory]
	[MemberData(nameof(GetFileSystems))]
	public async Task FileStreamFactory_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileStreamFactory sut = fileSystem.FileStream;

		IFileSystem result = sut.FileSystem;

		await That(result).IsSameAs(fileSystem);
	}

	[Theory]
	[MemberData(nameof(GetFileSystems))]
	public async Task FileSystemInfo_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileSystemInfo sut = fileSystem.FileInfo.New("foo");

		IFileSystem result = sut.FileSystem;

		await That(result).IsSameAs(fileSystem);
	}

	[Theory]
	[MemberData(nameof(GetFileSystems))]
	public async Task FileSystemWatcher_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileSystemWatcher sut = fileSystem.FileSystemWatcher.New();

		IFileSystem result = sut.FileSystem;

		await That(result).IsSameAs(fileSystem);
	}

	[Theory]
	[MemberData(nameof(GetFileSystems))]
	public async Task FileSystemWatcherFactory_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileSystemWatcherFactory sut = fileSystem.FileSystemWatcher;

		IFileSystem result = sut.FileSystem;

		await That(result).IsSameAs(fileSystem);
	}

	[Theory]
	[MemberData(nameof(GetFileSystems))]
	public async Task Path_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IPath sut = fileSystem.Path;

		IFileSystem result = sut.FileSystem;

		await That(result).IsSameAs(fileSystem);
	}
}
