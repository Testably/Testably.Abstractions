using System.Collections.Generic;
using Testably.Abstractions.RandomSystem;

namespace Testably.Abstractions.Tests.RandomSystem;

[RandomSystemTests]
public class RandomFactoryTests(RandomSystemTestData testData) : RandomSystemTestBase(testData)
{
	[Test]
	public async Task New_Next_ShouldReturnDifferentValues()
	{
		List<int> results = [];

		for (int i = 0; i < 100; i++)
		{
			results.Add(RandomSystem.Random.New().Next());
		}

		await That(results).AreAllUnique();
	}

	[Test]
	[Arguments(5)]
	public async Task New_Next_WithSeed_ShouldReturnSameValue(int seed)
	{
		List<int> results = [];

		for (int i = 0; i < 100; i++)
		{
			results.Add(RandomSystem.Random.New(seed).Next());
		}

		await That(results).All().AreEqualTo(results[0]);
	}

	[Test]
	public async Task New_Shared_ShouldReturnDifferentValues()
	{
		List<int> results = [];

		for (int i = 0; i < 100; i++)
		{
			results.Add(RandomSystem.Random.Shared.Next());
		}

		await That(results).AreAllUnique();
	}

	[Test]
	public async Task Shared_ShouldReturnSameReference()
	{
		IRandom shared1 = RandomSystem.Random.Shared;
		IRandom shared2 = RandomSystem.Random.Shared;

		await That(shared1).IsEqualTo(shared2);
	}
}
