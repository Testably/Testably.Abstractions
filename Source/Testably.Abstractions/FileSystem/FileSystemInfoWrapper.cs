using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.FileSystem;

internal class FileSystemInfoWrapper : IFileSystemInfo
{
	private readonly FileSystemInfo _instance;
	private readonly IFileSystem _fileSystem;

	internal FileSystemInfoWrapper(FileSystemInfo instance, IFileSystem fileSystem)
	{
		_instance = instance;
		_fileSystem = fileSystem;
		ExtensionContainer = new FileSystemExtensionContainer(_instance);
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

	/// <inheritdoc cref="IFileSystemInfo.ExtensionContainer" />
	public IFileSystemExtensionContainer ExtensionContainer { get; }

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
			_fileSystem);
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
		if (instance == null)
		{
			return null;
		}

		if (instance is FileInfo fileInfo)
		{
			return FileInfoWrapper.FromFileInfo(fileInfo, fileSystem);
		}

		if (instance is DirectoryInfo directoryInfo)
		{
			return DirectoryInfoWrapper.FromDirectoryInfo(directoryInfo, fileSystem);
		}

		return new FileSystemInfoWrapper(instance, fileSystem);
	}
}