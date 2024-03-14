using PublicApiGenerator;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Testably.Abstractions.Api.Tests;

public static class Helper
{
	public static string CreatePublicApi(string framework, string assemblyName)
	{
		string assemblyFile = CombinedPaths("Build", "Binaries", framework, $"{assemblyName}.dll");
		Assembly assembly = Assembly.LoadFile(assemblyFile);
		string publicApi = assembly.GeneratePublicApi(options: null);
		return publicApi.Replace("\r\n", "\n");
	}

	public static string GetExpectedApi(string framework, string assemblyName)
	{
		string expectedPath = CombinedPaths("Tests", "Api", "Testably.Abstractions.Api.Tests",
			"Expected", $"{assemblyName}_{framework}.txt");
		try
		{
			return File.ReadAllText(expectedPath)
				.Replace("\r\n", "\n");
		}
		catch
		{
			return string.Empty;
		}
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

	public static void SetExpectedApi(string framework, string assemblyName, string publicApi)
	{
		string expectedPath = CombinedPaths("Tests", "Api", "Testably.Abstractions.Api.Tests",
			"Expected", $"{assemblyName}_{framework}.txt");
		Directory.CreateDirectory(Path.GetDirectoryName(expectedPath)!);
		File.WriteAllText(expectedPath, publicApi);
	}

	private static string CombinedPaths(params string[] paths) =>
		Path.GetFullPath(Path.Combine(paths.Prepend(GetSolutionDirectory()).ToArray()));

	private static string GetSolutionDirectory([CallerFilePath] string path = "") =>
		Path.Combine(Path.GetDirectoryName(path)!, "..", "..", "..");
}
