using System.Collections.Generic;

namespace Testably.Abstractions.Testing.Tests.fileSystem;

public class FileSystemExtensibilityTests
{
	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void Directory_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileSystem result = fileSystem.Directory.FileSystem;

		result.Should().Be(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void DirectoryInfo_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileSystem result = fileSystem.DirectoryInfo.FileSystem;

		result.Should().Be(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void DriveInfo_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileSystem result = fileSystem.DriveInfo.FileSystem;

		result.Should().Be(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void File_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileSystem result = fileSystem.File.FileSystem;

		result.Should().Be(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void FileInfo_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileSystem result = fileSystem.FileInfo.FileSystem;

		result.Should().Be(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void FileStream_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileSystem result = fileSystem.FileStream.FileSystem;

		result.Should().Be(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void FileSystemInfo_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileSystem result = fileSystem.FileInfo.New("foo").FileSystem;

		result.Should().Be(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void FileSystemWatcher_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileSystem result = fileSystem.FileSystemWatcher.New().FileSystem;

		result.Should().Be(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void FileSystemWatcherFactory_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileSystem result = fileSystem.FileSystemWatcher.FileSystem;

		result.Should().Be(fileSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystems))]
	public void Path_ShouldSetExtensionPoint(IFileSystem fileSystem)
	{
		IFileSystem result = fileSystem.Path.FileSystem;

		result.Should().Be(fileSystem);
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
