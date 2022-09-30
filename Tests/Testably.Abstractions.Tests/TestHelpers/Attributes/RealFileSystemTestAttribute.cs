namespace Testably.Abstractions.Tests.TestHelpers.Attributes;

/// <summary>
///     Disables execution of tests against the real file system in DEBUG mode.
/// </summary>
public class RealFileSystemTestAttribute : RuntimeSwitchAttribute
{
    public RealFileSystemTestAttribute()
#if DEBUG && DISABLE_TESTS_REALFILESYSTEM
        : base(true)
#else
        : base(true)
#endif
    {
    }
}