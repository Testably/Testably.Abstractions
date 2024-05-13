namespace Testably.Abstractions.TestHelpers;

/// <summary>
///     Base class for generated tests.
/// </summary>
public abstract class TestBase
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
}
