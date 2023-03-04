using System.Collections.Generic;
using Testably.Abstractions.RandomSystem;

namespace Testably.Abstractions.Testing.Tests.RandomSystem;

public class RandomSystemExtensibilityTests
{
	[SkippableTheory]
	[MemberData(nameof(GetRandomSystems))]
	public void Guid_ShouldSetExtensionPoint(IRandomSystem randomSystem)
	{
		IGuid sut = randomSystem.Guid;

		IRandomSystem result = sut.RandomSystem;

		result.Should().Be(randomSystem);
	}

	[SkippableTheory]
	[MemberData(nameof(GetRandomSystems))]
	public void RandomFactory_ShouldSetExtensionPoint(IRandomSystem randomSystem)
	{
		IRandomFactory sut = randomSystem.Random;

		IRandomSystem result = sut.RandomSystem;

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
