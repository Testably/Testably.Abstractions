using System.Threading;

namespace Testably.Abstractions.TestHelpers;

[Retry(1)]
public abstract class RandomSystemTestBase
{
	public CancellationToken CancellationToken { get; }
		                
	public IRandomSystem RandomSystem { get; }
		                
	protected RandomSystemTestBase(RandomSystemTestData testData)
	{
		RandomSystem = testData.RandomSystem;
		CancellationToken = TestContext.Current!.Execution.CancellationToken;
	}
}

