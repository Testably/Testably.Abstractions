using System.IO;
using System.Threading;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

[Collection(nameof(IDirectoryCleaner))]
public sealed class FileSystemWatcherMockTests : IDisposable
{
	#region Test Setup

	/// <summary>
	///     Default number of messages before the buffer overflows is 64:<br />
	///     <c>internal buffer size / bytes per message = 8192 / 128 = 64</c>
	/// </summary>
	public static readonly int DefaultMaxMessages = 64;

	public string BasePath => _directoryCleaner.BasePath;
	public MockFileSystem FileSystem { get; }
	private readonly IDirectoryCleaner _directoryCleaner;

	public FileSystemWatcherMockTests()
	{
		FileSystem = new MockFileSystem();
		_directoryCleaner = FileSystem.SetCurrentDirectoryToEmptyTemporaryDirectory();
		FileSystem.Initialize();
	}

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _directoryCleaner.Dispose();

	#endregion

	[Theory]
	[AutoData]
	public async Task Error_DefaultTo64Messages_ShouldBeTriggeredWhenBufferOverflows(
		string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		using ManualResetEventSlim block1 = new();
		using ManualResetEventSlim block2 = new();
		ErrorEventArgs? result = null;
		fileSystemWatcher.Error += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				result = eventArgs;
				block1.Set();
				// ReSharper disable once AccessToDisposedClosure
				block2.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};
		fileSystemWatcher.Deleted += (_, _) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				block1.Wait(10000, TestContext.Current.CancellationToken);
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};
		fileSystemWatcher.EnableRaisingEvents = true;
		FileSystem.Directory.Delete(path);
		for (int i = 0; i <= DefaultMaxMessages; i++)
		{
			if (block1.IsSet)
			{
				break;
			}

			FileSystem.Directory.CreateDirectory($"{i}_{path}");
		}

		await That(block2.Wait(10000, TestContext.Current.CancellationToken)).IsTrue();
		await That(result).IsNotNull();
		await That(result!.GetException()).IsExactly<InternalBufferOverflowException>();
	}

	[Theory]
	[InlineAutoData(4096)]
	[InlineAutoData(8192)]
	public async Task Error_ShouldBeTriggeredWhenBufferOverflows(
		int internalBufferSize, string path)
	{
		int maxMessages = internalBufferSize / 128;
		FileSystem.Directory.CreateDirectory(path);
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		using ManualResetEventSlim block1 = new();
		using ManualResetEventSlim block2 = new();
		ErrorEventArgs? result = null;
		fileSystemWatcher.Error += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				result = eventArgs;
				block1.Set();
				// ReSharper disable once AccessToDisposedClosure
				block2.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};
		fileSystemWatcher.Deleted += (_, _) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				block1.Wait(5000, TestContext.Current.CancellationToken);
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};
		fileSystemWatcher.EnableRaisingEvents = true;
		fileSystemWatcher.InternalBufferSize = internalBufferSize;
		FileSystem.Directory.Delete(path);
		for (int i = 0; i <= maxMessages; i++)
		{
			if (block1.IsSet)
			{
				break;
			}

			FileSystem.Directory.CreateDirectory($"{i}_{path}");
		}

		await That(block2.Wait(5000, TestContext.Current.CancellationToken)).IsTrue();
		await That(result).IsNotNull();
		await That(result!.GetException()).IsExactly<InternalBufferOverflowException>();
	}

#if FEATURE_FILESYSTEMWATCHER_ADVANCED
	[Theory]
	[AutoData]
	public async Task Filter_ShouldResetFiltersToOnlyContainASingleValue(
		string[] filters, string expectedFilter)
	{
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		foreach (string filter in filters)
		{
			fileSystemWatcher.Filters.Add(filter);
		}

		await That(fileSystemWatcher.Filters.Count).IsEqualTo(filters.Length);

		fileSystemWatcher.Filter = expectedFilter;

		await That(fileSystemWatcher.Filters.Count).IsEqualTo(1);
		await That(fileSystemWatcher.Filters).HasSingle().Because(expectedFilter);
		await That(fileSystemWatcher.Filter).IsEqualTo(expectedFilter);
	}
#endif

	[Theory]
	[AutoData]
	public async Task InternalBufferSize_ShouldResetQueue(string path1, string path2)
	{
		Skip.If(true, "Brittle test fails on build system (disabled in #284)");

		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		using ManualResetEventSlim block1 = new();
		using ManualResetEventSlim block2 = new();
		ErrorEventArgs result = null!;
		fileSystemWatcher.Error += (_, eventArgs) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				result = eventArgs;
				block1.Set();
				// ReSharper disable once AccessToDisposedClosure
				block2.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};
		fileSystemWatcher.Created += (_, _) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				block1.Wait(100, TestContext.Current.CancellationToken);
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};
		fileSystemWatcher.EnableRaisingEvents = true;
		FileSystem.Directory.CreateDirectory(path1);
		for (int i = 0; i < DefaultMaxMessages; i++)
		{
			if (block1.IsSet)
			{
				break;
			}

			FileSystem.Directory.CreateDirectory($"{i}_{path1}");
		}

		fileSystemWatcher.InternalBufferSize = 4196;
		FileSystem.Directory.CreateDirectory(path2);
		for (int i = 0; i < 4196 / 128; i++)
		{
			if (block1.IsSet)
			{
				break;
			}

			FileSystem.Directory.CreateDirectory($"{i}_{path2}");
		}

		await That(block2.Wait(100, TestContext.Current.CancellationToken)).IsFalse();
		await That(result).IsNull();
	}

#if CAN_SIMULATE_OTHER_OS
	public sealed class EventArgsTests
	{
		[Theory]
		[InlineAutoData(SimulationMode.Linux)]
		[InlineAutoData(SimulationMode.MacOS)]
		[InlineAutoData(SimulationMode.Windows)]
		public async Task FileSystemEventArgs_ShouldUseDirectorySeparatorFromSimulatedFileSystem(
			SimulationMode simulationMode, string parentDirectory, string directoryName)
		{
			MockFileSystem fileSystem = new(o => o.SimulatingOperatingSystem(simulationMode));
			fileSystem.Directory.CreateDirectory(parentDirectory);
			FileSystemEventArgs? result = null;
			string expectedFullPath = fileSystem.Path.GetFullPath(
				fileSystem.Path.Combine(parentDirectory, directoryName));

			using IFileSystemWatcher fileSystemWatcher =
				fileSystem.FileSystemWatcher.New(fileSystem.Path.GetFullPath(parentDirectory));
			using ManualResetEventSlim ms = new();
			fileSystemWatcher.Created += (_, eventArgs) =>
			{
				result = eventArgs;
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					ms.Set();
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			};
			fileSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName;
			fileSystemWatcher.EnableRaisingEvents = true;
			fileSystem.Directory.CreateDirectory(expectedFullPath);
			ms.Wait(5000, TestContext.Current.CancellationToken);

			await That(result).IsNotNull();
			await That(result!.FullPath).IsEqualTo(expectedFullPath);
			await That(result.Name).IsEqualTo(directoryName);
			await That(result.ChangeType).IsEqualTo(WatcherChangeTypes.Created);
		}
#endif

#if CAN_SIMULATE_OTHER_OS
		[Theory]
		[InlineAutoData(SimulationMode.Linux)]
		[InlineAutoData(SimulationMode.MacOS)]
		[InlineAutoData(SimulationMode.Windows)]
		public async Task RenamedEventArgs_ShouldUseDirectorySeparatorFromSimulatedFileSystem(
			SimulationMode simulationMode, string parentDirectory,
			string sourceName, string destinationName)
		{
			MockFileSystem fileSystem = new(o => o.SimulatingOperatingSystem(simulationMode));
			fileSystem.Directory.CreateDirectory(parentDirectory);
			RenamedEventArgs? result = null;
			string expectedOldFullPath = fileSystem.Path.GetFullPath(
				fileSystem.Path.Combine(parentDirectory, sourceName));
			string expectedFullPath = fileSystem.Path.GetFullPath(
				fileSystem.Path.Combine(parentDirectory, destinationName));
			fileSystem.Directory.CreateDirectory(parentDirectory);
			fileSystem.File.WriteAllText(expectedOldFullPath, "foo");

			using IFileSystemWatcher fileSystemWatcher =
				fileSystem.FileSystemWatcher.New(fileSystem.Path.GetFullPath(parentDirectory));
			using ManualResetEventSlim ms = new();
			fileSystemWatcher.Renamed += (_, eventArgs) =>
			{
				result = eventArgs;
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					ms.Set();
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			};
			fileSystemWatcher.NotifyFilter = NotifyFilters.FileName;
			fileSystemWatcher.EnableRaisingEvents = true;
			fileSystem.File.Move(expectedOldFullPath, expectedFullPath);
			ms.Wait(5000, TestContext.Current.CancellationToken);

			await That(result).IsNotNull();
			await That(result!.FullPath).IsEqualTo(expectedFullPath);
			await That(result.Name).IsEqualTo(destinationName);
			await That(result!.OldFullPath).IsEqualTo(expectedOldFullPath);
			await That(result.OldName).IsEqualTo(sourceName);
			await That(result.ChangeType).IsEqualTo(WatcherChangeTypes.Renamed);
		}
	}
#endif
}
