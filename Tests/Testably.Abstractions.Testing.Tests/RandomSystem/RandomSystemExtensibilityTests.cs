using Testably.Abstractions.RandomSystem;

namespace Testably.Abstractions.Testing.Tests.RandomSystem;

public class RandomSystemExtensibilityTests
{
	#region Test Setup

	public static TheoryData<IRandomSystem> GetRandomSystems
		=> new()
		{
			(IRandomSystem)new RealRandomSystem(),
			(IRandomSystem)new MockRandomSystem(),
		};

	#endregion

	[Theory]
	[MemberData(nameof(GetRandomSystems))]
	public async Task Guid_ShouldSetExtensionPoint(IRandomSystem randomSystem)
	{
		IGuid sut = randomSystem.Guid;

		IRandomSystem result = sut.RandomSystem;

		await That(result).IsEqualTo(randomSystem);
	}

	[Theory]
	[MemberData(nameof(GetRandomSystems))]
	public async Task RandomFactory_ShouldSetExtensionPoint(IRandomSystem randomSystem)
	{
		IRandomFactory sut = randomSystem.Random;

		IRandomSystem result = sut.RandomSystem;

		await That(result).IsEqualTo(randomSystem);
	}
}
