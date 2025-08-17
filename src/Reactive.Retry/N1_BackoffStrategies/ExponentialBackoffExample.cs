using Polly;
using Polly.Retry;

namespace Reactive.Retry.N1_BackoffStrategies;

public class ExponentialBackoffExample
{
    public static async ValueTask RunExampleAsync()
    {
        var policy = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                OnRetry = args =>
                {
                    Console.WriteLine($"Attempting exponential retry #{args.AttemptNumber}, retry delay: {args.RetryDelay.TotalMilliseconds}ms");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        await policy.ExecuteAsync(_ => throw new InvalidOperationException());
    }
}