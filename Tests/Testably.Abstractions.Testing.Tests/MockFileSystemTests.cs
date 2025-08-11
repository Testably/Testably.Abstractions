using System.IO;
using System.Linq;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Helpers;
#if NET6_0_OR_GREATER
using Microsoft.Win32.SafeHandles;
#endif

namespace Testably.Abstractions.Testing.Tests;

public class MockFileSystemTests
{
	[Theory]
	[AutoData]
	public async Task FileSystemMock_File_Decrypt(string path, string contents)
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText(path, contents);
		#pragma warning disable CA1416
		sut.File.Encrypt(path);
		#pragma warning restore CA1416

		#pragma warning disable CA1416
		sut.File.Decrypt(path);
		#pragma warning restore CA1416

		await That(sut.File.ReadAllText(path)).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task FileSystemMock_File_Encrypt(string path, string contents)
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText(path, contents);

		#pragma warning disable CA1416
		sut.File.Encrypt(path);
		#pragma warning restore CA1416

		await That(sut.File.ReadAllText(path)).IsNotEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task FileSystemMock_FileInfo_Decrypt(string path, string contents)
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText(path, contents);
		#pragma warning disable CA1416
		sut.FileInfo.New(path).Encrypt();
		#pragma warning restore CA1416

		#pragma warning disable CA1416
		sut.FileInfo.New(path).Decrypt();
		#pragma warning restore CA1416

		await That(sut.File.ReadAllText(path)).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task FileSystemMock_FileInfo_Encrypt(string path, string contents)
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText(path, contents);

		#pragma warning disable CA1416
		sut.FileInfo.New(path).Encrypt();
		#pragma warning restore CA1416

		await That(sut.File.ReadAllText(path)).IsNotEqualTo(contents);
	}

	[Fact]
	public async Task FileSystemMock_ShouldBeInitializedWithADefaultDrive()
	{
		MockFileSystem sut = new();
		string expectedDriveName = "".PrefixRoot(sut);

		IDriveInfo[] drives = sut.DriveInfo.GetDrives();
		IDriveInfo drive = sut.GetDefaultDrive();

		await That(drives).IsNotEmpty();
		await That(drive.Name).IsEqualTo(expectedDriveName);
		await That(drive.AvailableFreeSpace).IsGreaterThan(0);
		await That(drive.DriveFormat).IsEqualTo(DriveInfoMock.DefaultDriveFormat);
		await That(drive.DriveType).IsEqualTo(DriveInfoMock.DefaultDriveType);
		await That(drive.VolumeLabel).IsNotNullOrEmpty();
	}

	[Theory]
	[InlineData("A:\\")]
	[InlineData("G:\\")]
	[InlineData("z:\\")]
	public async Task FileSystemMock_ShouldInitializeDriveFromCurrentDirectory(string driveName)
	{
		Skip.If(!Test.RunsOnWindows);

		MockFileSystem sut = new(o => o.UseCurrentDirectory($"{driveName}foo\\bar"));

		IDriveInfo[] drives = sut.DriveInfo.GetDrives();
		await That(drives.Length).IsEqualTo(2);
		await That(drives).Contains(d => string.Equals(d.Name, "C:\\", StringComparison.Ordinal));
		await That(drives)
			.Contains(d => string.Equals(d.Name, driveName, StringComparison.Ordinal));
	}

	[Fact]
	public async Task ToString_ShouldContainStorageInformation()
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText("foo", "bar");

		string result = sut.ToString();

		await That(result).Contains("directories: 0, files: 1");
	}

	[Theory]
	[AutoData]
	public async Task WithAccessControl_Denied_CreateDirectoryShouldThrowUnauthorizedAccessException(
		string path)
	{
		Skip.If(!Test.RunsOnWindows);

		MockFileSystem sut = new();
		sut.Initialize();
		sut.WithAccessControlStrategy(new DefaultAccessControlStrategy((_, _) => false));

		Exception? exception = Record.Exception(() =>
		{
			sut.Directory.CreateDirectory(path);
		});

		await That(exception).IsExactly<UnauthorizedAccessException>();
	}

	[Theory]
	[AutoData]
	public async Task WithAccessControl_ShouldConsiderPath(
		string allowedPath, string deniedPath)
	{
		Skip.If(!Test.RunsOnWindows);

		MockFileSystem sut = new();
		sut.Initialize();
		sut.WithAccessControlStrategy(new DefaultAccessControlStrategy((p, _)
			=> !string.Equals(p, sut.Path.GetFullPath(deniedPath), StringComparison.Ordinal)));

		sut.Directory.CreateDirectory(allowedPath);
		Exception? exception = Record.Exception(() =>
		{
			sut.Directory.CreateDirectory(deniedPath);
		});

		await That(exception).IsExactly<UnauthorizedAccessException>();
	}

	[Fact]
	public async Task
		WithAccessControlStrategy_OutsideWindows_ShouldThrowPlatformNotSupportedException()
	{
		Skip.If(Test.RunsOnWindows);

		MockFileSystem sut = new();

		void Act()
		{
			sut.WithAccessControlStrategy(new DefaultAccessControlStrategy((_, _) => true));
		}

		await That(Act).Throws<PlatformNotSupportedException>()
			.WithMessage("Access control lists are only supported on Windows.");
	}

	[Theory]
	[InlineData("D:\\")]
	public async Task WithDrive_Duplicate_ShouldUpdateExistingDrive(string driveName)
	{
		Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");

		MockFileSystem sut = new();
		sut.WithDrive(driveName, d => d.SetTotalSize(100));
		await That(sut.DriveInfo.GetDrives().Length).IsEqualTo(2);
		IDriveInfo drive = sut.DriveInfo.GetDrives()
			.Single(x => string.Equals(x.Name, driveName, StringComparison.Ordinal));
		await That(drive.TotalSize).IsEqualTo(100);

		sut.WithDrive(driveName, d => d.SetTotalSize(200));
		await That(sut.DriveInfo.GetDrives().Length).IsEqualTo(2);
		await That(drive.TotalSize).IsEqualTo(200);
	}

	[Fact]
	public async Task WithDrive_ExistingName_ShouldUpdateDrive()
	{
		MockFileSystem sut = new();
		string driveName = "".PrefixRoot(sut);
		sut.WithDrive(driveName);

		IDriveInfo[] drives = sut.DriveInfo.GetDrives();

		await That(drives.Length).IsGreaterThanOrEqualTo(1);
		await That(drives).HasSingle()
			.Matching(d => string.Equals(d.Name, driveName, StringComparison.Ordinal));
	}

	[Theory]
	[InlineData("D:\\")]
	public async Task WithDrive_NewName_ShouldCreateNewDrives(string driveName)
	{
		Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");

		MockFileSystem sut = new();
		sut.WithDrive(driveName);

		IDriveInfo[] drives = sut.DriveInfo.GetDrives();

		await That(drives.Length).IsEqualTo(2);
		await That(drives).HasSingle()
			.Matching(d => string.Equals(d.Name, driveName, StringComparison.Ordinal));
	}

	[Theory]
	[InlineData("D")]
	[InlineData("D:")]
	public async Task WithDrive_ShouldHavePathSeparatorSuffix(string driveName)
	{
		Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");

		string expectedDriveName = $"D:{Path.DirectorySeparatorChar}";
		MockFileSystem sut = new();
		sut.WithDrive(driveName);

		IDriveInfo[] drives = sut.DriveInfo.GetDrives();

		await That(drives.Length).IsLessThanOrEqualTo(2);
		await That(drives).HasSingle().Matching(d
			=> string.Equals(d.Name, expectedDriveName, StringComparison.Ordinal));
	}

	[Theory]
	[AutoData]
	public async Task WithDrive_WithCallback_ShouldUpdateDrive(long totalSize)
	{
		MockFileSystem sut = new();
		sut.WithDrive(d => d.SetTotalSize(totalSize));

		IDriveInfo drive = sut.GetDefaultDrive();

		await That(drive.TotalSize).IsEqualTo(totalSize);
		await That(drive.TotalFreeSpace).IsEqualTo(totalSize);
		await That(drive.AvailableFreeSpace).IsEqualTo(totalSize);
	}

#if NET6_0_OR_GREATER
	[Theory]
	[AutoData]
	public async Task WithSafeFileHandleStrategy_DefaultStrategy_ShouldUseMappedSafeFileHandleMock(
		string path, string contents)
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText(path, contents);
		sut.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		using FileSystemStream stream =
			sut.FileStream.New(new SafeFileHandle(), FileAccess.Read);
		using StreamReader streamReader = new(stream);
		string result = streamReader.ReadToEnd();

		await That(result).IsEqualTo(contents);
	}
#endif

#if NET6_0_OR_GREATER
	[Theory]
	[AutoData]
	public async Task WithSafeFileHandleStrategy_NullStrategy_ShouldThrowException(
		string path, string contents)
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText(path, contents);

		void Act()
			=> sut.FileStream.New(new SafeFileHandle(), FileAccess.Read);

		await That(Act).ThrowsExactly<ArgumentException>().WithParamName("handle");
	}
#endif

	[Theory]
	[AutoData]
	public async Task WithUncDrive_ShouldCreateUncDrive(
		string path, string contents)
	{
		MockFileSystem sut = new();
		sut.WithUncDrive("UNC-Path");
		string fullPath = sut.Path.Combine("//UNC-Path", path);
		sut.File.WriteAllText(fullPath, contents);

		string result = sut.File.ReadAllText(fullPath);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WithUncDrive_ShouldNotBeIncludedInGetDrives(
		string server)
	{
		MockFileSystem sut = new();
		string uncPrefix = new(sut.Path.DirectorySeparatorChar, 2);
		string uncDrive = $"{uncPrefix}{server}";
		int expectedLogicalDrives = sut.Directory.GetLogicalDrives().Length;
		int expectedDrives = sut.DriveInfo.GetDrives().Length;

		sut.WithUncDrive(uncDrive);

		await That(sut.Directory.GetLogicalDrives().Length).IsEqualTo(expectedLogicalDrives);
		await That(sut.DriveInfo.GetDrives().Length).IsEqualTo(expectedDrives);
	}

	[Theory]
	[AutoData]
	public async Task WithUncDrive_WriteBytes_ShouldReduceAvailableFreeSpace(
		string server, string path, byte[] bytes)
	{
		MockFileSystem sut = new();
		string uncPrefix = new(sut.Path.DirectorySeparatorChar, 2);
		string uncDrive = $"{uncPrefix}{server}";
		sut.WithUncDrive(uncDrive);
		IDriveInfo drive = sut.DriveInfo.New(uncDrive);
		long previousFreeSpace = drive.AvailableFreeSpace;

		sut.File.WriteAllBytes(Path.Combine(uncDrive, path), bytes);

		await That(drive.AvailableFreeSpace).IsEqualTo(previousFreeSpace - bytes.Length);
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[Fact]
	public async Task
		WithUnixFileModeStrategy_OnWindows_ShouldThrowPlatformNotSupportedException()
	{
		Skip.If(!Test.RunsOnWindows);

		MockFileSystem sut = new();

		void Act()
		{
			sut.WithUnixFileModeStrategy(new DummyUnixFileModeStrategy());
		}

		await That(Act).Throws<PlatformNotSupportedException>()
			.WithMessage("Unix file modes are not supported on this platform.");
	}
#endif

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[Theory]
	[AutoData]
	public async Task WithUnixFileModeStrategy_ShouldConsiderPath(
		string allowedPath, string deniedPath)
	{
		Skip.If(Test.RunsOnWindows);

		MockFileSystem sut = new();
		sut.Initialize();
		sut.WithUnixFileModeStrategy(new DummyUnixFileModeStrategy(p =>
			!p.EndsWith(deniedPath, StringComparison.Ordinal)));

		sut.Directory.CreateDirectory(allowedPath);
		Exception? exception = Record.Exception(() =>
		{
			sut.Directory.CreateDirectory(deniedPath);
		});

		await That(exception).IsExactly<UnauthorizedAccessException>();
	}
#endif

	[Theory]
	[AutoData]
	public async Task WriteAllText_OnUncPath_ShouldThrowDirectoryNotFoundException(
		string path, string contents)
	{
		MockFileSystem sut = new();
		string fullPath = sut.Path.Combine("//UNC-Path", path);
		Exception? exception = Record.Exception(() =>
		{
			sut.File.WriteAllText(fullPath, contents);
		});

		await That(exception).IsExactly<DirectoryNotFoundException>();
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	private sealed class DummyUnixFileModeStrategy(Func<string, bool>? pathPredicate = null)
		: IUnixFileModeStrategy
	{
		#region IUnixFileModeStrategy Members

		/// <inheritdoc />
		public bool IsAccessGranted(string fullPath, IFileSystemExtensibility extensibility,
			UnixFileMode mode,
			FileAccess requestedAccess)
			=> pathPredicate?.Invoke(fullPath) ?? false;

		/// <inheritdoc />
		public void OnSetUnixFileMode(string fullPath, IFileSystemExtensibility extensibility,
			UnixFileMode mode)
		{
			// Do nothing
		}

		#endregion
	}
#endif
}
