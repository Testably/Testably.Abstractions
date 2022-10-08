﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.Testing.Storage;

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

    /// <summary>
    ///     Deletes the container stored at <paramref name="location" />.
    /// </summary>
    /// <param name="location">The location at which the container is located.</param>
    /// <param name="recursive">(optional) Is set, also child-containers are deleted recursively.</param>
    /// <returns><see langword="true" />, if the container was found and deleted, otherwise <see langword="false" />.</returns>
    bool DeleteContainer(IStorageLocation location, bool recursive = false);

    /// <summary>
    ///     Enumerate the locations of files or directories stored under <paramref name="location" />.
    /// </summary>
    /// <param name="location">The parent location in which the files or directories are searched for.</param>
    /// <param name="type">The type of the container (file, directory or both).</param>
    /// <param name="expression">
    ///     (optional) The expression to filter the name of the locations.
    ///     <para />
    ///     Defaults to <c>"*"</c> which matches any name.
    /// </param>
    /// <param name="enumerationOptions">
    ///     (optional) The enumeration options.
    ///     <para />
    ///     Defaults to <see langword="null" /> which uses the `Compatible` enumeration options:<br />
    ///     - Search only in Top-Directory<br />
    ///     - Use Win32 expression MatchType
    /// </param>
    /// <returns>
    ///     The location of files or directories that match the <paramref name="type" />, <paramref name="expression" />
    ///     and <paramref name="enumerationOptions" />.
    /// </returns>
    IEnumerable<IStorageLocation> EnumerateLocations(
        IStorageLocation location,
        ContainerType type,
        string expression = "*",
        EnumerationOptions? enumerationOptions = null);

    /// <summary>
    ///     Gets the container at <paramref name="location" />.
    ///     <para />
    ///     If the container is not found, returns a <see cref="NullContainer" />.
    /// </summary>
    /// <param name="location">The location at which to look for the container.</param>
    /// <returns>
    ///     <see langword="null" />, if <paramref name="location" /> is null. Otherwise it returns the found container or
    ///     <see cref="NullContainer" />.
    /// </returns>
    [return: NotNullIfNotNull("location")]
    IStorageContainer? GetContainer(IStorageLocation? location);

    /// <summary>
    ///     Returns the drive if it is present.<br />
    ///     Returns <see langword="null" />, if the drive does not exist.
    /// </summary>
    IStorageDrive? GetDrive(string? driveName);

    /// <summary>
    ///     Returns the drives that are present.
    /// </summary>
    IEnumerable<IStorageDrive> GetDrives();

    /// <summary>
    ///     Gets the location for the given <paramref name="path" />.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="friendlyName">(optional) the friendly name for the <paramref name="path" />.</param>
    /// <returns>The <see cref="IStorageLocation" /> that corresponds to <paramref name="path" />.</returns>
    [return: NotNullIfNotNull("path")]
    IStorageLocation? GetLocation(string? path, string? friendlyName = null);

    /// <summary>
    ///     Returns the drives that are present.
    /// </summary>
    IStorageDrive GetOrAddDrive(string driveName);

    /// <summary>
    ///     Returns an existing container at <paramref name="location" />.<br />
    ///     If no container exists yet, creates a new container at this <paramref name="location" /> using the
    ///     <paramref name="containerGenerator" /> and returns the new generated container.
    /// </summary>
    /// <param name="location">The location at which to get or create the container.</param>
    /// <param name="containerGenerator">The callback used to create a new container at <paramref name="location" />.</param>
    /// <returns>The container at <paramref name="location" />.</returns>
    IStorageContainer GetOrCreateContainer(
        IStorageLocation location,
        Func<IStorageLocation, FileSystemMock, IStorageContainer> containerGenerator);

#if FEATURE_FILESYSTEM_LINK
    /// <summary>
    ///     Resolves the link target of the container stored at <paramref name="location" />.
    /// </summary>
    /// <param name="location">The location to start looking up the link targets.</param>
    /// <param name="returnFinalTarget">
    ///     (optional) <see langword="true" /> to follow links to the final target; <see langword="false" /> to return the
    ///     immediate next link.
    /// </param>
    /// <returns>The location of the link target.</returns>
    IStorageLocation? ResolveLinkTarget(IStorageLocation location,
                                        bool returnFinalTarget = false);
#endif

    /// <summary>
    ///     Tries to add a new container at <paramref name="location" /> using the <paramref name="containerGenerator" />.
    /// </summary>
    /// <param name="location">The location at which to add a new container.</param>
    /// <param name="containerGenerator">The callback used to create a new container at <paramref name="location" />.</param>
    /// <param name="container">(out) The created container if successful, otherwise <see langword="null" />.</param>
    /// <returns>
    ///     <see langword="true" /> if the container could be created at <paramref name="location" />,
    ///     otherwise <see langword="false" />.
    /// </returns>
    bool TryAddContainer(IStorageLocation location,
                         Func<IStorageLocation, FileSystemMock, IStorageContainer>
                             containerGenerator,
                         [NotNullWhen(true)] out IStorageContainer? container);
}