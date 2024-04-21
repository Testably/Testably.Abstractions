using System.Text;
using Testably.Abstractions.Tests.SourceGenerator.Model;

namespace Testably.Abstractions.Tests.SourceGenerator.ClassGenerators;

// ReSharper disable StringLiteralTypo
internal class FileSystemClassGenerator : ClassGeneratorBase
{
	/// <inheritdoc cref="ClassGeneratorBase.Marker" />
	public override string Marker
		=> "FileSystemTestBase<TFileSystem>";

	/// <inheritdoc cref="ClassGeneratorBase.GenerateSource(StringBuilder, ClassModel)" />
	protected override void GenerateSource(StringBuilder sourceBuilder, ClassModel @class)
	{
		sourceBuilder.Append(@$"
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Initializer;
using Testably.Abstractions.TestHelpers;
using Testably.Abstractions.TestHelpers.Settings;
using Xunit.Abstractions;

namespace {@class.Namespace}
{{
	public abstract partial class {@class.Name}<TFileSystem>
	{{
		protected {@class.Name}(Test test, TFileSystem fileSystem, ITimeSystem timeSystem)
			: base(test, fileSystem, timeSystem)
		{{
		}}
	}}
}}

namespace {@class.Namespace}.{@class.Name}
{{
	// ReSharper disable once UnusedMember.Global
	public sealed class MockFileSystemTests : {@class.Name}<MockFileSystem>, IDisposable
	{{
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.BasePath"" />
		public override string BasePath => _directoryCleaner.BasePath;

		private readonly IDirectoryCleaner _directoryCleaner;

		public MockFileSystemTests() : this(new MockFileSystem())
		{{
		}}

		private MockFileSystemTests(MockFileSystem mockFileSystem) : base(
			new Test(),
			mockFileSystem,
			mockFileSystem.TimeSystem)
		{{
			_directoryCleaner = FileSystem
			   .SetCurrentDirectoryToEmptyTemporaryDirectory();
		}}

		/// <inheritdoc cref=""IDisposable.Dispose()"" />
		public void Dispose()
			=> _directoryCleaner.Dispose();

		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.SkipIfBrittleTestsShouldBeSkipped(bool)"" />
		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		{{
			// Brittle tests are never skipped against the mock file system!
		}}

		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.LongRunningTestsShouldBeSkipped()"" />
		public override void SkipIfLongRunningTestsShouldBeSkipped()
		{{
			// Long-running tests are never skipped against the mock file system!
		}}
	}}
}}

namespace {@class.Namespace}.{@class.Name}
{{
	// ReSharper disable once UnusedMember.Global
	[Collection(nameof(RealFileSystemTests))]
	public sealed class RealFileSystemTests : {@class.Name}<RealFileSystem>, IDisposable
	{{
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.BasePath"" />
		public override string BasePath => _directoryCleaner.BasePath;

		private readonly IDirectoryCleaner _directoryCleaner;
		private readonly TestSettingsFixture _fixture;

		public RealFileSystemTests(ITestOutputHelper testOutputHelper, TestSettingsFixture fixture)
			: base(new Test(), new RealFileSystem(), new RealTimeSystem())
		{{
#if DEBUG
			if (fixture.RealFileSystemTests != TestSettingStatus.AlwaysEnabled)
			{{
				throw new SkipException($""Tests against the real file system are {{fixture.RealFileSystemTests}}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.RealFileSystemTests."");
			}}
#else
			if (fixture.RealFileSystemTests == TestSettingStatus.AlwaysDisabled)
			{{
				throw new SkipException($""Tests against the real file system are {{fixture.RealFileSystemTests}}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.RealFileSystemTests."");
			}}
#endif
			_fixture = fixture;
			_directoryCleaner = FileSystem
			   .SetCurrentDirectoryToEmptyTemporaryDirectory($""{@class.Namespace}{{FileSystem.Path.DirectorySeparatorChar}}{@class.Name}-"", testOutputHelper.WriteLine);
		}}

		/// <inheritdoc cref=""IDisposable.Dispose()"" />
		public void Dispose()
			=> _directoryCleaner.Dispose();

#if DEBUG
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.SkipIfBrittleTestsShouldBeSkipped(bool)"" />
		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
			=> Skip.If(condition && _fixture.BrittleTests != TestSettingStatus.AlwaysEnabled,
				$""Brittle tests are {{_fixture.BrittleTests}}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.BrittleTests."");
#else
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.SkipIfBrittleTestsShouldBeSkipped(bool)"" />
		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
			=> Skip.If(condition && _fixture.BrittleTests == TestSettingStatus.AlwaysDisabled,
				$""Brittle tests are {{_fixture.BrittleTests}}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.BrittleTests."");
#endif

#if DEBUG
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.LongRunningTestsShouldBeSkipped()"" />
		public override void SkipIfLongRunningTestsShouldBeSkipped()
			=> Skip.If(_fixture.LongRunningTests != TestSettingStatus.AlwaysEnabled,
				$""Long-running tests are {{_fixture.LongRunningTests}}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.LongRunningTests."");
#else
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.LongRunningTestsShouldBeSkipped()"" />
		public override void SkipIfLongRunningTestsShouldBeSkipped()
			=> Skip.If(_fixture.LongRunningTests == TestSettingStatus.AlwaysDisabled,
				$""Long-running tests are {{_fixture.LongRunningTests}}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.LongRunningTests."");
#endif
	}}
}}

#if !NETFRAMEWORK
namespace {@class.Namespace}.{@class.Name}
{{
	// ReSharper disable once UnusedMember.Global
	public sealed class LinuxFileSystemTests : {@class.Name}<MockFileSystem>, IDisposable
	{{
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.BasePath"" />
		public override string BasePath => _directoryCleaner.BasePath;

		private readonly IDirectoryCleaner _directoryCleaner;

		public LinuxFileSystemTests() : this(new MockFileSystem(i =>
			i.SimulatingOperatingSystem(SimulationMode.Linux)))
		{{
		}}

		private LinuxFileSystemTests(MockFileSystem mockFileSystem) : base(
			new Test(OSPlatform.Linux),
			mockFileSystem,
			mockFileSystem.TimeSystem)
		{{
			_directoryCleaner = FileSystem
			   .SetCurrentDirectoryToEmptyTemporaryDirectory();
		}}

		/// <inheritdoc cref=""IDisposable.Dispose()"" />
		public void Dispose()
			=> _directoryCleaner.Dispose();

		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.SkipIfBrittleTestsShouldBeSkipped(bool)"" />
		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		{{
			// Brittle tests are never skipped against the mock file system!
		}}
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.LongRunningTestsShouldBeSkipped()"" />
		public override void SkipIfLongRunningTestsShouldBeSkipped()
		{{
			// Long-running tests are never skipped against the mock file system!
		}}
	}}
#endif

#if !NETFRAMEWORK
	// ReSharper disable once UnusedMember.Global
	public sealed class MacFileSystemTests : {@class.Name}<MockFileSystem>, IDisposable
	{{
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.BasePath"" />
		public override string BasePath => _directoryCleaner.BasePath;

		private readonly IDirectoryCleaner _directoryCleaner;

		public MacFileSystemTests() : this(new MockFileSystem(i =>
			i.SimulatingOperatingSystem(SimulationMode.MacOS)))
		{{
		}}
		private MacFileSystemTests(MockFileSystem mockFileSystem) : base(
			new Test(OSPlatform.OSX),
			mockFileSystem,
			mockFileSystem.TimeSystem)
		{{
			_directoryCleaner = FileSystem
			   .SetCurrentDirectoryToEmptyTemporaryDirectory();
		}}

		/// <inheritdoc cref=""IDisposable.Dispose()"" />
		public void Dispose()
			=> _directoryCleaner.Dispose();

		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.SkipIfBrittleTestsShouldBeSkipped(bool)"" />
		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		{{
			// Brittle tests are never skipped against the mock file system!
		}}
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.LongRunningTestsShouldBeSkipped()"" />
		public override void SkipIfLongRunningTestsShouldBeSkipped()
		{{
			// Long-running tests are never skipped against the mock file system!
		}}
	}}
#endif

#if !NETFRAMEWORK
	// ReSharper disable once UnusedMember.Global
	public sealed class WindowsFileSystemTests : {@class.Name}<MockFileSystem>, IDisposable
	{{
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.BasePath"" />
		public override string BasePath => _directoryCleaner.BasePath;

		private readonly IDirectoryCleaner _directoryCleaner;

		public WindowsFileSystemTests() : this(new MockFileSystem(i =>
			i.SimulatingOperatingSystem(SimulationMode.Windows)))
		{{
		}}
		private WindowsFileSystemTests(MockFileSystem mockFileSystem) : base(
			new Test(OSPlatform.Windows),
			mockFileSystem,
			mockFileSystem.TimeSystem)
		{{
			_directoryCleaner = FileSystem
			   .SetCurrentDirectoryToEmptyTemporaryDirectory();
		}}

		/// <inheritdoc cref=""IDisposable.Dispose()"" />
		public void Dispose()
			=> _directoryCleaner.Dispose();

		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.SkipIfBrittleTestsShouldBeSkipped(bool)"" />
		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		{{
			// Brittle tests are never skipped against the mock file system!
		}}
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.LongRunningTestsShouldBeSkipped()"" />
		public override void SkipIfLongRunningTestsShouldBeSkipped()
		{{
			// Long-running tests are never skipped against the mock file system!
		}}
	}}
}}
#endif");
	}
}
