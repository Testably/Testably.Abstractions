using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Testably.Abstractions.RandomSystem;
using Testably.Abstractions.TimeSystem;
using Xunit.Abstractions;

namespace Testably.Abstractions.Parity.Tests;

public abstract class ParityTests
{
	#region Test Setup

	public TestHelpers.Parity Parity { get; }

	private readonly ITestOutputHelper _testOutputHelper;

	protected ParityTests(TestHelpers.Parity parity,
		ITestOutputHelper testOutputHelper)
	{
		Parity = parity;
		_testOutputHelper = testOutputHelper;
	}

	#endregion

	[Fact]
	public void IDirectory_EnsureParityWith_Directory()
	{
		List<string> parityErrors = Parity.Directory
			.GetErrorsToStaticType<IDirectory>(
				typeof(Directory),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	public void IDirectoryInfoAndIDirectoryInfoFactory_EnsureParityWith_DirectoryInfo()
	{
		List<string> parityErrors = Parity.DirectoryInfo
			.GetErrorsToInstanceType<IDirectoryInfo, IDirectoryInfoFactory>(
				typeof(DirectoryInfo),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	public void IDriveInfoAndIDriveInfoFactory_EnsureParityWith_DriveInfo()
	{
		List<string> parityErrors = Parity.Drive
			.GetErrorsToInstanceType<IDriveInfo, IDriveInfoFactory>(
				typeof(DriveInfo),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	public void IFile_EnsureParityWith_File()
	{
		List<string> parityErrors = Parity.File
			.GetErrorsToStaticType<IFile>(
				typeof(File),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	public void IFileInfoAndIFileInfoFactory_EnsureParityWith_FileInfo()
	{
		List<string> parityErrors = Parity.FileInfo
			.GetErrorsToInstanceType<IFileInfo, IFileInfoFactory>(
				typeof(FileInfo),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	public void IFileSystemInfo_EnsureParityWith_FileSystemInfo()
	{
		List<string> parityErrors = Parity.FileSystemInfo
			.GetErrorsToInstanceType<IFileSystemInfo>(
				typeof(FileSystemInfo),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	public void
		IFileSystemWatcherAndIFileSystemWatcherFactory_EnsureParityWith_FileSystemWatcher()
	{
		List<string> parityErrors = Parity.FileSystemWatcher
			.GetErrorsToInstanceType<IFileSystemWatcher, IFileSystemWatcherFactory>(
				typeof(FileSystemWatcher),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	public void IGuid_EnsureParityWith_Guid()
	{
		List<string> parityErrors = Parity.Guid
			.GetErrorsToStaticType<IGuid>(
				typeof(Guid),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	public void IPath_EnsureParityWith_Path()
	{
		List<string> parityErrors = Parity.Path
			.GetErrorsToStaticType<IPath>(
				typeof(Path),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	public void IRandomAndIRandomFactory_EnsureParityWith_Random()
	{
		List<string> parityErrors = Parity.Random
			.GetErrorsToInstanceType<IRandom, IRandomFactory>(
				typeof(Random),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	public void IZipArchive_EnsureParityWith_ZipArchive()
	{
		List<string> parityErrors = Parity.ZipArchive
			.GetErrorsToInstanceType<IZipArchive>(
				typeof(ZipArchive),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	public void IZipArchive_EnsureParityWith_ZipFileExtensions()
	{
		List<string> parityErrors = Parity.ZipArchive
			.GetErrorsToExtensionMethods<IZipArchive>(
				typeof(ZipFileExtensions),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	public void IZipArchiveEntry_EnsureParityWith_ZipArchiveEntry()
	{
		List<string> parityErrors = Parity.ZipArchiveEntry
			.GetErrorsToInstanceType<IZipArchiveEntry>(
				typeof(ZipArchiveEntry),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	public void IZipArchiveEntry_EnsureParityWith_ZipFileExtensions()
	{
		List<string> parityErrors = Parity.ZipArchiveEntry
			.GetErrorsToExtensionMethods<IZipArchiveEntry>(
				typeof(ZipFileExtensions),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	public void IZipFile_EnsureParityWith_ZipFile()
	{
		List<string> parityErrors = Parity.ZipFile
			.GetErrorsToStaticType<IZipFile>(
				typeof(ZipFile),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}
}
