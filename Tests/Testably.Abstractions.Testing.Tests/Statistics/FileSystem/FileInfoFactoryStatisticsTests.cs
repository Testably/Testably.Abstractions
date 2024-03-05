using System.IO;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileInfoFactoryStatisticsTests
{
	[SkippableFact]
	public void New_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string fileName = "foo";

		sut.FileInfo.New(fileName);

		sut.Statistics.FileInfo.ShouldOnlyContain(nameof(IFileInfoFactory.New),
			fileName);
	}

	[SkippableFact]
	public void Wrap_FileInfo_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileInfo fileInfo = new("foo");

		sut.FileInfo.Wrap(fileInfo);

		sut.Statistics.FileInfo.ShouldOnlyContain(nameof(IFileInfoFactory.Wrap),
			fileInfo);
	}
}
