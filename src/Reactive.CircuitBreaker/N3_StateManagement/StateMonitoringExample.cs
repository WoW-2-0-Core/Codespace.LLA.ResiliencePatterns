using Polly;
using Polly.CircuitBreaker;

namespace Reactive.CircuitBreaker.N3_StateManagement;

public static class StateMonitoringExample
{
    public static async ValueTask RunExampleAsync()
    {
        Console.WriteLine("\n\n----------  State monitoring example  ----------");
        
        var stateProvider = new CircuitBreakerStateProvider();

        var pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 2,
                SamplingDuration = TimeSpan.FromMilliseconds(500),
                BreakDuration = TimeSpan.FromSeconds(1),
                StateProvider = stateProvider
            })
            .Build();

        Console.WriteLine("Attempting operations with state monitoring");

        foreach (var outcome in Enumerable.Range(1, 2))
        {
            var fail = outcome == 1;
            await Executor.ExecutePipelineAsync(
                pipeline,
                fail ? [false, false] : [true, true],
                callSimulatedDelay: 225,
                executionSimulatedDelay: 25
            );
            
            Console.WriteLine($"\nCurrent state : {stateProvider.CircuitState}");
            await Task.Delay(1000);
        }
    }
}