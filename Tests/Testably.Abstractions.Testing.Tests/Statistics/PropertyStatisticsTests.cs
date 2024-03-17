using System.Linq;
using Testably.Abstractions.Testing.Statistics;

namespace Testably.Abstractions.Testing.Tests.Statistics;

public sealed class PropertyStatisticsTests
{
	[Fact]
	public void ToString_Get_ShouldContainNameAndGet()
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize().WithFile("foo");
		IFileInfo fileInfo = fileSystem.FileInfo.New("foo");
		_ = fileInfo.IsReadOnly;
		PropertyStatistic sut = fileSystem.Statistics.FileInfo["foo"].Properties.First();

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
		PropertyStatistic sut = fileSystem.Statistics.FileInfo["foo"].Properties.First();

		string result = sut.ToString();

		result.Should()
			.Contain(nameof(IFileInfo.IsReadOnly)).And
			.Contain("{set;}");
	}
}
