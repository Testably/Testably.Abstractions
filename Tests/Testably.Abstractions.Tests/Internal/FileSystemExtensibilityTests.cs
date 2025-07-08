using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Tests.Internal;

public class FileSystemExtensibilityTests
{
	[Theory]
	[AutoData]
	public async Task RetrieveMetadata_IncorrectType_ShouldReturnNull(string path, string key)
	{
		FileInfo value = new(path);
		RealFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New(path);
		IFileSystemExtensibility? extensibility = sut as IFileSystemExtensibility;
		extensibility!.StoreMetadata(key, value);

		DirectoryInfo? result = extensibility.RetrieveMetadata<DirectoryInfo>(key);

		await That(result).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task RetrieveMetadata_WithoutStoringBefore_ShouldReturnDefault(string path, string key)
	{
		RealFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New(path);
		IFileSystemExtensibility? extensibility = sut as IFileSystemExtensibility;

		object? result = extensibility!.RetrieveMetadata<object?>(key);

		await That(result).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task StoreMetadata_ShouldMakeValueRetrievable(string path, string key, object value)
	{
		RealFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New(path);
		IFileSystemExtensibility? extensibility = sut as IFileSystemExtensibility;

		extensibility!.StoreMetadata(key, value);

		object? result = extensibility.RetrieveMetadata<object>(key);
		await That(result).IsEqualTo(value);
	}

	[Theory]
	[AutoData]
	public async Task TryGetWrappedInstance_IncorrectType_ShouldReturnNull(string path)
	{
		RealFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New(path);
		IFileSystemExtensibility? extensibility = sut as IFileSystemExtensibility;

		bool result = extensibility!.TryGetWrappedInstance(out DirectoryInfo? fileInfo);

		await That(result).IsFalse();
		await That(fileInfo).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task TryGetWrappedInstance_ShouldReturnWrappedInstance(string path)
	{
		RealFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New(path);
		IFileSystemExtensibility? extensibility = sut as IFileSystemExtensibility;

		bool result = extensibility!.TryGetWrappedInstance(out FileInfo? fileInfo);

		await That(result).IsTrue();
		await That(fileInfo).IsNotNull();
	}
}
