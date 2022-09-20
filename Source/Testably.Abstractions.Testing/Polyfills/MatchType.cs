#if !FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
// ReSharper disable once CheckNamespace
namespace System.IO;

/// <summary>
///     Specifies the type of wildcard matching to use.
/// </summary>
public enum MatchType
{
    /// <summary>
    ///     <para>Matches using '*' and '?' wildcards.</para>
    ///     <para>
    ///         <c>*</c> matches from zero to any amount of characters. <c>?</c> matches exactly one character. <c>*.*</c>
    ///         matches any name with a period in it (with <see cref="MatchType.Win32" />, this would match all items).
    ///     </para>
    /// </summary>
    Simple,

    /// <summary>
    ///     <para>Match using Win32 DOS style matching semantics.</para>
    ///     <para>
    ///         '*', '?', '&lt;', '&gt;', and '"' are all considered wildcards. Matches in a traditional DOS <c>/</c> Windows
    ///         command prompt way. <c>*.*</c> matches all files. <c>?</c> matches collapse to periods. <c>file.??t</c> will
    ///         match <c>file.t</c>, <c>file.at</c>, and <c>file.txt</c>.
    ///     </para>
    /// </summary>
    Win32
}
#endif