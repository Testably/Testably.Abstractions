namespace Testably.Abstractions.Tests.TestHelpers.Attributes;

/// <summary>
///     Disables execution of tests against the real file system in DEBUG mode.
/// </summary>
public class RealFileSystemTestAttribute : RuntimeSwitchAttribute
{
    public RealFileSystemTestAttribute()
#if DEBUG
        : base(true)
#else
        : base(true)
#endif
    {
    }
}