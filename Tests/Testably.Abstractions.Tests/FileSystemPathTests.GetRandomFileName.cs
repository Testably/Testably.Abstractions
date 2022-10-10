using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	[FileSystemTests.Path(nameof(IFileSystem.IPath.GetRandomFileName))]
	public void GetRandomFileName_ShouldReturnRandomFileNameWithExtension()
	{
		string result = FileSystem.Path.GetRandomFileName();

#if FEATURE_PATH_RELATIVE
		FileSystem.Path.IsPathFullyQualified(result)
		   .Should().BeFalse();
#endif
		FileSystem.Path.GetExtension(result)
		   .Should().NotBeEmpty();
		FileSystem.Path.GetFileNameWithoutExtension(result)
		   .Should().NotBeEmpty();
	}

	[SkippableFact]
	[FileSystemTests.Path(nameof(IFileSystem.IPath.GetRandomFileName))]
	public void GetRandomFileName_ShouldReturnRandomStrings()
	{
		ConcurrentBag<string> results = new();

		Parallel.For(0, 100, _ =>
		{
			results.Add(FileSystem.Path.GetRandomFileName());
		});

		results.Should().OnlyHaveUniqueItems();
	}
}