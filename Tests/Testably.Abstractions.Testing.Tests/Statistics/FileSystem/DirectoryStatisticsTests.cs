using System.IO;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DirectoryStatisticsTests
{
	[Fact]
	public void CreateDirectory_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string path = "foo";

		sut.Directory.CreateDirectory(path);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(sut.Directory.CreateDirectory),
			path);
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[SkippableFact]
	public void CreateDirectory_String_UnixFileMode_ShouldRegisterCall()
	{
		Skip.If(!Test.RunsOnLinux);

		MockFileSystem sut = new();
		string path = "foo";
		UnixFileMode unixCreateMode = UnixFileMode.None;

		sut.Directory.CreateDirectory(path, unixCreateMode);

		sut.Statistics.Directory.ShouldOnlyContain(nameof(sut.Directory.CreateDirectory),
			path, unixCreateMode);
	}
#endif

}
