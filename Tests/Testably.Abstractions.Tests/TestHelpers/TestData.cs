using System.Collections.Generic;
using System.Text;

namespace Testably.Abstractions.Tests.TestHelpers;

public static class TestData
{
	private const string SpecialCharactersContent = "_€_Ä_Ö_Ü";

	public static IEnumerable<(string, Encoding, Encoding)> GetEncodingDifference()
	{
		yield return (SpecialCharactersContent, Encoding.ASCII, Encoding.UTF8);
	}

	public static IEnumerable<Encoding> GetEncodingsForReadAllText()
	{
		yield return new UTF32Encoding(false, true, true);
		// big endian
		yield return new UTF32Encoding(true, true, true);
		yield return new UTF8Encoding(true, true);
		yield return new ASCIIEncoding();
	}
}
