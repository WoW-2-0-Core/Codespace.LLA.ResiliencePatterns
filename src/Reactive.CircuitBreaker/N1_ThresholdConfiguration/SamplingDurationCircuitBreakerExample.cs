using Polly;
using Polly.CircuitBreaker;

namespace Reactive.CircuitBreaker.N1_ThresholdConfiguration;

public static class SamplingDurationExample
{
    public static async ValueTask RunExampleAsync()
    {
        Console.WriteLine("\n\n----------  Sampling duration example  ----------");
        
        var pipelineBuilder = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 4,
                SamplingDuration = TimeSpan.FromSeconds(3),
                BreakDuration = TimeSpan.FromSeconds(2),
                OnOpened = _ =>
                {
                    Console.Write(" - 🔴 circuit opened");
                    return ValueTask.CompletedTask;
                }
            });

        Console.WriteLine("Attempting operation with 3 second sampling duration with all requests fired within 2 seconds (circuit stays closed)");

        await Executor.ExecutePipelineAsync(
            pipelineBuilder.Build(),
            outcomes: [false, false, false, false],
            callSimulatedDelay: 50,
            executionSimulatedDelay: 1
        );

        Console.WriteLine(
            "\n\nAttempting operation with 3 second sampling duration with all requests fired within exactly 3 seconds (circuit opens)");
        
        await Executor.ExecutePipelineAsync(
            pipelineBuilder.Build(),
            outcomes: [false, false, true, false, true],
            callSimulatedDelay: 720,
            executionSimulatedDelay: 20
        );
    }
}