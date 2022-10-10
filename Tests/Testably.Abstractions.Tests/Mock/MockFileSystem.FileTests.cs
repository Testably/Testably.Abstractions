using System.Linq;
using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Mock;

public static partial class MockFileSystem
{
    // ReSharper disable once UnusedMember.Global
    [SystemTest(nameof(MockFileSystem))]
    public sealed class FileTests
        : FileSystemFileTests<FileSystemMock>, IDisposable
    {
        /// <inheritdoc cref="FileSystemFileTests{TFileSystem}.BasePath" />
        public override string BasePath => _directoryCleaner.BasePath;

        private readonly FileSystemInitializer.IDirectoryCleaner _directoryCleaner;

        public FileTests() : this(new FileSystemMock())
        {
        }

        private FileTests(FileSystemMock fileSystemMock) : base(
            fileSystemMock,
            fileSystemMock.TimeSystem)
        {
            _directoryCleaner = FileSystem
               .SetCurrentDirectoryToEmptyTemporaryDirectory();
        }

        [SkippableTheory]
        [AutoData]
        [FileSystemTests.File(nameof(IFileSystem.IFile.Move))]
        public void Move_DifferentDrive_ShouldAdjustAvailableFreeSpace(
            string sourceName,
            string destinationName,
            byte[] bytes)
        {
            Skip.IfNot(Test.RunsOnWindows,
                "Different drives are only supported on Windows.");

            FileSystem.WithDrive("D").WithDrive("E");
            IFileSystem.IDriveInfo[] drives = FileSystem.DriveInfo.GetDrives();
            IFileSystem.IDriveInfo drive1 = drives.First();
            IFileSystem.IDriveInfo drive2 = drives.Last();
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
        [FileSystemTests.File(nameof(IFileSystem.IFile.Move))]
        public void Move_Overwrite_ShouldAdjustAvailableFreeSpaceCorrectly(
            string sourceName,
            string destinationName,
            int bytes1Length,
            int bytes2Length)
        {
            RandomSystem randomSystem = new();
            byte[] bytes1 = new byte[bytes1Length];
            byte[] bytes2 = new byte[bytes2Length];
            randomSystem.Random.Shared.NextBytes(bytes1);
            randomSystem.Random.Shared.NextBytes(bytes2);

            FileSystem.File.WriteAllBytes(sourceName, bytes1);
            FileSystem.File.WriteAllBytes(destinationName, bytes2);
            IFileSystem.IDriveInfo drive = FileSystem.DriveInfo.GetDrives().First();
            long previousAvailableBytes = drive.AvailableFreeSpace;

            FileSystem.File.Move(sourceName, destinationName, true);

            drive.AvailableFreeSpace.Should()
               .Be(previousAvailableBytes + bytes2Length);
            FileSystem.File.Exists(sourceName).Should().BeFalse();
            FileSystem.File.Exists(destinationName).Should().BeTrue();
            FileSystem.File.ReadAllBytes(destinationName).Should().BeEquivalentTo(bytes1);
        }
#endif

        #region IDisposable Members

        /// <inheritdoc cref="IDisposable.Dispose()" />
        public void Dispose()
            => _directoryCleaner.Dispose();

        #endregion
    }
}