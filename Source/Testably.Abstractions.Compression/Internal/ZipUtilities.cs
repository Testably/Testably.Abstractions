using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Testably.Abstractions.Internal;

internal static class ZipUtilities
{
	internal static IZipArchiveEntry CreateEntryFromFile(
		IZipArchive destination,
		string sourceFileName,
		string entryName,
		CompressionLevel? compressionLevel = null)
	{
		if (destination == null)
		{
			throw new ArgumentNullException(nameof(destination));
		}

		if (sourceFileName == null)
		{
			throw new ArgumentNullException(nameof(sourceFileName));
		}

		if (entryName == null)
		{
			throw new ArgumentNullException(nameof(entryName));
		}

		// Checking of compressionLevel is passed down to DeflateStream and the IDeflater implementation
		// as it is a pluggable component that completely encapsulates the meaning of compressionLevel.

		// Argument checking gets passed down to FileStream's ctor and CreateEntry

		using (FileSystemStream fs = destination.FileSystem.FileStream.New(sourceFileName,
			FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 0x1000,
			useAsync: false))
		{
			IZipArchiveEntry entry = compressionLevel.HasValue
				? destination.CreateEntry(entryName, compressionLevel.Value)
				: destination.CreateEntry(entryName);

			DateTime lastWrite = File.GetLastWriteTime(sourceFileName);

			// If file to be archived has an invalid last modified time, use the first datetime representable in the Zip timestamp format
			// (midnight on January 1, 1980):
			if (lastWrite.Year < 1980 || lastWrite.Year > 2107)
			{
				lastWrite = new DateTime(1980, 1, 1, 0, 0, 0);
			}

			entry.LastWriteTime = lastWrite;

			using (Stream es = entry.Open())
			{
				fs.CopyTo(es);
			}

			return entry;
		}
	}

	/// <summary>
	///     Create a <c>ZipArchive</c> from the files and directories in <paramref name="sourceDirectoryName" />.
	/// </summary>
	/// <remarks>
	///     <see
	///         href="https://github.com/dotnet/runtime/blob/v6.0.10/src/libraries/System.IO.Compression.ZipFile/src/System/IO/Compression/ZipFile.Create.cs#L354" />
	/// </remarks>
	internal static void CreateFromDirectory(IFileSystem fileSystem,
	                                         string sourceDirectoryName,
	                                         string destinationArchiveFileName,
	                                         CompressionLevel? compressionLevel = null,
	                                         bool includeBaseDirectory = false,
	                                         Encoding? entryNameEncoding = null)

	{
		sourceDirectoryName = fileSystem.Path.GetFullPath(sourceDirectoryName);
		destinationArchiveFileName =
			fileSystem.Path.GetFullPath(destinationArchiveFileName);

		using (ZipArchive archive = Open(fileSystem, destinationArchiveFileName,
			ZipArchiveMode.Create, entryNameEncoding))
		{
			bool directoryIsEmpty = true;

			IFileSystem.IDirectoryInfo di =
				fileSystem.DirectoryInfo.New(sourceDirectoryName);

			string basePath = di.FullName;

			if (includeBaseDirectory && di.Parent != null)
			{
				basePath = di.Parent.FullName;
			}

			foreach (IFileSystem.IFileSystemInfo file in di
			   .EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
			{
				directoryIsEmpty = false;

				if (file is IFileSystem.IFileInfo fileInfo)
				{
					string entryName = file.FullName
					   .Substring(basePath.Length + 1)
					   .Replace("\\", "/");
					ZipArchiveEntry entry = compressionLevel.HasValue
						? archive.CreateEntry(entryName, compressionLevel.Value)
						: archive.CreateEntry(entryName);
					using Stream stream = entry.Open();
					fileInfo.OpenRead().CopyTo(stream);
				}
				else if (file is IFileSystem.IDirectoryInfo directoryInfo &&
				         directoryInfo.GetFileSystemInfos().Length == 0)
				{
#pragma warning disable CA1845
					string entryName = file.FullName.Substring(basePath.Length + 1) + "/";
#pragma warning restore CA1845
					archive.CreateEntry(entryName);
				}
			}

			if (includeBaseDirectory && directoryIsEmpty)
			{
				string entryName = di.Name + "/";
				archive.CreateEntry(entryName);
			}
		}
	}

	internal static void ExtractRelativeToDirectory(this IZipArchiveEntry source,
	                                                string destinationDirectoryName,
	                                                bool overwrite)
	{
		if (source == null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		if (destinationDirectoryName == null)
		{
			throw new ArgumentNullException(nameof(destinationDirectoryName));
		}

		// Note that this will give us a good DirectoryInfo even if destinationDirectoryName exists:
		IFileSystem.IDirectoryInfo di =
			source.FileSystem.Directory.CreateDirectory(destinationDirectoryName);
		string destinationDirectoryFullPath = di.FullName;
		if (!destinationDirectoryFullPath.EndsWith(
			$"{source.FileSystem.Path.DirectorySeparatorChar}"))
		{
			destinationDirectoryFullPath += source.FileSystem.Path.DirectorySeparatorChar;
		}

		string fileDestinationPath = source.FileSystem.Path.GetFullPath(
			source.FileSystem.Path.Combine(destinationDirectoryFullPath,
				source.FullName));

		//if (!fileDestinationPath.StartsWith(destinationDirectoryFullPath, PathInternal.StringComparison))
		//	throw new IOException(SR.IO_ExtractingResultsInOutside);

		if (source.FileSystem.Path.GetFileName(fileDestinationPath).Length == 0)
		{
			// If it is a directory:

			//if (source.Length != 0)
			//	throw new IOException(SR.IO_DirectoryNameWithData);

			source.FileSystem.Directory.CreateDirectory(fileDestinationPath);
		}
		else
		{
			// If it is a file:
			// Create containing directory:
			source.FileSystem.Directory.CreateDirectory(
				source.FileSystem.Path.GetDirectoryName(fileDestinationPath)!);
			source.ExtractToFile(fileDestinationPath, overwrite: overwrite);
		}
	}

	/// <summary>
	///     Extract the archive at <paramref name="sourceArchiveFileName" /> to the
	///     <paramref name="destinationDirectoryName" />.
	/// </summary>
	internal static void ExtractToDirectory(IFileSystem fileSystem,
	                                        string sourceArchiveFileName,
	                                        string destinationDirectoryName,
	                                        Encoding? entryNameEncoding = null,
	                                        bool overwriteFiles = false)
	{
		if (sourceArchiveFileName == null)
		{
			throw new ArgumentNullException(nameof(sourceArchiveFileName));
		}

		string destinationPath =
			fileSystem.Path.GetFullPath(destinationDirectoryName);
		using (ZipArchive archive = Open(fileSystem, sourceArchiveFileName,
			ZipArchiveMode.Read,
			entryNameEncoding))
		{
			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				string filePath =
					fileSystem.Path.Combine(destinationPath,
						entry.FullName.TrimStart(
							fileSystem.Path.DirectorySeparatorChar,
							fileSystem.Path.AltDirectorySeparatorChar));
				string? directoryPath = fileSystem.Path.GetDirectoryName(filePath);
				if (directoryPath != null &&
				    !fileSystem.Directory.Exists(directoryPath))
				{
					fileSystem.Directory.CreateDirectory(directoryPath);
				}

				if (fileSystem.File.Exists(filePath) && !overwriteFiles)
				{
					throw new IOException($"The file '{filePath}' already exists.");
				}

				using MemoryStream ms = new();
				entry.Open().CopyTo(ms);
				fileSystem.File.WriteAllBytes(filePath, ms.ToArray());
			}
		}
	}

	internal static void ExtractToFile(IZipArchiveEntry source,
	                                   string destinationFileName, bool overwrite)
	{
		FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;

		using (FileSystemStream fileStream = source.FileSystem.FileStream
		   .New(destinationFileName, mode, FileAccess.Write, FileShare.None))
		{
			using (Stream entryStream = source.Open())
			{
				entryStream.CopyTo(fileStream);
			}
		}

		try
		{
			source.FileSystem.File.SetLastWriteTime(destinationFileName,
				source.LastWriteTime.DateTime);
		}
		catch
		{
			// some OSes might not support setting the last write time
			// the extraction should not fail because of that
		}
	}

	/// <summary>
	///     Opens a <c>ZipArchive</c> on the specified <paramref name="archiveFileName" /> in the specified
	///     <paramref name="mode" />.
	/// </summary>
	/// <remarks>
	///     <see
	///         href="https://github.com/dotnet/runtime/blob/v6.0.10/src/libraries/System.IO.Compression.ZipFile/src/System/IO/Compression/ZipFile.Create.cs#L146" />
	/// </remarks>
	internal static ZipArchive Open(IFileSystem fileSystem,
	                                string archiveFileName,
	                                ZipArchiveMode mode,
	                                Encoding? entryNameEncoding = null)
	{
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

		FileSystemStream fs = fileSystem.FileStream.New(archiveFileName, fileMode,
			access, fileShare, bufferSize: 0x1000);

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
}