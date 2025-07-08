using System.IO;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class EnumerationOptionsHelperTests
{
	[Fact]
	public async Task FromSearchOption_InvalidValue_ShouldThrowArgumentOutOfRangeException()
	{
		SearchOption invalidSearchOption = (SearchOption)(-1);

		void Act()
		{
			EnumerationOptionsHelper.FromSearchOption(invalidSearchOption);
		}

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>().WithParamName("searchOption");
	}

	[Fact]
	public async Task MatchesPattern_InvalidMatchType_ShouldThrowArgumentOutOfRangeException()
	{
		EnumerationOptions invalidEnumerationOptions = new()
		{
			MatchType = (MatchType)(-1),
		};

		void Act()
		{
			EnumerationOptionsHelper.MatchesPattern(new Execute(new MockFileSystem()),
				invalidEnumerationOptions,
				"foo", "*");
		}

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>().WithParamName("enumerationOptions");
	}
}
