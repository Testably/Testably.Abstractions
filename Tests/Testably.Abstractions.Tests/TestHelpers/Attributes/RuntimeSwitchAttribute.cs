namespace Testably.Abstractions.Tests.TestHelpers.Attributes;

/// <summary>
///     Base attribute that implements <see cref="IRuntimeSwitch" />.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
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

    #region IRuntimeSwitch Members

    /// <inheritdoc cref="IRuntimeSwitch.IsEnabled" />
    public bool IsEnabled { get; }

    #endregion
}