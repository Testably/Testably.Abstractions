using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.RandomSystem;
using Testably.Abstractions.TimeSystem;
using ITimer = Testably.Abstractions.TimeSystem.ITimer;

namespace Testably.Abstractions.Parity.Tests;

public abstract class ParityTests(
	TestHelpers.Parity parity)
{
	public TestHelpers.Parity Parity { get; } = parity;

	[Test]
	public async Task IDirectory_EnsureParityWith_Directory()
	{
		List<string> parityErrors = Parity.Directory
			.GetErrorsToStaticType<IDirectory>(
				typeof(Directory));

		await That(parityErrors).IsEmpty();
	}

	[Test]
	public async Task IDirectoryInfoAndIDirectoryInfoFactory_EnsureParityWith_DirectoryInfo()
	{
		List<string> parityErrors = Parity.DirectoryInfo
			.GetErrorsToInstanceType<IDirectoryInfo, IDirectoryInfoFactory>(
				typeof(DirectoryInfo));

		await That(parityErrors).IsEmpty();
	}

	[Test]
	public async Task IDriveInfoAndIDriveInfoFactory_EnsureParityWith_DriveInfo()
	{
		List<string> parityErrors = Parity.Drive
			.GetErrorsToInstanceType<IDriveInfo, IDriveInfoFactory>(
				typeof(DriveInfo));

		await That(parityErrors).IsEmpty();
	}

	[Test]
	public async Task IFile_EnsureParityWith_File()
	{
		List<string> parityErrors = Parity.File
			.GetErrorsToStaticType<IFile>(
				typeof(File));

		await That(parityErrors).IsEmpty();
	}

	[Test]
	public async Task IFileInfoAndIFileInfoFactory_EnsureParityWith_FileInfo()
	{
		List<string> parityErrors = Parity.FileInfo
			.GetErrorsToInstanceType<IFileInfo, IFileInfoFactory>(
				typeof(FileInfo));

		await That(parityErrors).IsEmpty();
	}

	[Test]
	public async Task IFileSystemInfo_EnsureParityWith_FileSystemInfo()
	{
		List<string> parityErrors = Parity.FileSystemInfo
			.GetErrorsToInstanceType<IFileSystemInfo>(
				typeof(FileSystemInfo));

		await That(parityErrors).IsEmpty();
	}

	[Test]
	public async Task
		IFileSystemWatcherAndIFileSystemWatcherFactory_EnsureParityWith_FileSystemWatcher()
	{
		List<string> parityErrors = Parity.FileSystemWatcher
			.GetErrorsToInstanceType<IFileSystemWatcher, IFileSystemWatcherFactory>(
				typeof(FileSystemWatcher));

		await That(parityErrors).IsEmpty();
	}

	[Test]
	public async Task
		IFileVersionInfoAndIFileVersionInfoFactory_EnsureParityWith_FileVersionInfo()
	{
		List<string> parityErrors = Parity.FileVersionInfo
			.GetErrorsToInstanceType<IFileVersionInfo, IFileVersionInfoFactory>(
				typeof(FileVersionInfo));

		await That(parityErrors).IsEmpty();
	}

	[Test]
	public async Task IGuid_EnsureParityWith_Guid()
	{
		List<string> parityErrors = Parity.Guid
			.GetErrorsToStaticType<IGuid>(
				typeof(Guid));

		await That(parityErrors).IsEmpty();
	}

	[Test]
	public async Task IPath_EnsureParityWith_Path()
	{
		List<string> parityErrors = Parity.Path
			.GetErrorsToStaticType<IPath>(
				typeof(Path));

		await That(parityErrors).IsEmpty();
	}

#if FEATURE_PERIODIC_TIMER
	[Test]
	public async Task
		IPeriodicTimerAndIPeriodicTimerFactory_EnsureParityWith_PeriodicTimer()
	{
		List<string> parityErrors = Parity.PeriodicTimer
			.GetErrorsToInstanceType<IPeriodicTimer, IPeriodicTimerFactory>(
				typeof(PeriodicTimer));

		await That(parityErrors).IsEmpty();
	}
#endif

	[Test]
	public async Task IRandomAndIRandomFactory_EnsureParityWith_Random()
	{
		List<string> parityErrors = Parity.Random
			.GetErrorsToInstanceType<IRandom, IRandomFactory>(
				typeof(Random));

		await That(parityErrors).IsEmpty();
	}

	[Test]
	public async Task
		IStopwatchAndIStopwatchFactory_EnsureParityWith_Stopwatch()
	{
		List<string> parityErrors = Parity.Stopwatch
			.GetErrorsToInstanceType<IStopwatch, IStopwatchFactory>(
				typeof(Stopwatch));
		parityErrors.AddRange(Parity.Stopwatch
			.GetErrorsToStaticType<IStopwatchFactory>(
				typeof(Stopwatch)));

		await That(parityErrors).IsEmpty();
	}

	[Test]
	public async Task
		ITimerAndITimerFactory_EnsureParityWith_Timer()
	{
		List<string> parityErrors = Parity.Timer
			.GetErrorsToInstanceType<ITimer, ITimerFactory>(
				typeof(Timer));

		await That(parityErrors).IsEmpty();
	}

	[Test]
	public async Task IZipArchive_EnsureParityWith_ZipArchive()
	{
		List<string> parityErrors = Parity.ZipArchive
			.GetErrorsToInstanceType<IZipArchive>(
				typeof(ZipArchive));

		await That(parityErrors).IsEmpty();
	}

	[Test]
	public async Task IZipArchive_EnsureParityWith_ZipFileExtensions()
	{
		List<string> parityErrors = Parity.ZipArchive
			.GetErrorsToExtensionMethods<IZipArchive>(
				typeof(ZipFileExtensions));

		await That(parityErrors).IsEmpty();
	}

	[Test]
	public async Task IZipArchiveEntry_EnsureParityWith_ZipArchiveEntry()
	{
		List<string> parityErrors = Parity.ZipArchiveEntry
			.GetErrorsToInstanceType<IZipArchiveEntry>(
				typeof(ZipArchiveEntry));

		await That(parityErrors).IsEmpty();
	}

	[Test]
	public async Task IZipArchiveEntry_EnsureParityWith_ZipFileExtensions()
	{
		List<string> parityErrors = Parity.ZipArchiveEntry
			.GetErrorsToExtensionMethods<IZipArchiveEntry>(
				typeof(ZipFileExtensions));

		await That(parityErrors).IsEmpty();
	}

	[Test]
	public async Task IZipFile_EnsureParityWith_ZipFile()
	{
		List<string> parityErrors = Parity.ZipFile
			.GetErrorsToStaticType<IZipFile>(
				typeof(ZipFile));

		await That(parityErrors).IsEmpty();
	}
}
