namespace Testably.Abstractions.Tests.TestHelpers.Attributes;

/// <summary>
///     Disables execution of tests against the real file system in DEBUG mode
///     when the `DISABLE_TESTS_REALFILESYSTEM` variable is set
/// </summary>
public class RealFileSystemTestAttribute : RuntimeSwitchAttribute
{
    public RealFileSystemTestAttribute()
#if DEBUG && DISABLE_TESTS_REALFILESYSTEM
        : base(false)
#else
        : base(true)
#endif
    {
    }
}