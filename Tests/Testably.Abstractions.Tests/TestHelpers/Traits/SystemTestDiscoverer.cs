using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Testably.Abstractions.Tests.TestHelpers.Traits;

public class SystemTestDiscoverer : ITraitDiscoverer
{
    internal const string AssemblyName = nameof(Testably) +
                                         "." + nameof(Abstractions) +
                                         "." + nameof(Tests);

    internal const string DiscovererTypeName = AssemblyName +
                                               "." + nameof(TestHelpers) +
                                               "." + nameof(Traits) +
                                               "." + nameof(SystemTestDiscoverer);

    #region ITraitDiscoverer Members

    public IEnumerable<KeyValuePair<string, string>> GetTraits(
        IAttributeInfo traitAttribute)
    {
        string system = traitAttribute.GetNamedArgument<string>("System");

        yield return new KeyValuePair<string, string>("System", system);
    }

    #endregion
}