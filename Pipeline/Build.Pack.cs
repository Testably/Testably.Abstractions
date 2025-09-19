using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Utilities.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Serilog.Log;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	// ReSharper disable once UnusedMember.Local
	Target Pack => _ => _
		.DependsOn(Compile)
		.DependsOn(CalculateNugetVersion)
		.Executes(() =>
		{
			AbsolutePath packagesDirectory = ArtifactsDirectory / "Packages";
			packagesDirectory.CreateOrCleanDirectory();

			List<string> packages = new();
			if (BuildScope != BuildScope.CoreOnly)
			{
				Directory.CreateDirectory(packagesDirectory / "Main");
				foreach (Project mainProject in MainProjects)
				{
					foreach (string package in
						Directory.EnumerateFiles(mainProject.Directory / "bin", "*.nupkg",
							SearchOption.AllDirectories))
					{
						File.Move(package, packagesDirectory / "Main" / Path.GetFileName(package));
						Debug("Found nuget package: {PackagePath}", package);
						packages.Add(Path.GetFileName(package));
					}

					foreach (string symbolPackage in
						Directory.EnumerateFiles(mainProject.Directory / "bin", "*.snupkg",
							SearchOption.AllDirectories))
					{
						File.Move(symbolPackage,
							packagesDirectory / "Main" / Path.GetFileName(symbolPackage));
						Debug("Found symbol package: {PackagePath}", symbolPackage);
					}
				}
			}

			if (BuildScope != BuildScope.MainOnly)
			{
				Directory.CreateDirectory(packagesDirectory / "Core");
				foreach (Project coreProject in CoreProjects)
				{
					foreach (string package in
						Directory.EnumerateFiles(coreProject.Directory / "bin", "*.nupkg",
							SearchOption.AllDirectories))
					{
						File.Move(package, packagesDirectory / "Core" / Path.GetFileName(package));
						Debug("Found nuget package: {PackagePath}", package);
						packages.Add(Path.GetFileName(package));
					}

					foreach (string symbolPackage in
						Directory.EnumerateFiles(coreProject.Directory / "bin", "*.snupkg",
							SearchOption.AllDirectories))
					{
						File.Move(symbolPackage,
							packagesDirectory / "Core" / Path.GetFileName(symbolPackage));
						Debug("Found symbol package: {PackagePath}", symbolPackage);
					}
				}
			}

			ReportSummary(s => s
				.AddPair("Packages", string.Join(", ", packages)));
		});
}
