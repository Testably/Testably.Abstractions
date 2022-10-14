using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Testing.Internal;

internal static class Execute
{
	private static bool? _isNetFramework;

	public static bool IsLinux
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

	public static bool IsMac
		=> RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

	[ExcludeFromCodeCoverage]
	public static bool IsNetFramework
	{
		get
		{
			_isNetFramework ??= RuntimeInformation
			   .FrameworkDescription.StartsWith(".NET Framework");
			return _isNetFramework.Value;
		}
	}

	public static bool IsWindows
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

	public static void NotOnWindows(Action callback)
	{
		if (!IsWindows)
		{
			callback();
		}
	}

	public static T OnLinux<T>(Func<T> callback, Func<T> alternativeCallback)
	{
		if (IsLinux)
		{
			return callback();
		}

		return alternativeCallback();
	}

	public static void OnMac(Action callback)
	{
		if (IsMac)
		{
			callback();
		}
	}

	public static T OnNetFramework<T>(Func<T> callback, Func<T> alternativeCallback)
	{
		if (IsNetFramework)
		{
			return callback();
		}

		return alternativeCallback();
	}

	public static void OnNetFramework(Action callback)
	{
		if (IsNetFramework)
		{
			callback();
		}
	}

	public static void OnWindows(Action callback)
	{
		if (IsWindows)
		{
			callback();
		}
	}

	public static T OnWindows<T>(Func<T> callback, Func<T> alternativeCallback)
	{
		if (IsWindows)
		{
			return callback();
		}

		return alternativeCallback();
	}
}