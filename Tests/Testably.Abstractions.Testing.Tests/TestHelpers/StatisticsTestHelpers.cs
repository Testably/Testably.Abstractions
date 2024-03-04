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

	public static void ShouldOnlyContain(this IStatistics statistics, string name)
	{
		statistics.Calls.Count.Should().Be(1);
		statistics.Calls.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 0);
	}

	public static void ShouldOnlyContain<T1>(this IStatistics statistics, string name, T1 parameter1)
	{
		statistics.Calls.Count.Should().Be(1);
		statistics.Calls.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 1 &&
			                    c.Parameters[0].Is(parameter1));
	}

	public static void ShouldOnlyContain<T1, T2>(this IStatistics statistics, string name, T1 parameter1, T2 parameter2)
	{
		statistics.Calls.Count.Should().Be(1);
		statistics.Calls.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 2 &&
			                    c.Parameters[0].Is(parameter1) &&
			                    c.Parameters[1].Is(parameter2));
	}
}
