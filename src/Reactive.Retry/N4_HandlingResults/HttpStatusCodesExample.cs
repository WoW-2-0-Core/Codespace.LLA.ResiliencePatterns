using System.Net;
using Polly;
using Polly.Retry;

namespace Reactive.Retry.N4_HandlingResults;

public class HttpStatusCodesExample
{
    public static async ValueTask RunExampleAsync()
    {
        var policy = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = args => ValueTask.FromResult(
                    args.Outcome.Result?.StatusCode >= HttpStatusCode.InternalServerError),
                MaxRetryAttempts = 2,
                OnRetry = args =>
                {
                    Console.WriteLine($"Attempting retry #{args.AttemptNumber} with status code #{args.Outcome.Result?.StatusCode} from result");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        await Executor.ExecuteAsync(async () => await policy.ExecuteAsync(_ =>
        {
            Console.WriteLine("Attempting operation in retry policy with status code handling with 503 result");
            return new ValueTask<HttpResponseMessage>(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        }));

        await Executor.ExecuteAsync(async () => await policy.ExecuteAsync(_ =>
        {
            Console.WriteLine("Attempting operation in retry policy with status code handling with 200 result");
            return new ValueTask<HttpResponseMessage>(new HttpResponseMessage(HttpStatusCode.OK));
        }));
    }
}