#if NET6_0
using System.IO;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.Parity;

public abstract partial class ParityTests
{
    public class Net6 : ParityTests
    {
        public Net6(ITestOutputHelper testOutputHelper)
            : base(new Parity(), testOutputHelper)
        {
            Parity.File.ExcludedMethods.Add(
                typeof(File).GetMethod(nameof(File.OpenHandle)));
        }
    }
}
#endif