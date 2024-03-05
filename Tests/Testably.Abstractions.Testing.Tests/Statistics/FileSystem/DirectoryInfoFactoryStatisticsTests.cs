using System.IO;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DirectoryInfoFactoryStatisticsTests
{
	[SkippableFact]
	public void New_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.DirectoryInfo.New(path);

		sut.Statistics.DirectoryInfo.ShouldOnlyContain(nameof(IDirectoryInfoFactory.New),
			path);
	}

	[SkippableFact]
	public void Wrap_DirectoryInfo_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		DirectoryInfo directoryInfo = new(".");

		sut.DirectoryInfo.Wrap(directoryInfo);

		sut.Statistics.DirectoryInfo.ShouldOnlyContain(nameof(IDirectoryInfoFactory.Wrap),
			directoryInfo);
	}
}
