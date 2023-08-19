using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void BeginInit_ShouldStopListening(string path)
	{
		Test.SkipBrittleTestsOnRealFileSystem(FileSystem);

		FileSystem.Initialize();
		ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.EnableRaisingEvents = true;

		fileSystemWatcher.BeginInit();

		fileSystemWatcher.InternalBufferSize = 5000;

		fileSystemWatcher.EnableRaisingEvents.Should().BeTrue();
		try
		{
			Task.Run(() =>
			{
				while (!ms.IsSet)
				{
					Thread.Sleep(10);
					FileSystem.Directory.CreateDirectory(path);
					FileSystem.Directory.Delete(path);
				}
			});
			IWaitForChangedResult result =
				fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Created, 250);

			fileSystemWatcher.EnableRaisingEvents.Should().BeTrue();
			result.TimedOut.Should().BeTrue();
			result.ChangeType.Should().Be(0);
			result.Name.Should().BeNull();
			result.OldName.Should().BeNull();
		}
		finally
		{
			ms.Set();
		}
	}

	[SkippableFact]
	public void Container_ShouldBeInitializedWithNull()
	{
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.Container.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void EndInit_ShouldRestartListening(string path)
	{
		Test.SkipBrittleTestsOnRealFileSystem(FileSystem);

		FileSystem.Initialize();
		ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.EnableRaisingEvents = true;
		fileSystemWatcher.BeginInit();

		fileSystemWatcher.EndInit();

		fileSystemWatcher.EnableRaisingEvents.Should().BeTrue();
		try
		{
			Task.Run(() =>
			{
				while (!ms.IsSet)
				{
					Thread.Sleep(10);
					FileSystem.Directory.CreateDirectory(path);
					FileSystem.Directory.Delete(path);
				}
			});
			IWaitForChangedResult result =
				fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Created, 30000);

			fileSystemWatcher.EnableRaisingEvents.Should().BeTrue();
			result.TimedOut.Should().BeFalse();
		}
		finally
		{
			ms.Set();
		}
	}

	[SkippableTheory]
	[InlineData(-1, 4096)]
	[InlineData(4095, 4096)]
	[InlineData(4097, 4097)]
	public void InternalBufferSize_ShouldAtLeastHave4096Bytes(
		int bytes, int expectedBytes)
	{
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.InternalBufferSize = bytes;

		fileSystemWatcher.InternalBufferSize.Should().Be(expectedBytes);
	}

	[SkippableFact]
	public void Site_ShouldBeInitializedWithNull()
	{
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.Site.Should().BeNull();
	}

	[SkippableFact]
	public void Site_ShouldBeWritable()
	{
		ISite site = new MockSite();
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.Site = site;

		fileSystemWatcher.Site.Should().Be(site);
	}

	[SkippableFact]
	public void SynchronizingObject_ShouldBeInitializedWithNull()
	{
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.SynchronizingObject.Should().BeNull();
	}

	[SkippableFact]
	public void SynchronizingObject_ShouldBeWritable()
	{
		ISynchronizeInvoke synchronizingObject = new MockSynchronizeInvoke();
		FileSystem.Initialize();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.SynchronizingObject = synchronizingObject;

		fileSystemWatcher.SynchronizingObject.Should().Be(synchronizingObject);
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
			/// <inheritdoc />
			public void Dispose()
			{
				// Ignore any call in tests
			}

			/// <inheritdoc />
			public void Add(IComponent? component)
			{
				// Ignore any call in tests
			}

			/// <inheritdoc />
			public void Add(IComponent? component, string? name)
			{
				// Ignore any call in tests
			}

			/// <inheritdoc />
			public void Remove(IComponent? component)
			{
				// Ignore any call in tests
			}

			/// <inheritdoc />
			public ComponentCollection Components
				=> throw new NotSupportedException();
		}

		private sealed class ComponentMock : IComponent
		{
			public ComponentMock(ISite site)
			{
				Site = site;
			}

			/// <inheritdoc cref="IDisposable.Dispose()" />
			public void Dispose()
			{
			}

			/// <inheritdoc cref="IComponent.Site" />
			public ISite? Site { get; set; }

			/// <inheritdoc cref="IComponent.Disposed" />
			public event EventHandler? Disposed;
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
