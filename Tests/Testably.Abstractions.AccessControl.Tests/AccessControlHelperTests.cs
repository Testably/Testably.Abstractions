using System.IO;

namespace Testably.Abstractions.AccessControl.Tests;

[FileSystemTests]
public partial class AccessControlHelperTests
{
	[Fact]
	public async Task GetExtensibilityOrThrow_DirectoryInfo_ShouldNotThrow()
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("foo");

		await That(() => sut.GetExtensibilityOrThrow()).DoesNotThrow();
	}

	[Fact]
	public async Task GetExtensibilityOrThrow_FileInfo_ShouldNotThrow()
	{
		IFileInfo sut = FileSystem.FileInfo.New("foo");

		await That(() => sut.GetExtensibilityOrThrow()).DoesNotThrow();
	}

	[Fact]
	public async Task GetExtensibilityOrThrow_FileSystemStream_ShouldNotThrow()
	{
		FileSystemStream sut = FileSystem.FileStream.New("foo", FileMode.Create);

		await That(() => sut.GetExtensibilityOrThrow()).DoesNotThrow();
	}
}
