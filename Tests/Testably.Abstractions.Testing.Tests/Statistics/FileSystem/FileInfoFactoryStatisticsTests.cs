using System.IO;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileInfoFactoryStatisticsTests
{
	[Fact]
	public async Task Method_New_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string fileName = "foo";

		sut.FileInfo.New(fileName);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.FileInfo).OnlyContainsMethodCall(nameof(IFileInfoFactory.New),
			fileName);
	}

	[Fact]
	public async Task Method_Wrap_FileInfo_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileInfo fileInfo = new("foo");

		sut.FileInfo.Wrap(fileInfo);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.FileInfo).OnlyContainsMethodCall(nameof(IFileInfoFactory.Wrap),
			fileInfo);
	}

	[Fact]
	public async Task ToString_ShouldBeFileInfo()
	{
		IPathStatistics<IFileInfoFactory, IFileInfo> sut
			= new MockFileSystem().Statistics.FileInfo;

		string? result = sut.ToString();

		await That(result).IsEqualTo("FileInfo");
	}
}
