using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

public partial class MoveTests
{
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task Windows_ParentWatcher_MoveToChild_ShouldInvokeDeletedAndCreatedAndChanged(
		bool includeSubdirectories
	)
	{
		Skip.IfNot(Test.RunsOnWindows);

		// short names, otherwise the path will be too long
		const string parentDirectory = "parent";
		const string childDirectory = "child";

		FileSystem.Initialize().WithSubdirectories(parentDirectory, childDirectory);

		string newPath = FileSystem.Path.Combine(parentDirectory, childDirectory);

		using ManualResetEventSlim deletedMs = new();
		FileSystemEventArgs? deletedResult = null;

		using ManualResetEventSlim createdMs = new();
		FileSystemEventArgs? createdResult = null;

		using ManualResetEventSlim changedMs = new();
		FileSystemEventArgs? changedResult = null;

		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.Deleted += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				deletedResult = eventArgs;
				deletedMs.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		fileSystemWatcher.Created += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				createdResult = eventArgs;
				createdMs.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		fileSystemWatcher.Changed += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				changedResult = eventArgs;
				changedMs.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.Move(childDirectory, newPath);

		// wait triple the amount in case the other two events are handled first
		await That(deletedMs.Wait(ExpectTimeout * 3, TestContext.Current.CancellationToken))
			.IsTrue();

		await That(changedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsTrue();

		await That(createdMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(includeSubdirectories);

		await That(deletedResult).IsNotNull();
		await That(changedResult).IsNotNull();

		if (includeSubdirectories)
		{
			await That(createdResult).IsNotNull();
		}
		else
		{
			await That(createdResult).IsNull();
		}
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task Windows_ParentWatcher_MoveInPlace_ShouldInvokeChanged(
		bool includeSubdirectories
	)
	{
		Skip.IfNot(Test.RunsOnWindows);

		// short names, otherwise the path will be too long
		const string parentDirectory = "parent";
		const string childDirectory = "child";

		FileSystem.Initialize().WithSubdirectory(parentDirectory)
			.Initialized(x => x.WithSubdirectory(childDirectory));

		string oldPath = FileSystem.Path.Combine(parentDirectory, childDirectory);
		string newPath = FileSystem.Path.Combine(parentDirectory, "newChild");

		using ManualResetEventSlim renamedMs = new();
		FileSystemEventArgs? renamedResult = null;

		using ManualResetEventSlim changedMs = new();
		FileSystemEventArgs? changedResult = null;

		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.Renamed += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				renamedResult = eventArgs;
				renamedMs.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		fileSystemWatcher.Changed += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				changedResult = eventArgs;
				changedMs.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.Move(oldPath, newPath);

		// wait double the amount in case the other event is handled first
		await That(changedMs.Wait(ExpectTimeout * 2, TestContext.Current.CancellationToken)).IsTrue();

		await That(renamedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken))
			.IsEqualTo(includeSubdirectories);

		await That(changedResult).IsNotNull();

		if (includeSubdirectories)
		{
			await That(renamedResult).IsNotNull();
		}
		else
		{
			await That(renamedResult).IsNull();
		}
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task Windows_ParentWatcher_MoveToParent_IncludeSubDirectories_ShouldInvokeCreated(
		bool includeSubdirectories
	)
	{
		Skip.IfNot(Test.RunsOnWindows);

		// short names, otherwise the path will be too long
		const string parentDirectory = "parent";
		const string childDirectory = "child";

		FileSystem.Initialize().WithSubdirectory(parentDirectory)
			.Initialized(x => x.WithSubdirectory(childDirectory));

		string oldPath = FileSystem.Path.Combine(parentDirectory, childDirectory);

		using ManualResetEventSlim deletedMs = new();
		FileSystemEventArgs? deletedResult = null;

		using ManualResetEventSlim createdMs = new();
		FileSystemEventArgs? createdResult = null;

		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.Deleted += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				deletedResult = eventArgs;
				deletedMs.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		fileSystemWatcher.Created += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				createdResult = eventArgs;
				createdMs.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.Move(oldPath, childDirectory);

		// wait double the amount in case the other event is handled first
		await That(createdMs.Wait(ExpectTimeout * 2, TestContext.Current.CancellationToken)).IsTrue();
		await That(deletedMs.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsEqualTo(includeSubdirectories);
		
		await That(createdResult).IsNotNull();

		if (includeSubdirectories)
		{
			await That(deletedResult).IsNotNull();
		}
		else
		{
			await That(deletedResult).IsNull();
		}
	}
}
