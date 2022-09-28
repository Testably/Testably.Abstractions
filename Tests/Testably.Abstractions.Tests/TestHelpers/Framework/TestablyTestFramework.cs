using Testably.Abstractions.Tests.TestHelpers.Attributes;
using Xunit.Abstractions;
using Xunit.Sdk;
[assembly: TestFramework(
    "Testably.Abstractions.Tests.TestHelpers.Framework.TestablyTestFramework",
    "Testably.Abstractions.Tests")]

namespace Testably.Abstractions.Tests.TestHelpers.Framework;

/// <summary>
///     Custom <see cref="XunitTestFramework" /> that uses the <see cref="TestablyTestFrameworkDiscoverer" /> to support
///     the <see cref="RuntimeSwitchAttribute" />.
/// </summary>
public class TestablyTestFramework : XunitTestFramework
{
    public TestablyTestFramework(IMessageSink messageSink)
        : base(messageSink)
    {
    }

    protected override ITestFrameworkDiscoverer CreateDiscoverer(
        IAssemblyInfo assemblyInfo)
        => new TestablyTestFrameworkDiscoverer(
            assemblyInfo,
            SourceInformationProvider,
            DiagnosticMessageSink);
}