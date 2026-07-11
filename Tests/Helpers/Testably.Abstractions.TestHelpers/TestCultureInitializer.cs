using System.Globalization;
using System.Runtime.CompilerServices;

namespace Testably.Abstractions.TestHelpers;

/// <summary>
///     Forces all tests to run under <see cref="CultureInfo.InvariantCulture" /> so that
///     localized exception messages on .NET Framework are in english.
/// </summary>
internal static class TestCultureInitializer
{
	#pragma warning disable CA2255
	[ModuleInitializer]
	#pragma warning restore CA2255
	internal static void Initialize()
	{
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
		CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
	}
}
