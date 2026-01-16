using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public partial class MoveTests
{
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task MoveOutsideToInside_ShouldInvokeCreated(bool includeSubdirectories)
	{
		// Arrange

		// short names, otherwise the path will be too long
		const string outsideDirectory = "outside";
		const string insideDirectory = "inside";
		const string targetName = "target";
		string outsideTarget = FileSystem.Path.Combine(outsideDirectory, targetName);
		string insideTarget = FileSystem.Path.Combine(insideDirectory, targetName);

		FileSystem.Initialize().WithSubdirectories(
			outsideDirectory, insideDirectory, outsideTarget
		);

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

		await That(createdMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsTrue();

		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await That(createdBox.Value).IsNotNull();
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	[InlineData(true, "nested")]
	[InlineData(false, "nested")]
	[InlineData(true, "nested", "deep")]
	[InlineData(false, "nested", "deep")]
	public async Task MoveToOutside_ShouldInvokeDeleted(
		bool includeSubdirectories,
		params string[] paths
	)
	{
		// Arrange

		bool shouldInvokeDeleted = includeSubdirectories || paths.Length == 0;

		// short names, otherwise the path will be too long
		const string outsideDirectory = "outside";
		const string insideDirectory = "inside";
		const string targetName = "target";

		string insideSubDirectory = FileSystem.Path.Combine(
			insideDirectory, FileSystem.Path.Combine(paths)
		);

		string outsideTarget = FileSystem.Path.Combine(outsideDirectory, targetName);
		string insideTarget = FileSystem.Path.Combine(insideSubDirectory, targetName);

		FileSystem.Initialize().WithSubdirectories(outsideDirectory, insideDirectory, insideTarget);

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(insideDirectory);

		using ManualResetEventSlim deletedMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Deleted, out EventBox deletedBox
		);

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created
		);

		using ManualResetEventSlim renamedMs = AddRenamedEventHandler(fileSystemWatcher);

		fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
		fileSystemWatcher.EnableRaisingEvents = true;

		// Act

		FileSystem.Directory.Move(insideTarget, outsideTarget);

		// Assert

		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(shouldInvokeDeleted);

		await That(createdMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await ThatIsNullOrNot(deletedBox.Value, !shouldInvokeDeleted);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	[InlineData(true, "nested")]
	[InlineData(false, "nested")]
	[InlineData(true, "nested", "deep")]
	[InlineData(false, "nested", "deep")]
	public async Task MoveToSameDirectory_ShouldInvokeRenamed(
		bool includeSubdirectories,
		params string[] paths
	)
	{
		// Arrange

		bool shouldInvokeRenamed = includeSubdirectories || paths.Length == 0;

		// short names, otherwise the path will be too long
		const string insideDirectory = "inside";
		const string targetName = "target";
		const string targetName2 = "target2";

		string insideSubDirectory = FileSystem.Path.Combine(
			insideDirectory, FileSystem.Path.Combine(paths)
		);

		string insideTarget = FileSystem.Path.Combine(insideSubDirectory, targetName);
		string insideTarget2 = FileSystem.Path.Combine(insideSubDirectory, targetName2);

		FileSystem.Initialize().WithSubdirectories(insideDirectory, insideTarget);

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(insideDirectory);

		using ManualResetEventSlim renamedMs = AddRenamedEventHandler(
			fileSystemWatcher, out EventBox renamedBox
		);

		using ManualResetEventSlim deletedMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Deleted
		);

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created
		);

		fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
		fileSystemWatcher.EnableRaisingEvents = true;

		// Act

		FileSystem.Directory.Move(insideTarget, insideTarget2);

		// Assert

		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(shouldInvokeRenamed);

		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(createdMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await ThatIsNullOrNot(renamedBox.Value, !shouldInvokeRenamed);
	}

	private static async Task ThatIsNullOrNot<T>(T? value, bool isNull) where T : class
	{
		if (isNull)
		{
			await That(value).IsNull();
		}
		else
		{
			await That(value).IsNotNull();
		}
	}

	private static ManualResetEventSlim AddEventHandler(
		IFileSystemWatcher fileSystemWatcher,
		WatcherChangeTypes changeType
	)
	{
		return AddEventHandler(fileSystemWatcher, changeType, out _);
	}

	private static ManualResetEventSlim AddRenamedEventHandler(IFileSystemWatcher fileSystemWatcher)
	{
		return AddRenamedEventHandler(fileSystemWatcher, out _);
	}

	private static ManualResetEventSlim AddEventHandler(
		IFileSystemWatcher fileSystemWatcher,
		WatcherChangeTypes changeType,
		out EventBox eventBox
	)
	{
		ManualResetEventSlim ms = new();
		EventBox box = new();

		eventBox = box;

		FileSystemEventHandler handler = (_, args) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				box.Value = args;
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		switch (changeType)
		{
			case WatcherChangeTypes.Created:
				fileSystemWatcher.Created += handler;

				break;
			case WatcherChangeTypes.Changed:
				fileSystemWatcher.Changed += handler;

				break;
			case WatcherChangeTypes.Deleted:
				fileSystemWatcher.Deleted += handler;

				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(changeType), changeType, null);
		}

		return ms;
	}

	private static ManualResetEventSlim AddRenamedEventHandler(
		IFileSystemWatcher fileSystemWatcher,
		out EventBox eventBox
	)
	{
		ManualResetEventSlim ms = new();
		EventBox box = new();
		eventBox = box;

		fileSystemWatcher.Renamed += (_, args) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				box.Value = args;
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		return ms;
	}

	private class EventBox
	{
		public FileSystemEventArgs? Value { get; set; }
	}
}
