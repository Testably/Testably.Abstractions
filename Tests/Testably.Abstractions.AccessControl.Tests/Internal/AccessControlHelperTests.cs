using NSubstitute;
using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.AccessControl.Tests.Internal;

public sealed class AccessControlHelperTests
{
	[Fact]
	public void GetExtensibilityOrThrow_CustomDirectoryInfo_ShouldThrowNotSupportedException()
	{
		IDirectoryInfo? sut = Substitute.For<IDirectoryInfo>();

		Exception? exception = Record.Exception(() =>
		{
			sut.GetExtensibilityOrThrow();
		});

		exception.Should().BeOfType<NotSupportedException>()
			.Which.Message.Should()
			.Contain(nameof(IFileSystemExtensibility)).And
			.Contain(sut.GetType().Name);
	}

	[Fact]
	public void GetExtensibilityOrThrow_CustomFileInfo_ShouldThrowNotSupportedException()
	{
		IFileInfo? sut = Substitute.For<IFileInfo>();

		Exception? exception = Record.Exception(() =>
		{
			sut.GetExtensibilityOrThrow();
		});

		exception.Should().BeOfType<NotSupportedException>()
			.Which.Message.Should()
			.Contain(nameof(IFileSystemExtensibility)).And
			.Contain(sut.GetType().Name);
	}

	[Fact]
	public void GetExtensibilityOrThrow_CustomFileSystemStream_ShouldThrowNotSupportedException()
	{
		FileSystemStream sut = new CustomFileSystemStream();

		Exception? exception = Record.Exception(() =>
		{
			sut.GetExtensibilityOrThrow();
		});

		exception.Should().BeOfType<NotSupportedException>()
			.Which.Message.Should()
			.Contain(nameof(IFileSystemExtensibility)).And
			.Contain(sut.GetType().Name);
	}

	[Fact]
	public void ThrowIfMissing_ExistingDirectoryInfo_ShouldNotThrow()
	{
		MockFileSystem fileSystem = new();
		IDirectoryInfo sut = fileSystem.DirectoryInfo.New("foo");
		fileSystem.Directory.CreateDirectory("foo");

		Exception? exception = Record.Exception(() =>
		{
			sut.ThrowIfMissing();
		});

		exception.Should().BeNull();
	}

	[Fact]
	public void ThrowIfMissing_ExistingFileInfo_ShouldNotThrow()
	{
		MockFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New("foo");
		fileSystem.File.WriteAllText("foo", "some content");

		Exception? exception = Record.Exception(() =>
		{
			sut.ThrowIfMissing();
		});

		exception.Should().BeNull();
	}

	[Fact]
	public void ThrowIfMissing_MissingDirectoryInfo_ShouldThrowDirectoryNotFoundException()
	{
		MockFileSystem fileSystem = new();
		IDirectoryInfo sut = fileSystem.DirectoryInfo.New("foo");

		Exception? exception = Record.Exception(() =>
		{
			sut.ThrowIfMissing();
		});

		exception.Should().BeOfType<DirectoryNotFoundException>()
			.Which.HResult.Should().Be(-2147024893);
		exception!.Message.Should().Contain($"'{sut.FullName}'");
	}

	[Fact]
	public void ThrowIfMissing_MissingFileInfo_ShouldThrowFileNotFoundException()
	{
		MockFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New("foo");

		Exception? exception = Record.Exception(() =>
		{
			sut.ThrowIfMissing();
		});

		exception.Should().BeOfType<FileNotFoundException>()
			.Which.HResult.Should().Be(-2147024894);
		exception!.Message.Should().Contain($"'{sut.FullName}'");
	}

	private class CustomFileSystemStream() : FileSystemStream(Null, ".", false);
}
