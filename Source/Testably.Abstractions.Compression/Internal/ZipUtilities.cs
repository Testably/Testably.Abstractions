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
		if (sourceFileName == null)
		{
			throw new ArgumentNullException(nameof(sourceFileName));
		}

		using (FileSystemStream fs = destination.FileSystem.FileStream.New(sourceFileName,
			FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			IZipArchiveEntry entry = compressionLevel.HasValue
				? destination.CreateEntry(entryName, compressionLevel.Value)
				: destination.CreateEntry(entryName);

			DateTime lastWrite =
				destination.FileSystem.File.GetLastWriteTime(sourceFileName);

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

			IDirectoryInfo di =
				fileSystem.DirectoryInfo.New(sourceDirectoryName);

			string basePath = di.FullName;

			if (includeBaseDirectory && di.Parent != null)
			{
				basePath = di.Parent.FullName;
			}

			foreach (IFileSystemInfo file in di
			   .EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
			{
				directoryIsEmpty = false;

				if (file is IFileInfo fileInfo)
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
				else if (file is IDirectoryInfo directoryInfo &&
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
		if (destinationDirectoryName == null)
		{
			throw new ArgumentNullException(nameof(destinationDirectoryName));
		}

		string fileDestinationPath =
			source.FileSystem.Path.Combine(destinationDirectoryName,
				source.FullName.TrimStart(
					source.FileSystem.Path.DirectorySeparatorChar,
					source.FileSystem.Path.AltDirectorySeparatorChar));
		string? directoryPath =
			source.FileSystem.Path.GetDirectoryName(fileDestinationPath);
		if (directoryPath != null &&
		    !source.FileSystem.Directory.Exists(directoryPath))
		{
			source.FileSystem.Directory.CreateDirectory(directoryPath);
		}

		if (source.FullName.EndsWith("/"))
		{
			if (source.Length != 0)
			{
				throw new IOException(
					"Zip entry name ends in directory separator character but contains data.");
			}

			source.FileSystem.Directory.CreateDirectory(fileDestinationPath);
		}
		else
		{
			ExtractToFile(source, fileDestinationPath, overwrite);
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

		using (ZipArchive archive = Open(fileSystem, sourceArchiveFileName,
			ZipArchiveMode.Read,
			entryNameEncoding))
		{
			ZipArchiveWrapper wrappedArchive = new(fileSystem, archive);
			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				IZipArchiveEntry wrappedEntry = ZipArchiveEntryWrapper.New(
					fileSystem, wrappedArchive, entry);
				ExtractRelativeToDirectory(wrappedEntry, destinationDirectoryName,
					overwriteFiles);
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