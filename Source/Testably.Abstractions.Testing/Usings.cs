#if !NET11_0_OR_GREATER
global using Testably.Abstractions.Polyfills;
#endif
#if !NETSTANDARD2_0 && !NETSTANDARD2_1
global using System.Runtime.Versioning;
#endif
global using System.IO.Abstractions;
