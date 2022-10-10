using System.IO;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing.Tests.Internal;

public class PathHelperTests
{
	[Fact]
	public void RemoveLeadingDot_MultipleLocalDirectories_ShouldBeRemoved()
	{
		string path = Path.Combine(".", ".", ".", "foo");

		string result = path.RemoveLeadingDot();

		result.Should().Be("foo");
	}
}