#if NETSTANDARD2_0 || NETSTANDARD2_1
global using Testably.Abstractions.Polyfills;
#else
global using System.Runtime.Versioning;
#endif
global using Testably.Abstractions.FileSystem;
