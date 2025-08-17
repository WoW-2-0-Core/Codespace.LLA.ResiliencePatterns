using Polly;
using Polly.Retry;

namespace Reactive.Retry.N3_HandlingExceptions;

public class ExceptionsExclusivePatternsExample
{
    public static async ValueTask RunExampleAsync()
    {
        var policy = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = args => ValueTask.FromResult(
                    args.Outcome.Exception != null
                    && args.Outcome.Exception is not OperationCanceledException
                    && args.Outcome.Exception is not ArgumentException
                ),
                MaxRetryAttempts = 2,
                OnRetry = args =>
                {
                    Console.WriteLine(
                        $"Attempting constant retry #{args.AttemptNumber} with exception exclusive patterns");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        await Executor.ExecuteAsync(
            async () => await policy.ExecuteAsync(_ => throw new ArgumentException()),
            "InvalidOperationException retried"
        );

        await Executor.ExecuteAsync(
            async () => await policy.ExecuteAsync(_ => throw new OperationCanceledException()),
            "OperationCanceledException not retried"
        );
    }
}