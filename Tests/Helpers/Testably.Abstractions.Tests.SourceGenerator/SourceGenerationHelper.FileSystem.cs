using System;
using System.Text;

namespace Testably.Abstractions.Tests.SourceGenerator;

#pragma warning disable MA0051
#pragma warning disable MA0028
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

		                namespace {{model.Namespace}}
		                {
		                	public abstract partial class {{model.Name}}
		                	{
		                		/// <summary>
		                		///     The delay in milliseconds when wanting to ensure a timeout in the test.
		                		/// </summary>
		                		#if DEBUG
		                		public const int EnsureTimeout = 500;
		                		#else
		                		public const int EnsureTimeout = 2000;
		                		#endif
		                
		                		/// <summary>
		                		///     The delay in milliseconds when expecting a success in the test.
		                		/// </summary>
		                		public const int ExpectSuccess = 30000;
		                
		                		/// <summary>
		                		///     The delay in milliseconds when expecting a timeout in the test.
		                		/// </summary>
		                		public const int ExpectTimeout = 30;
		                
		                		public abstract string BasePath { get; }
		                		public IFileSystem FileSystem { get; }
		                		public Test Test { get; }
		                		public ITimeSystem TimeSystem { get; }
		                
		                		protected {{model.Name}}(Test test, IFileSystem fileSystem, ITimeSystem timeSystem)
		                		{
		                			Test = test;
		                			FileSystem = fileSystem;
		                			TimeSystem = timeSystem;
		                		}
		                
		                		/// <summary>
		                		///     Specifies, if brittle tests should be skipped on the real file system.
		                		/// </summary>
		                		/// <param name="condition">
		                		///     (optional) A condition that must be <see langword="true" /> for the test to be skipped on the
		                		///     real file system.
		                		/// </param>
		                		public abstract void SkipIfBrittleTestsShouldBeSkipped(bool condition = true);
		                
		                		/// <summary>
		                		///     Specifies, if long-running tests should be skipped on the real file system.
		                		/// </summary>
		                		public abstract void SkipIfLongRunningTestsShouldBeSkipped();
		                
		                
		                		// ReSharper disable once UnusedMember.Global
		                		public sealed class MockFileSystemTests : {{model.Name}}, IDisposable
		                		{
		                			/// <inheritdoc cref="{{model.Name}}.BasePath" />
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
		                
		                			/// <inheritdoc cref="{{model.Name}}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
		                			public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		                			{
		                				// Brittle tests are never skipped against the mock file system!
		                			}
		                
		                			/// <inheritdoc cref="{{model.Name}}.LongRunningTestsShouldBeSkipped()" />
		                			public override void SkipIfLongRunningTestsShouldBeSkipped()
		                			{
		                				// Long-running tests are never skipped against the mock file system!
		                			}
		                		}
		                		// ReSharper disable once UnusedMember.Global
		                		[Collection("RealFileSystemTests")]
		                		public sealed class RealFileSystemTests : {{model.Name}}, IDisposable
		                		{
		                			/// <inheritdoc cref="{{model.Name}}.BasePath" />
		                			public override string BasePath => _directoryCleaner.BasePath;
		                
		                			private readonly IDirectoryCleaner _directoryCleaner;
		                			private readonly TestSettingsFixture _fixture;
		                
		                			public RealFileSystemTests(ITestOutputHelper testOutputHelper, TestSettingsFixture fixture)
		                				: base(new Test(), new RealFileSystem(), new RealTimeSystem())
		                			{
		                #if DEBUG
		                				if (fixture.RealFileSystemTests != TestSettingStatus.AlwaysEnabled)
		                				{
		                					aweXpect.Skip.Test($"Tests against the real file system are {fixture.RealFileSystemTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.RealFileSystemTests.");
		                				}
		                #else
		                				if (fixture.RealFileSystemTests == TestSettingStatus.AlwaysDisabled)
		                				{
		                					aweXpect.Skip.Test($"Tests against the real file system are {fixture.RealFileSystemTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.RealFileSystemTests.");
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
		                			/// <inheritdoc cref="{{model.Name}}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
		                			public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		                				=> aweXpect.Skip.When(condition && _fixture.BrittleTests != TestSettingStatus.AlwaysEnabled,
		                					$"Brittle tests are {_fixture.BrittleTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.BrittleTests.");
		                #else
		                			/// <inheritdoc cref="{{model.Name}}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
		                			public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		                				=> aweXpect.Skip.When(condition && _fixture.BrittleTests == TestSettingStatus.AlwaysDisabled,
		                					$"Brittle tests are {_fixture.BrittleTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.BrittleTests.");
		                #endif

		                #if DEBUG
		                			/// <inheritdoc cref="{{model.Name}}.LongRunningTestsShouldBeSkipped()" />
		                			public override void SkipIfLongRunningTestsShouldBeSkipped()
		                				=> aweXpect.Skip.When(_fixture.LongRunningTests != TestSettingStatus.AlwaysEnabled,
		                					$"Long-running tests are {_fixture.LongRunningTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.LongRunningTests.");
		                #else
		                			/// <inheritdoc cref="{{model.Name}}.LongRunningTestsShouldBeSkipped()" />
		                			public override void SkipIfLongRunningTestsShouldBeSkipped()
		                				=> aweXpect.Skip.When(_fixture.LongRunningTests == TestSettingStatus.AlwaysDisabled,
		                					$"Long-running tests are {_fixture.LongRunningTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.LongRunningTests.");
		                #endif
		                		}
		                """);
		if (IncludeSimulatedTests(model))
		{
			sb.AppendLine($$"""

			                #if !NETFRAMEWORK
			                		// ReSharper disable once UnusedMember.Global
			                		public sealed class LinuxFileSystemTests : {{model.Name}}, IDisposable
			                		{
			                			/// <inheritdoc cref="{{model.Name}}.BasePath" />
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
			                
			                			/// <inheritdoc cref="{{model.Name}}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
			                			public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
			                			{
			                				// Brittle tests are never skipped against the mock file system!
			                			}
			                			/// <inheritdoc cref="{{model.Name}}.LongRunningTestsShouldBeSkipped()" />
			                			public override void SkipIfLongRunningTestsShouldBeSkipped()
			                			{
			                				// Long-running tests are never skipped against the mock file system!
			                			}
			                		}
			                #endif

			                #if !NETFRAMEWORK
			                		// ReSharper disable once UnusedMember.Global
			                		public sealed class MacFileSystemTests : {{model.Name}}, IDisposable
			                		{
			                			/// <inheritdoc cref="{{model.Name}}.BasePath" />
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
			                
			                			/// <inheritdoc cref="{{model.Name}}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
			                			public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
			                			{
			                				// Brittle tests are never skipped against the mock file system!
			                			}
			                			/// <inheritdoc cref="{{model.Name}}.LongRunningTestsShouldBeSkipped()" />
			                			public override void SkipIfLongRunningTestsShouldBeSkipped()
			                			{
			                				// Long-running tests are never skipped against the mock file system!
			                			}
			                		}
			                #endif

			                #if !NETFRAMEWORK
			                		// ReSharper disable once UnusedMember.Global
			                		public sealed class WindowsFileSystemTests : {{model.Name}}, IDisposable
			                		{
			                			/// <inheritdoc cref="{{model.Name}}.BasePath" />
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
			                
			                			/// <inheritdoc cref="{{model.Name}}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
			                			public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
			                			{
			                				// Brittle tests are never skipped against the mock file system!
			                			}
			                			/// <inheritdoc cref="{{model.Name}}.LongRunningTestsShouldBeSkipped()" />
			                			public override void SkipIfLongRunningTestsShouldBeSkipped()
			                			{
			                				// Long-running tests are never skipped against the mock file system!
			                			}
			                		}
			                #endif
			                """);
		}

		sb.AppendLine("""
		              	}
		              }
		              """);
		return sb.ToString();

		static bool IncludeSimulatedTests(ClassModel model)
			=> !model.Namespace.Equals(
				"Testably.Abstractions.AccessControl.Tests", StringComparison.Ordinal);
	}
}
#pragma warning restore MA0028
#pragma warning restore MA0051
