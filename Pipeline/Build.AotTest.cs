using System.Runtime.InteropServices;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using Fallout.Solutions;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable UnusedMember.Local
// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	/// <summary>
	///     Runs the smoke tests from the compiled output and again published with Native AOT for the current platform.
	/// </summary>
	/// <remarks>
	///     The analyzers enabled by <c>IsAotCompatible</c> only see the libraries, so only publishing a consumer verifies
	///     that the trimmer and the AOT compiler accept them and that the published app behaves the same.
	/// </remarks>
	Target AotSmokeTests => _ => _
		.DependsOn(Compile)
		.Executes(() =>
		{
			Project project = Solution.Tests.Aot.Testably_Abstractions_AotCompatibility_TestApp;
			foreach (string framework in project.GetTargetFrameworks()!)
			{
				DotNetRun(s => s
					.SetProjectFile(project)
					.SetConfiguration(Configuration)
					.SetFramework(framework)
					.SetProcessAdditionalArguments($"/p:SolutionDir={RootDirectory}/"));

				AbsolutePath output = ArtifactsDirectory / "Aot" / framework;
				DotNetPublish(s => s
					.SetProject(project)
					.SetConfiguration(Configuration)
					.SetFramework(framework)
					.SetRuntime(RuntimeInformation.RuntimeIdentifier)
					.SetOutput(output)
					.SetProcessAdditionalArguments($"/p:SolutionDir={RootDirectory}/"));

				string executable = output / (project.Name + (EnvironmentInfo.IsWin ? ".exe" : ""));
				ProcessTasks.StartProcess(executable, string.Empty, output)
					.AssertZeroExitCode();
			}
		});
}
