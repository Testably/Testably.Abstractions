using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.Mock;

public static partial class MockRandomSystem
{
    // ReSharper disable once UnusedMember.Global
    public sealed class RandomTests : RandomSystemRandomTests<RandomSystemMock>
    {
        public RandomTests() : base(new RandomSystemMock())
        {
        }
    }
}