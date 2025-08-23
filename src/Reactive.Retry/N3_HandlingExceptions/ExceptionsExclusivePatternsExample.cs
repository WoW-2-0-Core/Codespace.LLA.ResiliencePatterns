using Polly;
using Polly.Retry;

namespace Reactive.Retry.N3_HandlingExceptions;

public static class ExceptionsExclusivePatternsExample
{
    public static async ValueTask RunExampleAsync()
    {
        Console.WriteLine("\n\n----------  Exceptions exclusive patterns example  ----------");
        
        var pipeline = new ResiliencePipelineBuilder()
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
            async () => await pipeline.ExecuteAsync(_ => throw new ArgumentException()),
            "InvalidOperationException retried"
        );

        await Executor.ExecuteAsync(
            async () => await pipeline.ExecuteAsync(_ => throw new OperationCanceledException()),
            "OperationCanceledException not retried"
        );
    }
}