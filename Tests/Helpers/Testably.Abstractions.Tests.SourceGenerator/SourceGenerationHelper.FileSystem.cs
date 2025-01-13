using System;
using System.Text;

namespace Testably.Abstractions.Tests.SourceGenerator;

#pragma warning disable MA0051
internal static partial class SourceGenerationHelper
{
	public static string GenerateFileSystemTestClasses(ClassModel model)
	{
		StringBuilder? sb = GetSourceBuilder();
		sb.AppendLine($$"""
		                using System.Runtime.InteropServices;
		                using Testably.Abstractions.Testing.Initializer;
		                using Testably.Abstractions.TestHelpers;
		                using Testably.Abstractions.TestHelpers.Settings;
		                using Xunit.Abstractions;

		                namespace {{model.Namespace}}
		                {
		                	public abstract partial class {{model.Name}}<TFileSystem>
		                	{
		                		protected {{model.Name}}(Test test, TFileSystem fileSystem, ITimeSystem timeSystem)
		                			: base(test, fileSystem, timeSystem)
		                		{
		                		}
		                	}
		                }

		                namespace {{model.Namespace}}.{{model.Name}}
		                {
		                	// ReSharper disable once UnusedMember.Global
		                	public sealed class MockFileSystemTests : {{model.Name}}<MockFileSystem>, IDisposable
		                	{
		                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.BasePath" />
		                		public override string BasePath => _directoryCleaner.BasePath;
		                
		                		private readonly IDirectoryCleaner _directoryCleaner;
		                
		                		public MockFileSystemTests() : this(new MockFileSystem())
		                		{
		                		}
		                
		                		private MockFileSystemTests(MockFileSystem mockFileSystem) : base(
		                			new Test(),
		                			mockFileSystem,
		                			mockFileSystem.TimeSystem)
		                		{
		                			_directoryCleaner = FileSystem
		                			   .SetCurrentDirectoryToEmptyTemporaryDirectory();
		                		}
		                
		                		/// <inheritdoc cref="IDisposable.Dispose()" />
		                		public void Dispose()
		                			=> _directoryCleaner.Dispose();
		                
		                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
		                		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		                		{
		                			// Brittle tests are never skipped against the mock file system!
		                		}
		                
		                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.LongRunningTestsShouldBeSkipped()" />
		                		public override void SkipIfLongRunningTestsShouldBeSkipped()
		                		{
		                			// Long-running tests are never skipped against the mock file system!
		                		}
		                	}
		                }

		                namespace {{model.Namespace}}.{{model.Name}}
		                {
		                	// ReSharper disable once UnusedMember.Global
		                	[Collection(nameof(RealFileSystemTests))]
		                	public sealed class RealFileSystemTests : {{model.Name}}<RealFileSystem>, IDisposable
		                	{
		                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.BasePath" />
		                		public override string BasePath => _directoryCleaner.BasePath;
		                
		                		private readonly IDirectoryCleaner _directoryCleaner;
		                		private readonly TestSettingsFixture _fixture;
		                
		                		public RealFileSystemTests(ITestOutputHelper testOutputHelper, TestSettingsFixture fixture)
		                			: base(new Test(), new RealFileSystem(), new RealTimeSystem())
		                		{
		                #if DEBUG
		                			if (fixture.RealFileSystemTests != TestSettingStatus.AlwaysEnabled)
		                			{
		                				throw new Xunit.SkipException($"Tests against the real file system are {fixture.RealFileSystemTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.RealFileSystemTests.");
		                			}
		                #else
		                			if (fixture.RealFileSystemTests == TestSettingStatus.AlwaysDisabled)
		                			{
		                				throw new Xunit.SkipException($"Tests against the real file system are {fixture.RealFileSystemTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.RealFileSystemTests.");
		                			}
		                #endif
		                			_fixture = fixture;
		                			_directoryCleaner = FileSystem
		                			   .SetCurrentDirectoryToEmptyTemporaryDirectory($"{{model.Namespace}}{FileSystem.Path.DirectorySeparatorChar}{{model.Name}}-", testOutputHelper.WriteLine);
		                		}
		                
		                		/// <inheritdoc cref="IDisposable.Dispose()" />
		                		public void Dispose()
		                			=> _directoryCleaner.Dispose();

		                #if DEBUG
		                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
		                		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		                			=> Xunit.Skip.If(condition && _fixture.BrittleTests != TestSettingStatus.AlwaysEnabled,
		                				$"Brittle tests are {_fixture.BrittleTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.BrittleTests.");
		                #else
		                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
		                		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		                			=> Xunit.Skip.If(condition && _fixture.BrittleTests == TestSettingStatus.AlwaysDisabled,
		                				$"Brittle tests are {_fixture.BrittleTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.BrittleTests.");
		                #endif

		                #if DEBUG
		                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.LongRunningTestsShouldBeSkipped()" />
		                		public override void SkipIfLongRunningTestsShouldBeSkipped()
		                			=> Xunit.Skip.If(_fixture.LongRunningTests != TestSettingStatus.AlwaysEnabled,
		                				$"Long-running tests are {_fixture.LongRunningTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.LongRunningTests.");
		                #else
		                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.LongRunningTestsShouldBeSkipped()" />
		                		public override void SkipIfLongRunningTestsShouldBeSkipped()
		                			=> Xunit.Skip.If(_fixture.LongRunningTests == TestSettingStatus.AlwaysDisabled,
		                				$"Long-running tests are {_fixture.LongRunningTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.LongRunningTests.");
		                #endif
		                	}
		                }
		                """);
		if (IncludeSimulatedTests(model))
		{
			sb.AppendLine($$"""

			                #if !NETFRAMEWORK
			                namespace {{model.Namespace}}.{{model.Name}}
			                {
			                	// ReSharper disable once UnusedMember.Global
			                	public sealed class LinuxFileSystemTests : {{model.Name}}<MockFileSystem>, IDisposable
			                	{
			                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.BasePath" />
			                		public override string BasePath => _directoryCleaner.BasePath;
			                
			                		private readonly IDirectoryCleaner _directoryCleaner;
			                
			                		public LinuxFileSystemTests() : this(new MockFileSystem(o =>
			                			o.SimulatingOperatingSystem(SimulationMode.Linux)))
			                		{
			                		}
			                
			                		private LinuxFileSystemTests(MockFileSystem mockFileSystem) : base(
			                			new Test(OSPlatform.Linux),
			                			mockFileSystem,
			                			mockFileSystem.TimeSystem)
			                		{
			                			_directoryCleaner = FileSystem
			                			   .SetCurrentDirectoryToEmptyTemporaryDirectory();
			                		}
			                
			                		/// <inheritdoc cref="IDisposable.Dispose()" />
			                		public void Dispose()
			                			=> _directoryCleaner.Dispose();
			                
			                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
			                		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
			                		{
			                			// Brittle tests are never skipped against the mock file system!
			                		}
			                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.LongRunningTestsShouldBeSkipped()" />
			                		public override void SkipIfLongRunningTestsShouldBeSkipped()
			                		{
			                			// Long-running tests are never skipped against the mock file system!
			                		}
			                	}
			                #endif

			                #if !NETFRAMEWORK
			                	// ReSharper disable once UnusedMember.Global
			                	public sealed class MacFileSystemTests : {{model.Name}}<MockFileSystem>, IDisposable
			                	{
			                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.BasePath" />
			                		public override string BasePath => _directoryCleaner.BasePath;
			                
			                		private readonly IDirectoryCleaner _directoryCleaner;
			                
			                		public MacFileSystemTests() : this(new MockFileSystem(o =>
			                			o.SimulatingOperatingSystem(SimulationMode.MacOS)))
			                		{
			                		}
			                		private MacFileSystemTests(MockFileSystem mockFileSystem) : base(
			                			new Test(OSPlatform.OSX),
			                			mockFileSystem,
			                			mockFileSystem.TimeSystem)
			                		{
			                			_directoryCleaner = FileSystem
			                			   .SetCurrentDirectoryToEmptyTemporaryDirectory();
			                		}
			                
			                		/// <inheritdoc cref="IDisposable.Dispose()" />
			                		public void Dispose()
			                			=> _directoryCleaner.Dispose();
			                
			                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
			                		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
			                		{
			                			// Brittle tests are never skipped against the mock file system!
			                		}
			                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.LongRunningTestsShouldBeSkipped()" />
			                		public override void SkipIfLongRunningTestsShouldBeSkipped()
			                		{
			                			// Long-running tests are never skipped against the mock file system!
			                		}
			                	}
			                #endif

			                #if !NETFRAMEWORK
			                	// ReSharper disable once UnusedMember.Global
			                	public sealed class WindowsFileSystemTests : {{model.Name}}<MockFileSystem>, IDisposable
			                	{
			                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.BasePath" />
			                		public override string BasePath => _directoryCleaner.BasePath;
			                
			                		private readonly IDirectoryCleaner _directoryCleaner;
			                
			                		public WindowsFileSystemTests() : this(new MockFileSystem(o =>
			                			o.SimulatingOperatingSystem(SimulationMode.Windows)))
			                		{
			                		}
			                		private WindowsFileSystemTests(MockFileSystem mockFileSystem) : base(
			                			new Test(OSPlatform.Windows),
			                			mockFileSystem,
			                			mockFileSystem.TimeSystem)
			                		{
			                			_directoryCleaner = FileSystem
			                			   .SetCurrentDirectoryToEmptyTemporaryDirectory();
			                		}
			                
			                		/// <inheritdoc cref="IDisposable.Dispose()" />
			                		public void Dispose()
			                			=> _directoryCleaner.Dispose();
			                
			                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
			                		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
			                		{
			                			// Brittle tests are never skipped against the mock file system!
			                		}
			                		/// <inheritdoc cref="{{model.Name}}{TFileSystem}.LongRunningTestsShouldBeSkipped()" />
			                		public override void SkipIfLongRunningTestsShouldBeSkipped()
			                		{
			                			// Long-running tests are never skipped against the mock file system!
			                		}
			                	}
			                }
			                #endif
			                """);
		}

		return sb.ToString();

		static bool IncludeSimulatedTests(ClassModel model)
			=> !model.Namespace.Equals(
				"Testably.Abstractions.AccessControl.Tests", StringComparison.Ordinal);
	}
}
#pragma warning restore MA0051
