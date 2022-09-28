using System.Collections.Generic;
using Testably.Abstractions.Tests.TestHelpers.Attributes;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Testably.Abstractions.Tests.TestHelpers.Framework;

/// <summary>
/// Custom <see cref="XunitTestFrameworkDiscoverer"/> that supports the <see cref="IRuntimeSwitch"/> on attributes.
/// </summary>
public class TestablyTestFrameworkDiscoverer : XunitTestFrameworkDiscoverer
{
    public TestablyTestFrameworkDiscoverer(
        IAssemblyInfo assemblyInfo,
        ISourceInformationProvider sourceProvider,
        IMessageSink diagnosticMessageSink,
        IXunitTestCollectionFactory? collectionFactory = null)
        : base(
            assemblyInfo,
            sourceProvider,
            diagnosticMessageSink,
            collectionFactory)
    {
    }

    /// <inheritdoc />
    protected override bool IsValidTestClass(ITypeInfo type)
    {
        return base.IsValidTestClass(type) &&
               HasDisabledRuntimeSwitch(
                   type.GetCustomAttributes(typeof(IRuntimeSwitch)));
    }

    /// <inheritdoc />
    protected override bool FindTestsForMethod(ITestMethod testMethod,
                                               bool includeSourceInformation,
                                               IMessageBus messageBus,
                                               ITestFrameworkDiscoveryOptions
                                                   discoveryOptions)
    {
        return base.FindTestsForMethod(
                   testMethod,
                   includeSourceInformation,
                   messageBus,
                   discoveryOptions) &&
               HasDisabledRuntimeSwitch(
                   testMethod.Method.GetCustomAttributes(typeof(IRuntimeSwitch)));
    }

    private bool HasDisabledRuntimeSwitch(IEnumerable<IAttributeInfo?> attributeInfos)
    {
        foreach (IAttributeInfo? attributeInfo in attributeInfos)
        {
            IRuntimeSwitch? runtimeSwitch =
                (attributeInfo as ReflectionAttributeInfo)?
               .Attribute as IRuntimeSwitch;
            if (runtimeSwitch?.IsEnabled == false)
            {
                return false;
            }
        }

        return true;
    }
}