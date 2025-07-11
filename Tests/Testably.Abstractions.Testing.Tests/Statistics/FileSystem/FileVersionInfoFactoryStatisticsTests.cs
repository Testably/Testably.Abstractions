using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileVersionInfoFactoryStatisticsTests
{
	[Fact]
	public async Task Method_GetVersionInfo_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string fileName = "foo";
		sut.Initialize().WithFile(fileName);

		sut.FileVersionInfo.GetVersionInfo(fileName);

		await That(sut.Statistics.FileVersionInfo).OnlyContainsMethodCall(
			nameof(IFileVersionInfoFactory.GetVersionInfo),
			fileName);
	}
}
