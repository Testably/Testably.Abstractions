using System.Linq;
using Testably.Abstractions.Testing.Statistics;

namespace Testably.Abstractions.Testing.Tests.Statistics;

public sealed class PropertyStatisticsTests
{
	[Fact]
	public void Counter_ShouldBeInitializedWithOne()
	{
		MockFileSystem fileSystem = new();
		_ = fileSystem.Path.DirectorySeparatorChar;
		PropertyStatistic sut = fileSystem.Statistics.Path.Properties[0];

		sut.Counter.Should().Be(1);
	}

	[Fact]
	public void ToString_Get_ShouldContainNameAndGet()
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize().WithFile("foo");
		IFileInfo fileInfo = fileSystem.FileInfo.New("foo");
		_ = fileInfo.IsReadOnly;
		PropertyStatistic sut = fileSystem.Statistics.FileInfo["foo"].Properties[0];

		string result = sut.ToString();

		result.Should()
			.Contain(nameof(IFileInfo.IsReadOnly)).And
			.Contain("{get;}");
	}

	[Fact]
	public void ToString_Set_ShouldContainNameAndSet()
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize().WithFile("foo");
		IFileInfo fileInfo = fileSystem.FileInfo.New("foo");
		fileInfo.IsReadOnly = false;
		PropertyStatistic sut = fileSystem.Statistics.FileInfo["foo"].Properties[0];

		string result = sut.ToString();

		result.Should()
			.Contain(nameof(IFileInfo.IsReadOnly)).And
			.Contain("{set;}");
	}
}
