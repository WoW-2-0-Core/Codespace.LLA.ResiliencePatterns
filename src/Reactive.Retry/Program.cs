using System.Collections.Immutable;
using Polly;
using Polly.Retry;

# region Backoff Types

var attemptsCount = 0;
var callbackWithBadException = () => attemptsCount++ < 3
    ? throw new BadException()
    : ValueTask.CompletedTask;

var callbackWithMixedException = () => attemptsCount++ < 3
    ? attemptsCount % 2 == 0 ? throw new BadException() : throw new YetAnotherBadException()
    : ValueTask.CompletedTask;

var callbackForLogging = (string msg) =>
{
    Console.WriteLine(msg);
    return ValueTask.CompletedTask;
};

// Constant retry execution
var constantDelayRetryOptions = new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<BadException>(),
    BackoffType = DelayBackoffType.Constant,
    UseJitter = true,
    MaxRetryAttempts = 4,
    Delay = TimeSpan.FromMilliseconds(100),
    OnRetry = args => callbackForLogging($"Constant Retry, Attempt : {args.AttemptNumber}, Retry Delay : {args.RetryDelay}")
};

var pipelineA = new ResiliencePipelineBuilder()
    .AddRetry(constantDelayRetryOptions)
    .Build();
attemptsCount = 0;
await pipelineA.ExecuteAsync((_, _) => callbackWithBadException(),
    ResilienceContextPool.Shared.Get(),
    state: "some state"
);

// Linear retry execution
var linearRetryExecutionOptions = new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<BadException>(),
    BackoffType = DelayBackoffType.Linear,
    UseJitter = true,
    MaxRetryAttempts = 4,
    Delay = TimeSpan.FromMilliseconds(100),
    OnRetry = args => callbackForLogging($"Linear Retry, Attempt : {args.AttemptNumber}, Retry Delay : {args.RetryDelay}")
};

var pipelineB = new ResiliencePipelineBuilder()
    .AddRetry(linearRetryExecutionOptions)
    .Build();
attemptsCount = 0;
await pipelineB.ExecuteAsync((_, _) => callbackWithBadException(),
    ResilienceContextPool.Shared.Get(),
    state: "some state"
);

// Exponential retry execution
var exponentialRetryExecutionOptions = new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder().Handle<BadException>(),
    BackoffType = DelayBackoffType.Exponential,
    UseJitter = true,
    MaxRetryAttempts = 4,
    Delay = TimeSpan.FromMilliseconds(100),
    OnRetry = args => callbackForLogging($"Exponential Retry, Attempt : {args.AttemptNumber}, Retry Delay : {args.RetryDelay}")
};

var pipelineC = new ResiliencePipelineBuilder()
    .AddRetry(exponentialRetryExecutionOptions)
    .Build();
attemptsCount = 0;
await pipelineC.ExecuteAsync((_, _) => callbackWithBadException(),
    ResilienceContextPool.Shared.Get(),
    state: "some state"
);

#endregion

# region Http Request based retry

var criticalPath = "/api/critical";
var urlKey = new ResiliencePropertyKey<string>("url");

// Retry for critical paths only
var urlBasedRetryOptions = new RetryStrategyOptions
{
    ShouldHandle = args =>
    {
        var isHttpException = args.Outcome.Exception is HttpRequestException;
        var isCriticalPath = args.Context.Properties.TryGetValue(urlKey, out var url)
                             && url.Contains(criticalPath);

        return ValueTask.FromResult(isHttpException && isCriticalPath);
    },
    MaxRetryAttempts = 3,
    Delay = TimeSpan.FromMilliseconds(100),
    OnRetry = args => callbackForLogging($"URL-based Retry, Attempt: {args.AttemptNumber}, Retry Delay: {args.RetryDelay}")
};
var pipelineD = new ResiliencePipelineBuilder()
    .AddRetry(urlBasedRetryOptions)
    .Build();

var httpCallback = (string url) => () => attemptsCount++ < 3
    ? throw new HttpRequestException($"Failed request to {url}")
    : ValueTask.CompletedTask;

attemptsCount = 0;
var context1 = ResilienceContextPool.Shared.Get();
context1.Properties.Set(new ResiliencePropertyKey<string>("url"), "/api/critical/data");
await pipelineD.ExecuteAsync((_, _) => httpCallback("/api/critical/data")(), context1, "state");

attemptsCount = 0;
var context2 = ResilienceContextPool.Shared.Get();
context2.Properties.Set(new ResiliencePropertyKey<string>("url"), "/api/normal/data");
try
{
    await pipelineD.ExecuteAsync((_, _) => httpCallback("/api/normal/data")(), context2, "state");
}
catch
{
    Console.WriteLine("URL-based Retry not handled for /api/normal/data");
}

#endregion

#region Http Response based retry

var pipelineBuilderE = new ResiliencePipelineBuilder();

#endregion

#region Handling multiple exceptions

var exceptions = ImmutableArray.Create(
    typeof(BadException),
    typeof(YetAnotherBadException)
);

var multipleHandleRetryOptions = new RetryStrategyOptions
{
    ShouldHandle = args => ValueTask.FromResult(args.Outcome.Exception != null && exceptions.Contains(args.Outcome.Exception.GetType())),
    MaxRetryAttempts = 3,
    Delay = TimeSpan.FromMilliseconds(100),
    OnRetry = args => callbackForLogging($"Multi-exception Retry, Attempt: {args.AttemptNumber}, Exception: {args.Outcome.Exception?.GetType().Name}")
};

attemptsCount = 0;
var pipelineF = new ResiliencePipelineBuilder()
    .AddRetry(multipleHandleRetryOptions)
    .Build();

await pipelineF.ExecuteAsync((_, _) => callbackWithMixedException(), context1, "state");

#endregion

#region Handling operation cancelled and timeout exceptions

var retryExceptWhenCancelledOrTimedOutOptions = new RetryStrategyOptions
{
    ShouldHandle = args => ValueTask.FromResult(args.Outcome.Exception is not null
                                                && args.Outcome.Exception is not (OperationCanceledException or TimeoutException)),
    MaxRetryAttempts = 3,
    Delay = TimeSpan.FromMilliseconds(100),
    OnRetry = args => callbackForLogging(
        $"Cancelled or Timeout Retry, Attempt: {args.AttemptNumber}, Exception: {args.Outcome.Exception?.GetType().Name}")
};

var pipelineG = new ResiliencePipelineBuilder()
    .AddRetry(retryExceptWhenCancelledOrTimedOutOptions)
    .Build();

try
{
    await pipelineG.ExecuteAsync((_, _) => ValueTask.FromException<OperationCanceledException>(new OperationCanceledException()), context1, "state");
}
catch
{
    Console.WriteLine("Cancelled operation not handled.");
}

try
{
    await pipelineG.ExecuteAsync((_, _) => ValueTask.FromException<TimeoutException>(new TimeoutException()), context1, "state");
}
catch
{
    Console.WriteLine("Timeout operation not handled.");
}

#endregion

public class BadException : Exception;

public class YetAnotherBadException : Exception;