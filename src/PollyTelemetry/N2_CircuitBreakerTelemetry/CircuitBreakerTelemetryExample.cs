using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Telemetry;

namespace PollyTelemetry.N2_CircuitBreakerTelemetry;

public static class CircuitBreakerTelemetryExample
{
    public static async ValueTask RunExampleAsync()
    {
        var manualControl = new CircuitBreakerManualControl();

        var telemetryOptions = new TelemetryOptions
        {
            LoggerFactory = LoggerFactory.Create(builder => builder.AddConsole()),
        };

        var pipelineBuilder = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 2,
                SamplingDuration = TimeSpan.FromMilliseconds(500),
                BreakDuration = TimeSpan.FromSeconds(1),
                Name = "DemoCircuitBreaker",
                OnOpened = _ =>
                {
                    Console.Write(" - 🔴 circuit opened ");
                    return ValueTask.CompletedTask;
                },
                OnClosed = _ =>
                {
                    Console.Write(" - 🟢 circuit closed ");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = _ =>
                {
                    Console.Write(" - 🟡 circuit half-opened ");
                    return ValueTask.CompletedTask;
                }
            })
            .ConfigureTelemetry(telemetryOptions);

        pipelineBuilder.Name = "GenericPipeline";
        pipelineBuilder.InstanceName = "GenericPipelineInstance";

        var pipeline = pipelineBuilder.Build();

        Console.WriteLine("----- Circuit Breaker Policy Telemetry Example -----");

        Console.WriteLine("\nCase 1: Circuit is closed");
        await Executor.ExecutePipelineAsync(
            pipeline,
            [true, false],
            callSimulatedDelay: 225,
            executionSimulatedDelay: 25
        );

        Console.WriteLine("\nCase 2: Circuit is open");
        await Executor.ExecutePipelineAsync(
            pipeline,
            [true, false],
            callSimulatedDelay: 225,
            executionSimulatedDelay: 25
        );
        await Task.Delay(1000);

        Console.WriteLine("\nCase 3: Circuit is half-open");
        await Executor.ExecutePipelineAsync(
            pipeline,
            [true, false],
            callSimulatedDelay: 225,
            executionSimulatedDelay: 25
        );

        Console.WriteLine("\nCase 4: Circuit is isolated");
        await manualControl.IsolateAsync();
        await Executor.ExecutePipelineAsync(
            pipeline,
            [true, false],
            callSimulatedDelay: 225,
            executionSimulatedDelay: 25
        );
    }
}