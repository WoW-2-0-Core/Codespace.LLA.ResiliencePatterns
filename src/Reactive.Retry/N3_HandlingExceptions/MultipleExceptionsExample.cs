using System.Net.Sockets;
using Polly;
using Polly.Retry;

namespace Reactive.Retry.N3_HandlingExceptions;

public class MultipleExceptionsExample
{
    public static async ValueTask RunExampleAsync()
    {
        var retryableExceptions = new []
        {
            typeof(HttpRequestException),
            typeof(SocketException),
            typeof(TimeoutException)
        };

        var policy = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = args => ValueTask.FromResult(
                    args.Outcome.Exception != null
                    && retryableExceptions.Contains(args.Outcome.Exception.GetType())),
                MaxRetryAttempts = 2,
                OnRetry = args =>
                {
                    Console.WriteLine(
                        $"Attempting constant retry #{args.AttemptNumber} with multiple exceptions");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();


        await Task.WhenAll(retryableExceptions.Select(async exceptionType =>
        {
            await Executor.ExecuteAsync(async () =>
                {
                    var exception = (Exception)Activator.CreateInstance(exceptionType)!;
                    await policy.ExecuteAsync(_ => throw exception);
                },
                $"{exceptionType.Name} was retried"
            );
        }));
    }
}