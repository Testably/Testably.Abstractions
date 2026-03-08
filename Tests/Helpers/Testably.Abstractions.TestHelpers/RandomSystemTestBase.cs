using System.Threading;

namespace Testably.Abstractions.TestHelpers;

[Retry(1)]
public abstract class RandomSystemTestBase
{
	/// <summary>
	///     A cancellation token that can be used to cancel the test execution when it takes too long.
	/// </summary>
	public CancellationToken CancellationToken { get; }

	/// <summary>
	///     The random system to test.
	/// </summary>
	public IRandomSystem RandomSystem { get; }

	protected RandomSystemTestBase(RandomSystemTestData testData)
	{
		RandomSystem = testData.RandomSystem;
		CancellationToken = TestContext.Current!.Execution.CancellationToken;
	}
}
