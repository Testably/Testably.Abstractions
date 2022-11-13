using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileStreamFactory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void New_AppendAccessWithReadWriteMode_ShouldThrowArgumentException(
		string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(path, FileMode.Append, FileAccess.ReadWrite);
		});

		exception.Should().BeOfType<ArgumentException>()
			.Which.HResult.Should().Be(-2147024809);
#if !NETFRAMEWORK
		exception.Should().BeOfType<ArgumentException>()
			.Which.ParamName.Should().Be("access");
#endif
		exception!.Message.Should()
			.Contain(FileMode.Append.ToString());
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
	public void New_ExistingFileWithCreateNewMode_ShouldThrowIOException(
		string path)
	{
		FileSystem.File.WriteAllText(path, "foo");
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(path, FileMode.CreateNew);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<IOException>()
				.Which.HResult.Should().Be(-2147024816);
		}
		else
		{
			exception.Should().BeOfType<IOException>()
				.Which.HResult.Should().Be(17);
		}

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

		exception.Should().BeOfType<ArgumentException>()
			.Which.HResult.Should().Be(-2147024809);
#if !NETFRAMEWORK
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
			.Which.HResult.Should().Be(-2147024894);
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
			.Which.HResult.Should().Be(-2147024894);
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
			.Which.HResult.Should().Be(-2147467261);
		exception.Should().BeOfType<ArgumentNullException>()
			.Which.ParamName.Should().Be("path");
	}

	[SkippableTheory]
	[InlineAutoData(FileAccess.Read)]
	[InlineAutoData(FileAccess.ReadWrite)]
	[InlineAutoData(FileAccess.Write)]
	public void
		New_ReadOnlyFlag_WhenAccessContainsWrite_ShouldThrowUnauthorizedAccessException(
			FileAccess access,
			string path)
	{
		FileSystem.File.WriteAllText(path, "some content");
		FileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(path, FileMode.Open, access);
		});

		if (access.HasFlag(FileAccess.Write))
		{
			exception.Should().BeOfType<UnauthorizedAccessException>()
				.Which.HResult.Should().Be(-2147024891);
		}
		else
		{
			exception.Should().BeNull();
		}
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
				.Which.HResult.Should().Be(-2147024891);
			exception.Should().BeOfType<UnauthorizedAccessException>()
				.Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
		}
		else
		{
			exception.Should().BeOfType<IOException>()
				.Which.HResult.Should().Be(17);
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