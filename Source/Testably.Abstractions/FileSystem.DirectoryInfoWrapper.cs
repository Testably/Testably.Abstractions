using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
	private sealed class DirectoryInfoWrapper : FileSystemInfoWrapper,
		IDirectoryInfo
	{
		private readonly IFileSystem _fileSystem;
		private readonly DirectoryInfo _instance;

		private DirectoryInfoWrapper(DirectoryInfo instance, IFileSystem fileSystem)
			: base(instance, fileSystem)
		{
			_instance = instance;
			_fileSystem = fileSystem;
		}

		#region IDirectoryInfo Members

		/// <inheritdoc cref="IDirectoryInfo.Parent" />
		public IDirectoryInfo? Parent
			=> FromDirectoryInfo(_instance.Parent, _fileSystem);

		/// <inheritdoc cref="IDirectoryInfo.Root" />
		public IDirectoryInfo Root
			=> FromDirectoryInfo(_instance.Root, _fileSystem);

		/// <inheritdoc cref="IDirectoryInfo.Create()" />
		public void Create()
			=> _instance.Create();

		/// <inheritdoc cref="IDirectoryInfo.CreateSubdirectory(string)" />
		public IDirectoryInfo CreateSubdirectory(string path)
			=> FromDirectoryInfo(_instance.CreateSubdirectory(path), _fileSystem);

		/// <inheritdoc cref="IDirectoryInfo.Delete(bool)" />
		public void Delete(bool recursive)
			=> _instance.Delete(recursive);

		/// <inheritdoc cref="IDirectoryInfo.EnumerateDirectories()" />
		public IEnumerable<IDirectoryInfo> EnumerateDirectories()
			=> _instance.EnumerateDirectories()
			   .Select(directoryInfo => FromDirectoryInfo(directoryInfo, _fileSystem));

		/// <inheritdoc cref="IDirectoryInfo.EnumerateDirectories(string)" />
		public IEnumerable<IDirectoryInfo> EnumerateDirectories(
			string searchPattern)
			=> _instance.EnumerateDirectories(searchPattern)
			   .Select(directoryInfo =>
					FromDirectoryInfo(directoryInfo, _fileSystem));

		/// <inheritdoc cref="IDirectoryInfo.EnumerateDirectories(string, SearchOption)" />
		public IEnumerable<IDirectoryInfo> EnumerateDirectories(
			string searchPattern, SearchOption searchOption)
			=> _instance.EnumerateDirectories(searchPattern, searchOption)
			   .Select(directoryInfo =>
					FromDirectoryInfo(directoryInfo, _fileSystem));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IDirectoryInfo.EnumerateDirectories(string, EnumerationOptions)" />
		public IEnumerable<IDirectoryInfo> EnumerateDirectories(
			string searchPattern,
			EnumerationOptions enumerationOptions)
			=> _instance.EnumerateDirectories(searchPattern, enumerationOptions)
			   .Select(directoryInfo =>
					FromDirectoryInfo(directoryInfo, _fileSystem));
#endif

		/// <inheritdoc cref="IDirectoryInfo.EnumerateFiles()" />
		public IEnumerable<IFileInfo> EnumerateFiles()
			=> _instance.EnumerateFiles()
			   .Select(fileInfo =>
					FileInfoWrapper.FromFileInfo(fileInfo, _fileSystem));

		/// <inheritdoc cref="IDirectoryInfo.EnumerateFiles(string)" />
		public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern)
			=> _instance.EnumerateFiles(searchPattern)
			   .Select(fileInfo =>
					FileInfoWrapper.FromFileInfo(fileInfo, _fileSystem));

		/// <inheritdoc cref="IDirectoryInfo.EnumerateFiles(string, SearchOption)" />
		public IEnumerable<IFileInfo> EnumerateFiles(
			string searchPattern, SearchOption searchOption)
			=> _instance.EnumerateFiles(searchPattern, searchOption)
			   .Select(fileInfo =>
					FileInfoWrapper.FromFileInfo(fileInfo, _fileSystem));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IDirectoryInfo.EnumerateFiles(string, EnumerationOptions)" />
		public IEnumerable<IFileInfo> EnumerateFiles(
			string searchPattern, EnumerationOptions enumerationOptions)
			=> _instance.EnumerateFiles(searchPattern, enumerationOptions)
			   .Select(fileInfo =>
					FileInfoWrapper.FromFileInfo(fileInfo, _fileSystem));
#endif

		/// <inheritdoc cref="IDirectoryInfo.EnumerateFileSystemInfos()" />
		public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos()
			=> _instance.EnumerateFileSystemInfos()
			   .Select(fileSystemInfo =>
					FromFileSystemInfo(fileSystemInfo, _fileSystem));

		/// <inheritdoc cref="IDirectoryInfo.EnumerateFileSystemInfos(string)" />
		public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(
			string searchPattern)
			=> _instance.EnumerateFileSystemInfos(searchPattern)
			   .Select(fileSystemInfo =>
					FromFileSystemInfo(fileSystemInfo, _fileSystem));

		/// <inheritdoc cref="IDirectoryInfo.EnumerateFileSystemInfos(string, SearchOption)" />
		public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(
			string searchPattern, SearchOption searchOption)
			=> _instance.EnumerateFileSystemInfos(searchPattern, searchOption)
			   .Select(fileSystemInfo =>
					FromFileSystemInfo(fileSystemInfo, _fileSystem));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IDirectoryInfo.EnumerateFileSystemInfos(string, EnumerationOptions)" />
		public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(
			string searchPattern,
			EnumerationOptions enumerationOptions)
			=> _instance.EnumerateFileSystemInfos(searchPattern, enumerationOptions)
			   .Select(fileSystemInfo =>
					FromFileSystemInfo(fileSystemInfo, _fileSystem));
#endif

		/// <inheritdoc cref="IDirectoryInfo.GetDirectories()" />
		public IDirectoryInfo[] GetDirectories()
			=> _instance.GetDirectories()
			   .Select(directoryInfo =>
					(IDirectoryInfo)FromDirectoryInfo(directoryInfo,
						_fileSystem))
			   .ToArray();

		/// <inheritdoc cref="IDirectoryInfo.GetDirectories(string)" />
		public IDirectoryInfo[] GetDirectories(string searchPattern)
			=> _instance.GetDirectories(searchPattern)
			   .Select(directoryInfo =>
					(IDirectoryInfo)FromDirectoryInfo(directoryInfo,
						_fileSystem))
			   .ToArray();

		/// <inheritdoc cref="IDirectoryInfo.GetDirectories(string, SearchOption)" />
		public IDirectoryInfo[] GetDirectories(
			string searchPattern, SearchOption searchOption)
			=> _instance.GetDirectories(searchPattern, searchOption)
			   .Select(directoryInfo =>
					(IDirectoryInfo)FromDirectoryInfo(directoryInfo,
						_fileSystem))
			   .ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IDirectoryInfo.GetDirectories(string, EnumerationOptions)" />
		public IDirectoryInfo[] GetDirectories(
			string searchPattern, EnumerationOptions enumerationOptions)
			=> _instance.GetDirectories(searchPattern, enumerationOptions)
			   .Select(directoryInfo =>
					(IDirectoryInfo)FromDirectoryInfo(directoryInfo,
						_fileSystem))
			   .ToArray();
#endif

		/// <inheritdoc cref="IDirectoryInfo.GetFiles()" />
		public IFileInfo[] GetFiles()
			=> _instance.GetFiles()
			   .Select(fileInfo =>
					(IFileInfo)FileInfoWrapper.FromFileInfo(fileInfo,
						_fileSystem))
			   .ToArray();

		/// <inheritdoc cref="IDirectoryInfo.GetFiles(string)" />
		public IFileInfo[] GetFiles(string searchPattern)
			=> _instance.GetFiles(searchPattern)
			   .Select(fileInfo =>
					(IFileInfo)FileInfoWrapper.FromFileInfo(fileInfo,
						_fileSystem))
			   .ToArray();

		/// <inheritdoc cref="IDirectoryInfo.GetFiles(string, SearchOption)" />
		public IFileInfo[] GetFiles(string searchPattern,
		                                        SearchOption searchOption)
			=> _instance.GetFiles(searchPattern, searchOption)
			   .Select(fileInfo =>
					(IFileInfo)FileInfoWrapper.FromFileInfo(fileInfo,
						_fileSystem))
			   .ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IDirectoryInfo.GetFiles(string, EnumerationOptions)" />
		public IFileInfo[] GetFiles(string searchPattern,
		                                        EnumerationOptions enumerationOptions)
			=> _instance.GetFiles(searchPattern, enumerationOptions)
			   .Select(fileInfo =>
					(IFileInfo)FileInfoWrapper.FromFileInfo(fileInfo,
						_fileSystem))
			   .ToArray();
#endif

		/// <inheritdoc cref="IDirectoryInfo.GetFileSystemInfos()" />
		public IFileSystemInfo[] GetFileSystemInfos()
			=> _instance.GetFileSystemInfos()
			   .Select(fileSystemInfo =>
					(IFileSystemInfo)FromFileSystemInfo(fileSystemInfo,
						_fileSystem))
			   .ToArray();

		/// <inheritdoc cref="IDirectoryInfo.GetFileSystemInfos(string)" />
		public IFileSystemInfo[] GetFileSystemInfos(string searchPattern)
			=> _instance.GetFileSystemInfos(searchPattern)
			   .Select(fileSystemInfo =>
					(IFileSystemInfo)FromFileSystemInfo(fileSystemInfo,
						_fileSystem))
			   .ToArray();

		/// <inheritdoc cref="IDirectoryInfo.GetFileSystemInfos(string, SearchOption)" />
		public IFileSystemInfo[] GetFileSystemInfos(
			string searchPattern, SearchOption searchOption)
			=> _instance.GetFileSystemInfos(searchPattern, searchOption)
			   .Select(fileSystemInfo =>
					(IFileSystemInfo)FromFileSystemInfo(fileSystemInfo,
						_fileSystem))
			   .ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IDirectoryInfo.GetFileSystemInfos(string, EnumerationOptions)" />
		public IFileSystemInfo[] GetFileSystemInfos(string searchPattern,
			EnumerationOptions enumerationOptions)
			=> _instance.GetFileSystemInfos(searchPattern, enumerationOptions)
			   .Select(fileSystemInfo =>
					(IFileSystemInfo)FromFileSystemInfo(fileSystemInfo,
						_fileSystem))
			   .ToArray();
#endif

		/// <inheritdoc cref="IDirectoryInfo.MoveTo(string)" />
		public void MoveTo(string destDirName)
			=> _instance.MoveTo(destDirName);

		#endregion

		[return: NotNullIfNotNull("instance")]
		internal static DirectoryInfoWrapper? FromDirectoryInfo(DirectoryInfo? instance,
			IFileSystem fileSystem)
		{
			if (instance == null)
			{
				return null;
			}

			return new DirectoryInfoWrapper(instance, fileSystem);
		}
	}
}