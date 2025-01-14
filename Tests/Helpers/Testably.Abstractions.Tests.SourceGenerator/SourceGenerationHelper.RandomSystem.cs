using System.Text;

namespace Testably.Abstractions.Tests.SourceGenerator;

#pragma warning disable MA0051
internal static partial class SourceGenerationHelper
{
	public static string GenerateRandomSystemTestClasses(ClassModel model)
	{
		StringBuilder? sb = GetSourceBuilder();
		sb.AppendLine($$"""
		                using Testably.Abstractions.TestHelpers;
		                using Xunit.Abstractions;

		                namespace {{model.Namespace}}
		                {
		                	public abstract partial class {{model.Name}}
		                	{
		                		public IRandomSystem RandomSystem { get; }
		                
		                		protected {{model.Name}}(IRandomSystem randomSystem)
		                		{
		                			RandomSystem = randomSystem;
		                		}
		                
		                		// ReSharper disable once UnusedMember.Global
		                		public sealed class MockRandomSystemTests : {{model.Name}}
		                		{
		                			public MockRandomSystemTests() : base(new MockRandomSystem())
		                			{
		                			}
		                		}
		                
		                		// ReSharper disable once UnusedMember.Global
		                		public sealed class RealRandomSystemTests : {{model.Name}}
		                		{
		                			public RealRandomSystemTests() : base(new RealRandomSystem())
		                			{
		                			}
		                		}
		                	}
		                }
		                """);

		return sb.ToString();
	}
}
#pragma warning restore MA0051
