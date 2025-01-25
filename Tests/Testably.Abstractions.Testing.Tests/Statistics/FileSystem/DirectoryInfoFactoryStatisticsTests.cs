using System.IO;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DirectoryInfoFactoryStatisticsTests
{
	[Fact]
	public void Method_New_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.DirectoryInfo.New(path);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.DirectoryInfo.ShouldOnlyContainMethodCall(nameof(IDirectoryInfoFactory.New),
			path);
	}

	[Fact]
	public void Method_Wrap_DirectoryInfo_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		DirectoryInfo directoryInfo = new(".");

		sut.DirectoryInfo.Wrap(directoryInfo);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.DirectoryInfo.ShouldOnlyContainMethodCall(nameof(IDirectoryInfoFactory.Wrap),
			directoryInfo);
	}

	[Fact]
	public void ToString_ShouldBeDirectoryInfo()
	{
		IPathStatistics<IDirectoryInfoFactory, IDirectoryInfo> sut
			= new MockFileSystem().Statistics.DirectoryInfo;

		string? result = sut.ToString();

		result.Should().Be("DirectoryInfo");
	}
}
