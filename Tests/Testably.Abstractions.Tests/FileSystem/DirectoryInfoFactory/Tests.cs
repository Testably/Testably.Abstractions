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
	public async Task New_ShouldCreateNewDirectoryInfoFromPath(string path)
	{
		IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);

		result.ToString().Should().Be(path);
		await That(result.Exists).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task New_WithTrailingDirectorySeparatorChar_ShouldHavePathAsName(string path)
	{
		IDirectoryInfo result = FileSystem.DirectoryInfo
			.New($"{path}{FileSystem.Path.DirectorySeparatorChar}");

		await That(result.Name).IsEqualTo(path);
	}

	[Fact]
	public async Task Wrap_Null_ShouldReturnNull()
	{
		Skip.If(FileSystem is MockFileSystem mockFileSystem &&
				mockFileSystem.SimulationMode != SimulationMode.Native);

		IDirectoryInfo? result = FileSystem.DirectoryInfo.Wrap(null);

		await That(result).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task Wrap_ShouldWrapFromDirectoryInfo(string path)
	{
		Skip.If(FileSystem is MockFileSystem mockFileSystem &&
				mockFileSystem.SimulationMode != SimulationMode.Native);

		System.IO.DirectoryInfo directoryInfo = new(path);

		IDirectoryInfo result = FileSystem.DirectoryInfo.Wrap(directoryInfo);

		await That(result.FullName).IsEqualTo(directoryInfo.FullName);
		await That(result.Exists).IsEqualTo(directoryInfo.Exists);
	}

	[Theory]
	[AutoData]
	public async Task Wrap_WithSimulatedMockFileSystem_ShouldThrowNotSupportedException(string path)
	{
		Skip.IfNot(FileSystem is MockFileSystem mockFileSystem &&
				   mockFileSystem.SimulationMode != SimulationMode.Native);

		System.IO.DirectoryInfo directoryInfo = new(path);

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.DirectoryInfo.Wrap(directoryInfo);
		});

		await That(exception).IsExactly<NotSupportedException>().Whose(x => x.Message, it => it.Contains("Wrapping a DirectoryInfo in a simulated file system is not supported"));
	}
}
