using Xunit.Sdk;

namespace Testably.Abstractions.Tests.TestHelpers.Traits;

[TraitDiscoverer(
    TestabilityTraitDiscoverer.DiscovererTypeName,
    TestabilityTraitDiscoverer.AssemblyName)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class TestabilityTraitAttribute : Attribute, ITraitAttribute
{
    public string? Method { get; }
    public string Module { get; }
    public string Submodule { get; }

    public TestabilityTraitAttribute(string module, string submodule, string? method)
    {
        Module = module;
        Submodule = submodule;
        Method = method;
    }
}