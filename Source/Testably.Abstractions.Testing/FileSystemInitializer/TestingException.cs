using System;
using System.Runtime.Serialization;

namespace Testably.Abstractions.Testing.FileSystemInitializer;

/// <summary>
///     Custom <see cref="TestingException" /> when using the test system incorrectly.
/// </summary>
#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class TestingException : Exception
{
	/// <summary>
	///     Initializes a new instance of <see cref="TestingException" /> with the provided <paramref name="message" />.
	/// </summary>
	public TestingException(string message)
		: base(message)
	{
	}

	/// <summary>
	///     Initializes a new instance of <see cref="TestingException" /> with the provided <paramref name="message" /> and a
	///     <paramref name="inner" /> exception.
	/// </summary>
	public TestingException(string message, Exception inner)
		: base(message, inner)
	{
	}

#if !NET8_0_OR_GREATER
	/// <summary>
	///     Initializes a new instance of <see cref="TestingException" /> for serialization.
	///     <para />
	///     Without this constructor, deserialization will fail!
	/// </summary>
	protected TestingException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
#endif
}
