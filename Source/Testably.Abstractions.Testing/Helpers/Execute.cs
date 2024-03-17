using System;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Testing.Helpers;

internal class Execute
{
	/// <summary>
	///     The default execution engine, which uses the current operating system.
	/// </summary>
	public static Execute Default { get; } = new();

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
	///     The default <see cref="StringComparison" /> used for comparing paths.
	/// </summary>
	public StringComparison StringComparisonMode { get; }

	internal Execute(OSPlatform osPlatform, bool isNetFramework = false)
	{
		IsLinux = osPlatform == OSPlatform.Linux;
		IsMac = osPlatform == OSPlatform.OSX;
		IsWindows = osPlatform == OSPlatform.Windows;
		IsNetFramework = isNetFramework && IsWindows;
		StringComparisonMode = IsLinux
			? StringComparison.Ordinal
			: StringComparison.OrdinalIgnoreCase;
	}

	private Execute()
	{
		IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		IsMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
		IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		IsNetFramework = RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");
		StringComparisonMode = IsLinux
			? StringComparison.Ordinal
			: StringComparison.OrdinalIgnoreCase;
	}
}
