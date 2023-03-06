using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Tests.FileSystem;

public abstract partial class FileSystemTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void
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
			result.Should().BeTrue();
			fileInfo!.Name.Should().Be(name);
		}
		else
		{
			result.Should().BeFalse();
		}
	}

	[SkippableTheory]
	[AutoData]
	public void
		Extensibility_HasWrappedInstance_WithIncorrectType_ShouldReturnAlwaysFalse(
			string name)
	{
		IFileInfo entity = FileSystem.FileInfo.New(name);
		IFileSystemExtensibility? extensibility = entity as IFileSystemExtensibility;
		bool result = extensibility?.TryGetWrappedInstance(
			              out System.IO.DirectoryInfo? directoryInfo)
		              ?? throw new NotSupportedException(
			              $"{entity.GetType()} does not implement IFileSystemExtensibility");

		result.Should().BeFalse();
		directoryInfo.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void
		Extensibility_RetrieveMetadata_CorrectKeyAndType_ShouldReturnStoredValue(
			string name, DateTime time)
	{
		IFileInfo entity = FileSystem.FileInfo.New(name);
		IFileSystemExtensibility sut = entity as IFileSystemExtensibility
		                               ?? throw new NotSupportedException(
			                               $"{entity.GetType()} does not implement IFileSystemExtensibility");

		sut.StoreMetadata("foo", time);
		DateTime? result = sut.RetrieveMetadata<DateTime?>("foo");

		result.Should().Be(time);
	}

	[SkippableTheory]
	[AutoData]
	public void Extensibility_RetrieveMetadata_DifferentKey_ShouldReturnNull(
		string name)
	{
		IFileInfo entity = FileSystem.FileInfo.New(name);
		IFileSystemExtensibility sut = entity as IFileSystemExtensibility
		                               ?? throw new NotSupportedException(
			                               $"{entity.GetType()} does not implement IFileSystemExtensibility");

		sut.StoreMetadata("foo", DateTime.Now);
		DateTime? result = sut.RetrieveMetadata<DateTime?>("bar");

		result.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void Extensibility_RetrieveMetadata_DifferentType_ShouldReturnNull(
		string name)
	{
		IFileInfo entity = FileSystem.FileInfo.New(name);
		IFileSystemExtensibility sut = entity as IFileSystemExtensibility
		                               ?? throw new NotSupportedException(
			                               $"{entity.GetType()} does not implement IFileSystemExtensibility");

		sut.StoreMetadata("foo", DateTime.Now);
		TimeSpan? result = sut.RetrieveMetadata<TimeSpan?>("foo");

		result.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void Extensibility_RetrieveMetadata_NotRegisteredKey_ShouldReturnNull(
		string name)
	{
		IFileInfo entity = FileSystem.FileInfo.New(name);
		IFileSystemExtensibility extensibility = entity as IFileSystemExtensibility
		                                         ?? throw new NotSupportedException(
			                                         $"{entity.GetType()} does not implement IFileSystemExtensibility");

		object? result = extensibility.RetrieveMetadata<object?>("foo");

		result.Should().BeNull();
	}
}
