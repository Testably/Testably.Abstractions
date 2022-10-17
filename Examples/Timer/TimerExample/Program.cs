using System;
using System.Threading;
using Testably.Abstractions;
using TimerExample;

CancellationTokenSource cts = new();
TimeSystem timeSystem = new();
TimeSpan interval = TimeSpan.FromSeconds(2);

Console.WriteLine(
	$"This example illustrates usage of a timer to execute a callback repeatedly in a background timer every {interval}.");
Console.WriteLine("Press [Enter] to stop the timer...");
Console.WriteLine();

SynchronizationTimer synchronizationTimer = new(
	timeSystem,
	interval,
	i => Console.WriteLine($"[{timeSystem.DateTime.Now:HH:mm:ss}] Callback executed ({i} times)"));
synchronizationTimer.Start(cts.Token);

Console.ReadLine();
cts.Cancel();

Console.WriteLine("The timer is stopped.");
Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();