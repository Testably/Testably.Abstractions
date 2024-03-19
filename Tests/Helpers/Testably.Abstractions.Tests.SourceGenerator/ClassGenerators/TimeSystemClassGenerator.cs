using System.Text;
using Testably.Abstractions.Tests.SourceGenerator.Model;
// ReSharper disable StringLiteralTypo

namespace Testably.Abstractions.Tests.SourceGenerator.ClassGenerators;

internal class TimeSystemClassGenerator : ClassGeneratorBase
{
	/// <inheritdoc cref="ClassGeneratorBase.Marker" />
	public override string Marker
		=> "TimeSystemTestBase<TTimeSystem>";

	/// <inheritdoc cref="ClassGeneratorBase.GenerateSource(StringBuilder, ClassModel)" />
	protected override void GenerateSource(StringBuilder sourceBuilder, ClassModel @class)
		=> sourceBuilder.Append(@$"
using Testably.Abstractions.TestHelpers;
using Testably.Abstractions.TestHelpers.Settings;
using Xunit.Abstractions;

namespace {@class.Namespace}
{{
	public abstract partial class {@class.Name}<TTimeSystem>
	{{
		protected {@class.Name}(TTimeSystem timeSystem)
			: base(timeSystem)
		{{
		}}
	}}
}}

namespace {@class.Namespace}.{@class.Name}
{{
	// ReSharper disable once UnusedMember.Global
	public sealed class MockTimeSystemTests : {@class.Name}<MockTimeSystem>
	{{
		public MockTimeSystemTests() : base(new MockTimeSystem(Testing.TimeProvider.Now()))
		{{
		}}

		/// <inheritdoc cref=""{@class.Name}{{TTimeSystem}}.SkipIfBrittleTestsShouldBeSkipped(bool)"" />
		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		{{
			// Brittle tests are never skipped against the mock time system!
		}}
	}}

	// ReSharper disable once UnusedMember.Global
	[Collection(nameof(RealTimeSystemTests))]
	public sealed class RealTimeSystemTests : {@class.Name}<RealTimeSystem>
	{{
		private readonly TestSettingsFixture _fixture;

		public RealTimeSystemTests(TestSettingsFixture fixture) : base(new RealTimeSystem())
		{{
			_fixture = fixture;
		}}

#if DEBUG
		/// <inheritdoc cref=""{@class.Name}{{TTimeSystem}}.SkipIfBrittleTestsShouldBeSkipped(bool)"" />
		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
			=> Skip.If(condition && _fixture.BrittleTests != TestSettingStatus.AlwaysEnabled,
				$""Brittle tests are {{_fixture.BrittleTests}}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.BrittleTests."");
#else
		/// <inheritdoc cref=""{@class.Name}{{TTimeSystem}}.SkipIfBrittleTestsShouldBeSkipped(bool)"" />
		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
			=> Skip.If(condition && _fixture.BrittleTests == TestSettingStatus.AlwaysDisabled,
				$""Brittle tests are {{_fixture.BrittleTests}}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.BrittleTests."");
#endif
	}}
}}");
}
