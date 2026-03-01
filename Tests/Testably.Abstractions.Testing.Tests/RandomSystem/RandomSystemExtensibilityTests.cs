using System.Collections.Generic;
using Testably.Abstractions.RandomSystem;

namespace Testably.Abstractions.Testing.Tests.RandomSystem;

public class RandomSystemExtensibilityTests
{
	public static IEnumerable<IRandomSystem> GetRandomSystems()
	{
		yield return new RealRandomSystem();
		yield return new MockRandomSystem();
	}

	[Test]
	[MethodDataSource(nameof(GetRandomSystems))]
	public async Task Guid_ShouldSetExtensionPoint(IRandomSystem randomSystem)
	{
		IGuid sut = randomSystem.Guid;

		IRandomSystem result = sut.RandomSystem;

		await That(result).IsEqualTo(randomSystem);
	}

	[Test]
	[MethodDataSource(nameof(GetRandomSystems))]
	public async Task RandomFactory_ShouldSetExtensionPoint(IRandomSystem randomSystem)
	{
		IRandomFactory sut = randomSystem.Random;

		IRandomSystem result = sut.RandomSystem;

		await That(result).IsEqualTo(randomSystem);
	}
}
