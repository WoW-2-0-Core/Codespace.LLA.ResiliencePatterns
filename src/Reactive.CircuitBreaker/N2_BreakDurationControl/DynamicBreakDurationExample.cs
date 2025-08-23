using Polly;
using Polly.CircuitBreaker;

namespace Reactive.CircuitBreaker.N2_BreakDurationControl;

public static class DynamicBreakDurationExample
{
    public static async ValueTask RunExampleAsync()
    {
        Console.WriteLine("\n\n----------  Dynamic break duration example  ----------");
        
        var pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 3,
                SamplingDuration = TimeSpan.FromSeconds(2),
                BreakDurationGenerator = args =>
                {
                    var duration = TimeSpan.FromSeconds(args.FailureCount * 2);
                    Console.Write($" - break {duration.TotalSeconds}s after {args.FailureCount} failures");
                    return ValueTask.FromResult(duration);
                },
                OnOpened = _ =>
                {
                    Console.Write(" - 🔴 circuit opened");
                    return ValueTask.CompletedTask;
                },
                OnClosed = _ =>
                {
                    Console.Write(" - 🟢 circuit closed");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = _ =>
                {
                    Console.Write(" - 🟡 circuit half-opened");
                    return ValueTask.CompletedTask;
                }
            }).Build();

        Console.WriteLine("Attempting operations with dynamic break duration");
        await Executor.ExecutePipelineAsync(
            pipeline,
            [false, false, true, false, false],
            callSimulatedDelay: 225,
            executionSimulatedDelay: 20
        );
    }
}