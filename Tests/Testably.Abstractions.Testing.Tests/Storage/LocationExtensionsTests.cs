using System.IO;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests.Storage;

public sealed class LocationExtensionsTests
{
	[Fact]
	public void ThrowExceptionIfNotFound_MissingDirectory_ShouldThrowDirectoryNotFoundException()
	{
		MockFileSystem fileSystem = new();
		IStorageLocation location = fileSystem.Storage.GetLocation("foo/bar.txt");

		Exception? exception = Record.Exception(() =>
		{
			location.ThrowExceptionIfNotFound(fileSystem);
		});

		exception.Should().BeOfType<DirectoryNotFoundException>();
	}

	[Theory]
	[AutoData]
	public void
		ThrowExceptionIfNotFound_MissingDirectory_WithCustomCallback_ShouldThrowExceptionFromCallback(
			Exception expectedException)
	{
		MockFileSystem fileSystem = new();
		IStorageLocation location = fileSystem.Storage.GetLocation("foo/bar.txt");

		Exception? exception = Record.Exception(() =>
		{
			location.ThrowExceptionIfNotFound(fileSystem,
				onDirectoryNotFound: _ => expectedException);
		});

		exception.Should().BeSameAs(expectedException);
	}

	[Fact]
	public void ThrowExceptionIfNotFound_MissingFile_ShouldThrowFileNotFoundException()
	{
		MockFileSystem fileSystem = new();
		IStorageLocation location = fileSystem.Storage.GetLocation("foo.txt");

		Exception? exception = Record.Exception(() =>
		{
			location.ThrowExceptionIfNotFound(fileSystem);
		});

		exception.Should().BeOfType<FileNotFoundException>();
	}

	[Theory]
	[AutoData]
	public void
		ThrowExceptionIfNotFound_MissingFile_WithCustomCallback_ShouldThrowExceptionFromCallback(
			Exception expectedException)
	{
		MockFileSystem fileSystem = new();
		IStorageLocation location = fileSystem.Storage.GetLocation("foo");

		Exception? exception = Record.Exception(() =>
		{
			location.ThrowExceptionIfNotFound(fileSystem, onFileNotFound: _ => expectedException);
		});

		exception.Should().BeSameAs(expectedException);
	}

	[Theory]
	[AutoData]
	public void ThrowIfNotFound_MissingDirectory_ShouldExecuteFileNotFoundAction(
		Exception expectedException)
	{
		MockFileSystem fileSystem = new();
		IStorageLocation location = fileSystem.Storage.GetLocation("foo/bar.txt");

		Exception? exception = Record.Exception(() =>
		{
			location.ThrowIfNotFound(fileSystem, () => { },
				directoryNotFoundException: () => throw expectedException);
		});

		exception.Should().BeSameAs(expectedException);
	}

	[Theory]
	[AutoData]
	public void ThrowIfNotFound_MissingFile_ShouldExecuteFileNotFoundAction(
		Exception expectedException)
	{
		MockFileSystem fileSystem = new();
		IStorageLocation location = fileSystem.Storage.GetLocation("foo");

		Exception? exception = Record.Exception(() =>
		{
			location.ThrowIfNotFound(fileSystem, () => throw expectedException);
		});

		exception.Should().BeSameAs(expectedException);
	}

	[Theory]
	[AutoData]
	public void ThrowIfNotFound_Null_ShouldExecuteFileNotFoundAction(Exception expectedException)
	{
		MockFileSystem fileSystem = new();
		IStorageLocation? location = null;

		Exception? exception = Record.Exception(() =>
		{
			location.ThrowIfNotFound(fileSystem, () => throw expectedException);
		});

		exception.Should().BeSameAs(expectedException);
	}
}
