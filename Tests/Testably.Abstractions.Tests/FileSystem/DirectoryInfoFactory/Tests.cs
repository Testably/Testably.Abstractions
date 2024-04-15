namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfoFactory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
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
			paramName: "path",
#endif
			hResult: -2147024809);
	}

	[SkippableTheory]
	[AutoData]
	public void New_ShouldCreateNewDirectoryInfoFromPath(string path)
	{
		IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);

		result.ToString().Should().Be(path);
		result.Should().NotExist();
	}

	[SkippableTheory]
	[AutoData]
	public void New_WithTrailingDirectorySeparatorChar_ShouldHavePathAsName(string path)
	{
		IDirectoryInfo result = FileSystem.DirectoryInfo
			.New($"{path}{FileSystem.Path.DirectorySeparatorChar}");

		result.Name.Should().Be(path);
	}

	[SkippableFact]
	public void Wrap_Null_ShouldReturnNull()
	{
		Skip.If(FileSystem is MockFileSystem mockFileSystem &&
		        mockFileSystem.SimulationMode != SimulationMode.Native);

		IDirectoryInfo? result = FileSystem.DirectoryInfo.Wrap(null);

		result.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void Wrap_ShouldWrapFromDirectoryInfo(string path)
	{
		Skip.If(FileSystem is MockFileSystem mockFileSystem &&
		        mockFileSystem.SimulationMode != SimulationMode.Native);

		System.IO.DirectoryInfo directoryInfo = new("S:\\" + path);

		IDirectoryInfo result = FileSystem.DirectoryInfo.Wrap(directoryInfo);

		result.FullName.Should().Be(directoryInfo.FullName);
		result.Exists.Should().Be(directoryInfo.Exists);
	}

	[SkippableTheory]
	[AutoData]
	public void Wrap_WithSimulatedMockFileSystem_ShouldThrowNotSupportedException(string path)
	{
		Skip.IfNot(FileSystem is MockFileSystem mockFileSystem &&
		           mockFileSystem.SimulationMode != SimulationMode.Native);

		System.IO.DirectoryInfo directoryInfo = new("S:\\" + path);

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.DirectoryInfo.Wrap(directoryInfo);
		});

		exception.Should().BeOfType<NotSupportedException>().Which
			.Message.Should()
			.Contain("Wrapping a DirectoryInfo in a simulated file system is not supported");
	}
}
