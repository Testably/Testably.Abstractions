using System;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Custom <see cref="TestingException" /> when using the test system incorrectly.
/// </summary>
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
}