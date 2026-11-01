#if !FEATURE_STRING_CHAR_COMPARISON
global using Testably.Abstractions.Polyfills;
#endif
#if !NETSTANDARD2_0 && !NETSTANDARD2_1
global using System.Runtime.Versioning;
#endif
global using System.IO.Abstractions;
