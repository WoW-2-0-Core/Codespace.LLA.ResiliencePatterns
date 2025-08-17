using Polly;
using Polly.Retry;

namespace Reactive.Retry.N2_DelayControl;

public static class InfiniteRetryExample
{
    public static async ValueTask RunExampleAsync()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = int.MaxValue,
                Delay = TimeSpan.FromMilliseconds(100),
                OnRetry = args =>
                {
                    Console.WriteLine($"Attempting infinite retry #{args.AttemptNumber}, retry delay: {args.RetryDelay.TotalMilliseconds}ms");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        await pipeline.ExecuteAsync(_ => throw new InvalidOperationException(), cts.Token);
    }
}