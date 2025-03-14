﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Initializer;
using Testably.Abstractions.Testing.RandomSystem;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing;

/// <summary>
///     A test helper for simulating the file system. Implements <see cref="IFileSystem" />.
/// </summary>
public sealed class MockFileSystem : IFileSystem
{
	/// <summary>
	///     Intercept events in the <see cref="MockFileSystem" /> before they occur.
	/// </summary>
	public IInterceptionHandler Intercept => ChangeHandler;

	/// <summary>
	///     Get notified of events in the <see cref="MockFileSystem" /> after they occurred.
	/// </summary>
	public INotificationHandler Notify => ChangeHandler;

	/// <summary>
	///     The used random system.
	/// </summary>
	public IRandomSystem RandomSystem { get; }

#if CAN_SIMULATE_OTHER_OS
	/// <summary>
	///     The simulation mode for the underlying operating system.
	/// </summary>
	/// <remarks>
	///     Can be changed by setting <see cref="MockFileSystemOptions.SimulatingOperatingSystem(Testing.SimulationMode)" /> in
	///     the constructor.
	/// </remarks>
#else
	/// <summary>
	///     The simulation mode for the underlying operating system.
	/// </summary>
	/// <remarks>
	///     This functionality is only supported on .NET 6 or newer.
	/// </remarks>
#endif
	public SimulationMode SimulationMode { get; }

	/// <summary>
	///     Contains statistical information about the file system usage.
	/// </summary>
	public IFileSystemStatistics Statistics => StatisticsRegistration;

	/// <summary>
	///     The used time system.
	/// </summary>
	public ITimeSystem TimeSystem { get; }

	internal IAccessControlStrategy AccessControlStrategy
	{
		get;
		private set;
	}

	/// <summary>
	///     The change handler used to notify about events occurring in the <see cref="MockFileSystem" />.
	/// </summary>
	internal ChangeHandler ChangeHandler { get; }

	/// <summary>
	///     The execution engine for the underlying operating system.
	/// </summary>
	internal Execute Execute { get; }

	internal FileSystemRegistration Registration { get; }

	internal ISafeFileHandleStrategy SafeFileHandleStrategy
	{
		get;
		private set;
	}

	internal FileSystemStatistics StatisticsRegistration { get; }

	/// <summary>
	///     The underlying storage of directories and files.
	/// </summary>
	internal IStorage Storage => _storage;

	/// <summary>
	///     The registered containers in the in-Memory <see cref="Storage" />.
	/// </summary>
	internal IReadOnlyList<IStorageContainer> StorageContainers
		=> _storage.GetContainers();

	private readonly DirectoryMock _directoryMock;
	private readonly FileMock _fileMock;
	private readonly PathMock _pathMock;
	private readonly InMemoryStorage _storage;

	/// <summary>
	///     Initializes the <see cref="MockFileSystem" />.
	/// </summary>
	public MockFileSystem() : this(o => o) { }

	/// <summary>
	///     Initializes the <see cref="MockFileSystem" /> with the <paramref name="options" />.
	/// </summary>
	public MockFileSystem(Func<MockFileSystemOptions, MockFileSystemOptions> options)
	{
		MockFileSystemOptions initialization = new();
		initialization = options(initialization);

#if CAN_SIMULATE_OTHER_OS
		SimulationMode = initialization.SimulationMode;
		Execute = SimulationMode == SimulationMode.Native
			? new Execute(this)
			: new Execute(this, SimulationMode);
#else
		SimulationMode = SimulationMode.Native;
		Execute = new Execute(this);
#endif
		Registration = new FileSystemRegistration();
		StatisticsRegistration = new FileSystemStatistics(this);
		using IDisposable release = FileSystemRegistration.Ignore();
		RandomSystem =
			new MockRandomSystem(initialization.RandomProvider ?? RandomProvider.Default());
		TimeSystem = new MockTimeSystem(TimeProvider.Now());
		_pathMock = new PathMock(this);
		_storage = new InMemoryStorage(this);
		ChangeHandler = new ChangeHandler(this);
		_directoryMock = new DirectoryMock(this);
		_fileMock = new FileMock(this);
		DirectoryInfo = new DirectoryInfoFactoryMock(this);
		DriveInfo = new DriveInfoFactoryMock(this);
		FileInfo = new FileInfoFactoryMock(this);
		FileStream = new FileStreamFactoryMock(this);
		FileSystemWatcher = new FileSystemWatcherFactoryMock(this);
		FileVersionInfo = new FileVersionInfoFactoryMock(this);
		SafeFileHandleStrategy = new NullSafeFileHandleStrategy();
		AccessControlStrategy = new NullAccessControlStrategy();
		InitializeFileSystem(initialization);
	}

	#region IFileSystem Members

	/// <inheritdoc cref="IFileSystem.Directory" />
	public IDirectory Directory
		=> _directoryMock;

	/// <inheritdoc cref="IFileSystem.DirectoryInfo" />
	public IDirectoryInfoFactory DirectoryInfo { get; }

	/// <inheritdoc cref="IFileSystem.DriveInfo" />
	public IDriveInfoFactory DriveInfo { get; }

	/// <inheritdoc cref="IFileSystem.File" />
	public IFile File
		=> _fileMock;

	/// <inheritdoc cref="IFileSystem.FileInfo" />
	public IFileInfoFactory FileInfo { get; }

	/// <inheritdoc cref="IFileSystem.FileStream" />
	public IFileStreamFactory FileStream { get; }

	/// <inheritdoc cref="IFileSystem.FileSystemWatcher" />
	public IFileSystemWatcherFactory FileSystemWatcher { get; }

	/// <inheritdoc cref="IFileSystem.FileVersionInfo" />
	public IFileVersionInfoFactory FileVersionInfo { get; }

	/// <inheritdoc cref="IFileSystem.Path" />
	public IPath Path
		=> _pathMock;

	#endregion

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"MockFileSystem ({_storage})";

	/// <summary>
	///     Implements a custom access control (ACL) mechanism.
	///     <para />
	///     The <see cref="IAccessControlStrategy" /> defines a method that receives two values and allows or denies access:
	///     <br />
	///     - The full path of the file or directory as first parameter<br />
	///     - The <see cref="Testably.Abstractions.Helpers.IFileSystemExtensibility" /> as second parameter
	/// </summary>
	public MockFileSystem WithAccessControlStrategy(IAccessControlStrategy accessControlStrategy)
	{
		AccessControlStrategy = accessControlStrategy;
		return this;
	}

	/// <summary>
	///     Changes the parameters of the specified <paramref name="drive" />.
	///     <para />
	///     If the <paramref name="drive" /> does not exist, it will be created/mounted.
	/// </summary>
	public MockFileSystem WithDrive(string? drive,
		Action<IStorageDrive>? driveCallback = null)
	{
		IStorageDrive driveInfoMock =
			drive == null
				? Storage.MainDrive
				: Storage.GetOrAddDrive(drive);
		driveCallback?.Invoke(driveInfoMock);
		return this;
	}

	/// <summary>
	///     Registers a new <see cref="IFileVersionInfo" /> with values from
	///     the <paramref name="fileVersionInfoBuilder" /> returned for
	///     all files matching the <paramref name="globPattern" />.
	/// </summary>
	public MockFileSystem WithFileVersionInfo(string globPattern,
		Action<FileVersionInfoBuilder> fileVersionInfoBuilder)
	{
		FileVersionInfoBuilder builder = new();
		fileVersionInfoBuilder(builder);
		FileVersionInfoContainer container = builder.Create();
		_storage.AddFileVersion(globPattern, container);
		return this;
	}

	/// <summary>
	///     Registers the strategy how to deal with <see cref="SafeFileHandle" />s in the <see cref="MockFileSystem" />.
	///     <para />
	///     Defaults to <see cref="NullSafeFileHandleStrategy" />, if nothing is provided.
	/// </summary>
	public MockFileSystem WithSafeFileHandleStrategy(
		ISafeFileHandleStrategy safeFileHandleStrategy)
	{
		SafeFileHandleStrategy = safeFileHandleStrategy;
		return this;
	}

	private void InitializeFileSystem(MockFileSystemOptions initialization)
	{
		try
		{
			if (initialization.CurrentDirectory != null)
			{
				IDirectoryInfo directoryInfo = DirectoryInfo.New(initialization.CurrentDirectory);
				Storage.CurrentDirectory = directoryInfo.FullName;
			}

			string? root = Execute.Path.GetPathRoot(Directory.GetCurrentDirectory());
			Storage.GetOrAddDrive(root);
		}
		catch (IOException)
		{
			// Ignore any IOException, when trying to read the current directory
			// due to brittle tests on macOS
		}
	}

	/// <summary>
	///     The initialization options for the <see cref="MockFileSystem" />.
	/// </summary>
	public class MockFileSystemOptions
	{
		/// <summary>
		///     The current directory.
		/// </summary>
		internal string? CurrentDirectory { get; private set; }

		/// <summary>
		///     The <see cref="IRandomProvider" /> for the <see cref="RandomSystem" />.
		/// </summary>
		internal IRandomProvider? RandomProvider { get; private set; }

		/// <summary>
		///     The simulated operating system.
		/// </summary>
		internal SimulationMode SimulationMode { get; private set; } = SimulationMode.Native;

#if CAN_SIMULATE_OTHER_OS
		/// <summary>
		///     Specify the operating system that should be simulated.
		/// </summary>
		public MockFileSystemOptions SimulatingOperatingSystem(SimulationMode simulationMode)
		{
			SimulationMode = simulationMode;
			return this;
		}
#endif

		/// <summary>
		///     Use the provided <paramref name="path" /> as current directory.
		/// </summary>
		public MockFileSystemOptions UseCurrentDirectory(string path)
		{
			CurrentDirectory = path;
			return this;
		}

		/// <summary>
		///     Use <see cref="Directory.GetCurrentDirectory()" /> as current directory.
		/// </summary>
		public MockFileSystemOptions UseCurrentDirectory()
		{
			CurrentDirectory = System.IO.Directory.GetCurrentDirectory();
			return this;
		}

		/// <summary>
		///     Use the given <paramref name="randomProvider" /> for the <see cref="RandomSystem" />.
		/// </summary>
		public MockFileSystemOptions UseRandomProvider(IRandomProvider randomProvider)
		{
			RandomProvider = randomProvider;
			return this;
		}
	}
}
