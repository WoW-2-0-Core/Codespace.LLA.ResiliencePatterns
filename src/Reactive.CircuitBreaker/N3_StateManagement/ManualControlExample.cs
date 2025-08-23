using Polly;
using Polly.CircuitBreaker;

namespace Reactive.CircuitBreaker.N3_StateManagement;

public static class ManualControlExample
{
    public static async ValueTask RunExampleAsync()
    {
        Console.WriteLine("\n\n----------  Manual control example  ----------");
        
        var manualControl = new CircuitBreakerManualControl();
        var stateProvider = new CircuitBreakerStateProvider();

        var pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 2,
                SamplingDuration = TimeSpan.FromMilliseconds(500),
                BreakDuration = TimeSpan.FromSeconds(1),
                ManualControl = manualControl,
                StateProvider = stateProvider
            })
            .Build();
             
        Console.WriteLine("Attempting operations with manual control in open state");
        await manualControl.IsolateAsync();
        
        await Executor.ExecutePipelineAsync(
            pipeline,
            [true, true],
            callSimulatedDelay: 225,
            executionSimulatedDelay: 25
        );
        Console.WriteLine($"\nCurrent state : {stateProvider.CircuitState}");
        await Task.Delay(1000);
   
        Console.WriteLine("\nAttempting operations with manual control in closed state");
        await manualControl.CloseAsync();
        
        await Executor.ExecutePipelineAsync(
            pipeline,
            [true, true],
            callSimulatedDelay: 225,
            executionSimulatedDelay: 25
        );
        Console.WriteLine($"\nCurrent state : {stateProvider.CircuitState}");
        await Task.Delay(1000);
    }
}