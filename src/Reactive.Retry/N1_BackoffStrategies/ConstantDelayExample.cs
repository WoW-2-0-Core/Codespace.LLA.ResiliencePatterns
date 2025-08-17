using Polly;
using Polly.Retry;

namespace Reactive.Retry.N1_BackoffStrategies;

public static class ConstantDelayExample
{
    public static async ValueTask RunExampleAsync()
    {
        var policy = new ResiliencePipelineBuilder()
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

        await policy.ExecuteAsync(_ =>
        {
            Console.WriteLine("Attempting operation in constant retry policy");
            throw new InvalidOperationException();
        });
    }
}