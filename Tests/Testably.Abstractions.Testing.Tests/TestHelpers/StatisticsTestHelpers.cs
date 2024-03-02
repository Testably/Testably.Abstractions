using System.Linq;
using Testably.Abstractions.Testing.Statistics;

namespace Testably.Abstractions.Testing.Tests.TestHelpers;

public static class StatisticsTestHelpers
{
	public static void ShouldOnlyContain(this IStatistics statistics, string name, object?[] parameters, string because = "")
	{
		statistics.Calls.Count.Should().Be(1, because);
		statistics.Calls.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.SequenceEqual(parameters),
				because);
    }
}
