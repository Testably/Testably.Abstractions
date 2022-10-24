using System.Runtime.InteropServices;

namespace Testably.Abstractions.Compression.Tests.TestHelpers;

public static class Test
{
	private static bool? _isNetFramework;

	public static bool IsNetFramework
	{
		get
		{
			_isNetFramework ??= RuntimeInformation
			   .FrameworkDescription.StartsWith(".NET Framework");
			return _isNetFramework.Value;
		}
	}
}