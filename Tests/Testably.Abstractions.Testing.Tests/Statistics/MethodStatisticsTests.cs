using System.Linq;
using Testably.Abstractions.Testing.Statistics;

namespace Testably.Abstractions.Testing.Tests.Statistics;

public sealed class MethodStatisticsTests
{
	[Fact]
	public void Counter_ShouldBeInitializedWithOne()
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "bar");
		MethodStatistic sut = fileSystem.Statistics.File.Methods.First();

		sut.Counter.Should().Be(1);
	}

	[Fact]
	public void ToString_ShouldContainName()
	{
		MockFileSystem fileSystem = new();
		fileSystem.Directory.CreateDirectory("foo");
		MethodStatistic sut = fileSystem.Statistics.Directory.Methods.First();

		string result = sut.ToString();

		result.Should()
			.Contain(nameof(IDirectory.CreateDirectory)).And
			.Contain("\"foo\"").And
			.NotContain(",");
	}

	[Fact]
	public void ToString_ShouldContainParameters()
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", "bar");
		MethodStatistic sut = fileSystem.Statistics.File.Methods.First();

		string result = sut.ToString();

		result.Should()
			.Contain(nameof(IFile.WriteAllText)).And
			.Contain("\"foo\",\"bar\"");
	}
}
