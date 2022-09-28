namespace Testably.Abstractions.Tests.TestHelpers.Attributes;

public class RuntimeSwitchAttribute : Attribute, IRuntimeSwitch
{
    public RuntimeSwitchAttribute(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    public bool IsEnabled { get; }
}