using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Testably.Abstractions.Tests.TestHelpers.Traits;

public class TestabilityTraitDiscoverer : ITraitDiscoverer
{
    internal const string AssemblyName = nameof(Testably) + 
                                         "." + nameof(Abstractions) +
                                         "." + nameof(Tests);

    internal const string DiscovererTypeName = AssemblyName + 
                                               "." + nameof(TestHelpers) +
                                               "." + nameof(Traits) +
                                               "." + nameof(TestabilityTraitDiscoverer);

    public IEnumerable<KeyValuePair<string, string>> GetTraits(
        IAttributeInfo traitAttribute)
    {
        string? module = traitAttribute.GetNamedArgument<string>("Module");
        string? submodule = traitAttribute.GetNamedArgument<string>("Submodule");
        string? method = traitAttribute.GetNamedArgument<string>("Method");

        yield return new KeyValuePair<string, string>(module, submodule);
        if (method != null)
        {
            yield return
                new KeyValuePair<string, string>($"{module}:{submodule}", method);
        }
    }
}