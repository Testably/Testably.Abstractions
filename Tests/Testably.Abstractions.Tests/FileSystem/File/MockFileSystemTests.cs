using System.Linq;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Testing.FileSystemInitializer;

namespace Testably.Abstractions.Tests.FileSystem.File;

public sealed class MockFileSystemTests
	: FileSystemFileTests<MockFileSystem>, IDisposable
{
	/// <inheritdoc cref="FileSystemFileTests{TFileSystem}.BasePath" />
	public override string BasePath => _directoryCleaner.BasePath;

	private readonly IDirectoryCleaner _directoryCleaner;

	public MockFileSystemTests() : this(new MockFileSystem())
	{
	}

	private MockFileSystemTests(MockFileSystem mockFileSystem) : base(
		mockFileSystem,
		mockFileSystem.TimeSystem)
	{
		_directoryCleaner = FileSystem
		   .SetCurrentDirectoryToEmptyTemporaryDirectory();
	}

	#region IDisposable Members

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _directoryCleaner.Dispose();

	#endregion

	[SkippableTheory]
	[AutoData]
	public void Move_DifferentDrive_ShouldAdjustAvailableFreeSpace(
		string sourceName,
		string destinationName,
		byte[] bytes)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Different drives are only supported on Windows.");

		FileSystem.WithDrive("D").WithDrive("E");
		IDriveInfo[] drives = FileSystem.DriveInfo.GetDrives();
		IDriveInfo drive1 = drives.First();
		IDriveInfo drive2 = drives.Last();
		string sourcePath = FileSystem.Path.Combine(drive1.Name, sourceName);
		string destinationPath =
			FileSystem.Path.Combine(drive2.Name, destinationName);
		FileSystem.File.WriteAllBytes(sourcePath, bytes);
		long drive1AvailableBytes = drive1.AvailableFreeSpace;
		long drive2AvailableBytes = drive2.AvailableFreeSpace;

		FileSystem.File.Move(sourcePath, destinationPath);

		drive1.AvailableFreeSpace.Should().Be(drive1AvailableBytes + bytes.Length);
		drive2.AvailableFreeSpace.Should().Be(drive2AvailableBytes - bytes.Length);
		FileSystem.File.Exists(sourcePath).Should().BeFalse();
		FileSystem.File.Exists(destinationPath).Should().BeTrue();
		FileSystem.File.ReadAllBytes(destinationPath).Should().BeEquivalentTo(bytes);
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	[SkippableTheory]
	[AutoData]
	public void Move_Overwrite_ShouldAdjustAvailableFreeSpaceCorrectly(
		string sourceName,
		string destinationName,
		int bytes1Length,
		int bytes2Length)
	{
		RealRandomSystem randomSystem = new();
		byte[] bytes1 = new byte[bytes1Length];
		byte[] bytes2 = new byte[bytes2Length];
		randomSystem.Random.Shared.NextBytes(bytes1);
		randomSystem.Random.Shared.NextBytes(bytes2);

		FileSystem.File.WriteAllBytes(sourceName, bytes1);
		FileSystem.File.WriteAllBytes(destinationName, bytes2);
		IDriveInfo drive = FileSystem.DriveInfo.GetDrives().First();
		long previousAvailableBytes = drive.AvailableFreeSpace;

		FileSystem.File.Move(sourceName, destinationName, true);

		drive.AvailableFreeSpace.Should()
		   .Be(previousAvailableBytes + bytes2Length);
		FileSystem.File.Exists(sourceName).Should().BeFalse();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllBytes(destinationName).Should().BeEquivalentTo(bytes1);
	}
#endif
}