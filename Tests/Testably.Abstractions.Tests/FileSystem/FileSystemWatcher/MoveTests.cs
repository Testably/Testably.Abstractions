using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public partial class MoveTests
{
	[Fact]
	public async Task ParentWatcher_MoveToParent_ShouldInvokeCreated()
	{
		// short names, otherwise the path will be too long
		const string parentDirectory = "parent";
		const string childDirectory = "child";

		FileSystem.Initialize().WithSubdirectory(parentDirectory)
			.Initialized(s => s.WithSubdirectory(childDirectory));

		string oldPath = FileSystem.Path.Combine(parentDirectory, childDirectory);

		using ManualResetEventSlim ms = new();
		FileSystemEventArgs? result = null;

		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.Created += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				result = eventArgs;
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		fileSystemWatcher.IncludeSubdirectories = false;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.Move(oldPath, childDirectory);

		await That(ms.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsTrue();

		await That(result).IsNotNull();
	}

	[Fact]
	public async Task ChildWatcher_MoveToChild_ShouldInvokeCreated()
	{
		// short names, otherwise the path will be too long
		const string parentDirectory = "parent";
		const string childDirectory = "child";

		FileSystem.Initialize().WithSubdirectories(parentDirectory, childDirectory);

		string newPath = FileSystem.Path.Combine(parentDirectory, childDirectory);

		using ManualResetEventSlim ms = new();
		FileSystemEventArgs? result = null;

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(FileSystem.Path.Combine(BasePath, parentDirectory));

		fileSystemWatcher.Created += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				result = eventArgs;
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		fileSystemWatcher.IncludeSubdirectories = false;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.Move(childDirectory, newPath);

		await That(ms.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsTrue();

		await That(result).IsNotNull();
	}

	[Fact]
	public async Task ChildWatcher_MoveInPlace_ShouldInvokeRename()
	{
		// short names, otherwise the path will be too long
		const string parentDirectory = "parent";
		const string childDirectory = "child";
		
		FileSystem.Initialize().WithSubdirectory(parentDirectory)
			.Initialized(x => x.WithSubdirectory(childDirectory));
		
		string oldChildDirectory = FileSystem.Path.Combine(parentDirectory, childDirectory);
		string newChildDirectory = FileSystem.Path.Combine(parentDirectory, "newChild");

		using ManualResetEventSlim ms = new();
		FileSystemEventArgs? result = null;

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(FileSystem.Path.Combine(BasePath, parentDirectory));

		fileSystemWatcher.Renamed += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				result = eventArgs;
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		fileSystemWatcher.IncludeSubdirectories = false;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.Move(oldChildDirectory, newChildDirectory);

		await That(ms.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsTrue();

		await That(result).IsNotNull();
	}

	[Fact]
	public async Task ChildWatcher_MoveToParent_ShouldInvokeDeleted()
	{
		// short names, otherwise the path will be too long
		const string parentDirectory = "parent";
		const string childDirectory = "child";
		
		FileSystem.Initialize().WithSubdirectory(parentDirectory)
			.Initialized(x => x.WithSubdirectory(childDirectory));

		string oldPath = FileSystem.Path.Combine(parentDirectory, childDirectory);

		using ManualResetEventSlim ms = new();
		FileSystemEventArgs? result = null;

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(FileSystem.Path.Combine(BasePath, parentDirectory));

		fileSystemWatcher.Deleted += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				result = eventArgs;
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		fileSystemWatcher.IncludeSubdirectories = false;
		fileSystemWatcher.EnableRaisingEvents = true;

		FileSystem.Directory.Move(oldPath, childDirectory);

		await That(ms.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsTrue();

		await That(result).IsNotNull();
	}
}
