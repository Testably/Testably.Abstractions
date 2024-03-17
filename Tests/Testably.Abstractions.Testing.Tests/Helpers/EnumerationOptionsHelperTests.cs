using System.IO;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class EnumerationOptionsHelperTests
{
	[Fact]
	public void FromSearchOption_InvalidValue_ShouldThrowArgumentOutOfRangeException()
	{
		SearchOption invalidSearchOption = (SearchOption)(-1);

		Exception? exception = Record.Exception(() =>
		{
			EnumerationOptionsHelper.FromSearchOption(invalidSearchOption);
		});

		exception.Should().BeOfType<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("searchOption");
	}

	[Fact]
	public void MatchesPattern_InvalidMatchType_ShouldThrowArgumentOutOfRangeException()
	{
		EnumerationOptions invalidEnumerationOptions = new()
		{
			MatchType = (MatchType)(-1)
		};

		Exception? exception = Record.Exception(() =>
		{
			EnumerationOptionsHelper.MatchesPattern(new Execute(new MockFileSystem()),
				invalidEnumerationOptions,
				"foo", "*");
		});

		exception.Should().BeOfType<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("enumerationOptions");
	}
}
