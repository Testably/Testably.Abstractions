namespace Testably.Abstractions.Tests.TestHelpers.Attributes;

/// <summary>
///     Disables execution of tests against the real file system in DEBUG mode.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RealFileSystemTestAttribute : Attribute
{
    public RealFileSystemTestAttribute()
    {
    }
}