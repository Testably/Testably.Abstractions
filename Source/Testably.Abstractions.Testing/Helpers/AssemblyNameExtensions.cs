using System.Reflection;

namespace Testably.Abstractions.Testing.Helpers;

internal static class AssemblyNameExtensions
{
	/// <summary>
	///     Returns the name of the <paramref name="assemblyName" /> or the <paramref name="defaultName" />, if it is
	///     <see langword="null" />.
	/// </summary>
	internal static string GetNameOrDefault(this AssemblyName assemblyName, string defaultName = "")
	{
		return assemblyName.Name ?? defaultName;
	}
}
