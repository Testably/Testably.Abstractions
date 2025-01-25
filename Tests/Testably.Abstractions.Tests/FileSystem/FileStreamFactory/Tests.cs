using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileStreamFactory;

[FileSystemTests]
public partial class Tests
{
	[Theory]
	[AutoData]
	public void New_AppendAccessWithReadWriteMode_ShouldThrowArgumentException(
		string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(path, FileMode.Append, FileAccess.ReadWrite);
		});

		exception.Should().BeException<ArgumentException>(
			messageContains: nameof(FileMode.Append),
			hResult: -2147024809,
			paramName: Test.IsNetFramework ? null : "access");
	}

	[Theory]
	[AutoData]
	public void New_ExistingFileWithCreateMode_ShouldIgnoreContent(
		string path)
	{
		FileSystem.File.WriteAllText(path, "foo");
		FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Create);
		stream.Dispose();

		FileSystem.File.ReadAllText(path).Should().BeEmpty();
	}

	[Theory]
	[AutoData]
	public void New_ExistingFileWithCreateNewMode_ShouldThrowIOException(
		string path)
	{
		FileSystem.File.WriteAllText(path, "foo");
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(path, FileMode.CreateNew);
		});

		exception.Should().BeException<IOException>(
			$"'{FileSystem.Path.GetFullPath(path)}'",
			hResult: Test.RunsOnWindows ? -2147024816 : 17);
	}

	[Theory]
	[AutoData]
	public void New_ExistingFileWithTruncateMode_ShouldIgnoreContent(
		string path)
	{
		FileSystem.File.WriteAllText(path, "foo");
		FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Truncate);
		stream.Dispose();

		FileSystem.File.ReadAllText(path).Should().BeEmpty();
	}

	[Theory]
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

		exception.Should().BeException<ArgumentException>(
			hResult: -2147024809,
			paramName: Test.IsNetFramework ? null : "access");
		exception!.Message.Should()
			.Contain(mode.ToString()).And
			.Contain(access.ToString());
	}

	[Theory]
	[InlineAutoData(FileMode.Open)]
	[InlineAutoData(FileMode.Truncate)]
	public void New_MissingFileWithIncorrectMode_ShouldThrowFileNotFoundException(
		FileMode mode, string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(path, mode);
		});

		exception.Should().BeException<FileNotFoundException>(
			$"'{FileSystem.Path.GetFullPath(path)}'",
			hResult: -2147024894);
	}

	[Theory]
	[AutoData]
	public void New_MissingFileWithTruncateMode_ShouldThrowFileNotFoundException(
		string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(path, FileMode.Truncate);
		});

		exception.Should().BeException<FileNotFoundException>(
			$"'{FileSystem.Path.GetFullPath(path)}'",
			hResult: -2147024894);
	}

	[Theory]
	[InlineAutoData(FileAccess.Read)]
	[InlineAutoData(FileAccess.ReadWrite)]
	[InlineAutoData(FileAccess.Write)]
	public void
		New_ReadOnlyFlag_ShouldThrowUnauthorizedAccessException_WhenAccessContainsWrite(
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
			exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
		}
		else
		{
			exception.Should().BeNull();
		}
	}

	[Theory]
	[AutoData]
	public void New_SamePathAsExistingDirectory_ShouldThrowCorrectException(
		string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileStream.New(path, FileMode.CreateNew);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<UnauthorizedAccessException>(
				$"'{FileSystem.Path.GetFullPath(path)}'",
				hResult: -2147024891);
		}
		else
		{
			exception.Should().BeException<IOException>(
				$"'{FileSystem.Path.GetFullPath(path)}'",
				hResult: 17);
		}
	}

	[Theory]
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
