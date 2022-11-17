namespace Testably.Abstractions.Tests.RandomSystem;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TRandomSystem>
	: RandomSystemTestBase<TRandomSystem>
	where TRandomSystem : IRandomSystem
{
	[SkippableFact]
	public void Guid_ShouldSetExtensionPoint()
	{
		IRandomSystem result = RandomSystem.Guid.RandomSystem;

		result.Should().Be(RandomSystem);
	}

	[SkippableFact]
	public void Random_ShouldSetExtensionPoint()
	{
		IRandomSystem result = RandomSystem.Random.RandomSystem;

		result.Should().Be(RandomSystem);
	}
}
