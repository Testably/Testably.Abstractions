using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     The container storing the current data of the <see cref="IFileSystem" /> in memory.
    /// </summary>
    internal interface IStorage
    {
        /// <summary>
        ///     The current directory used in <see cref="System.IO.Directory.GetCurrentDirectory()" /> and
        ///     <see cref="System.IO.Directory.SetCurrentDirectory(string)" />
        /// </summary>
        string CurrentDirectory { get; set; }
        
        IEnumerable<InMemoryLocation> EnumerateLocations(
            InMemoryLocation location,
            InMemoryContainer.ContainerType type,
            string expression,
            EnumerationOptions enumerationOptions,
            Func<Exception> notFoundException);

#if FEATURE_FILESYSTEM_LINK
        InMemoryLocation? ResolveLinkTarget(InMemoryLocation location,
                                            bool returnFinalTarget = false);
#endif
        
        /// <summary>
        ///     Returns the drive if it is present.<br />
        ///     Returns <c>null</c>, if the drive does not exist.
        /// </summary>
        IDriveInfoMock? GetDrive(string? driveName);

        /// <summary>
        ///     Returns the drives that are present.
        /// </summary>
        IEnumerable<IDriveInfoMock> GetDrives();

        /// <summary>
        ///     Returns the drives that are present.
        /// </summary>
        IDriveInfoMock GetOrAddDrive(string driveName);
        
        /// <summary>
        ///     Returns the relative subdirectory path from <paramref name="fullFilePath" /> to the <paramref name="givenPath" />.
        /// </summary>
        string GetSubdirectoryPath(string fullFilePath, string givenPath);

        [return: NotNullIfNotNull("location")]
        IStorageContainer? GetContainer(InMemoryLocation? location);

        IStorageContainer GetOrCreateContainer(
            InMemoryLocation location,
            Func<InMemoryLocation, FileSystemMock, IStorageContainer> containerGenerator);

        bool DeleteContainer(InMemoryLocation location, bool recursive = false);

        bool TryAddContainer(InMemoryLocation location,
                             InMemoryContainer.ContainerType containerType,
                             [NotNullWhen(true)] out IStorageContainer? container);
    }
}