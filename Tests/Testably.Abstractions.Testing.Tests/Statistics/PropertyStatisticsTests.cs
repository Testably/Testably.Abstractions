using System.Linq;
using Testably.Abstractions.Testing.Statistics;

namespace Testably.Abstractions.Testing.Tests.Statistics;

public sealed class PropertyStatisticsTests
{
	[Fact]
	public async Task Counter_ShouldBeInitializedWithOne()
	{
		MockFileSystem fileSystem = new();
		_ = fileSystem.Path.DirectorySeparatorChar;
		PropertyStatistic sut = fileSystem.Statistics.Path.Properties[0];

		await That(sut.Counter).IsEqualTo(1);
	}

	[Fact]
	public async Task ToString_Get_ShouldContainNameAndGet()
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize().WithFile("foo");
		IFileInfo fileInfo = fileSystem.FileInfo.New("foo");
		_ = fileInfo.IsReadOnly;
		PropertyStatistic sut = fileSystem.Statistics.FileInfo["foo"].Properties[0];

		string result = sut.ToString();

		await That(result).Contains(nameof(IFileInfo.IsReadOnly)).And.Contains("{get;}");
	}

	[Fact]
	public async Task ToString_Set_ShouldContainNameAndSet()
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize().WithFile("foo");
		IFileInfo fileInfo = fileSystem.FileInfo.New("foo");
		fileInfo.IsReadOnly = false;
		PropertyStatistic sut = fileSystem.Statistics.FileInfo["foo"].Properties[0];

		string result = sut.ToString();

		await That(result).Contains(nameof(IFileInfo.IsReadOnly)).And.Contains("{set;}");
	}
}
