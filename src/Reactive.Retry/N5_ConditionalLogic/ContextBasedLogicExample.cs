using Polly;
using Polly.Retry;

namespace Reactive.Retry.N5_ConditionalLogic;

public class ContextBasedLogicExample
{
    private const string OperationTypeCritical = "Critical";
    private const string NormalOperationType = "Normal";
    private static string PropertyName => "OperationType";
    private static readonly ResiliencePropertyKey<string> OperationTypeKey = new(PropertyName);

    public static async ValueTask RunExampleAsync()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = args =>
                {
                    var isCritical = args.Context.Properties.TryGetValue(OperationTypeKey, out var operationType) &&
                                     operationType == OperationTypeCritical;
                    return ValueTask.FromResult(isCritical);
                },
                MaxRetryAttempts = 3,
                OnRetry = args =>
                {
                    Console.WriteLine(
                        $"Attempting constant retry #{args.AttemptNumber} with context-based logic");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        var criticalContext = ResilienceContextPool.Shared.Get();
        criticalContext.Properties.Set(OperationTypeKey, OperationTypeCritical);
        Console.WriteLine("Attempting operation in context based retry with critical priority: ");
        await Executor.ExecuteAsync(async () => await pipeline.ExecuteAsync(_ => throw new InvalidOperationException(), criticalContext));

        var normalContext = ResilienceContextPool.Shared.Get();
        normalContext.Properties.Set(OperationTypeKey, NormalOperationType);
        Console.WriteLine("Attempting operation in context based retry with normal priority: ");
        await Executor.ExecuteAsync(async () => await pipeline.ExecuteAsync(_ => throw new InvalidOperationException(), normalContext));
    }
}