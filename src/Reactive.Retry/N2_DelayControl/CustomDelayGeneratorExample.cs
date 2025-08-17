using Polly;
using Polly.Retry;

namespace Reactive.Retry.N2_DelayControl;

public static class CustomDelayGeneratorExample
{
    public static async ValueTask RunExampleAsync()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                DelayGenerator = args =>
                {
                    var delay = args.AttemptNumber switch
                    {
                        0 => TimeSpan.Zero,
                        1 => TimeSpan.FromMilliseconds(100),
                        2 => TimeSpan.FromMilliseconds(500),
                        _ => TimeSpan.FromSeconds(2)
                    };

                    return new ValueTask<TimeSpan?>(delay);
                },
                OnRetry = args =>
                {
                    Console.WriteLine(
                        $"Attempting constant retry #{args.AttemptNumber}, retry delay: {args.RetryDelay.TotalMilliseconds}ms with custom delay generator");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        await pipeline.ExecuteAsync(_ => throw new InvalidOperationException());
    }
}