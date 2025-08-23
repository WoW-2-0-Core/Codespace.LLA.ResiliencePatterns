using Polly;
using Polly.CircuitBreaker;

namespace Reactive.CircuitBreaker.N1_ThresholdConfiguration;

public static class MinimumThroughputExample
{
    public static async ValueTask RunExampleAsync()
    {
        Console.WriteLine("\n\n----------  Minimum through input example  ----------");
        
        var pipelineBuilder = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 5,
                SamplingDuration = TimeSpan.FromSeconds(5),
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
            });

        Console.WriteLine("Attempting operation with minimum throughput without enough sample requests (circuit stays closed)");
        await Executor.ExecutePipelineAsync(
            pipelineBuilder.Build(),
            [false, false, false, false],
            callSimulatedDelay: 180,
            executionSimulatedDelay: 20
        );

        Console.WriteLine("\n\nAttempting operation with minimum with enough sample requests (circuit opens)");
        await Executor.ExecutePipelineAsync(
            pipelineBuilder.Build(),
            [false, true, false, true, false, true],
            callSimulatedDelay: 180,
            executionSimulatedDelay: 20
        );
    }
}