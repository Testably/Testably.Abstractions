using System;
using System.IO;
using System.Threading;
using Testably.Abstractions.Tests.TestHelpers;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryTests
{
    // ReSharper disable once UnusedMember.Global
    public sealed class RealFileSystem : FileSystemDirectoryTests, IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public RealFileSystem(ITestOutputHelper testOutputHelper)
            : base(new FileSystem(), new TimeSystem(),
                FileTestHelper.CreateEmptyTemporaryDirectory())
        {
            _testOutputHelper = testOutputHelper;
            _testOutputHelper.WriteLine($"Use '{BasePath}' as current directory.");
            Directory.SetCurrentDirectory(BasePath);
        }

        #region IDisposable Members

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                // It is important to reset the current directory, as otherwise deleting the BasePath
                // results in a IOException, because the process cannot access the file.
                Directory.SetCurrentDirectory(Path.GetTempPath());

                _testOutputHelper.WriteLine($"Cleaning up '{BasePath}'...");
                for (int i = 10; i >= 0; i--)
                {
                    try
                    {
                        FileTestHelper.ForceDeleteDirectory(BasePath);
                    }
                    catch (Exception)
                    {
                        if (i == 0)
                        {
                            throw;
                        }

                        _testOutputHelper.WriteLine(
                            $"  Force delete failed! Retry again {i} times in 100ms...");
                        Thread.Sleep(100);
                    }
                }

                _testOutputHelper.WriteLine($"Cleaned up '{BasePath}'.");
            }
            catch (Exception ex)
            {
                _testOutputHelper.WriteLine(
                    $"Could not clean up '{BasePath}' because: {ex}");
            }
        }

        #endregion
    }
}