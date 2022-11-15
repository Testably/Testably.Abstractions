using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;

namespace Testably.Abstractions.Parity.Tests.TestHelpers;

public class Parity
{
	public static readonly ReadOnlyDictionary<string, string> AcceptedTypeMapping = new(
		new Dictionary<string, string>
		{
			{
				nameof(FileStream), nameof(FileSystemStream)
			}
		});

	public ParityCheck Directory { get; } = new();

	public ParityCheck DirectoryInfo { get; } = new(excludeMethods: new[]
	{
		typeof(DirectoryInfo).GetMethod(nameof(System.IO.DirectoryInfo
			.GetObjectData)),
		typeof(DirectoryInfo).GetMethod(nameof(System.IO.DirectoryInfo.ToString))
	});

	public ParityCheck Drive { get; } = new(excludeMethods: new[]
	{
		typeof(DriveInfo).GetMethod(nameof(DriveInfo.ToString))
	});

	public ParityCheck File { get; } = new();

	public ParityCheck FileInfo { get; } = new(excludeMethods: new[]
	{
		typeof(FileInfo).GetMethod(nameof(System.IO.FileInfo.GetObjectData)),
		typeof(FileInfo).GetMethod(nameof(System.IO.FileInfo.ToString))
	});

	public ParityCheck FileStream { get; } = new();

	public ParityCheck FileSystemInfo { get; } = new(excludeMethods: new[]
	{
		typeof(FileSystemInfo).GetMethod(
			nameof(System.IO.FileSystemInfo.GetObjectData)),
		typeof(FileSystemInfo).GetMethod(nameof(ToString))
	});

	public ParityCheck FileSystemWatcher { get; } = new(excludeMethods: new[]
	{
		typeof(FileSystemWatcher).GetMethod(
			nameof(System.IO.FileSystemWatcher.ToString))
	});

	public ParityCheck Guid { get; } = new();

	public ParityCheck Path { get; } = new(excludeFields: new[]
	{
		#pragma warning disable CS0618
		typeof(Path).GetField(nameof(System.IO.Path.InvalidPathChars))
		#pragma warning restore CS0618
	});

	public ParityCheck Random { get; } = new();
	public ParityCheck ZipArchive { get; } = new();

	public ParityCheck ZipArchiveEntry { get; } = new(excludeMethods: new[]
	{
		typeof(ZipArchiveEntry).GetMethod(nameof(ToString))
	});

	public ParityCheck ZipFile { get; } = new();
}
