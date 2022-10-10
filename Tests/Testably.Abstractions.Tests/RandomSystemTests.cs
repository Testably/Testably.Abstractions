using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests;

public abstract class RandomSystemTests<TRandomSystem>
	where TRandomSystem : IRandomSystem
{
	#region Test Setup

	public TRandomSystem RandomSystem { get; }

	protected RandomSystemTests(TRandomSystem randomSystem)
	{
		RandomSystem = randomSystem;
	}

	#endregion

	[Fact]
	public void Guid_ShouldSetExtensionPoint()
	{
		IRandomSystem result = RandomSystem.Guid.RandomSystem;

		result.Should().Be(RandomSystem);
	}

	[Fact]
	public void Random_ShouldSetExtensionPoint()
	{
		IRandomSystem result = RandomSystem.Random.RandomSystem;

		result.Should().Be(RandomSystem);
	}
}

/// <summary>
///     Attributes for <see cref="RandomSystemTests{TRandomSystem}" />
/// </summary>
public static class RandomSystemTests
{
	/// <summary>
	///     Tests for methods in <see cref="IRandomSystem.IRandom" /> in <see cref="IRandomSystem" />.
	/// </summary>
	public class Random : TestabilityTraitAttribute
	{
		public Random(string method) : base(nameof(IRandomSystem),
			nameof(IRandomSystem.IRandom), method)
		{
		}
	}

	/// <summary>
	///     Tests for methods in <see cref="IRandomSystem.IRandomFactory" /> in <see cref="IRandomSystem" />.
	/// </summary>
	public class RandomFactory : TestabilityTraitAttribute
	{
		public RandomFactory(string method) : base(nameof(IRandomSystem),
			nameof(IRandomSystem.IRandomFactory), method)
		{
		}
	}

	/// <summary>
	///     Tests for methods in <see cref="IRandomSystem.IGuid" /> in <see cref="IRandomSystem" />.
	/// </summary>
	public class Guid : TestabilityTraitAttribute
	{
		public Guid(string method) : base(nameof(IRandomSystem),
			nameof(IRandomSystem.IGuid),
			method)
		{
		}
	}
}