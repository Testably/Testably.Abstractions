using System.IO;

namespace Testably.Abstractions.AccessControl.Tests;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class AccessControlHelperTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[Fact]
	public void GetExtensibilityOrThrow_DirectoryInfo_ShouldNotThrow()
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("foo");

		Exception? exception = Record.Exception(() =>
		{
			sut.GetExtensibilityOrThrow();
		});

		exception.Should().BeNull();
	}

	[Fact]
	public void GetExtensibilityOrThrow_FileSystemStream_ShouldNotThrow()
	{
		FileSystemStream sut = FileSystem.FileStream.New("foo", FileMode.Create);

		Exception? exception = Record.Exception(() =>
		{
			sut.GetExtensibilityOrThrow();
		});

		exception.Should().BeNull();
	}

	[Fact]
	public void GetExtensibilityOrThrow_FileInfo_ShouldNotThrow()
	{
		IFileInfo sut = FileSystem.FileInfo.New("foo");

		Exception? exception = Record.Exception(() =>
		{
			sut.GetExtensibilityOrThrow();
		});

		exception.Should().BeNull();
	}
}
