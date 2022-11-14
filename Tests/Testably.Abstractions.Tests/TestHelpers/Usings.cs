global using AutoFixture.Xunit2;
global using FluentAssertions;
global using System;
global using Testably.Abstractions.Testing;
global using Testably.Abstractions.Tests.TestHelpers;
global using Xunit;
#if NET472
global using Testably.Abstractions.Polyfills;
#else
global using System.Runtime.Versioning;
#endif
