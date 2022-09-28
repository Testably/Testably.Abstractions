﻿namespace Testably.Abstractions.Tests.TestHelpers.Attributes;

/// <summary>
///     Disables execution of tests against the real file system in DEBUG mode.
/// </summary>
public class RealFileSystemTestAttribute : RuntimeSwitchAttribute
{
    public RealFileSystemTestAttribute()
#if DISABLE_TESTS_REALFILESYSTEM
        : base(false)
#else
        : base(true)
#endif
    {
    }
}