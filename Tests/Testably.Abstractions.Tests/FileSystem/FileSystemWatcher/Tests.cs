using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public partial class Tests
{
	[Theory]
	[AutoData]
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
						await Task.Delay(10, TestContext.Current.CancellationToken);
						FileSystem.Directory.CreateDirectory(path);
						FileSystem.Directory.Delete(path);
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}, TestContext.Current.CancellationToken);
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

	[Fact]
	public async Task Container_ShouldBeInitializedWithNull()
	{
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		await That(fileSystemWatcher.Container).IsNull();
	}

	[Theory]
	[AutoData]
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
						await Task.Delay(10, TestContext.Current.CancellationToken);
						FileSystem.Directory.CreateDirectory(path);
						FileSystem.Directory.Delete(path);
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}, TestContext.Current.CancellationToken);
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

	[Theory]
	[InlineData(-1, 4096)]
	[InlineData(4095, 4096)]
	[InlineData(4097, 4097)]
	public async Task InternalBufferSize_ShouldAtLeastHave4096Bytes(
		int bytes, int expectedBytes)
	{
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.InternalBufferSize = bytes;

		await That(fileSystemWatcher.InternalBufferSize).IsEqualTo(expectedBytes);
	}

	[Fact]
	public async Task Site_ShouldBeInitializedWithNull()
	{
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		await That(fileSystemWatcher.Site).IsNull();
	}

	[Fact]
	public async Task Site_ShouldBeWritable()
	{
		ISite site = new MockSite();
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.Site = site;

		await That(fileSystemWatcher.Site).IsEqualTo(site);
	}

	[Fact]
	public async Task SynchronizingObject_ShouldBeInitializedWithNull()
	{
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		await That(fileSystemWatcher.SynchronizingObject).IsNull();
	}

	[Fact]
	public async Task SynchronizingObject_ShouldBeWritable()
	{
		ISynchronizeInvoke synchronizingObject = new MockSynchronizeInvoke();
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.SynchronizingObject = synchronizingObject;

		await That(fileSystemWatcher.SynchronizingObject).IsEqualTo(synchronizingObject);
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
