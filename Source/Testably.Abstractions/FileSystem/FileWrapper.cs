using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Versioning;
using System.Text;
#if FEATURE_FILESYSTEM_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions.FileSystem;

internal sealed class FileWrapper : IFile
{
	internal FileWrapper(RealFileSystem fileSystem)
	{
		FileSystem = fileSystem;
	}

	#region IFile Members

	/// <inheritdoc cref="IFileSystemExtensionPoint.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IFile.AppendAllLines(string, IEnumerable{string})" />
	public void AppendAllLines(string path, IEnumerable<string> contents)
		=> File.AppendAllLines(path, contents);

	/// <inheritdoc cref="IFile.AppendAllLines(string, IEnumerable{string}, Encoding)" />
	public void AppendAllLines(string path, IEnumerable<string> contents,
	                           Encoding encoding)
		=> File.AppendAllLines(path, contents, encoding);

	/// <inheritdoc cref="IFile.AppendAllText(string, string?)" />
	public void AppendAllText(string path, string? contents)
		=> File.AppendAllText(path, contents);

	/// <inheritdoc cref="IFile.AppendAllText(string, string?, Encoding)" />
	public void AppendAllText(string path, string? contents, Encoding encoding)
		=> File.AppendAllText(path, contents, encoding);

	/// <inheritdoc cref="IFile.AppendText(string)" />
	public StreamWriter AppendText(string path)
		=> File.AppendText(path);

	/// <inheritdoc cref="IFile.Copy(string, string)" />
	public void Copy(string sourceFileName, string destFileName)
		=> File.Copy(sourceFileName, destFileName);

	/// <inheritdoc cref="IFile.Copy(string, string, bool)" />
	public void Copy(string sourceFileName, string destFileName, bool overwrite)
		=> File.Copy(sourceFileName, destFileName, overwrite);

	/// <inheritdoc cref="IFile.Create(string)" />
	public FileSystemStream Create(string path)
		=> new FileStreamWrapper(File.Create(path));

	/// <inheritdoc cref="IFile.Create(string, int)" />
	public FileSystemStream Create(string path, int bufferSize)
		=> new FileStreamWrapper(File.Create(path, bufferSize));

	/// <inheritdoc cref="IFile.Create(string, int, FileOptions)" />
	public FileSystemStream Create(string path, int bufferSize, FileOptions options)
		=> new FileStreamWrapper(File.Create(path, bufferSize, options));

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IFile.CreateSymbolicLink(string, string)" />
	public IFileSystemInfo CreateSymbolicLink(string path, string pathToTarget)
		=> FileSystemInfoWrapper.FromFileSystemInfo(
			File.CreateSymbolicLink(path, pathToTarget),
			FileSystem);
#endif

	/// <inheritdoc cref="IFile.CreateText(string)" />
	public StreamWriter CreateText(string path)
		=> File.CreateText(path);

	/// <inheritdoc cref="IFile.Decrypt(string)" />
	[SupportedOSPlatform("windows")]
	public void Decrypt(string path)
		=> File.Decrypt(path);

	/// <inheritdoc cref="IFile.Delete(string)" />
	public void Delete(string path)
		=> File.Delete(path);

	/// <inheritdoc cref="IFile.Encrypt(string)" />
	[SupportedOSPlatform("windows")]
	public void Encrypt(string path)
		=> File.Encrypt(path);

	/// <inheritdoc cref="IFile.Exists(string?)" />
	public bool Exists([NotNullWhen(true)] string? path)
		=> File.Exists(path);

	/// <inheritdoc cref="IFile.GetAttributes(string)" />
	public FileAttributes GetAttributes(string path)
		=> File.GetAttributes(path);

	/// <inheritdoc cref="IFile.GetCreationTime(string)" />
	public DateTime GetCreationTime(string path)
		=> File.GetCreationTime(path);

	/// <inheritdoc cref="IFile.GetCreationTimeUtc(string)" />
	public DateTime GetCreationTimeUtc(string path)
		=> File.GetCreationTimeUtc(path);

	/// <inheritdoc cref="IFile.GetLastAccessTime(string)" />
	public DateTime GetLastAccessTime(string path)
		=> File.GetLastAccessTime(path);

	/// <inheritdoc cref="IFile.GetLastAccessTimeUtc(string)" />
	public DateTime GetLastAccessTimeUtc(string path)
		=> File.GetLastAccessTimeUtc(path);

	/// <inheritdoc cref="IFile.GetLastWriteTime(string)" />
	public DateTime GetLastWriteTime(string path)
		=> File.GetLastWriteTime(path);

	/// <inheritdoc cref="IFile.GetLastWriteTimeUtc(string)" />
	public DateTime GetLastWriteTimeUtc(string path)
		=> File.GetLastWriteTimeUtc(path);

	/// <inheritdoc cref="IFile.Move(string, string)" />
	public void Move(string sourceFileName, string destFileName)
		=> File.Move(sourceFileName, destFileName);

#if FEATURE_FILE_MOVETO_OVERWRITE
	/// <inheritdoc cref="IFile.Move(string, string, bool)" />
	public void Move(string sourceFileName, string destFileName, bool overwrite)
		=> File.Move(sourceFileName, destFileName, overwrite);
#endif

	/// <inheritdoc cref="IFile.Open(string, FileMode)" />
	public FileSystemStream Open(string path, FileMode mode)
		=> new FileStreamWrapper(File.Open(path, mode));

	/// <inheritdoc cref="IFile.Open(string, FileMode, FileAccess)" />
	public FileSystemStream Open(string path, FileMode mode, FileAccess access)
		=> new FileStreamWrapper(File.Open(path, mode, access));

	/// <inheritdoc cref="IFile.Open(string, FileMode, FileAccess, FileShare)" />
	public FileSystemStream Open(string path, FileMode mode, FileAccess access,
	                             FileShare share)
		=> new FileStreamWrapper(File.Open(path, mode, access, share));

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	/// <inheritdoc cref="IFile.Open(string, FileStreamOptions)" />
	public FileSystemStream Open(string path, FileStreamOptions options)
		=> new FileStreamWrapper(File.Open(path, options));
#endif

	/// <inheritdoc cref="IFile.OpenRead(string)" />
	public FileSystemStream OpenRead(string path)
		=> new FileStreamWrapper(File.OpenRead(path));

	/// <inheritdoc cref="IFile.OpenText(string)" />
	public StreamReader OpenText(string path)
		=> File.OpenText(path);

	/// <inheritdoc cref="IFile.OpenWrite(string)" />
	public FileSystemStream OpenWrite(string path)
		=> new FileStreamWrapper(File.OpenWrite(path));

	/// <inheritdoc cref="IFile.ReadAllBytes(string)" />
	public byte[] ReadAllBytes(string path)
		=> File.ReadAllBytes(path);

#if FEATURE_FILESYSTEM_ASYNC
	/// <inheritdoc cref="IFile.ReadAllBytesAsync(string, CancellationToken)" />
	public Task<byte[]> ReadAllBytesAsync(string path,
	                                      CancellationToken cancellationToken =
		                                      default)
		=> File.ReadAllBytesAsync(path, cancellationToken);
#endif

	/// <inheritdoc cref="IFile.ReadAllLines(string)" />
	public string[] ReadAllLines(string path)
		=> File.ReadAllLines(path);

	/// <inheritdoc cref="IFile.ReadAllLines(string, Encoding)" />
	public string[] ReadAllLines(string path, Encoding encoding)
		=> File.ReadAllLines(path, encoding);

	/// <inheritdoc cref="IFile.ReadAllText(string)" />
	public string ReadAllText(string path)
		=> File.ReadAllText(path);

	/// <inheritdoc cref="IFile.ReadAllText(string, Encoding)" />
	public string ReadAllText(string path, Encoding encoding)
		=> File.ReadAllText(path, encoding);

	/// <inheritdoc cref="IFile.ReadLines(string)" />
	public IEnumerable<string> ReadLines(string path)
		=> File.ReadLines(path);

	/// <inheritdoc cref="IFile.ReadLines(string, Encoding)" />
	public IEnumerable<string> ReadLines(string path, Encoding encoding)
		=> File.ReadLines(path, encoding);

	/// <inheritdoc cref="IFile.Replace(string, string, string)" />
	public void Replace(string sourceFileName, string destinationFileName,
	                    string? destinationBackupFileName)
		=> File.Replace(sourceFileName, destinationFileName,
			destinationBackupFileName);

	/// <inheritdoc cref="IFile.Replace(string, string, string, bool)" />
	public void Replace(string sourceFileName, string destinationFileName,
	                    string? destinationBackupFileName, bool ignoreMetadataErrors)
		=> File.Replace(sourceFileName, destinationFileName,
			destinationBackupFileName, ignoreMetadataErrors);

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IFile.ResolveLinkTarget(string, bool)" />
	public IFileSystemInfo? ResolveLinkTarget(
		string linkPath, bool returnFinalTarget)
		=> FileSystemInfoWrapper.FromFileSystemInfo(
			File.ResolveLinkTarget(linkPath, returnFinalTarget),
			FileSystem);
#endif

	/// <inheritdoc cref="IFile.SetAttributes(string, FileAttributes)" />
	public void SetAttributes(string path, FileAttributes fileAttributes)
		=> File.SetAttributes(path, fileAttributes);

	/// <inheritdoc cref="IFile.SetCreationTime(string, DateTime)" />
	public void SetCreationTime(string path, DateTime creationTime)
		=> File.SetCreationTime(path, creationTime);

	/// <inheritdoc cref="IFile.SetCreationTimeUtc(string, DateTime)" />
	public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
		=> File.SetCreationTimeUtc(path, creationTimeUtc);

	/// <inheritdoc cref="IFile.SetLastAccessTime(string, DateTime)" />
	public void SetLastAccessTime(string path, DateTime lastAccessTime)
		=> File.SetLastAccessTime(path, lastAccessTime);

	/// <inheritdoc cref="IFile.SetLastAccessTimeUtc(string, DateTime)" />
	public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
		=> File.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

	/// <inheritdoc cref="IFile.SetLastWriteTime(string, DateTime)" />
	public void SetLastWriteTime(string path, DateTime lastWriteTime)
		=> File.SetLastWriteTime(path, lastWriteTime);

	/// <inheritdoc cref="IFile.SetLastWriteTimeUtc(string, DateTime)" />
	public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
		=> File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

	/// <inheritdoc cref="IFile.WriteAllBytes(string, byte[])" />
	public void WriteAllBytes(string path, byte[] bytes)
		=> File.WriteAllBytes(path, bytes);

#if FEATURE_FILESYSTEM_ASYNC
	/// <inheritdoc cref="IFile.WriteAllBytesAsync(string, byte[], CancellationToken)" />
	public Task WriteAllBytesAsync(string path, byte[] bytes,
	                               CancellationToken cancellationToken =
		                               default)
		=> File.WriteAllBytesAsync(path, bytes, cancellationToken);
#endif

	/// <inheritdoc cref="IFile.WriteAllLines(string, string[])" />
	public void WriteAllLines(string path, string[] contents)
		=> File.WriteAllLines(path, contents);

	/// <inheritdoc cref="IFile.WriteAllLines(string, IEnumerable{string})" />
	public void WriteAllLines(string path, IEnumerable<string> contents)
		=> File.WriteAllLines(path, contents);

	/// <inheritdoc cref="IFile.WriteAllLines(string, string[], Encoding)" />
	public void WriteAllLines(string path, string[] contents, Encoding encoding)
		=> File.WriteAllLines(path, contents, encoding);

	/// <inheritdoc cref="IFile.WriteAllLines(string, IEnumerable{string}, Encoding)" />
	public void WriteAllLines(string path, IEnumerable<string> contents,
	                          Encoding encoding)
		=> File.WriteAllLines(path, contents, encoding);

	/// <inheritdoc cref="IFile.WriteAllText(string, string?)" />
	public void WriteAllText(string path, string? contents)
		=> File.WriteAllText(path, contents);

	/// <inheritdoc cref="IFile.WriteAllText(string, string?, Encoding)" />
	public void WriteAllText(string path, string? contents, Encoding encoding)
		=> File.WriteAllText(path, contents, encoding);

	#endregion

#if FEATURE_FILESYSTEM_ASYNC
	/// <inheritdoc cref="IFile.AppendAllLinesAsync(string, IEnumerable{string}, CancellationToken)" />
	public Task AppendAllLinesAsync(string path, IEnumerable<string> contents,
	                                CancellationToken cancellationToken =
		                                default)
		=> File.AppendAllLinesAsync(path, contents, cancellationToken);

	/// <inheritdoc cref="IFile.AppendAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)" />
	public Task AppendAllLinesAsync(string path, IEnumerable<string> contents,
	                                Encoding encoding,
	                                CancellationToken cancellationToken =
		                                default)
		=> File.AppendAllLinesAsync(path, contents, encoding,
			cancellationToken);
#endif

#if FEATURE_FILESYSTEM_ASYNC
	/// <inheritdoc cref="IFile.AppendAllTextAsync(string, string?, CancellationToken)" />
	public Task AppendAllTextAsync(string path, string? contents,
	                               CancellationToken cancellationToken =
		                               default)
		=> File.AppendAllTextAsync(path, contents, cancellationToken);

	/// <inheritdoc cref="IFile.AppendAllTextAsync(string, string?, Encoding, CancellationToken)" />
	public Task AppendAllTextAsync(string path, string? contents, Encoding encoding,
	                               CancellationToken cancellationToken =
		                               default)
		=> File.AppendAllTextAsync(path, contents, encoding,
			cancellationToken);
#endif

#if FEATURE_FILESYSTEM_ASYNC
	/// <inheritdoc cref="IFile.ReadAllLinesAsync(string, CancellationToken)" />
	public Task<string[]> ReadAllLinesAsync(string path,
	                                        CancellationToken cancellationToken =
		                                        default)
		=> File.ReadAllLinesAsync(path, cancellationToken);

	/// <inheritdoc cref="IFile.ReadAllLinesAsync(string, Encoding, CancellationToken)" />
	public Task<string[]> ReadAllLinesAsync(string path, Encoding encoding,
	                                        CancellationToken cancellationToken =
		                                        default)
		=> File.ReadAllLinesAsync(path, encoding, cancellationToken);
#endif

#if FEATURE_FILESYSTEM_ASYNC
	/// <inheritdoc cref="IFile.ReadAllTextAsync(string, CancellationToken)" />
	public Task<string> ReadAllTextAsync(string path,
	                                     CancellationToken cancellationToken =
		                                     default)
		=> File.ReadAllTextAsync(path, cancellationToken);

	/// <inheritdoc cref="IFile.ReadAllTextAsync(string, Encoding, CancellationToken)" />
	public Task<string> ReadAllTextAsync(string path, Encoding encoding,
	                                     CancellationToken cancellationToken =
		                                     default)
		=> File.ReadAllTextAsync(path, encoding, cancellationToken);
#endif

#if FEATURE_FILESYSTEM_ASYNC
	/// <inheritdoc cref="IFile.WriteAllLinesAsync(string, IEnumerable{string}, CancellationToken)" />
	public Task WriteAllLinesAsync(string path, IEnumerable<string> contents,
	                               CancellationToken cancellationToken =
		                               default)
		=> File.WriteAllLinesAsync(path, contents, cancellationToken);

	/// <inheritdoc cref="IFile.WriteAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)" />
	public Task WriteAllLinesAsync(string path, IEnumerable<string> contents,
	                               Encoding encoding,
	                               CancellationToken cancellationToken =
		                               default)
		=> File.WriteAllLinesAsync(path, contents, encoding,
			cancellationToken);
#endif

#if FEATURE_FILESYSTEM_ASYNC
	/// <inheritdoc cref="IFile.WriteAllTextAsync(string, string?, CancellationToken)" />
	public Task WriteAllTextAsync(string path, string? contents,
	                              CancellationToken cancellationToken =
		                              default)
		=> File.WriteAllTextAsync(path, contents, cancellationToken);

	/// <inheritdoc cref="IFile.WriteAllTextAsync(string, string?, Encoding, CancellationToken)" />
	public Task WriteAllTextAsync(string path, string? contents, Encoding encoding,
	                              CancellationToken cancellationToken =
		                              default)
		=> File.WriteAllTextAsync(path, contents, encoding,
			cancellationToken);
#endif
}