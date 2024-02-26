using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Testing.Helpers;

[ExcludeFromCodeCoverage]
internal class Execute
{
	/// <summary>
	///     The default execution engine, which uses the current operating system.
	/// </summary>
	public static Execute Default { get; } = new();

	private bool? _isNetFramework;

	/// <summary>
	///     The default <see cref="StringComparison" /> used for comparing paths.
	/// </summary>
	public StringComparison StringComparisonMode => IsLinux
		? StringComparison.Ordinal
		: StringComparison.OrdinalIgnoreCase;

	/// <summary>
	///     Flag indicating if the code runs on <see cref="OSPlatform.Linux" />.
	/// </summary>
	public bool IsLinux
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

	/// <summary>
	///     Flag indicating if the code runs on <see cref="OSPlatform.OSX" />.
	/// </summary>
	public bool IsMac
		=> RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

	/// <summary>
	///     Flag indicating if the code runs in .NET Framework.
	/// </summary>
	/// <remarks>
	///     <see href="https://stackoverflow.com/a/53675231" />
	/// </remarks>
	public bool IsNetFramework
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
	public bool IsWindows
		=> RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

	/// <summary>
	///     The <paramref name="callback" /> is executed when the code runs not in .NET Framework.
	/// </summary>
	/// <remarks>
	///     See also: <seealso cref="IsNetFramework" />
	/// </remarks>
	public void NotOnNetFramework(Action callback)
	{
		if (!IsNetFramework)
		{
			callback();
		}
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed on all operating systems except <see cref="OSPlatform.Windows" />.
	/// </summary>
	public void NotOnWindows(Action callback)
	{
		if (!IsWindows)
		{
			callback();
		}
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed when the operating system is not <see cref="OSPlatform.Windows" />
	///     and the <paramref name="predicate" /> is <see langword="true" />.
	/// </summary>
	public void NotOnWindowsIf(bool predicate, Action callback)
	{
		if (predicate && !IsWindows)
		{
			callback();
		}
	}

	/// <summary>
	///     Returns the value from <paramref name="callback" />, when the operating system is <see cref="OSPlatform.Linux" />,
	///     otherwise the value from <paramref name="alternativeCallback" />.
	/// </summary>
	public T OnLinux<T>(Func<T> callback, Func<T> alternativeCallback)
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
	public void OnLinux(Action callback)
	{
		if (IsLinux)
		{
			callback();
		}
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed when the operating system is <see cref="OSPlatform.OSX" />.
	/// </summary>
	public void OnMac(Action callback, Action? alternativeCallback = null)
	{
		if (IsMac)
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
	///     See also: <seealso cref="IsNetFramework" />
	/// </remarks>
	public T OnNetFramework<T>(Func<T> callback, Func<T> alternativeCallback)
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
	public void OnNetFramework(Action callback, Action? alternativeCallback = null)
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
	public void OnNetFrameworkIf(bool predicate, Action callback)
	{
		if (IsNetFramework && predicate)
		{
			callback();
		}
	}

	/// <summary>
	///     The <paramref name="callback" /> is executed when the operating system is <see cref="OSPlatform.Windows" />.
	/// </summary>
	public void OnWindows(Action callback, Action? alternativeCallback = null)
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
	public T OnWindows<T>(Func<T> callback, Func<T> alternativeCallback)
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
	public void OnWindowsIf(bool predicate, Action callback)
	{
		if (predicate && IsWindows)
		{
			callback();
		}
	}
}
