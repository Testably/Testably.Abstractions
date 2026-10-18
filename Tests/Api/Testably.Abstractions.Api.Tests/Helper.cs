using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Testably.Abstractions.Api.Tests;

public static class Helper
{
	/// <summary>
	///     Returns the build of the <paramref name="assemblyName" /> for the <paramref name="framework" />, so that the
	///     builds of all target frameworks are verified, including those that the tests cannot run on.
	/// </summary>
	public static FileInfo GetAssemblyFile(string framework, string assemblyName)
	{
#if DEBUG
		string configuration = "Debug";
#else
		string configuration = "Release";
#endif
		return new FileInfo(
			CombinedPaths("Source", assemblyName, "bin", configuration, framework, $"{assemblyName}.dll"));
	}

	public static IEnumerable<string> GetTargetFrameworks()
	{
		string csproj = CombinedPaths("Source", "Directory.Build.props");
		XDocument project = XDocument.Load(csproj);
		XElement? targetFrameworks =
			project.XPathSelectElement("/Project/PropertyGroup/TargetFrameworks");
		foreach (string targetFramework in targetFrameworks!.Value.Split(';'))
		{
			yield return targetFramework;
		}
	}

	private static string CombinedPaths(params string[] paths) =>
		Path.GetFullPath(Path.Combine(paths.Prepend(GetSolutionDirectory()).ToArray()));

	/// <remarks>
	///     The directory is searched from the output directory of the tests, as a deterministic build maps the paths
	///     that the compiler records, e.g. with <see cref="CallerFilePathAttribute" />, to <c>/_/</c>.
	/// </remarks>
	private static string GetSolutionDirectory()
	{
		DirectoryInfo? directory = new(AppContext.BaseDirectory);
		while (directory is not null &&
		       !File.Exists(Path.Combine(directory.FullName, "Testably.Abstractions.slnx")))
		{
			directory = directory.Parent;
		}

		return directory?.FullName ?? throw new DirectoryNotFoundException(
			$"No directory above '{AppContext.BaseDirectory}' contains the solution 'Testably.Abstractions.slnx'.");
	}
}
