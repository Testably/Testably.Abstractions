using System;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Testing.Internal;

internal static class Execute
{
	private static bool? _isNetFramework;

	/// <summary>
	///     Flag indicating if the code runs on <see cref="OSPlatform.Linux" />.
	/// </summary>
	public static bool IsLinux
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

	/// <summary>
	///     Flag indicating if the code runs on <see cref="OSPlatform.OSX" />.
	/// </summary>
	public static bool IsMac
		=> RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

	/// <summary>
	///     Flag indicating if the code runs in .NET Framework.
	/// </summary>
	/// <remarks>
	///     <see href="https://stackoverflow.com/a/53675231" />
	/// </remarks>
	public static bool IsNetFramework
	{
		get
		{
			_isNetFramework ??= RuntimeInformation
			   .FrameworkDescription.StartsWith(".NET Framework");
			return _isNetFramework.Value;
		}
	}

	/// <summary>
	///     Flag indicating if the code runs on <see cref="OSPlatform.Windows" />.
	/// </summary>
	public static bool IsWindows
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

	/// <summary>
	///     The <paramref name="callback" /> is executed on all operating systems except <see cref="OSPlatform.Windows" />.
	/// </summary>
	public static void NotOnWindows(Action callback)
	{
		if (!IsWindows)
		{
			callback();
		}
	}

	/// <summary>
	///     Returns the value from <paramref name="callback" />, when the operating system is <see cref="OSPlatform.Linux" />,
	///     otherwise the value from <paramref name="alternativeCallback" />.
	/// </summary>
	public static T OnLinux<T>(Func<T> callback, Func<T> alternativeCallback)
	{
		if (IsLinux)
		{
			return callback();
		}

		return alternativeCallback();
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed when the operating system is <see cref="OSPlatform.Linux" />.
	/// </summary>
	public static void OnLinux(Action callback)
	{
		if (IsLinux)
		{
			callback();
		}
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed when the operating system is <see cref="OSPlatform.OSX" />.
	/// </summary>
	public static void OnMac(Action callback)
	{
		if (IsMac)
		{
			callback();
		}
	}

	/// <summary>
	///     Returns the value from <paramref name="callback" />, when the code runs in .NET Framework,
	///     otherwise the value from <paramref name="alternativeCallback" />.
	/// </summary>
	/// <remarks>
	///     See also: <seealso cref="IsNetFramework" />
	/// </remarks>
	public static T OnNetFramework<T>(Func<T> callback, Func<T> alternativeCallback)
	{
		if (IsNetFramework)
		{
			return callback();
		}

		return alternativeCallback();
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed when the code runs in .NET Framework.
	/// </summary>
	/// <remarks>
	///     See also: <seealso cref="IsNetFramework" />
	/// </remarks>
	public static void OnNetFramework(Action callback, Action? alternativeCallback = null)
	{
		if (IsNetFramework)
		{
			callback();
		}
		else
		{
			alternativeCallback?.Invoke();
		}
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed when the code runs in .NET Framework
	///     and the <paramref name="predicate" /> is <see langword="true" />.
	/// </summary>
	/// <remarks>
	///     See also: <seealso cref="IsNetFramework" />
	/// </remarks>
	public static void OnNetFrameworkIf(bool predicate, Action callback)
	{
		if (IsNetFramework && predicate)
		{
			callback();
		}
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed when the operating system is <see cref="OSPlatform.Windows" />.
	/// </summary>
	public static void OnWindows(Action callback, Action? alternativeCallback = null)
	{
		if (IsWindows)
		{
			callback();
		}
		else
		{
			alternativeCallback?.Invoke();
		}
	}

	/// <summary>
	///     Returns the value from <paramref name="callback" />, when the operating system is <see cref="OSPlatform.Windows" />
	///     ,
	///     otherwise the value from <paramref name="alternativeCallback" />.
	/// </summary>
	public static T OnWindows<T>(Func<T> callback, Func<T> alternativeCallback)
	{
		if (IsWindows)
		{
			return callback();
		}

		return alternativeCallback();
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed when the operating system is <see cref="OSPlatform.Windows" />
	///     and the <paramref name="predicate" /> is <see langword="true" />.
	/// </summary>
	public static void OnWindowsIf(bool predicate, Action callback)
	{
		if (predicate && IsWindows)
		{
			callback();
		}
	}
}