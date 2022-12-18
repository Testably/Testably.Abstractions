global using AutoFixture.Xunit2;
global using FluentAssertions;
global using System;
global using System.IO.Abstractions;
global using Testably.Abstractions.FileSystem;
global using Testably.Abstractions.Testing;
global using Testably.Abstractions.TestHelpers;
global using Testably.Abstractions.Tests.TestHelpers;
global using Xunit;
#if NET48
global using Testably.Abstractions.Polyfills;
#else
global using System.Runtime.Versioning;
#endif
