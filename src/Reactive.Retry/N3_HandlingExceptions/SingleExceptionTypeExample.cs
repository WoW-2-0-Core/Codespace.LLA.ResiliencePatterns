using Polly;
using Polly.Retry;

namespace Reactive.Retry.N3_HandlingExceptions;

public class SingleExceptionTypeExample
{
    public static async ValueTask RunExampleAsync()
    {
        var policy = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                OnRetry = args =>
                {
                    Console.WriteLine($"Attempting retry #{args.AttemptNumber} with single exception handling");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        await Executor.ExecuteAsync(
            async () => await policy.ExecuteAsync(_ => throw new HttpRequestException()),
            "HttpRequestException retried and failed");

        await Executor.ExecuteAsync(
            async () => await policy.ExecuteAsync(_ => throw new ArgumentException("Invalid argument")),
            "ArgumentException not retried");
    }
}