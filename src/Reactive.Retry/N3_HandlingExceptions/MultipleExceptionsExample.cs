using System.Net.Sockets;
using Polly;
using Polly.Retry;

namespace Reactive.Retry.N3_HandlingExceptions;

public static class MultipleExceptionsExample
{
    public static async ValueTask RunExampleAsync()
    {
        var retryableExceptions = new []
        {
            typeof(HttpRequestException),
            typeof(SocketException),
            typeof(TimeoutException)
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = args => ValueTask.FromResult(
                    args.Outcome.Exception != null
                    && retryableExceptions.Contains(args.Outcome.Exception.GetType())),
                OnRetry = args =>
                {
                    Console.WriteLine(
                        $"Attempting constant retry #{args.AttemptNumber} with multiple exceptions");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();


        foreach (var exceptionType in retryableExceptions)
        {
            Console.WriteLine($"Attempting operation in retry policy with {exceptionType.Name} handling");
            await Executor.ExecuteAsync(async () =>
                {
                    var exception = (Exception)Activator.CreateInstance(exceptionType)!;
                    await pipeline.ExecuteAsync(_ => throw exception);
                },
                $"{exceptionType.Name} was retried"
            );
        }
    }
}