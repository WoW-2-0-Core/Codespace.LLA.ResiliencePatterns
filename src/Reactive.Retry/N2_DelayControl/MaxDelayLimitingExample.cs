using Polly;
using Polly.Retry;

namespace Reactive.Retry.N2_DelayControl;

public static class MaxDelayLimitingExample
{
    public static async ValueTask RunExampleAsync()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(200),
                MaxDelay = TimeSpan.FromMilliseconds(350),
                OnRetry = args =>
                {
                    Console.WriteLine($"Attempting exponential retry #{args.AttemptNumber}, retry delay: {args.RetryDelay.TotalMilliseconds}ms with max delay");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        await pipeline.ExecuteAsync(_ => throw new InvalidOperationException());
    }
}