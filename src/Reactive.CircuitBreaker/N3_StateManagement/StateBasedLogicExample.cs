using Polly;
using Polly.CircuitBreaker;

namespace Reactive.CircuitBreaker.N3_StateManagement;

public static class StateBasedLogicExample
{
    public static async ValueTask RunExampleAsync()
    {
        Console.WriteLine("\n\n----------  State based logic example  ----------");
        
        var stateProvider = new CircuitBreakerStateProvider();
        var manualControl = new CircuitBreakerManualControl();

        var pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 2,
                SamplingDuration = TimeSpan.FromMilliseconds(500),
                StateProvider = stateProvider,
                BreakDuration = TimeSpan.FromSeconds(1),
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
            })
            .Build();

        Console.WriteLine("Attempting operations with state based logic with failing requests when circuit is closed:");
        await ExecuteAsync(
            pipeline,
            [true, false],
            callSimulatedDelay: 225,
            executionSimulatedDelay: 25,
            stateProvider: stateProvider
        );

        Console.WriteLine("\n\nAttempting operations with state based logic with requests when circuit is open:");
        await ExecuteAsync(
            pipeline,
            [true, false],
            callSimulatedDelay: 225,
            executionSimulatedDelay: 25,
            stateProvider: stateProvider
        );

        await Task.Delay(1000);

        Console.WriteLine("\n\nAttempting operations with state based logic with requests when circuit is half-open:");
        await ExecuteAsync(
            pipeline,
            [true, false],
            callSimulatedDelay: 225,
            executionSimulatedDelay: 25,
            stateProvider: stateProvider
        );

        Console.WriteLine("\n\nAttempting operations with state based logic with requests when circuit is isolated:");
        await manualControl.IsolateAsync();
        await ExecuteAsync(
            pipeline,
            [true, false],
            callSimulatedDelay: 225,
            executionSimulatedDelay: 25,
            stateProvider: stateProvider
        );
    }

    private static async Task ExecuteAsync(
        ResiliencePipeline pipeline,
        ICollection<bool> outcomes,
        ushort callSimulatedDelay,
        ushort executionSimulatedDelay,
        CircuitBreakerStateProvider stateProvider
    )
    {
        foreach (var outcome in outcomes)
        {
            Console.Write($"\nAttempting operation with {stateProvider.CircuitState.ToString().ToLower()} state");

            var operation = stateProvider.CircuitState switch
            {
                CircuitState.Open or CircuitState.Closed => pipeline.ExecuteAsync(async ct =>
                {
                    Console.Write(" - making a call");
                    await Task.Delay(callSimulatedDelay, ct);

                    if (outcome)
                    {
                        Console.Write(" - ✅ succeeded");
                        return;
                    }

                    Console.Write(" - ❌ failed");
                    throw new InvalidOperationException("Request failed");
                }),
                CircuitState.HalfOpen => pipeline.ExecuteAsync(async ct =>
                {
                    Console.Write(" - making a call");
                    await Task.Delay(callSimulatedDelay, ct);

                    if (outcome)
                    {
                        Console.Write(" - ✅ succeeded");
                        return;
                    }

                    Console.Write(" - ❌ failed");
                    throw new InvalidOperationException("Request failed");
                }),
                CircuitState.Isolated => ExecuteFallBackAsync(),
                _ => throw new InvalidOperationException()
            };

            await Executor.ExecuteAsync(async () => await operation);

            await Task.Delay(executionSimulatedDelay);
        }
    }

    private static ValueTask ExecuteFallBackAsync()
    {
        Console.Write(" - using fallback");
        return ValueTask.CompletedTask;
    }
}