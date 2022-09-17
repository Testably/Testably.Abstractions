using System;

namespace Testably.Abstractions.Testing.Tests;

public static class Helpers
{
    public static TimeSpan GetRandomInterval(double secondsMultiplier = 60)
    {
        Random random = new();
        return TimeSpan.FromSeconds(random.NextDouble() * secondsMultiplier);
    }

    public static DateTime GetRandomTime(DateTimeKind kind = DateTimeKind.Unspecified)
    {
        Random random = new();
        return new DateTime(1970, 1, 1, 0, 0, 0, kind)
           .AddSeconds(random.Next());
    }
}