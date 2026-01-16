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

		if (paths.Length == 0)
		{
			throw new ArgumentException("At least one path is required.", nameof(paths));
		}
		
		// Arrange

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

		await ThatIsNullOrNot(createdBox.Value, !includeSubdirectories);
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

		if (paths.Length == 0)
		{
			throw new ArgumentException("At least one path is required.", nameof(paths));
		}
		
		// Arrange

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

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created, out EventBox createdBox
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

		await That(deletedBox.Value).IsNotNull();
		
		await ThatIsNullOrNot(createdBox.Value, !includeSubdirectories);
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

		string nestedDirectory = FileSystem.Path.Combine(insideDirectory, "nested");

		string targetDir = path is null
			? insideDirectory
			: FileSystem.Path.Combine(nestedDirectory, path);

		string target = FileSystem.Path.Combine(targetDir, targetName);

		string source = FileSystem.Path.Combine(nestedDirectory, targetName);

		FileSystem.Initialize().WithSubdirectories(targetDir, source);

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(insideDirectory);

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created, out EventBox createdBox
		);

		using ManualResetEventSlim deletedMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Deleted, out EventBox deletedBox
		);

		using ManualResetEventSlim renamedMs = AddRenamedEventHandler(fileSystemWatcher);

		fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
		fileSystemWatcher.EnableRaisingEvents = true;

		// Act
		
		FileSystem.Directory.Move(source, target);

		// Assert
		
		await That(createdMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsEqualTo(isCreated);

		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(includeSubdirectories);

		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await ThatIsNullOrNot(createdBox.Value, !isCreated);
		await ThatIsNullOrNot(deletedBox.Value, !includeSubdirectories);
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

		string deepNestedDirectory = FileSystem.Path.Combine(insideDirectory, "nested", "deep");

		string targetDir = path is null
			? insideDirectory
			: FileSystem.Path.Combine(insideDirectory, path);

		string target = FileSystem.Path.Combine(targetDir, targetName);

		string source = FileSystem.Path.Combine(deepNestedDirectory, targetName);

		FileSystem.Initialize().WithSubdirectories(targetDir, source);

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(insideDirectory);

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created, out EventBox createdBox
		);

		using ManualResetEventSlim deletedMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Deleted, out EventBox deletedBox
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

		await ThatIsNullOrNot(createdBox.Value, !isCreated);
		await ThatIsNullOrNot(deletedBox.Value, !includeSubdirectories);
	}
}
