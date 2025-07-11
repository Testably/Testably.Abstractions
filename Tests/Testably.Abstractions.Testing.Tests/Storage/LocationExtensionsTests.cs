using System.IO;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests.Storage;

public sealed class LocationExtensionsTests
{
	[Fact]
	public async Task
		ThrowExceptionIfNotFound_MissingDirectory_ShouldThrowDirectoryNotFoundException()
	{
		MockFileSystem fileSystem = new();
		IStorageLocation location = fileSystem.Storage.GetLocation("foo/bar.txt");

		Exception? exception = Record.Exception(() =>
		{
			location.ThrowExceptionIfNotFound(fileSystem);
		});

		await That(exception).IsExactly<DirectoryNotFoundException>();
	}

	[Theory]
	[AutoData]
	public async Task
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

		await That(exception).IsSameAs(expectedException);
	}

	[Fact]
	public async Task ThrowExceptionIfNotFound_MissingFile_ShouldThrowFileNotFoundException()
	{
		MockFileSystem fileSystem = new();
		IStorageLocation location = fileSystem.Storage.GetLocation("foo.txt");

		Exception? exception = Record.Exception(() =>
		{
			location.ThrowExceptionIfNotFound(fileSystem);
		});

		await That(exception).IsExactly<FileNotFoundException>();
	}

	[Theory]
	[AutoData]
	public async Task
		ThrowExceptionIfNotFound_MissingFile_WithCustomCallback_ShouldThrowExceptionFromCallback(
			Exception expectedException)
	{
		MockFileSystem fileSystem = new();
		IStorageLocation location = fileSystem.Storage.GetLocation("foo");

		Exception? exception = Record.Exception(() =>
		{
			location.ThrowExceptionIfNotFound(fileSystem, onFileNotFound: _ => expectedException);
		});

		await That(exception).IsSameAs(expectedException);
	}

	[Theory]
	[AutoData]
	public async Task ThrowIfNotFound_MissingDirectory_ShouldExecuteFileNotFoundAction(
		Exception expectedException)
	{
		MockFileSystem fileSystem = new();
		IStorageLocation location = fileSystem.Storage.GetLocation("foo/bar.txt");

		Exception? exception = Record.Exception(() =>
		{
			location.ThrowIfNotFound(fileSystem, () => { },
				directoryNotFoundException: () => throw expectedException);
		});

		await That(exception).IsSameAs(expectedException);
	}

	[Theory]
	[AutoData]
	public async Task ThrowIfNotFound_MissingFile_ShouldExecuteFileNotFoundAction(
		Exception expectedException)
	{
		MockFileSystem fileSystem = new();
		IStorageLocation location = fileSystem.Storage.GetLocation("foo");

		Exception? exception = Record.Exception(() =>
		{
			location.ThrowIfNotFound(fileSystem, () => throw expectedException);
		});

		await That(exception).IsSameAs(expectedException);
	}

	[Theory]
	[AutoData]
	public async Task ThrowIfNotFound_Null_ShouldExecuteFileNotFoundAction(
		Exception expectedException)
	{
		MockFileSystem fileSystem = new();
		IStorageLocation? location = null;

		Exception? exception = Record.Exception(() =>
		{
			location.ThrowIfNotFound(fileSystem, () => throw expectedException);
		});

		await That(exception).IsSameAs(expectedException);
	}
}
