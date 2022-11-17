using System.Collections.Generic;
using System.Linq;

namespace Testably.Abstractions.Tests.RandomSystem;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class RandomFactoryTests<TRandomSystem>
	: RandomSystemTestBase<TRandomSystem>
	where TRandomSystem : IRandomSystem
{
	[SkippableFact]
	public void New_Next_ShouldReturnDifferentValues()
	{
		List<int> results = new();

		for (int i = 0; i < 100; i++)
		{
			results.Add(RandomSystem.Random.New().Next());
		}

		results.Should().OnlyHaveUniqueItems();
	}

	[SkippableTheory]
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

	[SkippableFact]
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
