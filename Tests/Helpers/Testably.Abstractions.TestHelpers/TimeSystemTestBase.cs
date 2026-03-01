using System.Threading;

namespace Testably.Abstractions.TestHelpers;

[Retry(1)]
public abstract class TimeSystemTestBase
{
	/// <summary>
	///     The delay in milliseconds when wanting to ensure a timeout in the test.
	/// </summary>
	public const int EnsureTimeout = 500;
		                
	/// <summary>
	///     The delay in milliseconds when expecting a success in the test.
	/// </summary>
	public const int ExpectSuccess = 30000;
		                
	/// <summary>
	///     The delay in milliseconds when expecting a timeout in the test.
	/// </summary>
	public const int ExpectTimeout = 30;

	/// <summary>
	///     Specifies, if brittle tests should be skipped on the real time system.
	/// </summary>
	/// <param name="condition">
	///     (optional) A condition that must be <see langword="true" /> for the test to be skipped on the
	///     real time system.
	/// </param>
	public void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
	{
		if (TimeSystem is RealTimeSystem timeSystem)
		{
#if DEBUG
			//aweXpect.Skip.When(condition && _fixture.BrittleTests != TestSettingStatus.AlwaysEnabled,
			//	$"Brittle tests are {_fixture.BrittleTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.BrittleTests.");
#else
			//aweXpect.Skip.When(condition && _fixture.BrittleTests == TestSettingStatus.AlwaysDisabled,
			//	$"Brittle tests are {_fixture.BrittleTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.BrittleTests.");
#endif
		}
	}

	public CancellationToken CancellationToken { get; }
		                
	public ITimeSystem TimeSystem { get; }
		                
	protected TimeSystemTestBase(TimeSystemTestData testData)
	{
		TimeSystem = testData.TimeSystem;
		CancellationToken = TestContext.Current!.Execution.CancellationToken;
	}
}
