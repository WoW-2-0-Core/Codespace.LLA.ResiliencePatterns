using Polly;
using Polly.Retry;

namespace Reactive.Retry.N1_BackoffStrategies;

public static class ConstantDelayExample
{
    public static async ValueTask RunExampleAsync()
    {
        Console.WriteLine("\n\n----------  Constant delay example  ----------");
        
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Constant,
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                OnRetry = args =>
                {
                    Console.WriteLine($"Attempting constant retry #{args.AttemptNumber}, retry delay: {args.RetryDelay.TotalMilliseconds}ms");
                    return ValueTask.CompletedTask;
                }
            }).Build();

        await pipeline.ExecuteAsync(_ => throw new InvalidOperationException());
    }
}