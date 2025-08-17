using Polly;
using Polly.Retry;

namespace Reactive.Retry.N2_DelayControl;

public static class MaxDelayLimitingExample
{
    public static async ValueTask RunExampleAsync()
    {
        var policy = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(100),
                MaxDelay = TimeSpan.FromSeconds(2),
                OnRetry = args =>
                {
                    Console.WriteLine($"Attempting exponential retry #{args.AttemptNumber}, retry delay: {args.RetryDelay.TotalMilliseconds}ms with max delay");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        await policy.ExecuteAsync(_ =>
        {
            Console.WriteLine("Attempting operation in exponential retry policy with max delay");
            throw new InvalidOperationException();
        });
    }
}