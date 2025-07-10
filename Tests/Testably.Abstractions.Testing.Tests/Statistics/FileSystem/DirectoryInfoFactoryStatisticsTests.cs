using System.IO;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DirectoryInfoFactoryStatisticsTests
{
	[Fact]
	public async Task Method_New_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.DirectoryInfo.New(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.DirectoryInfo).OnlyContainsMethodCall(
			nameof(IDirectoryInfoFactory.New),
			path);
	}

	[Fact]
	public async Task Method_Wrap_DirectoryInfo_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		DirectoryInfo directoryInfo = new(".");

		sut.DirectoryInfo.Wrap(directoryInfo);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.DirectoryInfo).OnlyContainsMethodCall(
			nameof(IDirectoryInfoFactory.Wrap),
			directoryInfo);
	}

	[Fact]
	public async Task ToString_ShouldBeDirectoryInfo()
	{
		IPathStatistics<IDirectoryInfoFactory, IDirectoryInfo> sut
			= new MockFileSystem().Statistics.DirectoryInfo;

		string? result = sut.ToString();

		await That(result).IsEqualTo("DirectoryInfo");
	}
}
