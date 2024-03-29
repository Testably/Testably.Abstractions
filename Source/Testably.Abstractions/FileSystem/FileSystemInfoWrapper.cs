﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.FileSystem;

internal class FileSystemInfoWrapper : IFileSystemInfo, IFileSystemExtensibility
{
	private readonly FileSystemInfo _instance;
	private readonly FileSystemExtensibility _extensibility;

	internal FileSystemInfoWrapper(FileSystemInfo instance, IFileSystem fileSystem)
	{
		_instance = instance;
		FileSystem = fileSystem;
		_extensibility = new FileSystemExtensibility(_instance);
	}

	#region IFileSystemInfo Members

	/// <inheritdoc cref="IFileSystemInfo.Attributes" />
	public FileAttributes Attributes
	{
		get => _instance.Attributes;
		set => _instance.Attributes = value;
	}

	/// <inheritdoc cref="IFileSystemInfo.CreationTime" />
	public DateTime CreationTime
	{
		get => _instance.CreationTime;
		set => _instance.CreationTime = value;
	}

	/// <inheritdoc cref="IFileSystemInfo.CreationTimeUtc" />
	public DateTime CreationTimeUtc
	{
		get => _instance.CreationTimeUtc;
		set => _instance.CreationTimeUtc = value;
	}

	/// <inheritdoc cref="IFileSystemInfo.Exists" />
	public bool Exists
		=> _instance.Exists;

	/// <inheritdoc cref="IFileSystemInfo.Extension" />
	public string Extension
		=> _instance.Extension;

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IFileSystemInfo.FullName" />
	public string FullName
		=> _instance.FullName;

	/// <inheritdoc cref="IFileSystemInfo.LastAccessTime" />
	public DateTime LastAccessTime
	{
		get => _instance.LastAccessTime;
		set => _instance.LastAccessTime = value;
	}

	/// <inheritdoc cref="IFileSystemInfo.LastAccessTimeUtc" />
	public DateTime LastAccessTimeUtc
	{
		get => _instance.LastAccessTimeUtc;
		set => _instance.LastAccessTimeUtc = value;
	}

	/// <inheritdoc cref="IFileSystemInfo.LastWriteTime" />
	public DateTime LastWriteTime
	{
		get => _instance.LastWriteTime;
		set => _instance.LastWriteTime = value;
	}

	/// <inheritdoc cref="IFileSystemInfo.LastWriteTimeUtc" />
	public DateTime LastWriteTimeUtc
	{
		get => _instance.LastWriteTimeUtc;
		set => _instance.LastWriteTimeUtc = value;
	}

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IFileSystemInfo.LinkTarget" />
	public string? LinkTarget
		=> _instance.LinkTarget;
#endif

	/// <inheritdoc cref="IFileSystemInfo.Name" />
	public string Name
		=> _instance.Name;

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	/// <inheritdoc cref="IFileSystemInfo.UnixFileMode" />
	public UnixFileMode UnixFileMode
	{
		get => _instance.UnixFileMode;
		[UnsupportedOSPlatform("windows")]
		#pragma warning disable CA1416
		set => _instance.UnixFileMode = value;
		#pragma warning restore CA1416
	}
#endif

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IFileSystemInfo.CreateAsSymbolicLink(string)" />
	public void CreateAsSymbolicLink(string pathToTarget)
		=> _instance.CreateAsSymbolicLink(pathToTarget);
#endif

	/// <inheritdoc cref="IFileSystemInfo.Delete()" />
	public void Delete()
		=> _instance.Delete();

	/// <inheritdoc cref="IFileSystemInfo.Refresh()" />
	public void Refresh()
		=> _instance.Refresh();

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IFileSystemInfo.ResolveLinkTarget(bool)" />
	public IFileSystemInfo? ResolveLinkTarget(bool returnFinalTarget)
		=> FromFileSystemInfo(_instance.ResolveLinkTarget(returnFinalTarget),
			FileSystem);
#endif

	#endregion

#if NETSTANDARD2_0
	/// <inheritdoc cref="object.ToString()" />
#else
	/// <inheritdoc cref="FileSystemInfo.ToString()" />
#endif
	public override string ToString()
		=> _instance.ToString();

	[return: NotNullIfNotNull("instance")]
	internal static FileSystemInfoWrapper? FromFileSystemInfo(
		FileSystemInfo? instance,
		IFileSystem fileSystem)
	{
		if (instance is FileInfo fileInfo)
		{
			return FileInfoWrapper.FromFileInfo(fileInfo, fileSystem);
		}

		if (instance is DirectoryInfo directoryInfo)
		{
			return DirectoryInfoWrapper.FromDirectoryInfo(directoryInfo, fileSystem);
		}

		return null;
	}

	/// <inheritdoc cref="IFileSystemExtensibility.TryGetWrappedInstance{T}" />
	public bool TryGetWrappedInstance<T>([NotNullWhen(true)] out T? wrappedInstance)
		=> _extensibility.TryGetWrappedInstance(out wrappedInstance);

	/// <inheritdoc cref="StoreMetadata{T}(string, T)" />
	public void StoreMetadata<T>(string key, T? value)
		=> _extensibility.StoreMetadata(key, value);

	/// <inheritdoc cref="RetrieveMetadata{T}(string)" />
	public T? RetrieveMetadata<T>(string key)
		=> _extensibility.RetrieveMetadata<T>(key);
}
