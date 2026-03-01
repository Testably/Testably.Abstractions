using Testably.Abstractions.Testing.Statistics;

namespace Testably.Abstractions.Testing.Tests.Statistics;

public sealed class MethodStatisticsTests
{
	[Test]
	public async Task Counter_ShouldBeInitializedWithOne()
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "bar");
		MethodStatistic sut = fileSystem.Statistics.File.Methods[0];

		await That(sut.Counter).IsEqualTo(1);
	}

	[Test]
	public async Task ToString_ShouldContainName()
	{
		MockFileSystem fileSystem = new();
		fileSystem.Directory.CreateDirectory("foo");
		MethodStatistic sut = fileSystem.Statistics.Directory.Methods[0];

		string result = sut.ToString();

		await That(result).Contains(nameof(IDirectory.CreateDirectory)).And.Contains("\"foo\"").And
			.DoesNotContain(",");
	}

	[Test]
	public async Task ToString_ShouldContainParameters()
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "bar");
		MethodStatistic sut = fileSystem.Statistics.File.Methods[0];

		string result = sut.ToString();

		await That(result).Contains(nameof(IFile.WriteAllText)).And.Contains("\"foo\",\"bar\"");
	}
}
