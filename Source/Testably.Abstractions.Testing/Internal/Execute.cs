using System;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Testing.Internal;

internal static class Execute
{
	public static bool IsLinux
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

	public static bool IsMac
		=> RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

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