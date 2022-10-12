﻿using System.Collections.Generic;
using System.IO;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.Parity;

public abstract partial class ParityTests
{
	#region Test Setup

	public Parity Parity { get; }

	private readonly ITestOutputHelper _testOutputHelper;

	protected ParityTests(Parity parity,
	                      ITestOutputHelper testOutputHelper)
	{
		Parity = parity;
		_testOutputHelper = testOutputHelper;
	}

	#endregion

	[Fact]
	[Trait(nameof(Testing), nameof(Parity))]
	public void IDirectory_EnsureParityWith_Directory()
	{
		List<string> parityErrors = Parity.Directory
		   .GetErrorsToStaticType<IFileSystem.IDirectory>(
				typeof(Directory),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	[Trait(nameof(Testing), nameof(Parity))]
	public void IDirectoryInfoAndIDirectoryInfoFactory_EnsureParityWith_DirectoryInfo()
	{
		List<string> parityErrors = Parity.DirectoryInfo
		   .GetErrorsToInstanceType<IFileSystem.IDirectoryInfo,
				IFileSystem.IDirectoryInfoFactory>(
				typeof(DirectoryInfo),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	[Trait(nameof(Testing), nameof(Parity))]
	public void IDriveInfoAndIDriveInfoFactory_EnsureParityWith_DriveInfo()
	{
		List<string> parityErrors = Parity.Drive
		   .GetErrorsToInstanceType<IFileSystem.IDriveInfo,
				IFileSystem.IDriveInfoFactory>(
				typeof(DriveInfo),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	[Trait(nameof(Testing), nameof(Parity))]
	public void IFile_EnsureParityWith_File()
	{
		List<string> parityErrors = Parity.File
		   .GetErrorsToStaticType<IFileSystem.IFile>(
				typeof(File),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	[Trait(nameof(Testing), nameof(Parity))]
	public void IFileInfoAndIFileInfoFactory_EnsureParityWith_FileInfo()
	{
		List<string> parityErrors = Parity.FileInfo
		   .GetErrorsToInstanceType<IFileSystem.IFileInfo,
				IFileSystem.IFileInfoFactory>(
				typeof(FileInfo),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	[Trait(nameof(Testing), nameof(Parity))]
	public void IFileSystemInfo_EnsureParityWith_FileSystemInfo()
	{
		List<string> parityErrors = Parity.FileSystemInfo
		   .GetErrorsToInstanceType<IFileSystem.IFileSystemInfo>(
				typeof(FileSystemInfo),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	[Trait(nameof(Testing), nameof(Parity))]
	public void IFileSystemWatcherAndIFileSystemWatcherFactory_EnsureParityWith_FileSystemWatcher()
	{
		List<string> parityErrors = Parity.FileSystemWatcher
		   .GetErrorsToInstanceType<IFileSystem.IFileSystemWatcher,
				IFileSystem.IFileSystemWatcherFactory>(
				typeof(FileSystemWatcher),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	[Trait(nameof(Testing), nameof(Parity))]
	public void IGuid_EnsureParityWith_Guid()
	{
		List<string> parityErrors = Parity.Guid
		   .GetErrorsToStaticType<IRandomSystem.IGuid>(
				typeof(Guid),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	[Trait(nameof(Testing), nameof(Parity))]
	public void IPath_EnsureParityWith_Path()
	{
		List<string> parityErrors = Parity.Path
		   .GetErrorsToStaticType<IFileSystem.IPath>(
				typeof(Path),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}

	[Fact]
	[Trait(nameof(Testing), nameof(Parity))]
	public void IRandomAndIRandomFactory_EnsureParityWith_Random()
	{
		List<string> parityErrors = Parity.Random
		   .GetErrorsToInstanceType<IRandomSystem.IRandom,
				IRandomSystem.IRandomFactory>(
				typeof(Random),
				_testOutputHelper);

		parityErrors.Should().BeEmpty();
	}
}