namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfoFactory;

public abstract class FileSystemDirectoryInfoFactoryTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected FileSystemDirectoryInfoFactoryTests(
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	[SkippableFact]
	public void New_EmptyPath_ShouldThrowArgumentException()
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.DirectoryInfo.New(string.Empty);
		});

#if NETFRAMEWORK
		exception.Should().BeOfType<ArgumentException>()
		   .Which.Message.Should().Be("The path is not of a legal form.");
#else
		exception.Should().BeOfType<ArgumentException>()
		   .Which.ParamName.Should().Be("path");
		exception.Should().BeOfType<ArgumentException>()
		   .Which.Message.Should()
		   .Be("The path is empty. (Parameter 'path')");
#endif
	}

	[SkippableFact]
	public void New_Null_ShouldThrowArgumentNullException()
	{
		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.DirectoryInfo.New(null!);
		});

		exception.Should().BeOfType<ArgumentNullException>();
	}

	[SkippableTheory]
	[AutoData]
	public void New_ShouldCreateNewDirectoryInfoFromPath(string path)
	{
		IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);

		result.ToString().Should().Be(path);
		result.Exists.Should().BeFalse();
	}

	[SkippableFact]
	public void Wrap_Null_ShouldReturnNull()
	{
		IDirectoryInfo? result = FileSystem.DirectoryInfo.Wrap(null);

		result.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void Wrap_ShouldWrapFromDirectoryInfo(string path)
	{
		System.IO.DirectoryInfo directoryInfo = new("S:\\" + path);

		IDirectoryInfo result = FileSystem.DirectoryInfo.Wrap(directoryInfo);

		result.FullName.Should().Be(directoryInfo.FullName);
		result.Exists.Should().Be(directoryInfo.Exists);
	}
}