using Polly;
using Polly.Retry;

namespace Reactive.Retry.N1_BackoffStrategies;

public class JitterVariationsExample
{
    public static async ValueTask RunExampleAsync()
    {
        var policy = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                OnRetry = args =>
                {
                    Console.WriteLine($"Attempting exponential retry with jitter #{args.AttemptNumber}, retry delay: {args.RetryDelay.TotalMilliseconds}ms");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        await policy.ExecuteAsync(_ =>
        {
            Console.WriteLine("Attempting operation in exponential retry policy with jitter");
            throw new InvalidOperationException();
        });
    }
}