using System.Net;
using Polly;
using Polly.Retry;

namespace Reactive.Retry.N3_HandlingExceptions;

public class ExceptionsFilteringExample
{
    public static async ValueTask RunExampleAsync()
    {
        var policy = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = args =>
                {
                    if (args.Outcome.Exception is HttpRequestException httpException)
                    {
                        return ValueTask.FromResult(
                            httpException.StatusCode.Equals(HttpStatusCode.InternalServerError)
                            || httpException.StatusCode.Equals(HttpStatusCode.TooManyRequests)
                        );
                    }

                    return ValueTask.FromResult(false);
                },
                OnRetry = args =>
                {
                    Console.WriteLine(
                        $"Attempting constant retry #{args.AttemptNumber} with exception filtering");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        await Executor.ExecuteAsync(
            async () => await policy
                .ExecuteAsync(_ => throw new HttpRequestException("Internal server error", null, HttpStatusCode.InternalServerError)),
            "500 error retried"
        );

        await Executor.ExecuteAsync(
            async () => await policy
                .ExecuteAsync(_ => throw new HttpRequestException("Too many requests", null, HttpStatusCode.TooManyRequests)),
            "429 error not retried"
        );
    }
}