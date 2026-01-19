using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public partial class MoveTests
{
	private static bool IsMac { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
	
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

		await That(createdMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsTrue();

		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();
		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await That(createdBag).HasSingle().Which
			.Satisfies(x => x.ChangeType == WatcherChangeTypes.Created
			                && string.Equals(x.Name, targetName, StringComparison.Ordinal)
			                && string.Equals(x.FullPath, insideTarget, StringComparison.Ordinal)
			);
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

		string expectedDeletedName
			= FileSystem.Path.Combine(FileSystem.Path.Combine(paths), targetName);

		FileSystem.Initialize().WithSubdirectories(outsideDirectory, insideDirectory, insideTarget);

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

		FileSystem.Directory.Move(insideTarget, outsideTarget);

		// Assert

		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(shouldInvokeDeleted);

		await That(createdMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(IsMac);

		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await RemoveMacArrangeEvents(createdBag, insideTarget, insideDirectory, insideTarget);

		await ThatIsSingleOrEmpty(deletedBag, !shouldInvokeDeleted);

		if (shouldInvokeDeleted)
		{
			await That(deletedBag.TryTake(out FileSystemEventArgs? deletedEvent)).IsTrue();

			await That(deletedEvent!).Satisfies(x => x.ChangeType == WatcherChangeTypes.Deleted
			                                         && string.Equals(
				                                         x.Name, expectedDeletedName,
				                                         StringComparison.Ordinal
			                                         )
			                                         && string.Equals(
				                                         x.FullPath, insideTarget,
				                                         StringComparison.Ordinal
			                                         )
			);
		}
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

		string expectedName = FileSystem.Path.Combine(FileSystem.Path.Combine(paths), targetName2);

		string expectedOldName = FileSystem.Path.Combine(
			FileSystem.Path.Combine(paths), targetName
		);

		FileSystem.Initialize().WithSubdirectories(insideDirectory, insideTarget);

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(insideDirectory);

		using ManualResetEventSlim renamedMs = AddRenamedEventHandler(
			fileSystemWatcher, out ConcurrentBag<RenamedEventArgs> renamedBag
		);

		using ManualResetEventSlim deletedMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Deleted
		);

		using ManualResetEventSlim createdMs = AddEventHandler(
			fileSystemWatcher, WatcherChangeTypes.Created,
			out ConcurrentBag<FileSystemEventArgs> createdBag
		);

		fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
		fileSystemWatcher.EnableRaisingEvents = true;

		// Act

		FileSystem.Directory.Move(insideTarget, insideTarget2);

		// Assert

		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(shouldInvokeRenamed);

		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await That(createdMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(IsMac);

		await RemoveMacArrangeEvents(createdBag, insideTarget, insideDirectory, insideTarget);

		await ThatIsSingleOrEmpty(renamedBag, !shouldInvokeRenamed);

		if (shouldInvokeRenamed)
		{
			await That(renamedBag.TryTake(out RenamedEventArgs? renamedEvent)).IsTrue();

			await That(renamedEvent!)
				.Satisfies(x => string.Equals(x.OldName, expectedOldName, StringComparison.Ordinal)
				                && string.Equals(x.Name, expectedName, StringComparison.Ordinal)
				                && string.Equals(
					                x.FullPath, insideTarget2, StringComparison.Ordinal
				                )
				                && string.Equals(
					                x.OldFullPath, insideTarget, StringComparison.Ordinal
				                )
				);
		}
	}

	private static bool EqualsOrdinal(string? x, string? y)
	{
		return string.Equals(x, y, StringComparison.Ordinal);
	}

	private static async Task ThatIsSingleOrEmpty<T>(IEnumerable<T> value, bool isEmpty)
		where T : class
	{
		if (isEmpty)
		{
			await That(value).IsEmpty();
		}
		else
		{
			await That(value).HasSingle();
		}
	}

	private static async Task RemoveMacArrangeEvents(
		ConcurrentBag<FileSystemEventArgs> createdBag,
		string expectedFullPath,
		params string[] initialDirectories
	)
	{
		if (!IsMac)
		{
			return;
		}

		FileSystemEventArgs? expectedEvent = null;

		while (createdBag.TryTake(out FileSystemEventArgs? createdEvent))
		{
			if (createdEvent.ChangeType == WatcherChangeTypes.Created
			    && EqualsOrdinal(createdEvent.FullPath, expectedFullPath))
			{
				expectedEvent = createdEvent;
			}

			await That(createdEvent)
				.Satisfies(x => initialDirectories.Any(directory => EqualsOrdinal(directory, x.Name)
				           )
				);
		}

		await That(createdBag).IsEmpty();

		if (expectedEvent is not null)
		{
			createdBag.Add(expectedEvent);
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
		out ConcurrentBag<FileSystemEventArgs> events
	)
	{
		ManualResetEventSlim ms = new();
		ConcurrentBag<FileSystemEventArgs> eventBag = new();

		events = eventBag;

		FileSystemEventHandler handler = (_, args) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				eventBag.Add(args);
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
		out ConcurrentBag<RenamedEventArgs> events
	)
	{
		ManualResetEventSlim ms = new();
		ConcurrentBag<RenamedEventArgs> eventBag = new();
		events = eventBag;

		fileSystemWatcher.Renamed += (_, args) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				eventBag.Add(args);
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		return ms;
	}
}
