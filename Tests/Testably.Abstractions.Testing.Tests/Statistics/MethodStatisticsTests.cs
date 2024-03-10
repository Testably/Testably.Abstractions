using System.Linq;

namespace Testably.Abstractions.Testing.Tests.Statistics;

public sealed class MethodStatisticsTests
{
	[Fact]
	public void ToString_ShouldContainName()
	{
		MockFileSystem sut = new();
		sut.Directory.CreateDirectory("foo");

		string result = sut.Statistics.Directory.Methods.First().ToString();

		result.Should()
			.Contain(nameof(IDirectory.CreateDirectory)).And
			.Contain("\"foo\"").And
			.NotContain(",");
	}

	[Fact]
	public void ToString_ShouldContainParameters()
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText("foo", "bar");

		string result = sut.Statistics.File.Methods.First().ToString();

		result.Should()
			.Contain(nameof(IFile.WriteAllText)).And
			.Contain("\"foo\",\"bar\"");
	}
}
