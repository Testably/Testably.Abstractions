using NSubstitute.ExceptionExtensions;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfoFactory;

[FileSystemTests]
public partial class Tests
{
	[Theory]
	[InlineData("\0foo")]
	[InlineData("foo\0bar")]
	public async Task New_NullCharacter_ShouldThrowArgumentException(string path)
	{
#if NET8_0_OR_GREATER
		string expectedMessage = "Null character in path.";
#else
		string expectedMessage = "Illegal characters in path.";
#endif
		void Act()
		{
			_ = FileSystem.DirectoryInfo.New(path);
		}

		await That(Act).Throws<ArgumentException>()
			.WithMessageContaining(expectedMessage).And
#if !NETFRAMEWORK
			.WithParamName(nameof(path)).And
#endif
			.WithHResult(-2147024809);
	}

	[Theory]
	[AutoData]
	public async Task New_ShouldCreateNewDirectoryInfoFromPath(string path)
	{
		IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);

		await That(result.ToString()).IsEqualTo(path);
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

		void Act()
		{
			_ = FileSystem.DirectoryInfo.Wrap(directoryInfo);
		}

		await That(Act).Throws<NotSupportedException>().Whose(x => x.Message, it => it.Contains("Wrapping a DirectoryInfo in a simulated file system is not supported"));
	}
}
