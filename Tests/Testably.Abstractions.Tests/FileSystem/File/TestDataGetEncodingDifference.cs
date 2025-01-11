using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

public class TestDataGetEncodingDifference : IEnumerable<object[]>
{
	private const string SpecialCharactersContent = "_�_�_�_�";

	#region IEnumerable<object[]> Members

	public IEnumerator<object[]> GetEnumerator()
	{
		yield return
		[
			SpecialCharactersContent, Encoding.ASCII, Encoding.UTF8,
		];
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	#endregion
}
