using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public class Tests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task BeginInit_ShouldStopListening(string path)
	{
		FileSystem.Initialize();
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.EnableRaisingEvents = true;

		fileSystemWatcher.BeginInit();

		fileSystemWatcher.InternalBufferSize = 5000;

		await That(fileSystemWatcher.EnableRaisingEvents).IsTrue();
		try
		{
			_ = Task.Run(async () =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					while (!ms.IsSet)
					{
						await Task.Delay(10, CancellationToken);
						FileSystem.Directory.CreateDirectory(path);
						FileSystem.Directory.Delete(path);
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}, CancellationToken);
			IWaitForChangedResult result =
				fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Created, 250);

			await That(fileSystemWatcher.EnableRaisingEvents).IsTrue();
			await That(result.TimedOut).IsTrue();
			await That(result.ChangeType).IsEqualTo(0);
			await That(result.Name).IsNull();
			await That(result.OldName).IsNull();
		}
		finally
		{
			ms.Set();
		}
	}

	[Test]
	public async Task Container_ShouldBeInitializedWithNull()
	{
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		await That(fileSystemWatcher.Container).IsNull();
	}

	[Test]
	public async Task Dispose_WhileEventsAreQueued_ShouldDropQueuedEvents()
	{
		const string blockingName = "first";
		const string pendingName = "pending";
		FileSystem.Initialize();
		string watched = FileSystem.Path.Combine(BasePath, "a");
		FileSystem.Directory.CreateDirectory(watched);

		string[] names = await CreateWhileDeliveryIsBlocked(
			watched, blockingName, pendingName,
			fileSystemWatcher => fileSystemWatcher.Dispose());

		await That(names).IsEqualTo([blockingName]);
	}

	[Test]
	public async Task EnableRaisingEvents_WhenDisabledWhileEventsAreQueued_ShouldDropQueuedEvents()
	{
		const string blockingName = "first";
		const string pendingName = "pending";
		FileSystem.Initialize();
		string watched = FileSystem.Path.Combine(BasePath, "a");
		FileSystem.Directory.CreateDirectory(watched);

		string[] names = await CreateWhileDeliveryIsBlocked(
			watched, blockingName, pendingName,
			fileSystemWatcher => fileSystemWatcher.EnableRaisingEvents = false);

		await That(names).IsEqualTo([blockingName]);
	}

	[Test]
	[AutoArguments]
	public async Task EndInit_ShouldRestartListening(string path)
	{
		FileSystem.Initialize();
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.EnableRaisingEvents = true;
		fileSystemWatcher.BeginInit();

		fileSystemWatcher.EndInit();

		await That(fileSystemWatcher.EnableRaisingEvents).IsTrue();
		try
		{
			_ = Task.Run(async () =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					while (!ms.IsSet)
					{
						await Task.Delay(10, CancellationToken);
						FileSystem.Directory.CreateDirectory(path);
						FileSystem.Directory.Delete(path);
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}, CancellationToken);
			IWaitForChangedResult result =
				fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Created, ExpectSuccess);

			await That(fileSystemWatcher.EnableRaisingEvents).IsTrue();
			await That(result.TimedOut).IsFalse();
		}
		finally
		{
			ms.Set();
		}
	}

	[Test]
	public async Task IncludeSubdirectories_WhenChangedWhileEventsAreQueued_ShouldDropQueuedEvents()
	{
		Skip.If(Test.RunsOnMac);

		const string blockingName = "first";
		const string pendingName = "pending";
		const string markerName = "marker";
		FileSystem.Initialize();
		string watched = FileSystem.Path.Combine(BasePath, "a");
		FileSystem.Directory.CreateDirectory(watched);

		string[] names = await CreateWhileDeliveryIsBlocked(
			watched, blockingName, pendingName,
			fileSystemWatcher => fileSystemWatcher.IncludeSubdirectories = true,
			(watched, markerName));

		await That(names).IsEqualTo([blockingName, markerName]);
	}

	[Test]
	public async Task IncludeSubdirectories_WhenChangedWhileEventsAreQueued_OnMac_ShouldDropQueuedEvents()
	{
		Skip.IfNot(Test.RunsOnMac);
		SkipIfBrittleTestsShouldBeSkipped();

		const string blockingName = "first";
		const string pendingName = "pending";
		const string markerName = "marker";
		FileSystem.Initialize();
		string watched = FileSystem.Path.Combine(BasePath, "a");
		FileSystem.Directory.CreateDirectory(watched);

		string[] names = await CreateWhileDeliveryIsBlocked(
			watched, blockingName, pendingName,
			fileSystemWatcher => fileSystemWatcher.IncludeSubdirectories = true,
			(watched, markerName));

		await That(names).IsEqualTo([blockingName, markerName]);
	}

	[Test]
	[Arguments(-1, 4096)]
	[Arguments(4095, 4096)]
	[Arguments(4097, 4097)]
	public async Task InternalBufferSize_ShouldAtLeastHave4096Bytes(
		int bytes, int expectedBytes)
	{
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.InternalBufferSize = bytes;

		await That(fileSystemWatcher.InternalBufferSize).IsEqualTo(expectedBytes);
	}

	[Test]
	public async Task InternalBufferSize_ShouldNotBeFilledByChangesOutsideTheWatchedPath()
	{
		const int unrelatedChanges = 1000;
		const string firstFolder = "first";
		const string lastFolder = "last";
		FileSystem.Initialize();
		string watched = FileSystem.Path.Combine(BasePath, "watched");
		string unrelated = FileSystem.Path.Combine(BasePath, "unrelated");
		FileSystem.Directory.CreateDirectory(watched);
		FileSystem.Directory.CreateDirectory(unrelated);
		using ManualResetEventSlim blocked = new();
		using ManualResetEventSlim release = new();
		using ManualResetEventSlim lastCreated = new();
		List<Exception> errors = [];
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(watched);
		fileSystemWatcher.InternalBufferSize = 4096;
		fileSystemWatcher.IncludeSubdirectories = false;

		fileSystemWatcher.Created += (_, args) =>
		{
			// ReSharper disable AccessToDisposedClosure
			switch (args.Name)
			{
				case firstFolder:
					// Blocks the delivery of events, like a starved thread, while the unrelated changes happen.
					blocked.Set();
					release.Wait(ExpectSuccess, CancellationToken);
					break;
				case lastFolder:
					lastCreated.Set();
					break;
			}
			// ReSharper restore AccessToDisposedClosure
		};

		fileSystemWatcher.Error += (_, args) =>
		{
			lock (errors)
			{
				errors.Add(args.GetException());
			}
		};

		fileSystemWatcher.EnableRaisingEvents = true;

		try
		{
			FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(watched, firstFolder));
			await That(blocked.Wait(ExpectSuccess, CancellationToken)).IsTrue();

			for (int i = 0; i < unrelatedChanges; i++)
			{
				// ReSharper disable once MethodHasAsyncOverload
				FileSystem.File.WriteAllText(FileSystem.Path.Combine(unrelated, $"file-{i}.txt"), "content");
			}
		}
		finally
		{
			release.Set();
		}

		FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(watched, lastFolder));

		await That(lastCreated.Wait(ExpectSuccess, CancellationToken)).IsTrue();

		Exception[] reported;

		lock (errors)
		{
			reported = [..errors];
		}

		// The unrelated changes are outside the watched path, so the real file systems never report them to the
		// watcher, and they cannot overflow its buffer.
		await That(reported).IsEmpty();
	}

	[Test]
	public async Task NotifyFilter_WhenChangedWhileEventsAreQueued_ShouldDropQueuedEvents()
	{
		Skip.If(Test.RunsOnMac);

		const string blockingName = "first";
		const string pendingName = "pending";
		const string markerName = "marker";
		FileSystem.Initialize();
		string watched = FileSystem.Path.Combine(BasePath, "a");
		FileSystem.Directory.CreateDirectory(watched);

		string[] names = await CreateWhileDeliveryIsBlocked(
			watched, blockingName, pendingName,
			fileSystemWatcher => fileSystemWatcher.NotifyFilter =
				NotifyFilters.DirectoryName | NotifyFilters.FileName,
			(watched, markerName));

		await That(names).IsEqualTo([blockingName, markerName]);
	}

	[Test]
	public async Task NotifyFilter_WhenChangedWhileEventsAreQueued_OnMac_ShouldDropQueuedEvents()
	{
		Skip.IfNot(Test.RunsOnMac);
		SkipIfBrittleTestsShouldBeSkipped();

		const string blockingName = "first";
		const string pendingName = "pending";
		const string markerName = "marker";
		FileSystem.Initialize();
		string watched = FileSystem.Path.Combine(BasePath, "a");
		FileSystem.Directory.CreateDirectory(watched);

		string[] names = await CreateWhileDeliveryIsBlocked(
			watched, blockingName, pendingName,
			fileSystemWatcher => fileSystemWatcher.NotifyFilter =
				NotifyFilters.DirectoryName | NotifyFilters.FileName,
			(watched, markerName));

		await That(names).IsEqualTo([blockingName, markerName]);
	}

	[Test]
	public async Task Path_WhenChangedWhileEventsAreQueued_ShouldDropQueuedEvents()
	{
		const string blockingName = "first";
		const string pendingName = "pending";
		const string markerName = "marker";
		FileSystem.Initialize();
		string watched = FileSystem.Path.Combine(BasePath, "a");
		string other = FileSystem.Path.Combine(BasePath, "b");
		FileSystem.Directory.CreateDirectory(watched);
		FileSystem.Directory.CreateDirectory(other);

		string[] names = await CreateWhileDeliveryIsBlocked(
			watched, blockingName, pendingName,
			fileSystemWatcher => fileSystemWatcher.Path = other,
			(other, markerName));

		await That(names).IsEqualTo([blockingName, markerName]);
	}

	[Test]
	public async Task Site_ShouldBeInitializedWithNull()
	{
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		await That(fileSystemWatcher.Site).IsNull();
	}

	[Test]
	public async Task Site_ShouldBeWritable()
	{
		ISite site = new MockSite();
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.Site = site;

		await That(fileSystemWatcher.Site).IsEqualTo(site);
	}

	[Test]
	public async Task SynchronizingObject_ShouldBeInitializedWithNull()
	{
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		await That(fileSystemWatcher.SynchronizingObject).IsNull();
	}

	[Test]
	public async Task SynchronizingObject_ShouldBeWritable()
	{
		ISynchronizeInvoke synchronizingObject = new MockSynchronizeInvoke();
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.SynchronizingObject = synchronizingObject;

		await That(fileSystemWatcher.SynchronizingObject).IsEqualTo(synchronizingObject);
	}

	/// <summary>
	///     Blocks the delivery of the <paramref name="blockingName" /> event in <paramref name="watched" />, creates
	///     <paramref name="pendingName" /> there and applies <paramref name="change" /> while it is blocked, then
	///     returns the names of all delivered <c>Created</c> events.
	/// </summary>
	/// <remarks>
	///     With a <paramref name="marker" />, the marker is created after the release and its event ends the wait.
	///     Without one (the watcher no longer raises events), the wait ends after <see cref="ExpectTimeout" />.
	/// </remarks>
	private async Task<string[]> CreateWhileDeliveryIsBlocked(
		string watched, string blockingName, string pendingName,
		Action<IFileSystemWatcher> change, (string Parent, string Name)? marker = null)
	{
		using ManualResetEventSlim blocked = new();
		using ManualResetEventSlim release = new();
		using ManualResetEventSlim blockingReturned = new();
		using ManualResetEventSlim pendingCreated = new();
		using ManualResetEventSlim markerCreated = new();
		List<string> names = [];
		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New(watched);
		fileSystemWatcher.IncludeSubdirectories = false;

		fileSystemWatcher.Created += (_, args) =>
		{
			// ReSharper disable AccessToDisposedClosure
			lock (names)
			{
				names.Add(args.Name ?? "");
			}

			if (string.Equals(args.Name, blockingName, StringComparison.Ordinal))
			{
				blocked.Set();
				release.Wait(ExpectSuccess, CancellationToken);
				blockingReturned.Set();
			}
			else if (string.Equals(args.Name, pendingName, StringComparison.Ordinal))
			{
				pendingCreated.Set();
			}
			else if (string.Equals(args.Name, marker?.Name, StringComparison.Ordinal))
			{
				markerCreated.Set();
			}
			// ReSharper restore AccessToDisposedClosure
		};

		fileSystemWatcher.EnableRaisingEvents = true;

		try
		{
			FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(watched, blockingName));
			await That(blocked.Wait(ExpectSuccess, CancellationToken)).IsTrue();

			FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(watched, pendingName));
			change(fileSystemWatcher);
		}
		finally
		{
			release.Set();
		}

		if (marker is { } m)
		{
			FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(m.Parent, m.Name));
			await That(markerCreated.Wait(ExpectSuccess, CancellationToken)).IsTrue();
		}
		else
		{
			await That(blockingReturned.Wait(ExpectSuccess, CancellationToken)).IsTrue();
			// The stopped watcher would deliver the pending event right after the blocking handler returns.
			_ = pendingCreated.Wait(ExpectTimeout, CancellationToken);
		}

		lock (names)
		{
			return [..names];
		}
	}

	private sealed class MockSite : ISite
	{
		public MockSite()
		{
			Component = new ComponentMock(this);
			Container = new ContainerMock();
		}

		#region ISite Members

		/// <inheritdoc cref="ISite.Component" />
		public IComponent Component { get; }

		/// <inheritdoc cref="ISite.Container" />
		public IContainer Container { get; }

		/// <inheritdoc cref="ISite.DesignMode" />
		public bool DesignMode => true;

		/// <inheritdoc cref="ISite.Name" />
		public string? Name { get; set; } = "";

		/// <inheritdoc cref="ISite.GetService(Type)" />
		public object GetService(Type serviceType)
			=> throw new NotSupportedException();

		#endregion

		private sealed class ContainerMock : IContainer
		{
			#region IContainer Members

			/// <inheritdoc cref="IContainer.Components" />
			public ComponentCollection Components
				=> throw new NotSupportedException();

			/// <inheritdoc cref="IContainer.Add(IComponent?)" />
			public void Add(IComponent? component)
			{
				// Ignore any call in tests
			}

			/// <inheritdoc cref="IContainer.Add(IComponent?, string?)" />
			public void Add(IComponent? component, string? name)
			{
				// Ignore any call in tests
			}

			/// <inheritdoc cref="IDisposable.Dispose()" />
			public void Dispose()
			{
				// Ignore any call in tests
			}

			/// <inheritdoc cref="IContainer.Remove(IComponent?)" />
			public void Remove(IComponent? component)
			{
				// Ignore any call in tests
			}

			#endregion
		}

		private sealed class ComponentMock(ISite site) : IComponent
		{
			#region IComponent Members

			/// <inheritdoc cref="IComponent.Site" />
			public ISite? Site { get; set; } = site;

			/// <inheritdoc cref="IDisposable.Dispose()" />
			public void Dispose()
			{
				// Ignore any call in tests
			}

			#pragma warning disable CS0067 // Event is required by the interface
			/// <inheritdoc cref="IComponent.Disposed" />
			public event EventHandler? Disposed;
			#pragma warning restore CS0067

			#endregion
		}
	}

	private sealed class MockSynchronizeInvoke : ISynchronizeInvoke
	{
		#region ISynchronizeInvoke Members

		/// <inheritdoc cref="ISynchronizeInvoke.InvokeRequired" />
		public bool InvokeRequired
			=> true;

		/// <inheritdoc cref="ISynchronizeInvoke.BeginInvoke(Delegate, object[])" />
		public IAsyncResult BeginInvoke(Delegate method, object?[]? args)
			=> throw new NotSupportedException();

		/// <inheritdoc cref="ISynchronizeInvoke.EndInvoke(IAsyncResult)" />
		public object EndInvoke(IAsyncResult result)
			=> throw new NotSupportedException();

		/// <inheritdoc cref="ISynchronizeInvoke.Invoke(Delegate, object[])" />
		// ReSharper disable once ReturnTypeCanBeNotNullable
		public object? Invoke(Delegate method, object?[]? args)
			=> throw new NotSupportedException();

		#endregion
	}
}
