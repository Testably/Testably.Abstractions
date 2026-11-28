using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public class EnableRaisingEventsTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task EnableRaisingEvents_SetToFalse_ShouldStop(string path1, string path2)
	{
		FileSystem.Initialize().WithSubdirectory(path1).WithSubdirectory(path2);
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Deleted += (_, _) =>
		{
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
		fileSystemWatcher.EnableRaisingEvents = true;
		FileSystem.Directory.Delete(path1);
		await That(ms.Wait(ExpectSuccess, CancellationToken)).IsTrue();
		ms.Reset();

		fileSystemWatcher.EnableRaisingEvents = false;

		FileSystem.Directory.Delete(path2);
		await That(ms.Wait(ExpectTimeout, CancellationToken)).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task EnableRaisingEvents_ShouldBeInitializedAsFalse(string path)
	{
		FileSystem.Initialize().WithSubdirectory(path);
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Deleted += (_, _) =>
		{
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

		FileSystem.Directory.Delete(path);

		await That(ms.Wait(ExpectTimeout, CancellationToken)).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task EnableRaisingEvents_WithoutPath_ShouldNotRaiseEvents(string path)
	{
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New();
		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.Created += (_, _) =>
		{
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

		_ = Record.Exception(() => fileSystemWatcher.EnableRaisingEvents = true);
		FileSystem.Directory.CreateDirectory(path);

		await That(ms.Wait(ExpectTimeout, CancellationToken)).IsFalse()
			.Because("a watcher without a path never watches anything");
	}

#if FEATURE_FILESYSTEMWATCHER_LINUX_THROWS_WITHOUT_PATH
	[Test]
	public async Task EnableRaisingEvents_WithoutPath_OnLinux_ShouldThrowArgumentException()
	{
		Skip.IfNot(Test.RunsOnLinux);

		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			fileSystemWatcher.EnableRaisingEvents = true;
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName("path");
		await That(fileSystemWatcher.EnableRaisingEvents).IsFalse()
			.Because("since .NET 11 the inotify watcher resolves the full path before it marks the watcher as enabled");
	}
#else
	[Test]
	public async Task EnableRaisingEvents_WithoutPath_OnLinux_ShouldRaiseErrorAndStayEnabled()
	{
		Skip.IfNot(Test.RunsOnLinux);

		List<Exception> errors = [];
		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New();
		fileSystemWatcher.Error += (_, e) => errors.Add(e.GetException());

		fileSystemWatcher.EnableRaisingEvents = true;
		fileSystemWatcher.EnableRaisingEvents = true;

		await That(errors).HasSingle()
			.Because("inotify_add_watch fails synchronously for the empty path and enabling an enabled watcher again does nothing");
		await That(errors[0]).IsExactly<DirectoryNotFoundException>();
		await That(errors[0].HResult).IsEqualTo(-2147024893);
		await That(fileSystemWatcher.EnableRaisingEvents).IsTrue();
	}

	[Test]
	[AutoArguments]
	public async Task EnableRaisingEvents_WithoutPath_SetPathAfterwards_ShouldStartListening(
		string path)
	{
		Skip.IfNot(Test.RunsOnLinux || Test.IsNetFramework);

		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New();
		fileSystemWatcher.Created += (_, _) =>
		{
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
		_ = Record.Exception(() => fileSystemWatcher.EnableRaisingEvents = true);

		fileSystemWatcher.Path = BasePath;
		FileSystem.Directory.CreateDirectory(path);

		await That(ms.Wait(ExpectSuccess, CancellationToken)).IsTrue()
			.Because("the failed start leaves the watcher enabled, so setting the path restarts it");
	}
#endif

	[Test]
	public async Task EnableRaisingEvents_WithoutPath_OnMac_ShouldThrowArgumentException()
	{
		Skip.IfNot(Test.RunsOnMac);

		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			fileSystemWatcher.EnableRaisingEvents = true;
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName("path");
		await That(fileSystemWatcher.EnableRaisingEvents).IsFalse();
	}

	[Test]
	public async Task EnableRaisingEvents_WithoutPath_OnNetFramework_ShouldThrowArgumentException()
	{
		Skip.IfNot(Test.IsNetFramework);

		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			fileSystemWatcher.EnableRaisingEvents = true;
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809);
		await That(fileSystemWatcher.EnableRaisingEvents).IsTrue()
			.Because("the .NET Framework sets the flag before it fails to start");
		await That(Act).DoesNotThrow()
			.Because("enabling an enabled watcher again does nothing");
	}

	[Test]
	public async Task EnableRaisingEvents_WithoutPath_OnWindows_ShouldThrowFileNotFoundException()
	{
		Skip.IfNot(Test.RunsOnWindows && !Test.IsNetFramework);

		using IFileSystemWatcher fileSystemWatcher = FileSystem.FileSystemWatcher.New();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			fileSystemWatcher.EnableRaisingEvents = true;
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithHResult(-2147024894).And
			.WithMessage("Error reading the  directory.");
		await That(fileSystemWatcher.EnableRaisingEvents).IsFalse();
	}
}
