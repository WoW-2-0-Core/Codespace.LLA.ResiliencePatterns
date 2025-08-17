using Polly;
using Polly.Retry;

namespace Reactive.Retry.N1_BackoffStrategies;

public class LinearBackoffExample
{
    public static async ValueTask RunExampleAsync()
    {
        var policy = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Linear,
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromMilliseconds(200),
                OnRetry = args =>
                {
                    Console.WriteLine($"Attempting linear retry #{args.AttemptNumber}, retry delay: {args.RetryDelay.TotalMilliseconds}ms");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        await policy.ExecuteAsync(token =>
        {
            Console.WriteLine("Attempting operation in linear retry policy");
            throw new InvalidOperationException();
        });
    }
}