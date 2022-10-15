using System.Collections.Generic;
using System.Linq;

namespace Testably.Abstractions.Tests.RandomSystem.RandomFactory;

public abstract class RandomSystemRandomFactoryTests<TRandomSystem>
	where TRandomSystem : IRandomSystem
{
	#region Test Setup

	public TRandomSystem RandomSystem { get; }

	protected RandomSystemRandomFactoryTests(TRandomSystem randomSystem)
	{
		RandomSystem = randomSystem;
	}

	#endregion

	[Fact]
	public void New_Next_ShouldReturnDifferentValues()
	{
		List<int> results = new();

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
		List<int> results = new();

		for (int i = 0; i < 100; i++)
		{
			results.Add(RandomSystem.Random.New(seed).Next());
		}

		results.Should().AllBeEquivalentTo(results.First());
	}

	[Fact]
	public void New_Shared_ShouldReturnDifferentValues()
	{
		List<int> results = new();

		for (int i = 0; i < 100; i++)
		{
			results.Add(RandomSystem.Random.Shared.Next());
		}

		results.Should().OnlyHaveUniqueItems();
	}
}