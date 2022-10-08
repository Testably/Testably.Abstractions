using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Testing.Internal;

internal static class Framework
{
    private static bool? _isNetFramework;

    [ExcludeFromCodeCoverage]
    public static bool IsNetFramework
    {
        get
        {
            _isNetFramework ??= RuntimeInformation
               .FrameworkDescription.StartsWith(".NET Framework");
            return _isNetFramework.Value;
        }
    }
}