﻿global using AutoFixture.Xunit3;
global using System;
global using System.IO.Abstractions;
global using System.Threading.Tasks;
global using Testably.Abstractions.FileSystem;
global using Testably.Abstractions.Testing;
global using Testably.Abstractions.TestHelpers;
global using Testably.Abstractions.Tests.TestHelpers;
global using Xunit;
global using aweXpect;
global using static aweXpect.Expect;
global using Skip = Testably.Abstractions.TestHelpers.Skip;
#if NET48
global using Testably.Abstractions.Polyfills;
#else
global using System.Runtime.Versioning;
#endif
