using System.Linq;
using Testably.Abstractions.Testing.Statistics;

namespace Testably.Abstractions.Testing.Tests.TestHelpers;

public static class StatisticsTestHelpers
{
	public static void ShouldOnlyContain(this IStatistics statistics, string name,
		object?[] parameters, string because = "")
	{
		statistics.Methods.Count.Should().Be(1, because);
		statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.SequenceEqual(parameters),
				because);
	}

	public static void ShouldOnlyContain(this IStatistics statistics, string name)
	{
		statistics.Methods.Count.Should().Be(1);
		statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 0);
	}

	public static void ShouldOnlyContain<T1>(this IStatistics statistics, string name,
		T1 parameter1)
	{
		statistics.Methods.Count.Should().Be(1);
		statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 1 &&
			                    c.Parameters[0].Is(parameter1));
	}

	public static void ShouldOnlyContain<T1>(this IStatistics statistics, string name,
		T1[] parameter1)
	{
		statistics.Methods.Count.Should().Be(1);
		statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 1 &&
			                    c.Parameters[0].Is(parameter1));
	}

#if FEATURE_SPAN
	public static void ShouldOnlyContain<T1>(this IStatistics statistics, string name,
		ReadOnlySpan<T1> parameter1)
	{
		statistics.Methods.Count.Should().Be(1);
		ParameterDescription parameter = statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 1).Which.Parameters[0];
		parameter.Is(parameter1).Should().BeTrue();
	}

	public static void ShouldOnlyContain<T1, T2>(this IStatistics statistics, string name,
		ReadOnlySpan<T1> parameter1, ReadOnlySpan<T2> parameter2)
	{
		statistics.Methods.Count.Should().Be(1);
		MethodStatistic? statistic = statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 2).Which;
		statistic.Parameters[0].Is(parameter1).Should().BeTrue();
		statistic.Parameters[1].Is(parameter2).Should().BeTrue();
	}

	public static void ShouldOnlyContain<T1, T2, T3>(this IStatistics statistics, string name,
		ReadOnlySpan<T1> parameter1, ReadOnlySpan<T2> parameter2, ReadOnlySpan<T3> parameter3)
	{
		statistics.Methods.Count.Should().Be(1);
		MethodStatistic? statistic = statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 3).Which;
		statistic.Parameters[0].Is(parameter1).Should().BeTrue();
		statistic.Parameters[1].Is(parameter2).Should().BeTrue();
		statistic.Parameters[2].Is(parameter3).Should().BeTrue();
	}

	public static void ShouldOnlyContain<T1, T2, T3, T4>(this IStatistics statistics, string name,
		ReadOnlySpan<T1> parameter1, ReadOnlySpan<T2> parameter2, ReadOnlySpan<T3> parameter3, ReadOnlySpan<T4> parameter4)
	{
		statistics.Methods.Count.Should().Be(1);
		MethodStatistic? statistic = statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 4).Which;
		statistic.Parameters[0].Is(parameter1).Should().BeTrue();
		statistic.Parameters[1].Is(parameter2).Should().BeTrue();
		statistic.Parameters[2].Is(parameter3).Should().BeTrue();
		statistic.Parameters[3].Is(parameter4).Should().BeTrue();
	}

	public static void ShouldOnlyContain<T1, T2, T3, T4>(this IStatistics statistics, string name,
		ReadOnlySpan<T1> parameter1, ReadOnlySpan<T2> parameter2, T3 parameter3, T4 parameter4)
	{
		statistics.Methods.Count.Should().Be(1);
		MethodStatistic? statistic = statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 4).Which;
		statistic.Parameters[0].Is(parameter1).Should().BeTrue();
		statistic.Parameters[1].Is(parameter2).Should().BeTrue();
		statistic.Parameters[2].Is(parameter3).Should().BeTrue();
		statistic.Parameters[3].Is(parameter4).Should().BeTrue();
	}

	public static void ShouldOnlyContain<T1, T2, T3, T4>(this IStatistics statistics, string name,
		ReadOnlySpan<T1> parameter1, ReadOnlySpan<T2> parameter2, Span<T3> parameter3, T4 parameter4)
	{
		statistics.Methods.Count.Should().Be(1);
		MethodStatistic? statistic = statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 4).Which;
		statistic.Parameters[0].Is(parameter1).Should().BeTrue();
		statistic.Parameters[1].Is(parameter2).Should().BeTrue();
		statistic.Parameters[2].Is(parameter3).Should().BeTrue();
		statistic.Parameters[3].Is(parameter4).Should().BeTrue();
	}

	public static void ShouldOnlyContain<T1, T2, T3, T4, T5>(this IStatistics statistics, string name,
		ReadOnlySpan<T1> parameter1, ReadOnlySpan<T2> parameter2, ReadOnlySpan<T3> parameter3, Span<T4> parameter4, T5 parameter5)
	{
		statistics.Methods.Count.Should().Be(1);
		MethodStatistic? statistic = statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 5).Which;
		statistic.Parameters[0].Is(parameter1).Should().BeTrue();
		statistic.Parameters[1].Is(parameter2).Should().BeTrue();
		statistic.Parameters[2].Is(parameter3).Should().BeTrue();
		statistic.Parameters[3].Is(parameter4).Should().BeTrue();
		statistic.Parameters[4].Is(parameter5).Should().BeTrue();
	}

	public static void ShouldOnlyContain<T1>(this IStatistics statistics, string name,
		Span<T1> parameter1)
	{
		statistics.Methods.Count.Should().Be(1);
		ParameterDescription parameter = statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 1).Which.Parameters[0];
		parameter.Is(parameter1).Should().BeTrue();
	}
#endif

	public static void ShouldOnlyContain<T1, T2>(this IStatistics statistics, string name,
		T1 parameter1, T2 parameter2)
	{
		statistics.Methods.Count.Should().Be(1);
		statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 2 &&
			                    c.Parameters[0].Is(parameter1) &&
			                    c.Parameters[1].Is(parameter2));
	}

	public static void ShouldOnlyContain<T1, T2, T3>(this IStatistics statistics, string name,
		T1 parameter1, T2 parameter2, T3 parameter3)
	{
		statistics.Methods.Count.Should().Be(1);
		statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 3 &&
			                    c.Parameters[0].Is(parameter1) &&
			                    c.Parameters[1].Is(parameter2) &&
			                    c.Parameters[2].Is(parameter3));
	}

	public static void ShouldOnlyContain<T1, T2, T3, T4>(this IStatistics statistics, string name,
		T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4)
	{
		statistics.Methods.Count.Should().Be(1);
		statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 4 &&
			                    c.Parameters[0].Is(parameter1) &&
			                    c.Parameters[1].Is(parameter2) &&
			                    c.Parameters[2].Is(parameter3) &&
			                    c.Parameters[3].Is(parameter4));
	}

	public static void ShouldOnlyContain<T1, T2, T3, T4, T5>(this IStatistics statistics,
		string name, T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4, T5 parameter5)
	{
		statistics.Methods.Count.Should().Be(1);
		statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 5 &&
			                    c.Parameters[0].Is(parameter1) &&
			                    c.Parameters[1].Is(parameter2) &&
			                    c.Parameters[2].Is(parameter3) &&
			                    c.Parameters[3].Is(parameter4) &&
			                    c.Parameters[4].Is(parameter5));
	}

	public static void ShouldOnlyContain<T1, T2, T3, T4, T5, T6>(this IStatistics statistics,
		string name, T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4, T5 parameter5,
		T6 parameter6)
	{
		statistics.Methods.Count.Should().Be(1);
		statistics.Methods.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.Length == 6 &&
			                    c.Parameters[0].Is(parameter1) &&
			                    c.Parameters[1].Is(parameter2) &&
			                    c.Parameters[2].Is(parameter3) &&
			                    c.Parameters[3].Is(parameter4) &&
			                    c.Parameters[4].Is(parameter5) &&
			                    c.Parameters[5].Is(parameter6));
	}
}
