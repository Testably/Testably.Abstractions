namespace Testably.Abstractions.Tests.TestHelpers.Attributes;

/// <summary>
///     Base attribute that implements <see cref="IRuntimeSwitch" />.
/// </summary>
public class RuntimeSwitchAttribute : Attribute, IRuntimeSwitch
{
    /// <summary>
    ///     Initializes a new instance of <see cref="RuntimeSwitchAttribute" /> with <see cref="IsEnabled" /> set to
    ///     <paramref name="isEnabled" />.
    /// </summary>
    public RuntimeSwitchAttribute(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    /// <inheritdoc cref="IRuntimeSwitch.IsEnabled" />
    public bool IsEnabled { get; }
}