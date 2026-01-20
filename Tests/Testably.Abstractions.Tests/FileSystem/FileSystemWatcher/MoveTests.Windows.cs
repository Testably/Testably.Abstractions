using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

public partial class MoveTests
{
	[Theory]
	[InlineData(true, "nested")]
	[InlineData(false, "nested")]
	[InlineData(true, "nested", "deep")]
	[InlineData(false, "nested", "deep")]
	public async Task Windows_MoveOutsideToNested_ShouldInvokeCreatedAndChanged(
		bool includeSubdirectories,
		params string[] paths
	)
	{
		Skip.IfNot(Test.RunsOnWindows);

		// Arrange

		// short names, otherwise the path will be too long
		const string outsideDirectory = "outside";
		const string insideDirectory = "inside";
		const string targetName = "target";

		string nestedDirectory = FileSystem.Path.Combine(paths);

		string expectedName = FileSystem.Path.Combine(nestedDirectory, targetName);

		string insideSubDirectory = FileSystem.Path.Combine(insideDirectory, nestedDirectory);

		string outsideTarget = FileSystem.Path.Combine(outsideDirectory, targetName);
		string insideTarget = FileSystem.Path.Combine(insideSubDirectory, targetName);

		FileSystem.Initialize().WithSubdirectories(insideSubDirectory, outsideTarget);

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(insideDirectory);

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created,
			out ConcurrentBag<FileSystemEventArgs> createdBag
		);

		using ManualResetEventSlim deletedMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Deleted
		);

		using ManualResetEventSlim renamedMs = AddRenamedEventHandler(fileSystemWatcher);

		fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
		fileSystemWatcher.EnableRaisingEvents = true;

		// Act

		FileSystem.Directory.Move(outsideTarget, insideTarget);

		// Assert

		await That(createdMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(includeSubdirectories);

		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await ThatIsSingleOrEmpty(createdBag, !includeSubdirectories);

		if (includeSubdirectories)
		{
			await That(createdBag.TryTake(out FileSystemEventArgs? createdEvent)).IsTrue();

			await That(createdEvent!)
				.For(x => x.ChangeType, it => it.IsEqualTo(WatcherChangeTypes.Created))
				.And
				.For(x => x.Name, it => it.IsEqualTo(expectedName))
				.And
				.For(x => x.FullPath, it => it.IsEqualTo(insideTarget));
		}
	}

	[Theory]
	[InlineData(true, "nested")]
	[InlineData(false, "nested")]
	[InlineData(true, "nested", "deep")]
	[InlineData(false, "nested", "deep")]
	public async Task Windows_MoveInsideToNested_ShouldInvokeDeletedCreatedAndChanged(
		bool includeSubdirectories,
		params string[] paths
	)
	{
		Skip.IfNot(Test.RunsOnWindows);

		// Arrange

		// short names, otherwise the path will be too long
		const string insideDirectory = "inside";
		const string targetName = "target";

		string nestedDirectory = FileSystem.Path.Combine(paths);

		string expectedCreatedName = FileSystem.Path.Combine(nestedDirectory, targetName);

		string insideSubDirectory = FileSystem.Path.Combine(insideDirectory, nestedDirectory);

		string insideTarget = FileSystem.Path.Combine(insideDirectory, targetName);
		string nestedTarget = FileSystem.Path.Combine(insideSubDirectory, targetName);

		FileSystem.Initialize().WithSubdirectories(insideSubDirectory, insideTarget);

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(insideDirectory);

		using ManualResetEventSlim deletedMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Deleted,
			out ConcurrentBag<FileSystemEventArgs> deletedBag
		);

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created,
			out ConcurrentBag<FileSystemEventArgs> createdBag
		);

		using ManualResetEventSlim renamedMs = AddRenamedEventHandler(fileSystemWatcher);

		fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
		fileSystemWatcher.EnableRaisingEvents = true;

		// Act

		FileSystem.Directory.Move(insideTarget, nestedTarget);

		// Assert

		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsTrue();

		await That(createdMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(includeSubdirectories);

		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await That(deletedBag).HasSingle();

		await That(deletedBag.TryTake(out FileSystemEventArgs? deletedEvent)).IsTrue();

		await That(deletedEvent!)
			.For(x => x.ChangeType, it => it.IsEqualTo(WatcherChangeTypes.Deleted))
			.And
			.For(x => x.Name, it => it.IsEqualTo(targetName))
			.And
			.For(x => x.FullPath, it => it.IsEqualTo(insideTarget));

		await ThatIsSingleOrEmpty(createdBag, !includeSubdirectories);

		if (includeSubdirectories)
		{
			await That(createdBag.TryTake(out FileSystemEventArgs? createdEvent)).IsTrue();

			await That(createdEvent!)
				.For(x => x.ChangeType, it => it.IsEqualTo(WatcherChangeTypes.Created))
				.And
				.For(x => x.Name, it => it.IsEqualTo(expectedCreatedName))
				.And
				.For(x => x.FullPath, it => it.IsEqualTo(nestedTarget));
		}
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	[InlineData(true, "deep")]
	[InlineData(false, "deep")]
	public async Task Windows_MoveNestedTo_ShouldInvokeDeletedCreatedAndChanged(
		bool includeSubdirectories,
		string? path = null
	)
	{
		Skip.IfNot(Test.RunsOnWindows);

		// Arrange

		bool isCreated = path is null || includeSubdirectories;

		// short names, otherwise the path will be too long
		const string insideDirectory = "inside";
		const string targetName = "target";
		const string nestedDirectoryName = "nested";

		string expectedDeletedName = FileSystem.Path.Combine(nestedDirectoryName, targetName);

		string expectedCreatedName = path is null
			? targetName
			: FileSystem.Path.Combine(nestedDirectoryName, path, targetName);

		string nestedDirectory = FileSystem.Path.Combine(insideDirectory, nestedDirectoryName);

		string targetDir = path is null
			? insideDirectory
			: FileSystem.Path.Combine(nestedDirectory, path);

		string target = FileSystem.Path.Combine(targetDir, targetName);

		string source = FileSystem.Path.Combine(nestedDirectory, targetName);

		FileSystem.Initialize().WithSubdirectories(targetDir, source);

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(insideDirectory);

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created,
			out ConcurrentBag<FileSystemEventArgs> createdBag
		);

		using ManualResetEventSlim deletedMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Deleted,
			out ConcurrentBag<FileSystemEventArgs> deletedBag
		);

		using ManualResetEventSlim renamedMs = AddRenamedEventHandler(fileSystemWatcher);

		fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
		fileSystemWatcher.EnableRaisingEvents = true;

		// Act

		FileSystem.Directory.Move(source, target);

		// Assert

		await That(createdMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(isCreated);

		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(includeSubdirectories);

		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await ThatIsSingleOrEmpty(createdBag, !isCreated);
		await ThatIsSingleOrEmpty(deletedBag, !includeSubdirectories);

		if (isCreated)
		{
			await That(createdBag.TryTake(out FileSystemEventArgs? createdEvent)).IsTrue();

			await That(createdEvent!)
				.For(x => x.ChangeType, it => it.IsEqualTo(WatcherChangeTypes.Created))
				.And
				.For(x => x.Name, it => it.IsEqualTo(expectedCreatedName))
				.And
				.For(x => x.FullPath, it => it.IsEqualTo(target));
		}

		if (includeSubdirectories)
		{
			await That(deletedBag.TryTake(out FileSystemEventArgs? deletedEvent)).IsTrue();

			await That(deletedEvent!)
				.For(x => x.ChangeType, it => it.IsEqualTo(WatcherChangeTypes.Deleted))
				.And
				.For(x => x.Name, it => it.IsEqualTo(expectedDeletedName))
				.And
				.For(x => x.FullPath, it => it.IsEqualTo(source));
		}
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	[InlineData(true, "nested")]
	[InlineData(false, "nested")]
	public async Task Windows_MoveDeepNestedTo_ShouldInvokeDeletedCreatedAndChanged(
		bool includeSubdirectories,
		string? path = null
	)
	{
		Skip.IfNot(Test.RunsOnWindows);

		// Arrange

		bool isCreated = path is null || includeSubdirectories;

		// short names, otherwise the path will be too long
		const string insideDirectory = "inside";
		const string targetName = "target";
		const string nestedDirectoryName = "nested";
		const string deepNestedDirectoryName = "deep";

		string expectedDeletedName = FileSystem.Path.Combine(
			nestedDirectoryName, deepNestedDirectoryName, targetName
		);

		string expectedCreatedName = path is null
			? targetName
			: FileSystem.Path.Combine(path, targetName);

		string deepNestedDirectory = FileSystem.Path.Combine(
			insideDirectory, nestedDirectoryName, deepNestedDirectoryName
		);

		string targetDir = path is null
			? insideDirectory
			: FileSystem.Path.Combine(insideDirectory, path);

		string target = FileSystem.Path.Combine(targetDir, targetName);

		string source = FileSystem.Path.Combine(deepNestedDirectory, targetName);

		FileSystem.Initialize().WithSubdirectories(targetDir, source);

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(insideDirectory);

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created,
			out ConcurrentBag<FileSystemEventArgs> createdBag
		);

		using ManualResetEventSlim deletedMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Deleted,
			out ConcurrentBag<FileSystemEventArgs> deletedBag
		);

		using ManualResetEventSlim renamedMs = AddRenamedEventHandler(fileSystemWatcher);

		fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
		fileSystemWatcher.EnableRaisingEvents = true;

		// Act

		FileSystem.Directory.Move(source, target);

		// Assert

		await That(createdMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(isCreated);

		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(includeSubdirectories);

		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await ThatIsSingleOrEmpty(createdBag, !isCreated);
		await ThatIsSingleOrEmpty(deletedBag, !includeSubdirectories);

		if (isCreated)
		{
			await That(createdBag.TryTake(out FileSystemEventArgs? createdEvent)).IsTrue();

			await That(createdEvent!)
				.For(x => x.ChangeType, it => it.IsEqualTo(WatcherChangeTypes.Created))
				.And
				.For(x => x.Name, it => it.IsEqualTo(expectedCreatedName))
				.And
				.For(x => x.FullPath, it => it.IsEqualTo(target));
		}

		if (includeSubdirectories)
		{
			await That(deletedBag.TryTake(out FileSystemEventArgs? deletedEvent)).IsTrue();

			await That(deletedEvent!)
				.For(x => x.ChangeType, it => it.IsEqualTo(WatcherChangeTypes.Deleted))
				.And
				.For(x => x.Name, it => it.IsEqualTo(expectedDeletedName))
				.And
				.For(x => x.FullPath, it => it.IsEqualTo(source));
		}
	}
}
