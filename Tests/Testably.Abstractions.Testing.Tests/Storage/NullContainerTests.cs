using System.IO;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests.Storage;

public class NullContainerTests
{
	[Theory]
	[AutoData]
	public void AppendBytes_ShouldReturnEmptyArray(byte[] bytes)
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.AppendBytes(bytes);

		sut.GetBytes().Should().BeEmpty();
	}

	[Fact]
	public void ClearBytes_ShouldReturnEmptyArray()
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.ClearBytes();

		sut.GetBytes().Should().BeEmpty();
	}

	[Fact]
	public void Constructor_ShouldSetFileAndTimeSystem()
	{
		MockFileSystem fileSystem = new();

		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.FileSystem.Should().BeSameAs(fileSystem);
		sut.TimeSystem.Should().BeSameAs(fileSystem.TimeSystem);
	}

	[Fact]
	public void CreationTime_WithUnspecifiedKind_ShouldReturnNullTime()
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		DateTime result = sut.CreationTime.Get(DateTimeKind.Unspecified);

		result.Should().Be(new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc));
	}

	[Fact]
	public void Decrypt_ShouldReturnEmptyArray()
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.Decrypt();

		sut.GetBytes().Should().BeEmpty();
	}

	[Fact]
	public void Encrypt_ShouldReturnEmptyArray()
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.Encrypt();

		sut.GetBytes().Should().BeEmpty();
	}

	[Fact]
	public void GetBytes_ShouldReturnEmptyArray()
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.GetBytes().Should().BeEmpty();
	}

	[Theory]
	[AutoData]
	public void LinkTarget_ShouldAlwaysReturnNull(string linkTarget)
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);
		sut.LinkTarget.Should().BeNull();

		sut.LinkTarget = linkTarget;

		sut.LinkTarget.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void RequestAccess_Dispose_Twice_ShouldDoNothing(FileAccess access, FileShare share)
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		IStorageAccessHandle result = sut.RequestAccess(access, share);
		result.Dispose();

		Exception? exception = Record.Exception(() =>
		{
			result.Dispose();
		});

		exception.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void RequestAccess_ShouldReturnNullObject(FileAccess access, FileShare share,
		bool deleteAccess)
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		IStorageAccessHandle result = sut.RequestAccess(access, share, deleteAccess);

		result.Access.Should().Be(access);
		result.Share.Should().Be(share);
		result.DeleteAccess.Should().Be(deleteAccess);
	}

	[Fact]
	public void Type_ShouldBeDirectoryOrFile()
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.Type.Should().Be(FileSystemTypes.DirectoryOrFile);
	}

	[Theory]
	[AutoData]
	public void WriteBytes_ShouldReturnEmptyArray(byte[] bytes)
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.WriteBytes(bytes);

		sut.GetBytes().Should().BeEmpty();
	}
}
