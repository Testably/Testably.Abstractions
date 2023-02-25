using System.Collections.Generic;

namespace Testably.Abstractions.Testing.Tests.RandomSystem;

public class RandomSystemExtensibilityTests
{
	[SkippableTheory]
	[MemberData(nameof(GetRandomSystems))]
	public void Guid_ShouldSetExtensionPoint(IRandomSystem randomSystem)
	{
		IRandomSystem result = randomSystem.Guid.RandomSystem;

		result.Should().Be(randomSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetRandomSystems))]
	public void Random_ShouldSetExtensionPoint(IRandomSystem randomSystem)
	{
		IRandomSystem result = randomSystem.Random.RandomSystem;

		result.Should().Be(randomSystem);
	}

	public static IEnumerable<object[]> GetRandomSystems =>
		new List<object[]>
		{
			new object[]
			{
				new RealRandomSystem()
			},
			new object[]
			{
				new MockRandomSystem()
			},
		};
}
