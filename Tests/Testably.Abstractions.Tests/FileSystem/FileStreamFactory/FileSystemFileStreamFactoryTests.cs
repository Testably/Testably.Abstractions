using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileStreamFactory;

public abstract class FileSystemFileStreamFactoryTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected FileSystemFileStreamFactoryTests(
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	[SkippableTheory]
	[AutoData]
	public void New_AppendAccessWithReadWriteMode_ShouldThrowArgumentException(
		string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(path, FileMode.Append, FileAccess.ReadWrite);
		});

#if NETFRAMEWORK
		exception.Should().BeOfType<ArgumentException>();
#else
		exception.Should().BeOfType<ArgumentException>()
		   .Which.ParamName.Should().Be("access");
#endif
		exception!.Message.Should()
		   .Contain(FileMode.Append.ToString());
	}

	[SkippableTheory]
	[AutoData]
	public void New_EmptyPath_ShouldThrowArgumentException(FileMode mode)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(string.Empty, mode);
		});

#if NETFRAMEWORK
		exception.Should().BeOfType<ArgumentException>();
#else
		exception.Should().BeOfType<ArgumentException>()
		   .Which.ParamName.Should().Be("path");
#endif
	}

	[SkippableTheory]
	[AutoData]
	public void New_ExistingFileWithCreateMode_ShouldIgnoreContent(
		string path)
	{
		FileSystem.File.WriteAllText(path, "foo");
		FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Create);
		stream.Dispose();

		FileSystem.File.ReadAllText(path).Should().BeEmpty();
	}

	[SkippableTheory]
	[AutoData]
	public void New_ExistingFileWithCreateNewMode_ShouldThrowArgumentException(
		string path)
	{
		FileSystem.File.WriteAllText(path, "foo");
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(path, FileMode.CreateNew);
		});

		exception.Should().BeOfType<IOException>()
		   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[SkippableTheory]
	[AutoData]
	public void New_ExistingFileWithTruncateMode_ShouldIgnoreContent(
		string path)
	{
		FileSystem.File.WriteAllText(path, "foo");
		FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Truncate);
		stream.Dispose();

		FileSystem.File.ReadAllText(path).Should().BeEmpty();
	}

	[SkippableTheory]
	[InlineAutoData(FileMode.Append)]
	[InlineAutoData(FileMode.Truncate)]
	[InlineAutoData(FileMode.Create)]
	[InlineAutoData(FileMode.CreateNew)]
	[InlineAutoData(FileMode.Append)]
	public void New_InvalidModeForReadAccess_ShouldThrowArgumentException(
		FileMode mode, string path)
	{
		FileAccess access = FileAccess.Read;
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(path, mode, access);
		});

#if NETFRAMEWORK
		exception.Should().BeOfType<ArgumentException>();
#else
		exception.Should().BeOfType<ArgumentException>()
		   .Which.ParamName.Should().Be("access");
#endif
		exception!.Message.Should()
		   .Contain(mode.ToString()).And
		   .Contain(access.ToString());
	}

	[SkippableTheory]
	[InlineAutoData(FileMode.Open)]
	[InlineAutoData(FileMode.Truncate)]
	public void New_MissingFileWithIncorrectMode_ShouldThrowArgumentException(
		FileMode mode, string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(path, mode);
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[SkippableTheory]
	[AutoData]
	public void New_MissingFileWithTruncateMode_ShouldIgnoreContent(
		string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(path, FileMode.Truncate);
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[SkippableTheory]
	[AutoData]
	public void New_NullPath_ShouldThrowArgumentNullException(FileMode mode)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(null!, mode);
		});

		exception.Should().BeOfType<ArgumentNullException>()
		   .Which.ParamName.Should().Be("path");
	}

	[SkippableTheory]
	[AutoData]
	public void New_SamePathAsExistingDirectory_ShouldThrowException(
		string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(path, FileMode.CreateNew);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<UnauthorizedAccessException>()
			   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
		}
		else
		{
			exception.Should().BeOfType<IOException>()
			   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
		}
	}

	[SkippableTheory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public void New_WithUseAsyncSet_ShouldSetProperty(bool useAsync, string path)
	{
		using FileSystemStream stream = FileSystem.FileStream.New(
			path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 1,
			useAsync);

		stream.IsAsync.Should().Be(useAsync);
	}
}