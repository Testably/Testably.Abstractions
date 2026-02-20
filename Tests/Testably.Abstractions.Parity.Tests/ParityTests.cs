using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.RandomSystem;
using Testably.Abstractions.TimeSystem;
using ITimer = Testably.Abstractions.TimeSystem.ITimer;

namespace Testably.Abstractions.Parity.Tests;

public abstract class ParityTests(
	TestHelpers.Parity parity,
	ITestOutputHelper testOutputHelper)
{
	#region Test Setup

	public TestHelpers.Parity Parity { get; } = parity;

	#endregion

	[Fact]
	public async Task IDirectory_EnsureParityWith_Directory()
	{
		List<string> parityErrors = Parity.Directory
			.GetErrorsToStaticType<IDirectory>(
				typeof(Directory),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task IDirectoryInfoAndIDirectoryInfoFactory_EnsureParityWith_DirectoryInfo()
	{
		List<string> parityErrors = Parity.DirectoryInfo
			.GetErrorsToInstanceType<IDirectoryInfo, IDirectoryInfoFactory>(
				typeof(DirectoryInfo),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task IDriveInfoAndIDriveInfoFactory_EnsureParityWith_DriveInfo()
	{
		List<string> parityErrors = Parity.Drive
			.GetErrorsToInstanceType<IDriveInfo, IDriveInfoFactory>(
				typeof(DriveInfo),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task IFile_EnsureParityWith_File()
	{
		List<string> parityErrors = Parity.File
			.GetErrorsToStaticType<IFile>(
				typeof(File),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task IFileInfoAndIFileInfoFactory_EnsureParityWith_FileInfo()
	{
		List<string> parityErrors = Parity.FileInfo
			.GetErrorsToInstanceType<IFileInfo, IFileInfoFactory>(
				typeof(FileInfo),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task IFileSystemInfo_EnsureParityWith_FileSystemInfo()
	{
		List<string> parityErrors = Parity.FileSystemInfo
			.GetErrorsToInstanceType<IFileSystemInfo>(
				typeof(FileSystemInfo),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task
		IFileSystemWatcherAndIFileSystemWatcherFactory_EnsureParityWith_FileSystemWatcher()
	{
		List<string> parityErrors = Parity.FileSystemWatcher
			.GetErrorsToInstanceType<IFileSystemWatcher, IFileSystemWatcherFactory>(
				typeof(FileSystemWatcher),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task
		IFileVersionInfoAndIFileVersionInfoFactory_EnsureParityWith_FileVersionInfo()
	{
		List<string> parityErrors = Parity.FileVersionInfo
			.GetErrorsToInstanceType<IFileVersionInfo, IFileVersionInfoFactory>(
				typeof(FileVersionInfo),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task IGuid_EnsureParityWith_Guid()
	{
		List<string> parityErrors = Parity.Guid
			.GetErrorsToStaticType<IGuid>(
				typeof(Guid),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task IPath_EnsureParityWith_Path()
	{
		List<string> parityErrors = Parity.Path
			.GetErrorsToStaticType<IPath>(
				typeof(Path),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task IRandomAndIRandomFactory_EnsureParityWith_Random()
	{
		List<string> parityErrors = Parity.Random
			.GetErrorsToInstanceType<IRandom, IRandomFactory>(
				typeof(Random),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task
		ITimerAndITimerFactory_EnsureParityWith_Timer()
	{
		List<string> parityErrors = Parity.Timer
			.GetErrorsToInstanceType<ITimer, ITimerFactory>(
				typeof(Timer),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task
		IStopwatchAndIStopwatchFactory_EnsureParityWith_Stopwatch()
	{
		List<string> parityErrors = Parity.Stopwatch
			.GetErrorsToInstanceType<IStopwatch, IStopwatchFactory>(
				typeof(Stopwatch),
				testOutputHelper);
		parityErrors.AddRange(Parity.Stopwatch
			.GetErrorsToStaticType<IStopwatchFactory>(
				typeof(Stopwatch),
				testOutputHelper));

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task IZipArchive_EnsureParityWith_ZipArchive()
	{
		List<string> parityErrors = Parity.ZipArchive
			.GetErrorsToInstanceType<IZipArchive>(
				typeof(ZipArchive),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task IZipArchive_EnsureParityWith_ZipFileExtensions()
	{
		List<string> parityErrors = Parity.ZipArchive
			.GetErrorsToExtensionMethods<IZipArchive>(
				typeof(ZipFileExtensions),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task IZipArchiveEntry_EnsureParityWith_ZipArchiveEntry()
	{
		List<string> parityErrors = Parity.ZipArchiveEntry
			.GetErrorsToInstanceType<IZipArchiveEntry>(
				typeof(ZipArchiveEntry),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task IZipArchiveEntry_EnsureParityWith_ZipFileExtensions()
	{
		List<string> parityErrors = Parity.ZipArchiveEntry
			.GetErrorsToExtensionMethods<IZipArchiveEntry>(
				typeof(ZipFileExtensions),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}

	[Fact]
	public async Task IZipFile_EnsureParityWith_ZipFile()
	{
		List<string> parityErrors = Parity.ZipFile
			.GetErrorsToStaticType<IZipFile>(
				typeof(ZipFile),
				testOutputHelper);

		await That(parityErrors).IsEmpty();
	}
}
