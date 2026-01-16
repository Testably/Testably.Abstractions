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
		
		// Arrange

		if (paths.Length == 0)
		{
			throw new ArgumentException("At least one path is required.", nameof(paths));
		}

		// short names, otherwise the path will be too long
		const string outsideDirectory = "outside";
		const string insideDirectory = "inside";
		const string targetName = "target";

		string insideSubDirectory = FileSystem.Path.Combine(
			insideDirectory, FileSystem.Path.Combine(paths)
		);

		string outsideTarget = FileSystem.Path.Combine(outsideDirectory, targetName);
		string insideTarget = FileSystem.Path.Combine(insideSubDirectory, targetName);

		FileSystem.Initialize().WithSubdirectories(insideSubDirectory, outsideTarget);

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(insideDirectory);

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created, out EventBox createdBox
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

		await ThatIsNullOrNot(createdBox.Value, !includeSubdirectories);
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

		string insideSubDirectory = FileSystem.Path.Combine(
			insideDirectory, FileSystem.Path.Combine(paths)
		);

		string insideTarget = FileSystem.Path.Combine(insideDirectory, targetName);
		string nestedTarget = FileSystem.Path.Combine(insideSubDirectory, targetName);

		FileSystem.Initialize().WithSubdirectories(insideSubDirectory, insideTarget);

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(insideDirectory);

		using ManualResetEventSlim deletedMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Deleted, out EventBox deletedBox
		);

		using ManualResetEventSlim renamedMs = AddRenamedEventHandler(
			fileSystemWatcher, out EventBox renamedBox
		);

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created
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
		await That(createdMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await ThatIsNullOrNot(deletedBox.Value, isRenamed);
		await ThatIsNullOrNot(renamedBox.Value, !isRenamed);
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

		// short names, otherwise the path will be too long
		const string insideDirectory = "inside";
		const string targetName = "target";

		string nestedDirectory = FileSystem.Path.Combine(insideDirectory, "nested");

		string targetDir = path is null
			? insideDirectory
			: FileSystem.Path.Combine(nestedDirectory, path);

		string target = FileSystem.Path.Combine(targetDir, targetName);

		string source = FileSystem.Path.Combine(nestedDirectory, targetName);

		FileSystem.Initialize().WithSubdirectories(targetDir, source);

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(insideDirectory);

		using ManualResetEventSlim renamedMs = AddRenamedEventHandler(
			fileSystemWatcher, out EventBox renamedBox
		);

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created, out EventBox createdBox
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
			.IsEqualTo(isCreated);

		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(includeSubdirectories);

		await That(changedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await ThatIsNullOrNot(createdBox.Value, !isCreated);
		await ThatIsNullOrNot(renamedBox.Value, !includeSubdirectories);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	[InlineData(true, "nested")]
	[InlineData(false, "nested")]
	public async Task Unix_MoveDeepNestedTo_ShouldInvokeRenamed(
		bool includeSubdirectories,
		string? path = null
	)
	{
		Skip.If(Test.RunsOnWindows);
		
		// Arrange

		bool isCreated = !includeSubdirectories && path is null;

		// short names, otherwise the path will be too long
		const string insideDirectory = "inside";
		const string targetName = "target";
		
		string deepNestedDirectory = FileSystem.Path.Combine(insideDirectory, "nested", "deep");

		string targetDir = path is null
			? insideDirectory
			: FileSystem.Path.Combine(insideDirectory, path);

		string target = FileSystem.Path.Combine(targetDir, targetName);

		string source = FileSystem.Path.Combine(deepNestedDirectory, targetName);

		FileSystem.Initialize().WithSubdirectories(targetDir, source);

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(insideDirectory);

		using ManualResetEventSlim renamedMs = AddRenamedEventHandler(
			fileSystemWatcher, out EventBox renamedBox
		);

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created, out EventBox createdBox
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
			.IsEqualTo(isCreated);

		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(includeSubdirectories);

		await That(changedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();
		
		await ThatIsNullOrNot(createdBox.Value, !isCreated);
		await ThatIsNullOrNot(renamedBox.Value, !includeSubdirectories);
	}
}
