using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Tests.Internal;

public class FileSystemExtensibilityTests
{
	[Theory]
	[AutoData]
	public void RetrieveMetadata_IncorrectType_ShouldReturnNull(string path, string key)
	{
		FileInfo value = new(path);
		RealFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New(path);
		IFileSystemExtensibility? extensibility = sut as IFileSystemExtensibility;
		extensibility!.StoreMetadata(key, value);

		DirectoryInfo? result = extensibility.RetrieveMetadata<DirectoryInfo>(key);

		result.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void RetrieveMetadata_WithoutStoringBefore_ShouldReturnDefault(string path, string key)
	{
		RealFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New(path);
		IFileSystemExtensibility? extensibility = sut as IFileSystemExtensibility;

		object? result = extensibility!.RetrieveMetadata<object?>(key);

		result.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void StoreMetadata_ShouldMakeValueRetrievable(string path, string key, object value)
	{
		RealFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New(path);
		IFileSystemExtensibility? extensibility = sut as IFileSystemExtensibility;

		extensibility!.StoreMetadata(key, value);

		object? result = extensibility.RetrieveMetadata<object>(key);
		result.Should().Be(value);
	}

	[Theory]
	[AutoData]
	public void TryGetWrappedInstance_IncorrectType_ShouldReturnNull(string path)
	{
		RealFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New(path);
		IFileSystemExtensibility? extensibility = sut as IFileSystemExtensibility;

		bool result = extensibility!.TryGetWrappedInstance(out DirectoryInfo? fileInfo);

		result.Should().BeFalse();
		fileInfo.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void TryGetWrappedInstance_ShouldReturnWrappedInstance(string path)
	{
		RealFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New(path);
		IFileSystemExtensibility? extensibility = sut as IFileSystemExtensibility;

		bool result = extensibility!.TryGetWrappedInstance(out FileInfo? fileInfo);

		result.Should().BeTrue();
		fileInfo.Should().NotBeNull();
	}
}
