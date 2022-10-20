using System.IO;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests.Storage;

public class NullContainerTests
{
	[Theory]
	[AutoData]
	public void AppendBytes_ShouldReturnEmptyArray(byte[] bytes)
	{
		Testing.FileSystemMock fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.AppendBytes(bytes);

		sut.GetBytes().Should().BeEmpty();
	}

	[Fact]
	public void ClearBytes_ShouldReturnEmptyArray()
	{
		Testing.FileSystemMock fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.ClearBytes();

		sut.GetBytes().Should().BeEmpty();
	}

	[Fact]
	public void Constructor_ShouldSetFileAndTimeSystem()
	{
		Testing.FileSystemMock fileSystem = new();

		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.FileSystem.Should().Be(fileSystem);
		sut.TimeSystem.Should().Be(fileSystem.TimeSystem);
	}

	[Theory]
	[AutoData]
	public void CreationTime_WithUnspecifiedKind_ShouldReturnNullTime(string path)
	{
		Testing.FileSystemMock fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		DateTime result = sut.CreationTime.Get(DateTimeKind.Unspecified);

		result.Should().Be(new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc));
	}

	[Fact]
	public void Decrypt_ShouldReturnEmptyArray()
	{
		Testing.FileSystemMock fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.Decrypt();

		sut.GetBytes().Should().BeEmpty();
	}

	[Fact]
	public void Encrypt_ShouldReturnEmptyArray()
	{
		Testing.FileSystemMock fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.Encrypt();

		sut.GetBytes().Should().BeEmpty();
	}

	[Fact]
	public void GetBytes_ShouldReturnEmptyArray()
	{
		Testing.FileSystemMock fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.GetBytes().Should().BeEmpty();
	}

	[Theory]
	[AutoData]
	public void LinkTarget_ShouldAlwaysReturnNull(string linkTarget)
	{
		Testing.FileSystemMock fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);
		sut.LinkTarget.Should().BeNull();

		sut.LinkTarget = linkTarget;

		sut.LinkTarget.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void RequestAccess_ShouldReturnEmptyArray(FileAccess access, FileShare share)
	{
		Testing.FileSystemMock fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		IStorageAccessHandle result = sut.RequestAccess(access, share);

		result.Access.Should().Be(access);
		result.Share.Should().Be(share);
	}

	[Theory]
	[AutoData]
	public void Type_ShouldBeDirectoryOrFile(string linkTarget)
	{
		Testing.FileSystemMock fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.Type.Should().Be(FileSystemTypes.DirectoryOrFile);
	}

	[Theory]
	[AutoData]
	public void WriteBytes_ShouldReturnEmptyArray(byte[] bytes)
	{
		Testing.FileSystemMock fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.WriteBytes(bytes);

		sut.GetBytes().Should().BeEmpty();
	}
}