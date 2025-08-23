using Polly;
using Polly.CircuitBreaker;

namespace Reactive.CircuitBreaker;

public static class Executor
{
    public static async ValueTask ExecuteAsync(
        Func<ValueTask> action,
        string? successMsg = null,
        string? failureMsg = null,
        bool addNewLine = false)
    {
        try
        {
            await action();

            if (successMsg is not null)
                Console.WriteLine(successMsg);
        }
        catch (Exception ex)
        {
            if (ex is BrokenCircuitException)
                Console.Write(" - ❌ blocked");

            if (failureMsg is not null)
                Console.WriteLine(failureMsg);
        }

        if (addNewLine)
            Console.WriteLine();
    }

    public static async Task ExecutePipelineAsync(
        ResiliencePipeline pipeline,
        ICollection<bool> outcomes,
        ushort callSimulatedDelay,
        ushort executionSimulatedDelay
    )
    {
        foreach (var outcome in outcomes)
        {
            Console.Write("\nAttempting operation");
            await ExecuteAsync(
                action: async () => await pipeline.ExecuteAsync(async ct =>
                {
                    Console.Write(" - making a call");
                    await Task.Delay(callSimulatedDelay, ct);

                    if (outcome)
                    {
                        Console.Write(" - ✅ succeeded");
                        return;
                    }

                    Console.Write(" - ❌ failed");
                    throw new InvalidOperationException("Request failed");
                }));

            await Task.Delay(executionSimulatedDelay);
        }
    }
}