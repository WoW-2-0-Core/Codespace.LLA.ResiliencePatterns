using Reactive.Retry;
using Reactive.Retry.N1_BackoffStrategies;
using Reactive.Retry.N2_DelayControl;
using Reactive.Retry.N3_HandlingExceptions;
using Reactive.Retry.N4_HandlingResults;
using Reactive.Retry.N5_ConditionalLogic;

// Backoff Strategy examples
await Executor.ExecuteAsync(ConstantDelayExample.RunExampleAsync, addNewLine: true);
await Executor.ExecuteAsync(LinearBackoffExample.RunExampleAsync, addNewLine: true);
await Executor.ExecuteAsync(ExponentialBackoffExample.RunExampleAsync, addNewLine: true);
await Executor.ExecuteAsync(JitterVariationsExample.RunExampleAsync, addNewLine: true);

// Delay Control 
await Executor.ExecuteAsync(CustomDelayGeneratorExample.RunExampleAsync, addNewLine: true);
await Executor.ExecuteAsync(MaxDelayLimitingExample.RunExampleAsync, addNewLine: true);
await Executor.ExecuteAsync(ExtractDelayFromResultExample.RunExampleAsync, addNewLine: true);
await Executor.ExecuteAsync(InfiniteRetryExample.RunExampleAsync, addNewLine: true);

// Handling Exceptions
await Executor.ExecuteAsync(SingleExceptionTypeExample.RunExampleAsync, addNewLine: true);
await Executor.ExecuteAsync(MultipleExceptionsExample.RunExampleAsync, addNewLine: true);
await Executor.ExecuteAsync(ExceptionsFilteringExample.RunExampleAsync, addNewLine: true);
await Executor.ExecuteAsync(ExceptionsExclusivePatternsExample.RunExampleAsync, addNewLine: true);

// Handling Results
await Executor.ExecuteAsync(HttpStatusCodesExample.RunExampleAsync, addNewLine: true);
await Executor.ExecuteAsync(CustomResultTypesExample.RunExampleAsync, addNewLine: true);

// Conditional Logic
await Executor.ExecuteAsync(ContextBasedLogicExample.RunExampleAsync, addNewLine: true);
await Executor.ExecuteAsync(UrlBasedLogicExample.RunExampleAsync, addNewLine: true);