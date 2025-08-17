using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Telemetry;

namespace PollyTelemetry.N1_RetryTelemetry;

public static class RetryTelemetryExample
{
    public static async ValueTask RunExampleAsync()
    {
        // Configure logging
        var telemetryOptions = new TelemetryOptions
        {
            LoggerFactory = LoggerFactory.Create(builder => builder.AddConsole()),
        };

        var pipelineBuilder = new ResiliencePipelineBuilder
        {
            Name = "GenericPipeline",
            InstanceName = "GenericPipelineInstance"
        };

        var pipeline = pipelineBuilder
            .AddRetry(new RetryStrategyOptions
            {
                Name = "ConstantRetryPolicy",
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(200),
                OnRetry = _ => ValueTask.CompletedTask,
                BackoffType = DelayBackoffType.Constant,
            })
            .ConfigureTelemetry(telemetryOptions)
            .Build();

        Console.WriteLine("Retry Policy Telemetry Example");

        Console.WriteLine("\nCase 1: Unhandled Exception (Information Severity)");
        await RunSuccessfulOperationAsync(pipeline);

        Console.WriteLine("\nCase 2: Handled with Successful Retry (Warning Severity)");
        await RunEventuallySuccessfulOperationAsync(pipeline);

        Console.WriteLine("\nCase 3: Handled but Exhausted Retries (Warning -> Error Severity)");
        await RunPermanentlyFailingOperationAsync(pipeline);
    }

    private static async ValueTask RunSuccessfulOperationAsync(ResiliencePipeline pipeline)
    {
        await Executor.ExecuteAsync(
            async () => await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask),
            "No retries performed"
        );
    }

    private static async ValueTask RunEventuallySuccessfulOperationAsync(ResiliencePipeline pipeline)
    {
        var attemptCount = 0;

        await Executor.ExecuteAsync(
            async () => await pipeline.ExecuteAsync(_ =>
                ++attemptCount <= 2 // Fail first 2 attempts, succeed on the 3rd
                    ? throw new InvalidOperationException($"Attempt {attemptCount} failed")
                    : ValueTask.CompletedTask),
            "Handled case - operation succeeds after retries"
        );
    }

    private static async ValueTask RunPermanentlyFailingOperationAsync(ResiliencePipeline pipeline)
    {
        await Executor.ExecuteAsync(
            async () => await pipeline.ExecuteAsync(_ => throw new InvalidOperationException("This will exhaust all retries")),
            "Handled case - retries exhausted, final error"
        );
    }
}