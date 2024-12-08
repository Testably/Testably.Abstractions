using aweXpect;
using System.IO;
using System.Threading.Tasks;

namespace Testably.Abstractions.AccessControl.Tests;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class AccessControlHelperTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public async Task GetExtensibilityOrThrow_DirectoryInfo_ShouldNotThrow()
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("foo");

		await That(() => sut.GetExtensibilityOrThrow()).Should().NotThrow();
	}

	[SkippableFact]
	public async Task GetExtensibilityOrThrow_FileInfo_ShouldNotThrow()
	{
		IFileInfo sut = FileSystem.FileInfo.New("foo");

		await That(() => sut.GetExtensibilityOrThrow()).Should().NotThrow();
	}

	[SkippableFact]
	public async Task GetExtensibilityOrThrow_FileSystemStream_ShouldNotThrow()
	{
		FileSystemStream sut = FileSystem.FileStream.New("foo", FileMode.Create);

		await That(() => sut.GetExtensibilityOrThrow()).Should().NotThrow();
	}
}
