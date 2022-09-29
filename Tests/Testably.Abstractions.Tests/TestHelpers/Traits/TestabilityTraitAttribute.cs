using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Testably.Abstractions.Tests.TestHelpers.Traits;

[TraitDiscoverer(TestabilityTraitDiscoverer.DiscovererTypeName, TestabilityTraitDiscoverer.AssemblyName)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class TestabilityTraitAttribute : Attribute, ITraitAttribute
{
    public TestabilityTraitAttribute(string module, string submodule, string? method)
    {
        Module = module;
        Submodule = submodule;
        Method = method;
    }

    public string Module { get; private set; }
    public string Submodule { get; private set; }
    public string? Method { get; private set; }
}