using AutoFixture.Xunit2;
using FluentAssertions;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace Testably.Abstractions.Testing.Tests.File;

public abstract partial class FileSystemMockPathTests
{
    // ReSharper disable once UnusedMember.Global
    public class MockFileSystem : FileSystemMockPathTests
    {
        public MockFileSystem() : base(new FileSystemMock())
        {
        }
    }
}