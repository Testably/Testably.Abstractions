using Xunit.Sdk;

namespace Testably.Abstractions.Tests.TestHelpers.Traits;

[TraitDiscoverer(
    SystemTestDiscoverer.DiscovererTypeName,
    SystemTestDiscoverer.AssemblyName)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class SystemTestAttribute : Attribute, ITraitAttribute
{
    public string System { get; }

    public SystemTestAttribute(string system)
    {
        System = system;
    }
}