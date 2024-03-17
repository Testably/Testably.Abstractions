using System.IO;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileInfoFactoryStatisticsTests
{
	[SkippableFact]
	public void Method_New_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string fileName = "foo";

		sut.FileInfo.New(fileName);

		sut.Statistics.FileInfo.ShouldOnlyContainMethodCall(nameof(IFileInfoFactory.New),
			fileName);
	}

	[SkippableFact]
	public void Method_Wrap_FileInfo_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileInfo fileInfo = new("foo");

		sut.FileInfo.Wrap(fileInfo);

		sut.Statistics.FileInfo.ShouldOnlyContainMethodCall(nameof(IFileInfoFactory.Wrap),
			fileInfo);
	}

	[SkippableFact]
	public void ToString_ShouldBeFileInfo()
	{
		IPathStatistics sut = new MockFileSystem().Statistics.FileInfo;

		string? result = sut.ToString();

		result.Should().Be("FileInfo");
	}
}
