using Polly;
using Polly.CircuitBreaker;

namespace Reactive.CircuitBreaker.N1_ThresholdConfiguration;

public static class FailureRatioExample
{
    public static async ValueTask RunExampleAsync()
    {
        Console.WriteLine("\n\n----------  Failure ratio example  ----------");
        
        var pipelineBuilder = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 4,
                SamplingDuration = TimeSpan.FromSeconds(1),
                BreakDuration = TimeSpan.FromSeconds(3),
                OnOpened = _ =>
                {
                    Console.Write(" - 🔴 circuit opened");
                    return ValueTask.CompletedTask;
                }
            });

        Console.WriteLine("Attempting operation 50% failure ratio with 25% failing requests within 1 second (circuit stays closed)");
        await Executor.ExecutePipelineAsync(
            pipeline: pipelineBuilder.Build(),
            outcomes: [true, true, true, false, true, true, true],
            callSimulatedDelay: 225,
            executionSimulatedDelay: 30
        );

        Console.WriteLine("\n\nAttempting operation 50% failure ratio with 25% failing requests within 1 second (circuit stays closed)");
        await Executor.ExecutePipelineAsync(
            pipelineBuilder.Build(),
            [false, false, true, false, true, true, true],
            callSimulatedDelay: 225,
            executionSimulatedDelay: 30
        );
    }
}