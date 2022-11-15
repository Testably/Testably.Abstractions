using Testably.Abstractions.FileSystem;

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
		bool result = FileSystem.FileInfo.New(name).Extensibility
			.TryGetWrappedInstance(out System.IO.FileInfo? fileInfo);

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
		bool result = FileSystem.FileInfo.New(name).Extensibility
			.TryGetWrappedInstance(out System.IO.DirectoryInfo? directoryInfo);

		result.Should().BeFalse();
		directoryInfo.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void
		Extensibility_RetrieveMetadata_CorrectKeyAndType_ShouldReturnStoredValue(
			string name, DateTime time)
	{
		IFileSystemExtensibility sut = FileSystem.FileInfo.New(name)
			.Extensibility;

		sut.StoreMetadata("foo", time);
		DateTime? result = sut.RetrieveMetadata<DateTime?>("foo");

		result.Should().Be(time);
	}

	[SkippableTheory]
	[AutoData]
	public void Extensibility_RetrieveMetadata_DifferentKey_ShouldReturnNull(
		string name)
	{
		IFileSystemExtensibility sut = FileSystem.FileInfo.New(name)
			.Extensibility;

		sut.StoreMetadata("foo", DateTime.Now);
		DateTime? result = sut.RetrieveMetadata<DateTime?>("bar");

		result.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void Extensibility_RetrieveMetadata_DifferentType_ShouldReturnNull(
		string name)
	{
		IFileSystemExtensibility sut = FileSystem.FileInfo.New(name)
			.Extensibility;

		sut.StoreMetadata("foo", DateTime.Now);
		TimeSpan? result = sut.RetrieveMetadata<TimeSpan?>("foo");

		result.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void Extensibility_RetrieveMetadata_NotRegisteredKey_ShouldReturnNull(
		string name)
	{
		object? result = FileSystem.FileInfo.New(name).Extensibility
			.RetrieveMetadata<object?>("foo");

		result.Should().BeNull();
	}
}
