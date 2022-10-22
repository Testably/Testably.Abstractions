using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Testably.Abstractions.Internal;

internal class ZipFileWrapper : IZipFile
{
	private readonly IFileSystem _fileSystem;

	public ZipFileWrapper(IFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	#region IZipFile Members

	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, string)" />
	public void CreateFromDirectory(string sourceDirectoryName,
	                                string destinationArchiveFileName)
	{
		if (_fileSystem is FileSystem)
		{
			ZipFile.CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName);
		}
		else
		{
			DoCreateFromDirectory(sourceDirectoryName, destinationArchiveFileName,
				compressionLevel: null, includeBaseDirectory: false,
				entryNameEncoding: null);
		}
	}

	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, string, CompressionLevel, bool)" />
	public void CreateFromDirectory(string sourceDirectoryName,
	                                string destinationArchiveFileName,
	                                CompressionLevel compressionLevel,
	                                bool includeBaseDirectory)
	{
		if (_fileSystem is FileSystem)
		{
			ZipFile.CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName,
				compressionLevel, includeBaseDirectory);
		}
		else
		{
			DoCreateFromDirectory(sourceDirectoryName, destinationArchiveFileName,
				compressionLevel, includeBaseDirectory, entryNameEncoding: null);
		}
	}

	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, string, CompressionLevel, bool, Encoding)" />
	public void CreateFromDirectory(string sourceDirectoryName,
	                                string destinationArchiveFileName,
	                                CompressionLevel compressionLevel,
	                                bool includeBaseDirectory,
	                                Encoding entryNameEncoding)
	{
		if (_fileSystem is FileSystem)
		{
			ZipFile.CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName,
				compressionLevel, includeBaseDirectory, entryNameEncoding);
		}
		else
		{
			DoCreateFromDirectory(sourceDirectoryName, destinationArchiveFileName,
				compressionLevel, includeBaseDirectory, entryNameEncoding);
		}
	}

	/// <inheritdoc cref="IZipFile.ExtractToDirectory(string, string)" />
	public void ExtractToDirectory(string sourceArchiveFileName,
	                               string destinationDirectoryName)
	{
		if (_fileSystem is FileSystem)
		{
			ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName);
		}
		else
		{
			DoExtractToDirectory(sourceArchiveFileName, destinationDirectoryName,
				entryNameEncoding: null, overwriteFiles: false);
		}
	}

#if FEATURE_COMPRESSION_OVERWRITE
	/// <inheritdoc cref="IZipFile.ExtractToDirectory(string, string, bool)" />
	public void ExtractToDirectory(string sourceArchiveFileName,
	                               string destinationDirectoryName,
	                               bool overwriteFiles)
	{
		if (_fileSystem is FileSystem)
		{
			ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, overwriteFiles);
		}
		else
		{
			DoExtractToDirectory(sourceArchiveFileName, destinationDirectoryName,
				entryNameEncoding: null, overwriteFiles: overwriteFiles);
		}
	}
#endif

	/// <inheritdoc cref="IZipFile.ExtractToDirectory(string, string, Encoding?)" />
	public void ExtractToDirectory(string sourceArchiveFileName,
	                               string destinationDirectoryName,
	                               Encoding? entryNameEncoding)
	{
		if (_fileSystem is FileSystem)
		{
			ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName,
				entryNameEncoding);
		}
		else
		{
			DoExtractToDirectory(sourceArchiveFileName, destinationDirectoryName,
				entryNameEncoding: entryNameEncoding, overwriteFiles: false);
		}
	}

#if FEATURE_COMPRESSION_OVERWRITE
	/// <inheritdoc cref="IZipFile.ExtractToDirectory(string, string, Encoding?, bool)" />
	public void ExtractToDirectory(string sourceArchiveFileName,
	                               string destinationDirectoryName,
	                               Encoding? entryNameEncoding,
	                               bool overwriteFiles)
	{
		if (_fileSystem is FileSystem)
		{
			ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, entryNameEncoding, overwriteFiles);
		}
		else
		{
			DoExtractToDirectory(sourceArchiveFileName, destinationDirectoryName,
				entryNameEncoding, overwriteFiles);
		}
	}
#endif

	/// <inheritdoc cref="IZipFile.Open(string, ZipArchiveMode)" />
	public ZipArchive Open(string archiveFileName, ZipArchiveMode mode)
	{
		if (_fileSystem is FileSystem)
		{
			return ZipFile.Open(archiveFileName, mode);
		}

		return Open(archiveFileName, mode, entryNameEncoding: null);
	}

	/// <inheritdoc cref="IZipFile.Open(string, ZipArchiveMode, Encoding?)" />
	public ZipArchive Open(string archiveFileName, ZipArchiveMode mode,
	                       Encoding? entryNameEncoding)
	{
		if (_fileSystem is FileSystem)
		{
			return ZipFile.Open(archiveFileName, mode, entryNameEncoding);
		}

		FileMode fileMode;
		FileAccess access;
		FileShare fileShare;

		switch (mode)
		{
			case ZipArchiveMode.Read:
				fileMode = FileMode.Open;
				access = FileAccess.Read;
				fileShare = FileShare.Read;
				break;

			case ZipArchiveMode.Create:
				fileMode = FileMode.CreateNew;
				access = FileAccess.Write;
				fileShare = FileShare.None;
				break;

			case ZipArchiveMode.Update:
				fileMode = FileMode.OpenOrCreate;
				access = FileAccess.ReadWrite;
				fileShare = FileShare.None;
				break;

			default:
				throw new ArgumentOutOfRangeException(nameof(mode));
		}

		FileSystemStream fs = _fileSystem.FileStream.New(archiveFileName, fileMode,
			access, fileShare, bufferSize: 0x1000, useAsync: false);

		try
		{
			return new ZipArchive(fs, mode, leaveOpen: false,
				entryNameEncoding: entryNameEncoding);
		}
		catch
		{
			fs.Dispose();
			throw;
		}
	}

	/// <inheritdoc cref="IZipFile.OpenRead(string)" />
	public ZipArchive OpenRead(string archiveFileName)
		=> Open(archiveFileName, ZipArchiveMode.Read);

	#endregion

	private void DoExtractToDirectory(string sourceArchiveFileName,
	                                  string destinationDirectoryName,
	                                  Encoding? entryNameEncoding,
	                                  bool overwriteFiles)
	{
		if (sourceArchiveFileName == null)
		{
			throw new ArgumentNullException(nameof(sourceArchiveFileName));
		}

		string destinationPath =
			_fileSystem.Path.GetFullPath(destinationDirectoryName);
		using (ZipArchive archive = Open(sourceArchiveFileName, ZipArchiveMode.Read,
			entryNameEncoding))
		{
			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				string filePath =
					_fileSystem.Path.Combine(destinationPath,
						entry.FullName.TrimStart(
							_fileSystem.Path.DirectorySeparatorChar,
							_fileSystem.Path.AltDirectorySeparatorChar));
				string? directoryPath = _fileSystem.Path.GetDirectoryName(filePath);
				if (directoryPath != null &&
				    !_fileSystem.Directory.Exists(directoryPath))
				{
					_fileSystem.Directory.CreateDirectory(directoryPath);
				}

				using MemoryStream ms = new();
				entry.Open().CopyTo(ms);
				_fileSystem.File.WriteAllBytes(filePath, ms.ToArray());
			}
		}
	}

	private void DoCreateFromDirectory(string sourceDirectoryName,
	                                   string destinationArchiveFileName,
	                                   CompressionLevel? compressionLevel,
	                                   bool includeBaseDirectory,
	                                   Encoding? entryNameEncoding)

	{
		sourceDirectoryName = _fileSystem.Path.GetFullPath(sourceDirectoryName);
		destinationArchiveFileName =
			_fileSystem.Path.GetFullPath(destinationArchiveFileName);

		using (ZipArchive archive = Open(destinationArchiveFileName,
			ZipArchiveMode.Create, entryNameEncoding))
		{
			bool directoryIsEmpty = true;

			IFileSystem.IDirectoryInfo di =
				_fileSystem.DirectoryInfo.New(sourceDirectoryName);

			string basePath = di.FullName;

			if (includeBaseDirectory && di.Parent != null)
			{
				basePath = di.Parent.FullName;
			}

			foreach (IFileSystem.IFileSystemInfo file in di
			   .EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
			{
				directoryIsEmpty = false;

				int entryNameLength = file.FullName.Length - basePath.Length;
				Debug.Assert(entryNameLength > 0);

				if (file is IFileSystem.IFileInfo fileInfo)
				{
					string entryName = file.FullName.Substring(basePath.Length);
					ZipArchiveEntry entry = compressionLevel.HasValue
						? archive.CreateEntry(entryName, compressionLevel.Value)
						: archive.CreateEntry(entryName);
					using Stream stream = entry.Open();
					fileInfo.OpenRead().CopyTo(stream);
				}
				else if (file is IFileSystem.IDirectoryInfo directoryInfo &&
				         directoryInfo.GetFileSystemInfos().Length == 0)
				{
					string entryName = file.FullName.Substring(basePath.Length);
					archive.CreateEntry(entryName);
				}
			}

			if (includeBaseDirectory && directoryIsEmpty)
			{
				string entryName = di.Name;
				archive.CreateEntry(entryName);
			}
		}
	}
}