using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected FileSystemFileInfoTests(
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	[SkippableFact]
	public void Directory_ShouldReturnParentDirectory()
	{
		FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.Initialize()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());
		IFileSystem.IFileInfo? file = initialized[1] as IFileSystem.IFileInfo;

		file?.Directory.Should().NotBeNull();
		file!.Directory!.FullName.Should().Be(initialized[0].FullName);
	}

	[SkippableFact]
	public void DirectoryName_ShouldReturnNameOfParentDirectory()
	{
		FileSystemInitializer.IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.Initialize()
			   .WithASubdirectory().Initialized(s => s
				   .WithAFile());
		IFileSystem.IFileInfo? file = initialized[1] as IFileSystem.IFileInfo;

		file?.Should().NotBeNull();
		file!.DirectoryName.Should().Be(initialized[0].FullName);
	}

	[SkippableTheory]
	[AutoData]
	public void IsReadOnly_SetToFalse_ShouldRemoveReadOnlyAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		fileInfo.Attributes = FileAttributes.ReadOnly;

		fileInfo.IsReadOnly = false;

		fileInfo.IsReadOnly.Should().BeFalse();
		fileInfo.Attributes.Should().Be(FileAttributes.Normal);
	}

	[SkippableTheory]
	[AutoData]
	public void IsReadOnly_SetToTrue_ShouldAddReadOnlyAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		fileInfo.IsReadOnly = true;

		fileInfo.IsReadOnly.Should().BeTrue();
		fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly).Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void IsReadOnly_ShouldChangeWhenSettingReadOnlyAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		fileInfo.Attributes = FileAttributes.ReadOnly | FileAttributes.Encrypted;

		fileInfo.IsReadOnly.Should().BeTrue();
		fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly).Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void IsReadOnly_ShouldInitializeToReadOnlyAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		fileInfo.IsReadOnly.Should().BeFalse();
		fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly).Should().BeFalse();
	}
}