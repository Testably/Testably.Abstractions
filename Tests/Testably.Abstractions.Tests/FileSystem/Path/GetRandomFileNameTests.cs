using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetRandomFileNameTests
{
	[Fact]
	public void GetRandomFileName_ShouldMatch8Dot3Pattern()
	{
		string result = FileSystem.Path.GetRandomFileName();

		result.Should().Match("????????.???");
	}

	[Fact]
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

	[Fact]
	public void GetRandomFileName_ShouldReturnRandomStrings()
	{
		ConcurrentBag<string> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(FileSystem.Path.GetRandomFileName());
		});

		results.Should().OnlyHaveUniqueItems();
	}
}
