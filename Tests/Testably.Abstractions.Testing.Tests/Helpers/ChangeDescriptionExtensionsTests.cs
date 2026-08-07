using System.IO;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class ChangeDescriptionExtensionsTests
{
	[Test]
	[Arguments(WatcherChangeTypes.Created, WatcherChangeTypes.Created, true)]
	[Arguments(WatcherChangeTypes.Created, WatcherChangeTypes.Changed, false)]
	[Arguments(WatcherChangeTypes.Created, WatcherChangeTypes.Created | WatcherChangeTypes.Deleted,
		true)]
	[Arguments(WatcherChangeTypes.Deleted, WatcherChangeTypes.Created | WatcherChangeTypes.Deleted,
		true)]
	[Arguments(WatcherChangeTypes.Changed, WatcherChangeTypes.Created | WatcherChangeTypes.Deleted,
		false)]
	public async Task Matches_ChangeType_ShouldSupportCombinedFlags(
		WatcherChangeTypes changeType, WatcherChangeTypes filter, bool expectedResult)
	{
		MockFileSystem fileSystem = new();
		ChangeDescription changeDescription =
			CreateChangeDescription(fileSystem, changeType, FileSystemTypes.File);

		bool result = changeDescription.Matches(
			fileSystem.Execute, FileSystemTypes.DirectoryOrFile, filter, "*");

		await That(result).IsEqualTo(expectedResult);
	}

	[Test]
	[Arguments(FileSystemTypes.Directory, FileSystemTypes.Directory, true)]
	[Arguments(FileSystemTypes.Directory, FileSystemTypes.File, false)]
	[Arguments(FileSystemTypes.Directory, FileSystemTypes.DirectoryOrFile, true)]
	[Arguments(FileSystemTypes.File, FileSystemTypes.Directory, false)]
	[Arguments(FileSystemTypes.File, FileSystemTypes.File, true)]
	[Arguments(FileSystemTypes.File, FileSystemTypes.DirectoryOrFile, true)]
	public async Task Matches_FileSystemType_ShouldSupportCombinedFlags(
		FileSystemTypes fileSystemType, FileSystemTypes filter, bool expectedResult)
	{
		MockFileSystem fileSystem = new();
		ChangeDescription changeDescription =
			CreateChangeDescription(fileSystem, WatcherChangeTypes.Created, fileSystemType);

		bool result = changeDescription.Matches(
			fileSystem.Execute, filter, WatcherChangeTypes.Created, "*");

		await That(result).IsEqualTo(expectedResult);
	}

	private static ChangeDescription CreateChangeDescription(MockFileSystem fileSystem,
		WatcherChangeTypes changeType, FileSystemTypes fileSystemType)
		=> new(changeType, fileSystemType, NotifyFilters.FileName,
			fileSystem.Storage.GetLocation("foo"), null);
}
