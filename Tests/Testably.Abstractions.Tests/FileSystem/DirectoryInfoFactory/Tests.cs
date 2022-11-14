using Testably.Abstractions.FileSystem;

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
		string expectedMessage = "Illegal characters in path.";
		Exception? exception =
			Record.Exception(() => FileSystem.DirectoryInfo.New(path));

		exception.Should().BeException<ArgumentException>(expectedMessage,
			hResult: -2147024809);
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
