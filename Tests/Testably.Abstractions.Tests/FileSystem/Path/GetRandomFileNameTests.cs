using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetRandomFileNameTests
{
	[Fact]
	public async Task GetRandomFileName_ShouldMatch8Dot3Pattern()
	{
		string result = FileSystem.Path.GetRandomFileName();

		await That(result).IsEqualTo("????????.???").AsWildcard();
	}

	[Fact]
	public async Task GetRandomFileName_ShouldReturnRandomFileNameWithExtension()
	{
		string result = FileSystem.Path.GetRandomFileName();

#if FEATURE_PATH_RELATIVE
		await That(FileSystem.Path.IsPathFullyQualified(result)).IsFalse();
#endif
		await That(FileSystem.Path.GetExtension(result)).IsNotEmpty();
		await That(FileSystem.Path.GetFileNameWithoutExtension(result)).IsNotEmpty();
	}

	[Fact]
	public async Task GetRandomFileName_ShouldReturnRandomStrings()
	{
		ConcurrentBag<string> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(FileSystem.Path.GetRandomFileName());
		});

		await That(results).AreAllUnique();
	}
}
