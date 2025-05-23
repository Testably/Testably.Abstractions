namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfoFactory;

[FileSystemTests]
public partial class Tests
{
	[Theory]
	[InlineData("\0foo")]
	[InlineData("foo\0bar")]
	public void New_NullCharacter_ShouldThrowArgumentException(string path)
	{
#if NET8_0_OR_GREATER
		string expectedMessage = "Null character in path.";
#else
		string expectedMessage = "Illegal characters in path.";
#endif
		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.DirectoryInfo.New(path);
		});

		exception.Should().BeException<ArgumentException>(expectedMessage,
#if !NETFRAMEWORK
			paramName: nameof(path),
#endif
			hResult: -2147024809);
	}

	[Theory]
	[AutoData]
	public void New_ShouldCreateNewDirectoryInfoFromPath(string path)
	{
		IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);

		result.ToString().Should().Be(path);
		result.Exists.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void New_WithTrailingDirectorySeparatorChar_ShouldHavePathAsName(string path)
	{
		IDirectoryInfo result = FileSystem.DirectoryInfo
			.New($"{path}{FileSystem.Path.DirectorySeparatorChar}");

		result.Name.Should().Be(path);
	}

	[Fact]
	public void Wrap_Null_ShouldReturnNull()
	{
		Skip.If(FileSystem is MockFileSystem mockFileSystem &&
		        mockFileSystem.SimulationMode != SimulationMode.Native);

		IDirectoryInfo? result = FileSystem.DirectoryInfo.Wrap(null);

		result.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void Wrap_ShouldWrapFromDirectoryInfo(string path)
	{
		Skip.If(FileSystem is MockFileSystem mockFileSystem &&
		        mockFileSystem.SimulationMode != SimulationMode.Native);

		System.IO.DirectoryInfo directoryInfo = new(path);

		IDirectoryInfo result = FileSystem.DirectoryInfo.Wrap(directoryInfo);

		result.FullName.Should().Be(directoryInfo.FullName);
		result.Exists.Should().Be(directoryInfo.Exists);
	}

	[Theory]
	[AutoData]
	public void Wrap_WithSimulatedMockFileSystem_ShouldThrowNotSupportedException(string path)
	{
		Skip.IfNot(FileSystem is MockFileSystem mockFileSystem &&
		           mockFileSystem.SimulationMode != SimulationMode.Native);

		System.IO.DirectoryInfo directoryInfo = new(path);

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.DirectoryInfo.Wrap(directoryInfo);
		});

		exception.Should().BeOfType<NotSupportedException>().Which
			.Message.Should()
			.Contain("Wrapping a DirectoryInfo in a simulated file system is not supported");
	}
}
