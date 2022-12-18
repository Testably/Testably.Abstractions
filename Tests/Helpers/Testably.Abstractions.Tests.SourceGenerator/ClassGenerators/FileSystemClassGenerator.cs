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
using System.IO.Abstractions.TestingHelpers;
using Xunit.Abstractions;

namespace {@class.Namespace}
{{
	public abstract partial class {@class.Name}<TFileSystem>
	{{
		protected {@class.Name}(TFileSystem fileSystem, ITimeSystem timeSystem)
			: base(fileSystem, timeSystem)
		{{
		}}
	}}
}}

namespace {@class.Namespace}.{@class.Name}
{{
	// ReSharper disable once UnusedMember.Global
	public sealed class MockFileSystemTests : {@class.Name}<Testably.Abstractions.Testing.MockFileSystem>
	{{
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.BasePath"" />
		public override string BasePath => _directoryCleaner.BasePath;

		private readonly IDirectoryCleaner _directoryCleaner;

		public MockFileSystemTests() : this(new Testably.Abstractions.Testing.MockFileSystem())
		{{
		}}

		private MockFileSystemTests(Testably.Abstractions.Testing.MockFileSystem mockFileSystem) : base(
			mockFileSystem,
			mockFileSystem.TimeSystem)
		{{
			_directoryCleaner = FileSystem
			   .SetCurrentDirectoryToEmptyTemporaryDirectory();
		}}

		/// <inheritdoc cref=""IDisposable.Dispose()"" />
		public void Dispose()
			=> _directoryCleaner.Dispose();
	}}

	// ReSharper disable once UnusedMember.Global
	public sealed class TestableIoMockFileSystemTests : {@class.Name}<System.IO.Abstractions.TestingHelpers.MockFileSystem>
	{{
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.BasePath"" />
		public override string BasePath => _directoryCleaner.BasePath;

		private readonly IDirectoryCleaner _directoryCleaner;

		public TestableIoMockFileSystemTests() : this(new System.IO.Abstractions.TestingHelpers.MockFileSystem())
		{{
		}}

		private TestableIoMockFileSystemTests(System.IO.Abstractions.TestingHelpers.MockFileSystem mockFileSystem) : base(
			mockFileSystem,
			new RealTimeSystem())
		{{
			_directoryCleaner = FileSystem
			   .SetCurrentDirectoryToEmptyTemporaryDirectory();
		}}

		/// <inheritdoc cref=""IDisposable.Dispose()"" />
		public void Dispose()
			=> _directoryCleaner.Dispose();
	}}
}}

#if !DEBUG || ENABLE_REALFILESYSTEMTESTS_IN_DEBUG

namespace {@class.Namespace}.{@class.Name}
{{
	// ReSharper disable once UnusedMember.Global
	[Collection(nameof(RealFileSystemTests))]
	public sealed class RealFileSystemTests : {@class.Name}<RealFileSystem>
	{{
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.BasePath"" />
		public override string BasePath => _directoryCleaner.BasePath;

		private readonly IDirectoryCleaner _directoryCleaner;

		public RealFileSystemTests(ITestOutputHelper testOutputHelper)
			: base(new RealFileSystem(), new RealTimeSystem())
		{{
			_directoryCleaner = FileSystem
			   .SetCurrentDirectoryToEmptyTemporaryDirectory($""{@class.Namespace}{{FileSystem.Path.DirectorySeparatorChar}}{@class.Name}-"", testOutputHelper.WriteLine);
		}}

		/// <inheritdoc cref=""IDisposable.Dispose()"" />
		public void Dispose()
			=> _directoryCleaner.Dispose();
	}}
}}
#endif");
}
