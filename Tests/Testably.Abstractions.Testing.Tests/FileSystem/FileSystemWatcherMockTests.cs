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

	[SkippableTheory]
	[AutoData]
	public void Error_DefaultTo64Messages_ShouldBeTriggeredWhenBufferOverflows(
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
				block1.Wait(10000);
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

		block2.Wait(10000).Should().BeTrue();
		result.Should().NotBeNull();
		result!.GetException().Should().BeOfType<InternalBufferOverflowException>();
	}

	[SkippableTheory]
	[InlineAutoData(4096)]
	[InlineAutoData(8192)]
	public void Error_ShouldBeTriggeredWhenBufferOverflows(
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
				block1.Wait(5000);
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

		block2.Wait(5000).Should().BeTrue();
		result.Should().NotBeNull();
		result!.GetException().Should().BeOfType<InternalBufferOverflowException>();
	}

#if FEATURE_FILESYSTEMWATCHER_ADVANCED
	[SkippableTheory]
	[AutoData]
	public void Filter_ShouldResetFiltersToOnlyContainASingleValue(
		string[] filters, string expectedFilter)
	{
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		foreach (string filter in filters)
		{
			fileSystemWatcher.Filters.Add(filter);
		}

		fileSystemWatcher.Filters.Count.Should().Be(filters.Length);

		fileSystemWatcher.Filter = expectedFilter;

		fileSystemWatcher.Filters.Count.Should().Be(1);
		fileSystemWatcher.Filters.Should().ContainSingle(expectedFilter);
		fileSystemWatcher.Filter.Should().Be(expectedFilter);
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void InternalBufferSize_ShouldResetQueue(string path1, string path2)
	{
		Skip.If(true, "Brittle test fails on build system (disabled in #284)");

		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		using ManualResetEventSlim block1 = new();
		using ManualResetEventSlim block2 = new();
		ErrorEventArgs result = null;
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
				block1.Wait(100);
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

		block2.Wait(100).Should().BeFalse();
		result.Should().BeNull();
	}

#if !NETFRAMEWORK
	public sealed class EventArgsTests
	{
		[SkippableTheory]
		[InlineAutoData(SimulationMode.Linux)]
		[InlineAutoData(SimulationMode.MacOS)]
		[InlineAutoData(SimulationMode.Windows)]
		public void FileSystemEventArgs_ShouldUseDirectorySeparatorFromSimulatedFileSystem(
			SimulationMode simulationMode, string parentDirectory, string directoryName)
		{
			MockFileSystem fileSystem =
				new MockFileSystem(s => s.SimulatingOperatingSystem(simulationMode));
			fileSystem.Directory.CreateDirectory(parentDirectory);
			FileSystemEventArgs? result = null;
			string expectedFullPath = fileSystem.Path.GetFullPath(
				fileSystem.Path.Combine(parentDirectory, directoryName));
			string expectedName = fileSystem.Path.Combine(parentDirectory, directoryName);

			using IFileSystemWatcher fileSystemWatcher =
				fileSystem.FileSystemWatcher.New(parentDirectory);
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
			fileSystem.Directory.CreateDirectory(expectedName);
			ms.Wait(5000);

			result.Should().NotBeNull();
			result!.FullPath.Should().Be(expectedFullPath);
			result.Name.Should().Be(expectedName);
			result.ChangeType.Should().Be(WatcherChangeTypes.Created);
		}

		[SkippableTheory]
		[InlineAutoData(SimulationMode.Linux)]
		[InlineAutoData(SimulationMode.MacOS)]
		[InlineAutoData(SimulationMode.Windows)]
		public void RenamedEventArgs_ShouldUseDirectorySeparatorFromSimulatedFileSystem(
			SimulationMode simulationMode, string parentDirectory,
			string sourceName, string destinationName)
		{
			MockFileSystem fileSystem =
				new MockFileSystem(s => s.SimulatingOperatingSystem(simulationMode));
			fileSystem.Directory.CreateDirectory(parentDirectory);
			RenamedEventArgs? result = null;
			string expectedOldFullPath = fileSystem.Path.GetFullPath(
				fileSystem.Path.Combine(parentDirectory, sourceName));
			string expectedFullPath = fileSystem.Path.GetFullPath(
				fileSystem.Path.Combine(parentDirectory, destinationName));
			fileSystem.Directory.CreateDirectory(parentDirectory);
			fileSystem.File.WriteAllText(expectedOldFullPath, "foo");

			using IFileSystemWatcher fileSystemWatcher =
				fileSystem.FileSystemWatcher.New(parentDirectory);
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
			ms.Wait(5000);

			result.Should().NotBeNull();
			result!.FullPath.Should().Be(expectedFullPath);
			result.Name.Should().Be(destinationName);
			result!.OldFullPath.Should().Be(expectedOldFullPath);
			result.OldName.Should().Be(sourceName);
			result.ChangeType.Should().Be(WatcherChangeTypes.Renamed);
		}
	}
#endif
}
