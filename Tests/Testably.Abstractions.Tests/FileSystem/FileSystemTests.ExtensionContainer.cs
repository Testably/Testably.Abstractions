using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem;

public abstract partial class FileSystemTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void
		ExtensionContainer_HasWrappedInstance_WithCorrectType_ShouldReturnTrueOnRealFileSystem(
			string name)
	{
		bool result = FileSystem.FileInfo.New(name).ExtensionContainer
			.HasWrappedInstance(out System.IO.FileInfo? fileInfo);

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
		ExtensionContainer_HasWrappedInstance_WithIncorrectType_ShouldReturnAlwaysFalse(
			string name)
	{
		bool result = FileSystem.FileInfo.New(name).ExtensionContainer
			.HasWrappedInstance(out System.IO.DirectoryInfo? directoryInfo);

		result.Should().BeFalse();
		directoryInfo.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void
		ExtensionContainer_RetrieveMetadata_CorrectKeyAndType_ShouldReturnStoredValue(
			string name, DateTime time)
	{
		IFileSystemExtensionContainer sut = FileSystem.FileInfo.New(name)
			.ExtensionContainer;

		sut.StoreMetadata("foo", time);
		DateTime? result = sut.RetrieveMetadata<DateTime?>("foo");

		result.Should().Be(time);
	}

	[SkippableTheory]
	[AutoData]
	public void ExtensionContainer_RetrieveMetadata_DifferentKey_ShouldReturnNull(
		string name)
	{
		IFileSystemExtensionContainer sut = FileSystem.FileInfo.New(name)
			.ExtensionContainer;

		sut.StoreMetadata("foo", DateTime.Now);
		DateTime? result = sut.RetrieveMetadata<DateTime?>("bar");

		result.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void ExtensionContainer_RetrieveMetadata_DifferentType_ShouldReturnNull(
		string name)
	{
		IFileSystemExtensionContainer sut = FileSystem.FileInfo.New(name)
			.ExtensionContainer;

		sut.StoreMetadata("foo", DateTime.Now);
		TimeSpan? result = sut.RetrieveMetadata<TimeSpan?>("foo");

		result.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void ExtensionContainer_RetrieveMetadata_NotRegisteredKey_ShouldReturnNull(
		string name)
	{
		object? result = FileSystem.FileInfo.New(name).ExtensionContainer
			.RetrieveMetadata<object?>("foo");

		result.Should().BeNull();
	}
}
