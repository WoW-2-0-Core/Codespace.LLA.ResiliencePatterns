using System.Net;
using Polly;
using Polly.Retry;

namespace Reactive.Retry.N4_HandlingResults;

public static class CustomResultTypesExample
{
    public record ApiResult(bool Success, string Message, int StatusCode);

    public static async ValueTask RunExampleAsync()
    {
        Console.WriteLine("\n\n----------  Custom result types example  ----------");
        
        var pipeline = new ResiliencePipelineBuilder<ApiResult>()
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

        Console.WriteLine("Attempting operation in custom result retry policy with 503 status: ");
        await Executor.ExecuteAsync(async () =>
            await pipeline.ExecuteAsync(_ => ValueTask.FromResult(new ApiResult(false, "Service Unavailable", 503))));

        Console.WriteLine("Attempting operation in custom result retry policy with 200 status: ");
        await Executor.ExecuteAsync(async () => await pipeline.ExecuteAsync(_ => ValueTask.FromResult(new ApiResult(true, "OK", 200))));
    }
}