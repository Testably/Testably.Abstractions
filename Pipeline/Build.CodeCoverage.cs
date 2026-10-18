using Fallout.Common;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.ReportGenerator;
using static Fallout.Common.Tools.ReportGenerator.ReportGeneratorTasks;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	Target CodeCoverage => _ => _
		.DependsOn(UnitTests)
		.OnlyWhenDynamic(() => BuildScope != BuildScope.CoreOnly)
		.Executes(() =>
		{
			ReportGenerator(s => s
				.SetProcessToolPath(NuGetToolPathResolver.GetPackageExecutable("ReportGenerator", "ReportGenerator.dll",
					framework: "net10.0"))
				.SetTargetDirectory(TestResultsDirectory / "reports")
				.AddReports(TestResultsDirectory / "**/*.cobertura.xml")
				// The generic format of Sonar lists the lines per file. A format that lists them per method leaves out the
				// lines of a lambda in a generic class, because ReportGenerator keeps them in a class without methods.
				.AddReportTypes(ReportTypes.SonarQube)
				.AddFileFilters("-*.g.cs")
				.SetAssemblyFilters("+Testably*"));
		});
}
