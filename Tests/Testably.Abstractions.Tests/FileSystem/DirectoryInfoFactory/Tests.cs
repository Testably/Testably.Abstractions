using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfoFactory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void New_EmptyPath_ShouldThrowArgumentException()
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.DirectoryInfo.New(string.Empty);
		});

		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
#if !NETFRAMEWORK
		exception.Should().BeOfType<ArgumentException>()
		   .Which.ParamName.Should().Be("path");
#endif
	}

	[SkippableFact]
	public void New_Null_ShouldThrowArgumentNullException()
	{
		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.DirectoryInfo.New(null!);
		});

		exception.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
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