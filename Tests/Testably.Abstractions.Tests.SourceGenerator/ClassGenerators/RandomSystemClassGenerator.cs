using System.Text;
using Testably.Abstractions.Tests.SourceGenerator.Model;

namespace Testably.Abstractions.Tests.SourceGenerator.ClassGenerators;

internal class RandomSystemClassGenerator : ClassGeneratorBase
{
	/// <inheritdoc cref="ClassGeneratorBase.Marker" />
	public override string Marker
		=> "RandomSystemTestBase<TRandomSystem>";

	/// <inheritdoc cref="ClassGeneratorBase.GenerateSource(StringBuilder, ClassModel)" />
	protected override void GenerateSource(StringBuilder sourceBuilder, ClassModel @class)
		=> sourceBuilder.Append(@$"
using Testably.Abstractions.Tests.TestHelpers;
using Xunit.Abstractions;

namespace {@class.Namespace}
{{
	public abstract partial class {@class.Name}<TRandomSystem>
	{{
		protected {@class.Name}(TRandomSystem randomSystem)
			: base(randomSystem)
		{{
		}}
	}}
}}

namespace {@class.Namespace}.{@class.Name}
{{
	// ReSharper disable once UnusedMember.Global
	public sealed class MockRandomSystemTests : {@class.Name}<MockRandomSystem>
	{{
		public MockRandomSystemTests() : base(new MockRandomSystem())
		{{
		}}
	}}

	// ReSharper disable once UnusedMember.Global
	public sealed class RealRandomSystemTests : {@class.Name}<RealRandomSystem>
	{{
		public RealRandomSystemTests() : base(new RealRandomSystem())
		{{
		}}
	}}
}}");
}
