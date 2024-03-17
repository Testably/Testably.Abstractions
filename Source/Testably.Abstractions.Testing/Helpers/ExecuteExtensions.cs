using System;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Testing.Helpers;

internal static class ExecuteExtensions
{
	/// <summary>
	///     The <paramref name="callback" /> is executed when the code runs not in .NET Framework.
	/// </summary>
	/// <remarks>
	///     See also: <seealso cref="Execute.IsNetFramework" />
	/// </remarks>
	public static void NotOnNetFramework(this Execute execute, Action callback)
	{
		if (!execute.IsNetFramework)
		{
			callback();
		}
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed on all operating systems except <see cref="OSPlatform.Windows" />.
	/// </summary>
	public static void NotOnWindows(this Execute execute, Action callback)
	{
		if (!execute.IsWindows)
		{
			callback();
		}
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed when the operating system is not <see cref="OSPlatform.Windows" />
	///     and the <paramref name="predicate" /> is <see langword="true" />.
	/// </summary>
	public static void NotOnWindowsIf(this Execute execute, bool predicate, Action callback)
	{
		if (predicate && !execute.IsWindows)
		{
			callback();
		}
	}

	/// <summary>
	///     Returns the value from <paramref name="callback" />, when the operating system is <see cref="OSPlatform.Linux" />,
	///     otherwise the value from <paramref name="alternativeCallback" />.
	/// </summary>
	public static T OnLinux<T>(this Execute execute, Func<T> callback, Func<T> alternativeCallback)
	{
		if (execute.IsLinux)
		{
			return callback();
		}

		return alternativeCallback();
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed when the operating system is <see cref="OSPlatform.Linux" />.
	/// </summary>
	public static void OnLinux(this Execute execute, Action callback)
	{
		if (execute.IsLinux)
		{
			callback();
		}
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed when the operating system is <see cref="OSPlatform.OSX" />.
	/// </summary>
	public static void OnMac(this Execute execute, Action callback,
		Action? alternativeCallback = null)
	{
		if (execute.IsMac)
		{
			callback();
		}
		else
		{
			alternativeCallback?.Invoke();
		}
	}

	/// <summary>
	///     Returns the value from <paramref name="callback" />, when the code runs in .NET Framework,
	///     otherwise the value from <paramref name="alternativeCallback" />.
	/// </summary>
	/// <remarks>
	///     See also: <seealso cref="Execute.IsNetFramework" />
	/// </remarks>
	public static T OnNetFramework<T>(this Execute execute, Func<T> callback,
		Func<T> alternativeCallback)
	{
		if (execute.IsNetFramework)
		{
			return callback();
		}

		return alternativeCallback();
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed when the code runs in .NET Framework.
	/// </summary>
	/// <remarks>
	///     See also: <seealso cref="Execute.IsNetFramework" />
	/// </remarks>
	public static void OnNetFramework(this Execute execute, Action callback,
		Action? alternativeCallback = null)
	{
		if (execute.IsNetFramework)
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
	///     See also: <seealso cref="Execute.IsNetFramework" />
	/// </remarks>
	public static void OnNetFrameworkIf(this Execute execute, bool predicate, Action callback)
	{
		if (execute.IsNetFramework && predicate)
		{
			callback();
		}
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed when the operating system is <see cref="OSPlatform.Windows" />.
	/// </summary>
	public static void OnWindows(this Execute execute, Action callback,
		Action? alternativeCallback = null)
	{
		if (execute.IsWindows)
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
	public static T OnWindows<T>(this Execute execute, Func<T> callback,
		Func<T> alternativeCallback)
	{
		if (execute.IsWindows)
		{
			return callback();
		}

		return alternativeCallback();
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed when the operating system is <see cref="OSPlatform.Windows" />
	///     and the <paramref name="predicate" /> is <see langword="true" />.
	/// </summary>
	public static void OnWindowsIf(this Execute execute, bool predicate, Action callback)
	{
		if (predicate && execute.IsWindows)
		{
			callback();
		}
	}
}
