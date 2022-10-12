using System.Collections.Generic;
using System.IO;

namespace Testably.Abstractions.Tests.Parity;

public class Parity
{
	public static readonly Dictionary<string, string> AcceptedTypeMapping = new()
	{
		{ nameof(FileStream), nameof(FileSystemStream) }
	};

	public ParityCheck Directory = new();

	public ParityCheck DirectoryInfo = new(excludeMethods: new[]
	{
		typeof(DirectoryInfo).GetMethod(nameof(System.IO.DirectoryInfo
		   .GetObjectData)),
		typeof(DirectoryInfo).GetMethod(nameof(System.IO.DirectoryInfo.ToString))
	});

	public ParityCheck Drive = new(excludeMethods: new[]
	{
		typeof(DriveInfo).GetMethod(nameof(DriveInfo.ToString))
	});

	public ParityCheck File = new();

	public ParityCheck FileInfo = new(excludeMethods: new[]
	{
		typeof(FileInfo).GetMethod(nameof(System.IO.FileInfo.GetObjectData)),
		typeof(FileInfo).GetMethod(nameof(System.IO.FileInfo.ToString))
	});

	public ParityCheck FileStream = new();

	public ParityCheck FileSystemInfo = new(excludeMethods: new[]
	{
		typeof(FileSystemInfo).GetMethod(
			nameof(System.IO.FileSystemInfo.GetObjectData)),
		typeof(FileSystemInfo).GetMethod(nameof(ToString))
	});

	public ParityCheck FileSystemWatcher = new();

	public ParityCheck Guid = new();

	public ParityCheck Path = new(excludeFields: new[]
	{
#pragma warning disable CS0618
		typeof(Path).GetField(nameof(System.IO.Path.InvalidPathChars))
#pragma warning restore CS0618
	});

	public ParityCheck Random = new();
}