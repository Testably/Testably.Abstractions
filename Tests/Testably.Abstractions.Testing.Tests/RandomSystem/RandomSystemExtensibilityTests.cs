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
	public void Guid_ShouldSetExtensionPoint(IRandomSystem randomSystem)
	{
		IGuid sut = randomSystem.Guid;

		IRandomSystem result = sut.RandomSystem;

		result.Should().Be(randomSystem);
	}

	[Theory]
	[MemberData(nameof(GetRandomSystems))]
	public void RandomFactory_ShouldSetExtensionPoint(IRandomSystem randomSystem)
	{
		IRandomFactory sut = randomSystem.Random;

		IRandomSystem result = sut.RandomSystem;

		result.Should().Be(randomSystem);
	}
}
