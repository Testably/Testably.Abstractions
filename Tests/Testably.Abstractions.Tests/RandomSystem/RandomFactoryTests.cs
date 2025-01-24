using System.Collections.Generic;
using System.Linq;
using Testably.Abstractions.RandomSystem;

namespace Testably.Abstractions.Tests.RandomSystem;

[RandomSystemTests]
public partial class RandomFactoryTests
{
	[Fact]
	public void New_Next_ShouldReturnDifferentValues()
	{
		List<int> results = [];

		for (int i = 0; i < 100; i++)
		{
			results.Add(RandomSystem.Random.New().Next());
		}

		results.Should().OnlyHaveUniqueItems();
	}

	[Theory]
	[AutoData]
	public void New_Next_WithSeed_ShouldReturnSameValue(int seed)
	{
		List<int> results = [];

		for (int i = 0; i < 100; i++)
		{
			results.Add(RandomSystem.Random.New(seed).Next());
		}

		results.Should().AllBeEquivalentTo(results[0]);
	}

	[Fact]
	public void New_Shared_ShouldReturnDifferentValues()
	{
		List<int> results = [];

		for (int i = 0; i < 100; i++)
		{
			results.Add(RandomSystem.Random.Shared.Next());
		}

		results.Should().OnlyHaveUniqueItems();
	}

	[Fact]
	public void Shared_ShouldReturnSameReference()
	{
		IRandom shared1 = RandomSystem.Random.Shared;
		IRandom shared2 = RandomSystem.Random.Shared;

		shared1.Should().Be(shared2);
	}
}
