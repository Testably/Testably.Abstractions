using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Testably.Abstractions.Parity.Tests.TestHelpers;

public class Parity
{
	public static readonly ReadOnlyDictionary<string, string> AcceptedTypeMapping = new(
		new Dictionary<string, string>(StringComparer.Ordinal)
		{
			{
				nameof(FileStream), nameof(FileSystemStream)
			}
		});

	public ParityCheck Directory { get; } = new();

	public ParityCheck DirectoryInfo { get; } = new(excludeMethods: new[]
	{
		#pragma warning disable SYSLIB0051
		typeof(DirectoryInfo).GetMethod(nameof(System.IO.DirectoryInfo.GetObjectData)),
		#pragma warning restore SYSLIB0051
		typeof(DirectoryInfo).GetMethod(nameof(System.IO.DirectoryInfo.ToString))
	});

	public ParityCheck Drive { get; } = new(excludeMethods: new[]
	{
		typeof(DriveInfo).GetMethod(nameof(DriveInfo.ToString))
	});

	public ParityCheck File { get; } = new();

	public ParityCheck FileInfo { get; } = new(excludeMethods: new[]
	{
		#pragma warning disable SYSLIB0051
		typeof(FileInfo).GetMethod(nameof(System.IO.FileInfo.GetObjectData)),
		#pragma warning restore SYSLIB0051
		typeof(FileInfo).GetMethod(nameof(System.IO.FileInfo.ToString))
	});

	public ParityCheck FileStream { get; } = new();

	public ParityCheck FileSystemInfo { get; } = new(excludeMethods: new[]
	{
		#pragma warning disable SYSLIB0051
		typeof(FileSystemInfo).GetMethod(nameof(System.IO.FileSystemInfo.GetObjectData)),
		#pragma warning restore SYSLIB0051
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

	public ParityCheck Timer { get; } = new(excludeMethods: new[]
	{
		typeof(Timer).GetMethod(nameof(System.Threading.Timer.Change), new[]
		{
			typeof(uint), typeof(uint)
		})
	}, excludeConstructors: new[]
	{
		typeof(Timer).GetConstructor(new[]
		{
			typeof(TimerCallback),
			typeof(object),
			typeof(uint),
			typeof(uint)
		})
	});

	public ParityCheck ZipArchive { get; } = new();

	public ParityCheck ZipArchiveEntry { get; } = new(excludeMethods: new[]
	{
		typeof(ZipArchiveEntry).GetMethod(nameof(ToString))
	});

	public ParityCheck ZipFile { get; } = new();
}
