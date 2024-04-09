using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Testably.Abstractions.Internal;

internal static class ZipUtilities
{
	private const string SearchPattern = "*";
	private static readonly DateTime FallbackTime = new(1980, 1, 1, 0, 0, 0);

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

			if (lastWrite.Year is < 1980 or > 2107)
			{
				lastWrite = FallbackTime;
			}

			entry.LastWriteTime = new DateTimeOffset(lastWrite);

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
				.EnumerateFileSystemInfos(SearchPattern, SearchOption.AllDirectories))
			{
				directoryIsEmpty = false;

				if (file is IFileInfo fileInfo)
				{
					#pragma warning disable MA0074
					string entryName = file.FullName
						.Substring(basePath.Length + 1)
						.Replace("\\", "/");
					#pragma warning restore MA0074
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

#if FEATURE_COMPRESSION_STREAM
	/// <summary>
	///     Create a <c>ZipArchive</c> from the files and directories in <paramref name="sourceDirectoryName" />.
	/// </summary>
	/// <remarks>
	///     <see
	///         href="https://github.com/dotnet/runtime/blob/v6.0.10/src/libraries/System.IO.Compression.ZipFile/src/System/IO/Compression/ZipFile.Create.cs#L354" />
	/// </remarks>
	#pragma warning disable MA0051 // Method is too long
	internal static void CreateFromDirectory(
		IFileSystem fileSystem,
		string sourceDirectoryName,
		Stream destination,
		CompressionLevel? compressionLevel = null,
		bool includeBaseDirectory = false,
		Encoding? entryNameEncoding = null)
	{
		ArgumentNullException.ThrowIfNull(destination);
		if (!destination.CanWrite)
		{
			throw new ArgumentException("The stream is unwritable.", nameof(destination));
		}

		sourceDirectoryName = fileSystem.Path.GetFullPath(sourceDirectoryName);

		using (ZipArchive archive = new(destination, ZipArchiveMode.Create,
			leaveOpen: true,
			entryNameEncoding: entryNameEncoding))
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
				.EnumerateFileSystemInfos(SearchPattern, SearchOption.AllDirectories))
			{
				directoryIsEmpty = false;

				if (file is IFileInfo fileInfo)
				{
					#pragma warning disable MA0074
					string entryName = file.FullName
						.Substring(basePath.Length + 1)
						.Replace("\\", "/");
					#pragma warning restore MA0074
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
	#pragma warning restore MA0051 // Method is too long
#endif

	internal static void ExtractRelativeToDirectory(this IZipArchiveEntry source,
		string destinationDirectoryName,
		bool overwrite)
	{
		string fileDestinationPath =
			source.FileSystem.Path.Combine(destinationDirectoryName,
				source.FullName.TrimStart(
					source.FileSystem.Path.DirectorySeparatorChar,
					source.FileSystem.Path.AltDirectorySeparatorChar));

		if (source.FullName.EndsWith('/'))
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
			source.FileSystem.Directory.CreateDirectory(
				source.FileSystem.Path.GetDirectoryName(fileDestinationPath) ?? ".");
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

#if FEATURE_COMPRESSION_STREAM
	/// <summary>
	///     Extract the archive at <paramref name="source" /> to the
	///     <paramref name="destinationDirectoryName" />.
	/// </summary>
	internal static void ExtractToDirectory(IFileSystem fileSystem,
		Stream source,
		string destinationDirectoryName,
		Encoding? entryNameEncoding = null,
		bool overwriteFiles = false)
	{
		ArgumentNullException.ThrowIfNull(source);
		if (!source.CanRead)
		{
			throw new ArgumentException("The stream is unreadable.", nameof(source));
		}

		using (ZipArchive archive = new(source, ZipArchiveMode.Read, true, entryNameEncoding))
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
#endif

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
