using System.Text;
using Testably.Abstractions.Tests.SourceGenerator.Model;

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
using Testably.Abstractions.Tests.TestHelpers;
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
	public sealed class MockFileSystemTests : {@class.Name}<MockFileSystem>
	{{
		/// <inheritdoc cref=""{@class.Name}{{TFileSystem}}.BasePath"" />
		public override string BasePath => _directoryCleaner.BasePath;

		private readonly IDirectoryCleaner _directoryCleaner;

		public MockFileSystemTests() : this(new MockFileSystem())
		{{
		}}

		private MockFileSystemTests(MockFileSystem mockFileSystem) : base(
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
}}

#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM

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
			   .SetCurrentDirectoryToEmptyTemporaryDirectory(testOutputHelper.WriteLine);
		}}

		/// <inheritdoc cref=""IDisposable.Dispose()"" />
		public void Dispose()
			=> _directoryCleaner.Dispose();
	}}
}}
#endif");
}
