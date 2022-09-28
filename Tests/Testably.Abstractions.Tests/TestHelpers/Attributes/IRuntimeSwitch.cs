namespace Testably.Abstractions.Tests.TestHelpers.Attributes;

/// <summary>
///     Enables tests at runtime.
/// </summary>
public interface IRuntimeSwitch
{
    /// <summary>
    ///     Flag indicating if the test is enabled.
    /// </summary>
    bool IsEnabled { get; }
}