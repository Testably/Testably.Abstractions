using Nuke.Common;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
using System;
using System.Linq;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	const int MaxRetries = 1;

	Target DotNetUnitTests => _ => _
		.Unlisted()
		.DependsOn(Compile)
		.OnlyWhenDynamic(() => BuildScope != BuildScope.CoreOnly)
		.Executes(() =>
		{
			string[] excludedFrameworks =
				EnvironmentInfo.IsWin
					? []
					: ["net48"];
			for (int retry = MaxRetries; retry >= 0; retry--)
			{
				try
				{
					DotNetTest(s => s
							.SetConfiguration(Configuration)
							.SetProcessEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE", "en-US")
							.SetDataCollector("XPlat Code Coverage")
							.SetResultsDirectory(TestResultsDirectory)
							.CombineWith(
								UnitTestProjects,
								(settings, project) => settings
									.SetProjectFile(project)
									.CombineWith(
										project.GetTargetFrameworks()?.Except(excludedFrameworks),
										(frameworkSettings, framework) => frameworkSettings
											.SetFramework(framework)
											.AddLoggers(
												$"trx;LogFileName={project.Name}_{framework}.trx")
									)
							), completeOnFailure: true
					);
				}
				catch (Exception ex)
				{
					if (retry == 0)
					{
						Log.Error($"All {MaxRetries + 1} tries failed: {ex}");
						throw;
					}

					Log.Error($"Error during unit tests: {ex}");
					Log.Information($"Retry {MaxRetries - retry + 1} of {MaxRetries} times:");
				}
			}
		});

	Project[] UnitTestProjects =>
	[
		Solution.Tests.Testably_Abstractions_Parity_Tests,
		Solution.Tests.Testably_Abstractions_Tests,
		Solution.Tests.Testably_Abstractions_Testing_Tests,
		Solution.Tests.Testably_Abstractions_Compression_Tests,
		Solution.Tests.Testably_Abstractions_AccessControl_Tests
	];

	Target UnitTests => _ => _
		.DependsOn(DotNetUnitTests);
}
