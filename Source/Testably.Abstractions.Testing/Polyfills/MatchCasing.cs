#if !FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
// ReSharper disable once CheckNamespace
namespace System.IO;

/// <summary>
///     Specifies the type of character casing to match.
/// </summary>
public enum MatchCasing
{
	/// <summary>
	///     Matches using the default casing for the given platform.
	/// </summary>
	PlatformDefault,

	/// <summary>
	///     Matches respecting character casing.
	/// </summary>
	CaseSensitive,

	/// <summary>
	///     Matches ignoring character casing.
	/// </summary>
	CaseInsensitive,
}
#endif
