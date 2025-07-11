using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Tests.FileSystem;

public partial class FileSystemTests
{
	[Theory]
	[AutoData]
	public async Task
		Extensibility_HasWrappedInstance_WithCorrectType_ShouldReturnTrueOnRealFileSystem(
			string name)
	{
		IFileInfo entity = FileSystem.FileInfo.New(name);
		IFileSystemExtensibility? extensibility = entity as IFileSystemExtensibility;
		bool result = extensibility?.TryGetWrappedInstance(out System.IO.FileInfo? fileInfo)
		              ?? throw new NotSupportedException(
			              $"{entity.GetType()} does not implement IFileSystemExtensibility");

		if (FileSystem is RealFileSystem)
		{
			await That(result).IsTrue();
			await That(fileInfo!.Name).IsEqualTo(name);
		}
		else
		{
			await That(result).IsFalse();
		}
	}

	[Theory]
	[AutoData]
	public async Task Extensibility_HasWrappedInstance_WithIncorrectType_ShouldReturnAlwaysFalse(
		string name)
	{
		IFileInfo entity = FileSystem.FileInfo.New(name);
		IFileSystemExtensibility? extensibility = entity as IFileSystemExtensibility;
		bool result = extensibility?.TryGetWrappedInstance(
			              out System.IO.DirectoryInfo? directoryInfo)
		              ?? throw new NotSupportedException(
			              $"{entity.GetType()} does not implement IFileSystemExtensibility");

		await That(result).IsFalse();
		await That(directoryInfo).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task Extensibility_RetrieveMetadata_CorrectKeyAndType_ShouldReturnStoredValue(
		string name, DateTime time)
	{
		IFileInfo entity = FileSystem.FileInfo.New(name);
		IFileSystemExtensibility sut = entity as IFileSystemExtensibility
		                               ?? throw new NotSupportedException(
			                               $"{entity.GetType()} does not implement IFileSystemExtensibility");

		sut.StoreMetadata("foo", time);
		DateTime? result = sut.RetrieveMetadata<DateTime?>("foo");

		await That(result).IsEqualTo(time);
	}

	[Theory]
	[AutoData]
	public async Task Extensibility_RetrieveMetadata_DifferentKey_ShouldReturnNull(
		string name)
	{
		IFileInfo entity = FileSystem.FileInfo.New(name);
		IFileSystemExtensibility sut = entity as IFileSystemExtensibility
		                               ?? throw new NotSupportedException(
			                               $"{entity.GetType()} does not implement IFileSystemExtensibility");

		sut.StoreMetadata("foo", DateTime.Now);
		DateTime? result = sut.RetrieveMetadata<DateTime?>("bar");

		await That(result).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task Extensibility_RetrieveMetadata_DifferentType_ShouldReturnNull(
		string name)
	{
		IFileInfo entity = FileSystem.FileInfo.New(name);
		IFileSystemExtensibility sut = entity as IFileSystemExtensibility
		                               ?? throw new NotSupportedException(
			                               $"{entity.GetType()} does not implement IFileSystemExtensibility");

		sut.StoreMetadata("foo", DateTime.Now);
		TimeSpan? result = sut.RetrieveMetadata<TimeSpan?>("foo");

		await That(result).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task Extensibility_RetrieveMetadata_NotRegisteredKey_ShouldReturnNull(
		string name)
	{
		IFileInfo entity = FileSystem.FileInfo.New(name);
		IFileSystemExtensibility extensibility = entity as IFileSystemExtensibility
		                                         ?? throw new NotSupportedException(
			                                         $"{entity.GetType()} does not implement IFileSystemExtensibility");

		object? result = extensibility.RetrieveMetadata<object?>("foo");

		await That(result).IsNull();
	}
}
