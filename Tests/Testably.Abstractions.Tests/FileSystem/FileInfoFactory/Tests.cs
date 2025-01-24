using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfoFactory;

[FileSystemTests]
public partial class Tests
{
	[Theory]
	[InlineData(259)]
	[InlineData(260)]
	public void New_PathTooLong_ShouldThrowPathTooLongException_OnNetFramework(
		int maxLength)
	{
		string rootDrive = FileTestHelper.RootDrive(Test);
		string path = new('a', maxLength - rootDrive.Length);
		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.FileInfo.New(rootDrive + path);
		});

		if (Test.IsNetFramework)
		{
			exception.Should().BeException<PathTooLongException>(hResult: -2147024690);
		}
		else
		{
			exception.Should().BeNull();
		}
	}

	[Theory]
	[AutoData]
	public void New_ShouldCreateNewFileInfoFromPath(string path)
	{
		IFileInfo result = FileSystem.FileInfo.New(path);

		result.ToString().Should().Be(path);
		result.Should().NotExist();
	}

	[Theory]
	[AutoData]
	public void New_ShouldOpenWithExistingContent(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		IFileInfo sut = FileSystem.FileInfo.New(path);

		using StreamReader streamReader = new(sut.OpenRead());
		string result = streamReader.ReadToEnd();
		result.Should().Be(contents);
	}

	[Theory]
	[AutoData]
	public void New_ShouldSetLength(string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);

		FileSystemStream stream = FileSystem.File.Open(path, FileMode.Open,
			FileAccess.Read, FileShare.Write);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		long result = sut.Length;

		stream.Dispose();

		result.Should().Be(bytes.Length);
	}

	[Theory]
	[AutoData]
	public void New_WithTrailingDirectorySeparatorChar_ShouldHaveEmptyName(string path)
	{
		IFileInfo result =
			FileSystem.FileInfo.New($"{path}{FileSystem.Path.DirectorySeparatorChar}");

		result.Name.Should().Be(string.Empty);
	}

	[Fact]
	public void New_WithUnicodeWhitespace_ShouldNotThrow()
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New("\u00A0"); // Unicode char that's treated as whitespace
		});

		if (Test.IsNetFramework)
		{
			exception.Should().BeOfType<ArgumentException>();
		}
		else
		{
			exception.Should().BeNull();
		}
	}

	[Fact]
	public void New_WithWhitespace_ShouldThrowOnlyOnWindows()
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New("   ");
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<ArgumentException>();
		}
		else
		{
			exception.Should().BeNull();
		}
	}

	[Fact]
	public void Wrap_Null_ShouldReturnNull()
	{
		Skip.If(FileSystem is MockFileSystem mockFileSystem &&
		        mockFileSystem.SimulationMode != SimulationMode.Native);

		IFileInfo? result = FileSystem.FileInfo.Wrap(null);

		result.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void Wrap_ShouldWrapFromFileInfo(string path)
	{
		Skip.If(FileSystem is MockFileSystem mockFileSystem &&
		        mockFileSystem.SimulationMode != SimulationMode.Native);

		System.IO.FileInfo fileInfo = new(path);

		IFileInfo result = FileSystem.FileInfo.Wrap(fileInfo);

		result.FullName.Should().Be(fileInfo.FullName);
		result.Exists.Should().Be(fileInfo.Exists);
	}

	[Theory]
	[AutoData]
	public void Wrap_WithSimulatedMockFileSystem_ShouldThrowNotSupportedException(string path)
	{
		Skip.IfNot(FileSystem is MockFileSystem mockFileSystem &&
		           mockFileSystem.SimulationMode != SimulationMode.Native);

		System.IO.FileInfo fileInfo = new(path);

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.FileInfo.Wrap(fileInfo);
		});

		exception.Should().BeOfType<NotSupportedException>().Which
			.Message.Should()
			.Contain("Wrapping a FileInfo in a simulated file system is not supported");
	}
}
