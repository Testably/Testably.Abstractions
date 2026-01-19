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
	public async Task Unix_MoveOutsideToNested_ShouldInvokeNothingOrCreated(
		bool includeSubdirectories,
		params string[] paths
	)
	{
		Skip.If(Test.RunsOnWindows);

		if (paths.Length == 0)
		{
			throw new ArgumentException("At least one path is required.", nameof(paths));
		}

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

		using ManualResetEventSlim changedMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Changed
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
		await That(changedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await ThatIsSingleOrEmpty(createdBag, !includeSubdirectories);

		if (includeSubdirectories)
		{
			await That(createdBag.TryTake(out FileSystemEventArgs? createdEvent)).IsTrue();

			await That(createdEvent!).Satisfies(x => x.ChangeType == WatcherChangeTypes.Created
			                                         && EqualsOrdinal(x.Name, expectedName)
			                                         && EqualsOrdinal(x.FullPath, insideTarget)
			);
		}
	}

	[Theory]
	[InlineData(true, "nested")]
	[InlineData(false, "nested")]
	[InlineData(true, "nested", "deep")]
	[InlineData(false, "nested", "deep")]
	public async Task Unix_MoveInsideToNested_ShouldInvokeDeletedOrRenamed(
		bool includeSubdirectories,
		params string[] paths
	)
	{
		Skip.If(Test.RunsOnWindows);

		if (paths.Length == 0)
		{
			throw new ArgumentException("At least one path is required.", nameof(paths));
		}

		// Arrange

		// When moving items from inside to nested on Mac when IncludeSubdirectories is false, then it will invoke a Renamed rather than Deleted
		bool isRenamed = Test.RunsOnMac || includeSubdirectories;

		// short names, otherwise the path will be too long
		const string insideDirectory = "inside";
		const string targetName = "target";

		string nestedDirectory = FileSystem.Path.Combine(paths);

		string expectedName = FileSystem.Path.Combine(nestedDirectory, targetName);

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

		using ManualResetEventSlim renamedMs = AddRenamedEventHandler(
			fileSystemWatcher, out ConcurrentBag<RenamedEventArgs> renamedBag
		);

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created,
			out ConcurrentBag<FileSystemEventArgs> createdBag
		);

		using ManualResetEventSlim changedMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Changed
		);

		fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
		fileSystemWatcher.EnableRaisingEvents = true;

		// Act

		FileSystem.Directory.Move(insideTarget, nestedTarget);

		// Assert

		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(!isRenamed);

		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(isRenamed);

		await That(changedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await That(createdMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(IsMac);

		await RemoveMacArrangeEvents(
			createdBag, string.Empty /*None expected*/, insideSubDirectory, insideTarget
		);

		await ThatIsSingleOrEmpty(deletedBag, isRenamed);
		await ThatIsSingleOrEmpty(renamedBag, !isRenamed);

		if (isRenamed)
		{
			await That(renamedBag.TryTake(out RenamedEventArgs? renamedEvent)).IsTrue();

			await That(renamedEvent!).Satisfies(x => EqualsOrdinal(x.Name, expectedName)
			                                         && EqualsOrdinal(x.FullPath, nestedTarget)
			                                         && EqualsOrdinal(x.OldName, targetName)
			                                         && EqualsOrdinal(x.OldFullPath, insideTarget)
			);
		}
		else
		{
			await That(deletedBag.TryTake(out FileSystemEventArgs? createdEvent)).IsTrue();

			await That(createdEvent!).Satisfies(x => x.ChangeType == WatcherChangeTypes.Deleted
			                                         && EqualsOrdinal(x.Name, targetName)
			                                         && EqualsOrdinal(x.FullPath, insideTarget)
			);
		}
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	[InlineData(true, "deep")]
	[InlineData(false, "deep")]
	public async Task Unix_MoveNestedTo_ShouldInvokeCreatedOrRenamed(
		bool includeSubdirectories,
		string? path = null
	)
	{
		Skip.If(Test.RunsOnWindows);

		// Arrange

		bool isCreated = !includeSubdirectories && path is null;
		bool isMacCreated = IsMac && includeSubdirectories;

		// short names, otherwise the path will be too long
		const string insideDirectory = "inside";
		const string targetName = "target";
		const string nestedDirName = "nested";

		string expectedOldName = FileSystem.Path.Combine(nestedDirName, targetName);

		string expectedName = path is null
			? targetName
			: FileSystem.Path.Combine(nestedDirName, path, targetName);

		string nestedDirectory = FileSystem.Path.Combine(insideDirectory, nestedDirName);

		string targetDir = path is null
			? insideDirectory
			: FileSystem.Path.Combine(nestedDirectory, path);

		string target = FileSystem.Path.Combine(targetDir, targetName);

		string source = FileSystem.Path.Combine(nestedDirectory, targetName);

		FileSystem.Initialize().WithSubdirectories(targetDir, source);

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(insideDirectory);

		using ManualResetEventSlim renamedMs = AddRenamedEventHandler(
			fileSystemWatcher, out ConcurrentBag<RenamedEventArgs> renamedBag
		);

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created,
			out ConcurrentBag<FileSystemEventArgs> createdBag
		);

		using ManualResetEventSlim changedMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Changed
		);

		using ManualResetEventSlim deletedMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Deleted
		);

		fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
		fileSystemWatcher.EnableRaisingEvents = true;

		// Act

		FileSystem.Directory.Move(source, target);

		// Assert

		await That(createdMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(isCreated || isMacCreated);

		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(includeSubdirectories);

		await That(changedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await RemoveMacArrangeEvents(createdBag, target, targetDir, source);

		await ThatIsSingleOrEmpty(createdBag, !isCreated);

		if (isCreated)
		{
			await That(createdBag.TryTake(out FileSystemEventArgs? createdEvent)).IsTrue();

			await That(createdEvent!).Satisfies(x => x.ChangeType == WatcherChangeTypes.Created
			                                         && EqualsOrdinal(x.Name, expectedName)
			                                         && EqualsOrdinal(x.FullPath, target)
			);
		}

		await ThatIsSingleOrEmpty(renamedBag, !includeSubdirectories);

		if (includeSubdirectories)
		{
			await That(renamedBag.TryTake(out RenamedEventArgs? renamedEvent)).IsTrue();

			await That(renamedEvent!).Satisfies(x => EqualsOrdinal(x.Name, expectedName)
			                                         && EqualsOrdinal(x.FullPath, target)
			                                         && EqualsOrdinal(x.OldName, expectedOldName)
			                                         && EqualsOrdinal(x.OldFullPath, source)
			);
		}
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	[InlineData(true, "nested")]
	[InlineData(false, "nested")]
	public async Task Unix_MoveDeepNestedTo_ShouldInvokeRenamedOrCreated(
		bool includeSubdirectories,
		string? path = null
	)
	{
		Skip.If(Test.RunsOnWindows);

		// Arrange

		bool isCreated = !includeSubdirectories && path is null;
		bool isMacCreated = IsMac && includeSubdirectories;

		// short names, otherwise the path will be too long
		const string insideDirectory = "inside";
		const string targetName = "target";

		string deepNestedDirectory = FileSystem.Path.Combine("nested", "deep");

		string expectedOldName = FileSystem.Path.Combine(deepNestedDirectory, targetName);
		string expectedName = path is null ? targetName : FileSystem.Path.Combine(path, targetName);

		string targetDir = path is null
			? insideDirectory
			: FileSystem.Path.Combine(insideDirectory, path);

		string target = FileSystem.Path.Combine(targetDir, targetName);

		string source = FileSystem.Path.Combine(insideDirectory, deepNestedDirectory, targetName);

		FileSystem.Initialize().WithSubdirectories(targetDir, source);

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(insideDirectory);

		using ManualResetEventSlim renamedMs = AddRenamedEventHandler(
			fileSystemWatcher, out ConcurrentBag<RenamedEventArgs> renamedBag
		);

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created,
			out ConcurrentBag<FileSystemEventArgs> createdBag
		);

		using ManualResetEventSlim changedMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Changed
		);

		using ManualResetEventSlim deletedMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Deleted
		);

		fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
		fileSystemWatcher.EnableRaisingEvents = true;

		// Act

		FileSystem.Directory.Move(source, target);

		// Assert

		await That(createdMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(isMacCreated || isCreated);

		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(includeSubdirectories);

		await That(changedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await RemoveMacArrangeEvents(createdBag, target, targetDir, source);

		await ThatIsSingleOrEmpty(createdBag, !isCreated);
		await ThatIsSingleOrEmpty(renamedBag, !includeSubdirectories);

		if (isCreated)
		{
			await That(createdBag.TryTake(out FileSystemEventArgs? createdEvent)).IsTrue();

			await That(createdEvent!).Satisfies(x => x.ChangeType == WatcherChangeTypes.Created
			                                         && EqualsOrdinal(x.Name, expectedName)
			                                         && EqualsOrdinal(x.FullPath, target)
			);
		}

		if (includeSubdirectories)
		{
			await That(renamedBag.TryTake(out RenamedEventArgs? renamedEvent)).IsTrue();

			await That(renamedEvent!).Satisfies(x => EqualsOrdinal(x.Name, expectedName)
			                                         && EqualsOrdinal(x.FullPath, target)
			                                         && EqualsOrdinal(x.OldName, expectedOldName)
			                                         && EqualsOrdinal(x.OldFullPath, source)
			);
		}
	}
}
