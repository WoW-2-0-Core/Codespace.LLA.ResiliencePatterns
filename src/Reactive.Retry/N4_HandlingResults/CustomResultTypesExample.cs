using System.Net;
using Polly;
using Polly.Retry;

namespace Reactive.Retry.N4_HandlingResults;

public class CustomResultTypesExample
{
    public record ApiResult(bool Success, string Message, int StatusCode);

    public static async ValueTask RunExampleAsync()
    {
        var policy = new ResiliencePipelineBuilder<ApiResult>()
            .AddRetry(new RetryStrategyOptions<ApiResult>
            {
                ShouldHandle = args => ValueTask.FromResult(
                    args.Outcome.Result is not null
                    && !args.Outcome.Result.Success
                    && args.Outcome.Result.StatusCode >= (int)HttpStatusCode.InternalServerError),
                OnRetry = args =>
                {
                    Console.WriteLine(
                        $"Attempting retry #{args.AttemptNumber} with value #{args.Outcome.Result?.StatusCode} from custom result");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        await Executor.ExecuteAsync(async () => await policy.ExecuteAsync(_ =>
        {
            Console.WriteLine("Attempting operation in custom result retry policy with 503 status");
            return ValueTask.FromResult(new ApiResult(false, "Service Unavailable", 503));
        }));

        await Executor.ExecuteAsync(async () => await policy.ExecuteAsync(_ =>
        {
            Console.WriteLine("Attempting operation in custom result retry policy with 200 status");
            return ValueTask.FromResult(new ApiResult(true, "OK", 200));
        }));
    }
}