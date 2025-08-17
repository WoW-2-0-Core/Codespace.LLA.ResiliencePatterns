using Polly;
using Polly.Retry;

namespace Reactive.Retry.N5_ConditionalLogic;

public class UrlBasedLogicExample
{
    private static readonly ResiliencePropertyKey<string> UrlKey = new("RequestUrl");
    private static readonly string CriticalPath = "api/users";
    private static readonly string NormalPath = "api/orders";

    public static async ValueTask RunExampleAsync()
    {
        var policy = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = args =>
                {
                    var shouldRetry = args.Outcome.Exception is HttpRequestException && args.Context.Properties.TryGetValue(UrlKey, out var url) &&
                                      url.Contains(CriticalPath);

                    return ValueTask.FromResult(shouldRetry);
                },
                MaxRetryAttempts = 2,
                OnRetry = args =>
                {
                    Console.WriteLine(
                        $"Attempting constant retry #{args.AttemptNumber} with property in context");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        // Critical request
        var getOrderByIdUrl = $"{CriticalPath}/1";
        var criticalContext = ResilienceContextPool.Shared.Get();
        criticalContext.Properties.Set(UrlKey, getOrderByIdUrl);
        await Executor.ExecuteAsync(async () => await policy.ExecuteAsync(_ =>
        {
            Console.WriteLine("Attempting request in url based retry with critical url");
            throw new HttpRequestException();
        }));

        // Normal request
        var getUserByIdUrl = $"{NormalPath}/2";
        var normalContext = ResilienceContextPool.Shared.Get();
        normalContext.Properties.Set(UrlKey, getUserByIdUrl);
        await Executor.ExecuteAsync(async () => await policy.ExecuteAsync(_ =>
        {
            Console.WriteLine("Attempting request in url based retry with normal url");
            throw new HttpRequestException();
        }));
    }
}