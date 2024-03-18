using System.Text;
using Testably.Abstractions.Tests.SourceGenerator.Model;
// ReSharper disable StringLiteralTypo

namespace Testably.Abstractions.Tests.SourceGenerator.ClassGenerators;

internal class FileSystemClassGenerator : ClassGeneratorBase
{
	/// <inheritdoc cref="ClassGeneratorBase.Marker" />
	public override string Marker
		=> "FileSystemTestBase<TFileSystem>";

	/// <inheritdoc cref="ClassGeneratorBase.GenerateSource(StringBuilder, ClassModel)" />
	protected override void GenerateSource(StringBuilder sourceBuilder, ClassModel @class)
		=> sourceBuilder.Append(@$"
using Testably.Abstractions.Testing.FileSystemInitializer;
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

		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.LongRunningTestsShouldBeSkipped()"" />
		public override bool LongRunningTestsShouldBeSkipped() => false;
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
		private readonly RealFileSystemFixture _fixture;

		public RealFileSystemTests(ITestOutputHelper testOutputHelper, RealFileSystemFixture fixture)
			: base(new Test(), new RealFileSystem(), new RealTimeSystem())
		{{
#if DEBUG
			if (fixture.RealFileSystemTests != TestSettingStatus.AlwaysEnabled)
			{{
				throw new SkipException($""RealFileSystemTests are {{fixture.RealFileSystemTests}}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.RealFileSystemTests."");
			}}
#else
			if (fixture.RealFileSystemTests == TestSettingStatus.AlwaysDisabled)
			{{
				throw new SkipException($""RealFileSystemTests are {{fixture.RealFileSystemTests}}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.RealFileSystemTests."");
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
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.LongRunningTestsShouldBeSkipped()"" />
		public override bool LongRunningTestsShouldBeSkipped() => _fixture.LongRunningTests != TestSettingStatus.AlwaysEnabled;
#else
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.LongRunningTestsShouldBeSkipped()"" />
		public override bool LongRunningTestsShouldBeSkipped() => _fixture.LongRunningTests == TestSettingStatus.AlwaysDisabled;
#endif
	}}
}}");
}
