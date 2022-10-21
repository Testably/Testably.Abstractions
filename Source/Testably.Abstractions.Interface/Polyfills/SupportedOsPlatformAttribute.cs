#if NETSTANDARD2_0 || NETSTANDARD2_1

// ReSharper disable once CheckNamespace
namespace System.Runtime.Versioning;

/// <summary>
///     Records the operating system (and minimum version) that supports an API. Multiple attributes can be
///     applied to indicate support on multiple operating systems.
/// </summary>
/// <remarks>
///     Callers can apply a <see cref="System.Runtime.Versioning.SupportedOSPlatformAttribute " />
///     or use guards to prevent calls to APIs on unsupported operating systems.
///     A given platform should only be specified once.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly |
                AttributeTargets.Class |
                AttributeTargets.Constructor |
                AttributeTargets.Enum |
                AttributeTargets.Event |
                AttributeTargets.Field |
                AttributeTargets.Interface |
                AttributeTargets.Method |
                AttributeTargets.Module |
                AttributeTargets.Property |
                AttributeTargets.Struct,
	AllowMultiple = true, Inherited = false)]
public sealed class SupportedOSPlatformAttribute : Attribute
{
	public string PlatformName { get; }

	public SupportedOSPlatformAttribute(string platformName)
	{
		PlatformName = platformName;
	}
}

#endif