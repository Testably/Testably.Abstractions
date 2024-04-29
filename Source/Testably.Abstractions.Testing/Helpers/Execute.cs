using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Testing.Helpers;

internal partial class Execute
{
	/// <summary>
	///     Flag indicating if the code runs on <see cref="OSPlatform.Linux" />.
	/// </summary>
	public bool IsLinux { get; }

	/// <summary>
	///     Flag indicating if the code runs on <see cref="OSPlatform.OSX" />.
	/// </summary>
	public bool IsMac { get; }

	/// <summary>
	///     Flag indicating if the code runs in .NET Framework.
	/// </summary>
	/// <remarks>
	///     <see href="https://stackoverflow.com/a/53675231" />
	/// </remarks>
	public bool IsNetFramework { get; }

	/// <summary>
	///     Flag indicating if the code runs on <see cref="OSPlatform.Windows" />.
	/// </summary>
	public bool IsWindows { get; }

	/// <summary>
	///     The internal implementation of the <see cref="IPath" /> functionality.
	/// </summary>
	public IPath Path { get; }

	/// <summary>
	///     The default <see cref="StringComparison" /> used for comparing paths.
	/// </summary>
	public StringComparison StringComparisonMode { get; }

#if !CAN_SIMULATE_OTHER_OS
	[Obsolete("Simulating other operating systems is not supported on .NET Framework")]
#endif
	internal Execute(MockFileSystem fileSystem, SimulationMode simulationMode,
		bool isNetFramework = false)
	{
		IsLinux = simulationMode == SimulationMode.Linux;
		IsMac = simulationMode == SimulationMode.MacOS;
		IsWindows = simulationMode == SimulationMode.Windows;
		IsNetFramework = IsWindows && isNetFramework;
		StringComparisonMode = IsLinux
			? StringComparison.Ordinal
			: StringComparison.OrdinalIgnoreCase;
		if (IsLinux)
		{
			Path = new LinuxPath(fileSystem);
		}
		else if (IsMac)
		{
			Path = new MacPath(fileSystem);
		}
		else if (IsWindows)
		{
			Path = new WindowsPath(fileSystem);
		}
		else
		{
			throw new NotSupportedException(
				"The operating system must be one of Linux, MacOS or Windows");
		}
	}

	internal Execute(MockFileSystem fileSystem)
	{
		IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		IsMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
		IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		IsNetFramework = RuntimeInformation.FrameworkDescription
			.StartsWith(".NET Framework", StringComparison.OrdinalIgnoreCase);
		StringComparisonMode = IsLinux
			? StringComparison.Ordinal
			: StringComparison.OrdinalIgnoreCase;
		Path = new NativePath(fileSystem);
	}

	internal static string CreateTempFileName(MockFileSystem fileSystem)
	{
		int i = 0;
		string tempPath = fileSystem.Path.GetTempPath();
		fileSystem.Directory.CreateDirectory(tempPath);
		while (true)
		{
			string fileName = $"{RandomString(fileSystem, 8)}.tmp";
			string path = string.Concat(tempPath, fileName);
			try
			{
				fileSystem.File.Open(path, FileMode.CreateNew, FileAccess.Write).Dispose();
				return path;
			}
			catch (IOException) when (i < 100)
			{
				i++; // Don't let unforeseen circumstances cause us to loop forever
			}
		}
	}

	internal static string RandomString(MockFileSystem fileSystem, int length)
	{
		const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
		return new string(Enumerable.Repeat(chars, length)
			.Select(s => s[fileSystem.RandomSystem.Random.Shared.Next(s.Length)]).ToArray());
	}
}
