using System.IO;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class ChangeDescriptionTests
{
	[Theory]
	[AutoData]
	public async Task ToString_ShouldIncludeChangeType(
		WatcherChangeTypes changeType,
		FileSystemTypes fileSystemType,
		NotifyFilters notifyFilters,
		string path)
	{
		string fullPath = Path.GetFullPath(path);
		IStorageLocation location = InMemoryLocation.New(new MockFileSystem(), null, fullPath);
		ChangeDescription sut = new(
			changeType,
			fileSystemType,
			notifyFilters,
			location,
			null);

		string result = sut.ToString();

		await That(result).Contains(changeType.ToString());
	}

	[Theory]
	[AutoData]
	public async Task ToString_ShouldIncludeFileSystemType(
		WatcherChangeTypes changeType,
		FileSystemTypes fileSystemType,
		NotifyFilters notifyFilters,
		string path)
	{
		string fullPath = Path.GetFullPath(path);
		IStorageLocation location = InMemoryLocation.New(new MockFileSystem(), null, fullPath);
		ChangeDescription sut = new(
			changeType,
			fileSystemType,
			notifyFilters,
			location,
			null);

		string result = sut.ToString();

		await That(result).Contains(fileSystemType.ToString());
	}

	[Theory]
	[AutoData]
	public async Task ToString_ShouldIncludeNotifyFilters(
		WatcherChangeTypes changeType,
		FileSystemTypes fileSystemType,
		NotifyFilters notifyFilters,
		string path)
	{
		string fullPath = Path.GetFullPath(path);
		IStorageLocation location = InMemoryLocation.New(new MockFileSystem(), null, fullPath);
		ChangeDescription sut = new(
			changeType,
			fileSystemType,
			notifyFilters,
			location,
			null);

		string result = sut.ToString();

		await That(result).Contains(notifyFilters.ToString());
	}

	[Theory]
	[AutoData]
	public async Task ToString_ShouldIncludePath(
		WatcherChangeTypes changeType,
		FileSystemTypes fileSystemType,
		NotifyFilters notifyFilters,
		string path)
	{
		string fullPath = Path.GetFullPath(path);
		IStorageLocation location = InMemoryLocation.New(new MockFileSystem(), null, fullPath);
		ChangeDescription sut = new(
			changeType,
			fileSystemType,
			notifyFilters,
			location,
			null);

		string result = sut.ToString();

		await That(result).Contains(fullPath);
	}
}
