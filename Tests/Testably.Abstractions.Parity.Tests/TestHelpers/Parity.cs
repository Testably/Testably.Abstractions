using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Testably.Abstractions.Parity.Tests.TestHelpers;

public class Parity
{
	public static readonly ReadOnlyDictionary<string, string> AcceptedTypeMapping = new(
		new Dictionary<string, string>(StringComparer.Ordinal)
		{
			{
				nameof(FileStream), nameof(FileSystemStream)
			},
			{
				nameof(MemoryMappedViewStream), nameof(MemoryMappedFileSystemViewStream)
			},
		});

	public ParityCheck DateTime { get; } = new(excludeMethods:
	[
		typeof(DateTime).GetMethod(nameof(System.DateTime.Compare)),
		typeof(DateTime).GetMethod(nameof(System.DateTime.DaysInMonth)),
		typeof(DateTime).GetMethod(nameof(System.DateTime.Equals), BindingFlags.Static | BindingFlags.Public),
		typeof(DateTime).GetMethod(nameof(System.DateTime.FromBinary)),
		typeof(DateTime).GetMethod(nameof(System.DateTime.FromFileTime)),
		typeof(DateTime).GetMethod(nameof(System.DateTime.FromFileTimeUtc)),
		typeof(DateTime).GetMethod(nameof(System.DateTime.FromOADate)),
		typeof(DateTime).GetMethod(nameof(System.DateTime.IsLeapYear)),
		typeof(DateTime).GetMethod(nameof(System.DateTime.SpecifyKind)),
		..typeof(DateTime).GetMethods().Where(x => string.Equals(x.Name, nameof(System.DateTime.Parse), StringComparison.Ordinal)),
		..typeof(DateTime).GetMethods().Where(x => string.Equals(x.Name, nameof(System.DateTime.ParseExact), StringComparison.Ordinal)),
		..typeof(DateTime).GetMethods().Where(x => string.Equals(x.Name, nameof(System.DateTime.TryParse), StringComparison.Ordinal)),
		..typeof(DateTime).GetMethods().Where(x => string.Equals(x.Name, nameof(System.DateTime.TryParseExact), StringComparison.Ordinal)),
	]);

	public ParityCheck DateTimeOffset { get; } = new(excludeMethods:
	[
		typeof(DateTimeOffset).GetMethod(nameof(System.DateTimeOffset.Compare)),
		typeof(DateTimeOffset).GetMethod(nameof(System.DateTimeOffset.Equals), BindingFlags.Static | BindingFlags.Public),
		typeof(DateTimeOffset).GetMethod(nameof(System.DateTimeOffset.FromFileTime)),
		typeof(DateTimeOffset).GetMethod(nameof(System.DateTimeOffset.FromUnixTimeMilliseconds)),
		typeof(DateTimeOffset).GetMethod(nameof(System.DateTimeOffset.FromUnixTimeSeconds)),
		..typeof(DateTimeOffset).GetMethods().Where(x => string.Equals(x.Name, nameof(System.DateTimeOffset.Parse), StringComparison.Ordinal)),
		..typeof(DateTimeOffset).GetMethods().Where(x => string.Equals(x.Name, nameof(System.DateTimeOffset.ParseExact), StringComparison.Ordinal)),
		..typeof(DateTimeOffset).GetMethods().Where(x => string.Equals(x.Name, nameof(System.DateTimeOffset.TryParse), StringComparison.Ordinal)),
		..typeof(DateTimeOffset).GetMethods().Where(x => string.Equals(x.Name, nameof(System.DateTimeOffset.TryParseExact), StringComparison.Ordinal)),
	]);

	public ParityCheck Directory { get; } = new();

	public ParityCheck DirectoryInfo { get; } = new(excludeMethods: new[]
	{
		#pragma warning disable SYSLIB0051
		typeof(DirectoryInfo).GetMethod(nameof(System.IO.DirectoryInfo.GetObjectData)),
		#pragma warning restore SYSLIB0051
		typeof(DirectoryInfo).GetMethod(nameof(System.IO.DirectoryInfo.ToString)),
	});

	public ParityCheck Drive { get; } = new(excludeMethods:
	[
		typeof(DriveInfo).GetMethod(nameof(DriveInfo.ToString)),
	]);

	public ParityCheck File { get; } = new();

	public ParityCheck FileInfo { get; } = new(excludeMethods: new[]
	{
		#pragma warning disable SYSLIB0051
		typeof(FileInfo).GetMethod(nameof(System.IO.FileInfo.GetObjectData)),
		#pragma warning restore SYSLIB0051
		typeof(FileInfo).GetMethod(nameof(System.IO.FileInfo.ToString)),
	});

	public ParityCheck FileStream { get; } = new();

	public ParityCheck FileSystemInfo { get; } = new(excludeMethods: new[]
	{
		#pragma warning disable SYSLIB0051
		typeof(FileSystemInfo).GetMethod(nameof(System.IO.FileSystemInfo.GetObjectData)),
		#pragma warning restore SYSLIB0051
		typeof(FileSystemInfo).GetMethod(nameof(ToString)),
	});

	public ParityCheck FileSystemWatcher { get; } = new(excludeMethods:
	[
		typeof(FileSystemWatcher).GetMethod(
			nameof(System.IO.FileSystemWatcher.ToString)),
	]);

	public ParityCheck FileVersionInfo { get; } = new(excludeMethods:
	[
		typeof(FileVersionInfo).GetMethod(
			nameof(System.Diagnostics.FileVersionInfo.ToString)),
	]);

	public ParityCheck Guid { get; } = new();

	public ParityCheck MemoryMappedFile { get; } = new(excludeMethods:
		[
			..typeof(MemoryMappedFile).GetMethods().Where(m =>
				string.Equals(m.ReturnType.Name, "MemoryMappedFileSecurity",
					StringComparison.Ordinal) ||
				m.GetParameters().Any(p => p.ParameterType.Name is "MemoryMappedFileSecurity"
					or "SafeMemoryMappedFileHandle" or "SafeFileHandle")),
		], excludeProperties:
		[
			typeof(MemoryMappedFile).GetProperty(nameof(System.IO.MemoryMappedFiles
				.MemoryMappedFile.SafeMemoryMappedFileHandle)),
		]);

	public ParityCheck MemoryMappedViewAccessor { get; } = new(excludeProperties:
	[
		typeof(MemoryMappedViewAccessor).GetProperty(nameof(System.IO.MemoryMappedFiles
			.MemoryMappedViewAccessor.SafeMemoryMappedViewHandle)),
	]);

	public ParityCheck MemoryMappedViewStream { get; } = new(excludeMethods:
	[
		typeof(MemoryMappedViewStream).GetMethod(nameof(Stream.Seek),
			[typeof(long), typeof(SeekOrigin)]),
	], excludeProperties:
	[
		typeof(MemoryMappedViewStream).GetProperty(nameof(System.IO.MemoryMappedFiles
			.MemoryMappedViewStream.SafeMemoryMappedViewHandle)),
	]);

	public ParityCheck Path { get; } = new(excludeFields: new[]
	{
		#pragma warning disable CS0618
		typeof(Path).GetField(nameof(System.IO.Path.InvalidPathChars)),
		#pragma warning restore CS0618
	});

#if FEATURE_PERIODIC_TIMER
	public ParityCheck PeriodicTimer { get; } = new(excludeConstructors:
	[
		typeof(PeriodicTimer).GetConstructor([
			typeof(TimeSpan),
			typeof(TimeProvider),
		]),
	]);
#endif

	public ParityCheck Random { get; } = new();

	public ParityCheck Stopwatch { get; } = new(excludeMethods:
	[
		typeof(Stopwatch).GetMethod(nameof(ToString)),
	]);

	public ParityCheck Timer { get; } = new(excludeMethods:
		[
			typeof(Timer).GetMethod(nameof(System.Threading.Timer.Change), [
				typeof(uint), typeof(uint),
			]),
		], excludeConstructors:
		[
			typeof(Timer).GetConstructor([
				typeof(TimerCallback),
				typeof(object),
				typeof(uint),
				typeof(uint),
			]),
		]);

	public ParityCheck TimeZoneInfo { get; } = new(excludeMethods:
	[
		typeof(TimeZoneInfo).GetMethod(nameof(System.TimeZoneInfo.ClearCachedData)),
		typeof(TimeZoneInfo).GetMethod(nameof(System.TimeZoneInfo.FromSerializedString)),
		..typeof(TimeZoneInfo).GetMethods().Where(x => string.Equals(x.Name, nameof(System.TimeZoneInfo.ConvertTime), StringComparison.Ordinal)),
		..typeof(TimeZoneInfo).GetMethods().Where(x => string.Equals(x.Name, nameof(System.TimeZoneInfo.ConvertTimeBySystemTimeZoneId), StringComparison.Ordinal)),
		..typeof(TimeZoneInfo).GetMethods().Where(x => string.Equals(x.Name, nameof(System.TimeZoneInfo.ConvertTimeFromUtc), StringComparison.Ordinal)),
		..typeof(TimeZoneInfo).GetMethods().Where(x => string.Equals(x.Name, nameof(System.TimeZoneInfo.ConvertTimeToUtc), StringComparison.Ordinal)),
		..typeof(TimeZoneInfo).GetMethods().Where(x => string.Equals(x.Name, nameof(System.TimeZoneInfo.CreateCustomTimeZone), StringComparison.Ordinal)),
		..typeof(TimeZoneInfo).GetMethods().Where(x => string.Equals(x.Name, "GetSystemTimeZones", StringComparison.Ordinal) && x.GetParameters().Length > 0),
		..typeof(TimeZoneInfo).GetMethods().Where(x => string.Equals(x.Name, "TryConvertIanaIdToWindowsId", StringComparison.Ordinal)),
		..typeof(TimeZoneInfo).GetMethods().Where(x => string.Equals(x.Name, "TryConvertWindowsIdToIanaId", StringComparison.Ordinal)),
		..typeof(TimeZoneInfo).GetMethods().Where(x => string.Equals(x.Name, "TryFindSystemTimeZoneById", StringComparison.Ordinal)),
	]);

	public ParityCheck ZipArchive { get; } = new();

	public ParityCheck ZipArchiveEntry { get; } = new(excludeMethods:
	[
		typeof(ZipArchiveEntry).GetMethod(nameof(ToString)),
	]);

	public ParityCheck ZipFile { get; } = new();
}
