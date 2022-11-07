using System.IO;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class ChangeDescriptionTests
{
	[Theory]
	[AutoData]
	public void ToString_ShouldIncludeChangeType(
		WatcherChangeTypes changeType,
		FileSystemTypes fileSystemType,
		NotifyFilters notifyFilters,
		string path)
	{
		string fullPath = Path.GetFullPath(path);
		IStorageLocation location = InMemoryLocation.New(null, fullPath);
		ChangeDescription sut = new(
			changeType,
			fileSystemType,
			notifyFilters,
			location,
			null);

		string result = sut.ToString();

		result.Should().Contain(changeType.ToString());
	}

	[Theory]
	[AutoData]
	public void ToString_ShouldIncludeFileSystemType(
		WatcherChangeTypes changeType,
		FileSystemTypes fileSystemType,
		NotifyFilters notifyFilters,
		string path)
	{
		string fullPath = Path.GetFullPath(path);
		IStorageLocation location = InMemoryLocation.New(null, fullPath);
		ChangeDescription sut = new(
			changeType,
			fileSystemType,
			notifyFilters,
			location,
			null);

		string result = sut.ToString();

		result.Should().Contain(fileSystemType.ToString());
	}

	[Theory]
	[AutoData]
	public void ToString_ShouldIncludeNotifyFilters(
		WatcherChangeTypes changeType,
		FileSystemTypes fileSystemType,
		NotifyFilters notifyFilters,
		string path)
	{
		string fullPath = Path.GetFullPath(path);
		IStorageLocation location = InMemoryLocation.New(null, fullPath);
		ChangeDescription sut = new(
			changeType,
			fileSystemType,
			notifyFilters,
			location,
			null);

		string result = sut.ToString();

		result.Should().Contain(notifyFilters.ToString());
	}

	[Theory]
	[AutoData]
	public void ToString_ShouldIncludePath(
		WatcherChangeTypes changeType,
		FileSystemTypes fileSystemType,
		NotifyFilters notifyFilters,
		string path)
	{
		string fullPath = Path.GetFullPath(path);
		IStorageLocation location = InMemoryLocation.New(null, fullPath);
		ChangeDescription sut = new(
			changeType,
			fileSystemType,
			notifyFilters,
			location,
			null);

		string result = sut.ToString();

		result.Should().Contain(fullPath);
	}
}