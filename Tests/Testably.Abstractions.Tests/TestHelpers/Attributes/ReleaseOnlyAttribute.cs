namespace Testably.Abstractions.Tests.TestHelpers.Attributes;

public class ReleaseOnlyAttribute : RuntimeSwitchAttribute
{
    public ReleaseOnlyAttribute()
#if DEBUG
        : base(false)
#else
        : base(true)
#endif
    {
    }
}