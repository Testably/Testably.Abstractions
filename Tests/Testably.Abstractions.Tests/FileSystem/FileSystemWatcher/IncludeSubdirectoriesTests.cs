using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public partial class IncludeSubdirectoriesTests
{
	[Theory]
	[AutoData]
	public async Task IncludeSubdirectories_SetToFalse_ShouldNotTriggerNotification(
		string baseDirectory,
		string path
	)
	{
		FileSystem.Initialize().WithSubdirectory(baseDirectory)
			.Initialized(s => s.WithSubdirectory(path));

		using ManualResetEventSlim ms = new();
		FileSystemEventArgs? result = null;
		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New(BasePath);

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
		FileSystem.Directory.Delete(FileSystem.Path.Combine(baseDirectory, path));
		await That(ms.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await That(result).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task IncludeSubdirectories_SetToTrue_ShouldOnlyTriggerNotificationOnSubdirectories(
		string baseDirectory,
		string subdirectoryName,
		string otherDirectory
	)
	{
		FileSystem.Initialize().WithSubdirectory(baseDirectory)
			.Initialized(s => s.WithSubdirectory(subdirectoryName))
			.WithSubdirectory(otherDirectory);

		using ManualResetEventSlim ms = new();
		FileSystemEventArgs? result = null;

		using IFileSystemWatcher fileSystemWatcher
			= FileSystem.FileSystemWatcher.New(baseDirectory);

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

		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.EnableRaisingEvents = true;
		FileSystem.Directory.Delete(otherDirectory);
		await That(ms.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsFalse();

		await That(result).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task IncludeSubdirectories_SetToTrue_ShouldTriggerNotificationOnSubdirectories(
		string baseDirectory,
		string subdirectoryName
	)
	{
		FileSystem.Initialize().WithSubdirectory(baseDirectory)
			.Initialized(s => s.WithSubdirectory(subdirectoryName));

		string subdirectoryPath = FileSystem.Path.Combine(baseDirectory, subdirectoryName);
		using ManualResetEventSlim ms = new();
		FileSystemEventArgs? result = null;
		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New(BasePath);

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

		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.EnableRaisingEvents = true;
		FileSystem.Directory.Delete(subdirectoryPath);
		await That(ms.Wait(ExpectSuccess, TestContext.Current.CancellationToken)).IsTrue();

		await That(result).IsNotNull();
		await That(result!.FullPath).IsEqualTo(FileSystem.Path.GetFullPath(subdirectoryPath));
		await That(result.Name).IsEqualTo(subdirectoryPath);
		await That(result!.ChangeType).IsEqualTo(WatcherChangeTypes.Deleted);
	}

	[Theory]
	[InlineAutoData(true)]
	[InlineAutoData(false)]
	public async Task IncludeSubdirectories_SetToTrue_Created_ArgsNameShouldContainRelativePath(
		bool watchRootedPath,
		string baseDirectory,
		string subdirectoryName,
		string subSubdirectoryName,
		string fileName
	)
	{
		// Arrange
		FileSystem.Initialize().WithSubdirectory(baseDirectory)
			.Initialized(s => s.WithSubdirectory(subdirectoryName)
				             .Initialized(ss => ss.WithSubdirectory(subSubdirectoryName))
			);

		string filePath = FileSystem.Path.Combine(
			baseDirectory, subdirectoryName, subSubdirectoryName, fileName
		);

		string expectedFileName = FileSystem.Path.Combine(
			subdirectoryName, subSubdirectoryName, fileName
		);

		string watchPath = watchRootedPath
			? FileSystem.Path.Combine(FileSystem.Directory.GetCurrentDirectory(), baseDirectory)
			: baseDirectory;

		using ManualResetEventSlim createdMre = new();
		FileSystemEventArgs? createdArgs = null;

		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New(watchPath);

		fileSystemWatcher.Created += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				createdArgs = eventArgs;
				createdMre.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.EnableRaisingEvents = true;
		
		// Act

		FileSystem.File.Create(filePath).Dispose();
		
		// Assert

		await That(createdMre.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsTrue();

		await That(createdArgs).IsNotNull().And
			.Satisfies(args => string.Equals(args?.Name, expectedFileName, StringComparison.Ordinal)
			);
	}

	[Theory]
	[InlineAutoData(true)]
	[InlineAutoData(false)]
	public async Task IncludeSubdirectories_SetToTrue_Changed_ArgsNameShouldContainRelativePath(
		bool watchRootedPath,
		string baseDirectory,
		string subdirectoryName,
		string subSubdirectoryName,
		string fileName
	)
	{
		// Arrange
		FileSystem.Initialize().WithSubdirectory(baseDirectory)
			.Initialized(s => s.WithSubdirectory(subdirectoryName)
				             .Initialized(ss => ss.WithSubdirectory(subSubdirectoryName))
			);

		string filePath = FileSystem.Path.Combine(
			baseDirectory, subdirectoryName, subSubdirectoryName, fileName
		);

		string expectedFileName = FileSystem.Path.Combine(
			subdirectoryName, subSubdirectoryName, fileName
		);

		string watchPath = watchRootedPath
			? FileSystem.Path.Combine(FileSystem.Directory.GetCurrentDirectory(), baseDirectory)
			: baseDirectory;

		using ManualResetEventSlim changedMre = new();
		FileSystemEventArgs? changedArgs = null;

		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New(watchPath);

		fileSystemWatcher.Changed += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				// OS fires for subSubDir due to item changing
				if (!string.Equals(eventArgs.Name, expectedFileName, StringComparison.Ordinal))
				{
					return;
				}

				changedArgs ??= eventArgs;
				changedMre.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};
		
		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.EnableRaisingEvents = true;
		
		// Act

		FileSystem.File.Create(filePath).Dispose();
		FileSystem.File.WriteAllText(filePath, "Hello World!");
		
		// Assert

		await That(changedMre.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsTrue();

		await That(changedArgs).IsNotNull().And
			.Satisfies(args => string.Equals(args?.Name, expectedFileName, StringComparison.Ordinal)
			);
	}

	[Theory]
	[InlineAutoData(true)]
	[InlineAutoData(false)]
	public async Task IncludeSubdirectories_SetToTrue_Renamed_ArgsNameShouldContainRelativePath(
		bool watchRootedPath,
		string baseDirectory,
		string subdirectoryName,
		string subSubdirectoryName,
		string fileName
	)
	{
		// Arrange
		FileSystem.Initialize().WithSubdirectory(baseDirectory)
			.Initialized(s => s.WithSubdirectory(subdirectoryName)
				             .Initialized(ss => ss.WithSubdirectory(subSubdirectoryName))
			);

		string filePath = FileSystem.Path.Combine(
			baseDirectory, subdirectoryName, subSubdirectoryName, fileName
		);

		string newFilePath = filePath + ".new";

		string expectedFileName = FileSystem.Path.Combine(
			subdirectoryName, subSubdirectoryName, fileName
		);

		string expectedNewFileName = expectedFileName + ".new";

		string watchPath = watchRootedPath
			? FileSystem.Path.Combine(FileSystem.Directory.GetCurrentDirectory(), baseDirectory)
			: baseDirectory;

		using ManualResetEventSlim renamedMre = new();
		RenamedEventArgs? renamedArgs = null;

		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New(watchPath);

		fileSystemWatcher.Renamed += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				renamedArgs = eventArgs;
				renamedMre.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.EnableRaisingEvents = true;
		
		// Act

		FileSystem.File.Create(filePath).Dispose();
		FileSystem.File.Move(filePath, newFilePath);
		
		// Assert

		await That(renamedMre.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsTrue();

		await That(renamedArgs).IsNotNull().And
			.Satisfies(args => string.Equals(
				           args?.Name, expectedNewFileName, StringComparison.Ordinal
			           )
			).And.Satisfies(args => string.Equals(
				                args?.OldName, expectedFileName, StringComparison.Ordinal
			                )
			);
	}

	[Theory]
	[InlineAutoData(true)]
	[InlineAutoData(false)]
	public async Task IncludeSubdirectories_SetToTrue_Deleted_ArgsNameShouldContainRelativePath(
		bool watchRootedPath,
		string baseDirectory,
		string subdirectoryName,
		string subSubdirectoryName,
		string fileName
	)
	{
		// Arrange
		FileSystem.Initialize().WithSubdirectory(baseDirectory)
			.Initialized(s => s.WithSubdirectory(subdirectoryName)
				             .Initialized(ss => ss.WithSubdirectory(subSubdirectoryName))
			);

		string filePath = FileSystem.Path.Combine(
			baseDirectory, subdirectoryName, subSubdirectoryName, fileName
		);

		string expectedFileName = FileSystem.Path.Combine(
			subdirectoryName, subSubdirectoryName, fileName
		);

		string watchPath = watchRootedPath
			? FileSystem.Path.Combine(FileSystem.Directory.GetCurrentDirectory(), baseDirectory)
			: baseDirectory;

		using ManualResetEventSlim deletedMre = new();
		FileSystemEventArgs? deletedArgs = null;

		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New(watchPath);

		fileSystemWatcher.Deleted += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				deletedArgs ??= eventArgs;
				deletedMre.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.EnableRaisingEvents = true;
		
		// Act

		FileSystem.File.Create(filePath).Dispose();
		FileSystem.File.Delete(filePath);
		
		// Assert

		await That(deletedMre.Wait(ExpectTimeout, TestContext.Current.CancellationToken)).IsTrue();

		await That(deletedArgs).IsNotNull().And.Satisfies(args => string.Equals(
			                                                  args?.Name, expectedFileName,
			                                                  StringComparison.Ordinal
		                                                  )
		);
	}
}
