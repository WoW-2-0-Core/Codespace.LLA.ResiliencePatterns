namespace Reactive.Retry;

public static class Executor
{
    public static async ValueTask ExecuteAsync(Func<ValueTask> action, string? message = null, bool addNewLine = false)
    {
        try
        {
            await action();
        }
        catch
        {
            if (message is not null)
                Console.WriteLine(message);
        }

        if (addNewLine)
            Console.WriteLine();
    }
}