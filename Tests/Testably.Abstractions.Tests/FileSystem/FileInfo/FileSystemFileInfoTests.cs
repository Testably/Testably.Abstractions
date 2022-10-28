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
		IFileInfo? file = initialized[1] as IFileInfo;

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
		IFileInfo? file = initialized[1] as IFileInfo;

		file?.Should().NotBeNull();
		file!.DirectoryName.Should().Be(initialized[0].FullName);
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_Directory_ShouldReturnFalse(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Exists.Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void IsReadOnly_SetToFalse_ShouldRemoveReadOnlyAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
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
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		fileInfo.IsReadOnly = true;

		fileInfo.IsReadOnly.Should().BeTrue();
		fileInfo.Attributes.Should().HaveFlag(FileAttributes.ReadOnly);

		fileInfo.IsReadOnly = true;

		fileInfo.IsReadOnly.Should().BeTrue();
		fileInfo.Attributes.Should().HaveFlag(FileAttributes.ReadOnly);

		fileInfo.IsReadOnly = false;

		fileInfo.IsReadOnly.Should().BeFalse();
		fileInfo.Attributes.Should().NotHaveFlag(FileAttributes.ReadOnly);
	}

	[SkippableTheory]
	[AutoData]
	public void IsReadOnly_ShouldChangeWhenSettingReadOnlyAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		fileInfo.Attributes = FileAttributes.ReadOnly | FileAttributes.Encrypted;

		fileInfo.IsReadOnly.Should().BeTrue();
		fileInfo.Attributes.Should().HaveFlag(FileAttributes.ReadOnly);
	}

	[SkippableTheory]
	[AutoData]
	public void IsReadOnly_ShouldInitializeToReadOnlyAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		fileInfo.IsReadOnly.Should().BeFalse();
		fileInfo.Attributes.Should().NotHaveFlag(FileAttributes.ReadOnly);
	}
}