using System.Net;
using System.Net.Http.Headers;
using Polly;
using Polly.Retry;

namespace Reactive.Retry.N2_DelayControl;

public static class ExtractDelayFromResultExample
{
    public static async ValueTask RunExampleAsync()
    {
        var policy = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                DelayGenerator = args =>
                {
                    if (args.Outcome.Result?.Headers.RetryAfter?.Delta is { } retryAfter)
                    {
                        Console.WriteLine($"Extracted retry from response header : {retryAfter.TotalMilliseconds}ms");
                        return new ValueTask<TimeSpan?>(retryAfter);
                    }

                    return new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(1));
                },
                OnRetry = args =>
                {
                    Console.WriteLine(
                        $"Attempting linear retry #{args.AttemptNumber} with extracted delay from result retry delay: {args.RetryDelay.TotalMilliseconds}ms");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(3));

        await policy.ExecuteAsync(_ => ValueTask.FromResult(response));
    }
}