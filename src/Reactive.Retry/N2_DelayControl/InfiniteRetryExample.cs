using Polly;
using Polly.Retry;

namespace Reactive.Retry.N2_DelayControl;

public static class InfiniteRetryExample
{
    public static async ValueTask RunExampleAsync()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var policy = new ResiliencePipelineBuilder()
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

        await policy.ExecuteAsync(_ =>
        {
            Console.WriteLine("Attempting operation with infinite retry policy");
            throw new InvalidOperationException();
        }, cts.Token);
    }
}