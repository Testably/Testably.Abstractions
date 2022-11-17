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
		public MockTimeSystemTests() : base(new MockTimeSystem(TimeProvider.Now()))
		{{
		}}
	}}

	// ReSharper disable once UnusedMember.Global
	public sealed class RealTimeSystemTests : {@class.Name}<RealTimeSystem>
	{{
		public RealTimeSystemTests() : base(new RealTimeSystem())
		{{
		}}
	}}
}}");
}
