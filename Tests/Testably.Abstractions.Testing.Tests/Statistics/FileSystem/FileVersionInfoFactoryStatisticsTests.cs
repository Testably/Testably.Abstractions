using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileVersionInfoFactoryStatisticsTests
{
	[SkippableFact]
	public void Method_GetVersionInfo_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string fileName = "foo";
		sut.Initialize().WithFile(fileName);

		sut.FileVersionInfo.GetVersionInfo(fileName);

		sut.Statistics.FileVersionInfo.ShouldOnlyContainMethodCall(
			nameof(IFileVersionInfoFactory.GetVersionInfo),
			fileName);
	}
}
