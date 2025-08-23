using Polly;
using Polly.CircuitBreaker;

namespace Reactive.CircuitBreaker.N2_BreakDurationControl;

public static class FixedBreakDurationExample
{
    public static async ValueTask RunExampleAsync()
    {
        Console.WriteLine("\n\n----------  Fixed break duration example  ----------");
        
        var pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 2,
                SamplingDuration = TimeSpan.FromSeconds(5),
                BreakDuration = TimeSpan.FromSeconds(2),
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
        
        Console.WriteLine("Attempting operation with fixed break duration");
        await Executor.ExecutePipelineAsync(
            pipeline,
            [false, false, false],
            callSimulatedDelay: 300,
            executionSimulatedDelay: 30
        );
        
        Console.WriteLine("\n\nAttempting operation with fixed break duration during open state");
        await Executor.ExecutePipelineAsync(
            pipeline,
            [false, false, false],
            callSimulatedDelay: 300,
            executionSimulatedDelay: 30
        );
        
        Console.WriteLine("\n\nWaiting for circuit to half-open...");
        await Task.Delay(2000);
        
        Console.WriteLine("\nAttempting operation with fixed break duration during half-open state");
        await Executor.ExecutePipelineAsync(
            pipeline,
            [true, true, true, true],
            callSimulatedDelay: 300,
            executionSimulatedDelay: 30
        );
    }
}