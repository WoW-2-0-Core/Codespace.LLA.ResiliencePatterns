# Polly Retry Strategy

## Overview

Retry strategy re-executes failed operations with configurable delays and conditions. Handles transient failures that may self-correct.

### When to Use
- External API/database calls
- Network-dependent operations
- Strict uptime requirements

### When NOT to Use
- Local, in-memory operations
- Operations causing data corruption on retry
- When latency overhead > reliability

## Key Concepts

### Backoff Types
- **Constant**: Same delay between attempts
- **Linear**: Delay increases linearly (base × attempt)
- **Exponential**: Delay doubles each attempt (base × 2^attempt)

### Core Properties
```csharp
var options = new RetryStrategyOptions
{
    MaxRetryAttempts = 3,           // Default: 3
    Delay = TimeSpan.FromSeconds(2), // Default: 2s
    BackoffType = DelayBackoffType.Constant, // Default
    UseJitter = false,              // Default: false
    MaxDelay = null,                // No cap by default
    ShouldHandle = /* predicate */,  // What to retry
    OnRetry = /* callback */         // Retry notification
};
```

## Configuration Examples

### Backoff Strategies

- [**Constant delay**](../src/Reactive.Retry/N1_BackoffStrategies/ConstantDelayExample.cs)
- [**Linear backoff**](../src/Reactive.Retry/N1_BackoffStrategies/LinearBackoffExample.cs)
- [**Exponential with jitter**](../src/Reactive.Retry/N1_BackoffStrategies/ExponentialBackoffExample.cs)
- [**Jitter variations**](../src/Reactive.Retry/N1_BackoffStrategies/JitterVariationsExample.cs)

### Delay Control

- [**Custom DelayGenerator**](../src/Reactive.Retry/N2_DelayControl/CustomDelayGeneratorExample.cs)
- [**MaxDelay limiting**](../src/Reactive.Retry/N2_DelayControl/MaxDelayLimitingExample.cs)
- [**Extract from HTTP headers**](../src/Reactive.Retry/N2_DelayControl/ExtractDelayFromResultExample.cs)
- [**Infinite retry**](../src/Reactive.Retry/N2_DelayControl/InfiniteRetryExample.cs)

## Exception Handling

### Exception Patterns
- **Single exception type**: `src/HandledExceptions/SingleExceptionType.cs`
- **Multiple exceptions**: `src/HandledExceptions/MultipleExceptions.cs`
- **Exception filtering**: `src/HandledExceptions/ExceptionFiltering.cs`
- **Exclusion patterns**: `src/HandledExceptions/ExclusionPatterns.cs`

## Result-Based Retry

### Result Patterns
- **HTTP status codes**: `src/HandledResults/HttpStatusCodes.cs`
- **Custom result types**: `src/HandledResults/CustomResultTypes.cs`
- **Result predicates**: `src/HandledResults/ResultPredicates.cs`

## Context-Based Logic

### Conditional Patterns
- **Context properties**: `src/ConditionalLogic/ContextBasedRetry.cs`
- **URL-based logic**: `src/ConditionalLogic/UrlBasedLogic.cs`
- **Property-driven retry**: `src/ConditionalLogic/PropertyDrivenRetry.cs`

## Monitoring & Telemetry

### Monitoring Patterns
- **OnRetry callbacks**: `src/MonitoringTelemetry/OnRetryCallbacks.cs`
- **Telemetry events**: `src/MonitoringTelemetry/TelemetryEvents.cs`
- **Performance metrics**: `src/MonitoringTelemetry/PerformanceMetrics.cs`

### Telemetry Events
- **ExecutionAttempt**: Information/Warning/Error severity
- **OnRetry**: Warning severity
- Attempt 0 = original execution
- Only last failed attempt gets Error severity

## Best Practices

### ✅ DO
- Use exception collections instead of multiple `Handle<>()` calls
- Combine separate retry policies in pipeline for different delay strategies
- Put shared logic inside `ExecuteAsync` delegate for all attempts
- Use `ShouldHandle` for retry conditions, not `OnRetry`
- Separate HTTP request retry from response parsing retry

### ❌ DON'T
- Use retry for periodic job execution (use schedulers instead)
- Mix increasing/constant delays in single `DelayGenerator`
- Embed cancellation logic in `OnRetry` callback
- Retry operations that cause data corruption

## Integration Examples

### Common Integrations
- **HttpClient**: See examples in `src/` for typed client patterns
- **Pipeline composition**: Multiple examples throughout `src/` folders
- **DI container**: Configuration patterns in example files

---

## Questions & Exercises

### BackoffStrategies

#### Questions
- What's the formula for exponential backoff delay calculation?
- How does jitter prevent thundering herd problem?
- When would you choose linear over exponential backoff?
- What's the difference between jitter calculation for exponential vs other backoff types?

#### Practice Exercises
- Write constant retry with 500ms delay, 3 attempts
- Implement linear backoff starting at 200ms, 4 attempts
- Create exponential backoff with jitter, base 100ms, 5 attempts
- Build custom backoff: 100ms, 300ms, 1000ms, 3000ms

### DelayControl

#### Questions
- When should DelayGenerator override BackoffType calculations?
- How does MaxDelay interact with DelayGenerator?
- What happens when DelayGenerator returns null?
- Why use infinite retry (int.MaxValue) in production?

#### Practice Exercises
- Write DelayGenerator with fibonacci sequence: 100ms, 100ms, 200ms, 300ms, 500ms
- Implement retry that caps at 5 seconds regardless of exponential calculation
- Extract delay from HTTP Retry-After header, fallback to 2-second default
- Create infinite retry with cancellation token timeout after 30 seconds

### HandledExceptions

#### Questions
- What exceptions are handled by default in RetryStrategyOptions?
- Why exclude OperationCanceledException from retry?
- How do you handle both specific exceptions and their derived types?
- When would you filter exceptions by message content?

#### Practice Exercises
- Handle only SqlException and HttpRequestException
- Retry on any IOException except DirectoryNotFoundException
- Filter HttpRequestException to retry only on 5xx status messages
- Create predicate for network-related exceptions using exception inheritance

### HandledResults

#### Questions
- What's the difference between exception-based and result-based retry?
- How do you combine exception and result handling in same policy?
- When is result-based retry more appropriate than exception-based?
- How does generic typing work with result-based retry?

#### Practice Exercises
- Retry on HTTP status codes 502, 503, 504
- Handle custom Result<T> type where Success=false and ErrorCode>=500
- Retry when string result is null, empty, or contains "RETRY"
- Create retry for API response where Status != "Success"

### ConditionalLogic

#### Questions
- How do ResiliencePropertyKey and context properties work?
- What's the execution order when multiple conditions are checked?
- How do you pass context data through the pipeline?
- When should you use context-based vs global retry policies?

#### Practice Exercises
- Retry only for URLs containing "/api/critical/"
- Different retry counts based on operation priority (1-10 scale)
- Retry database operations but not cache operations using service type property
- Implement tenant-based retry where premium tenants get more attempts

### MonitoringTelemetry

#### Questions
- What's the difference between OnRetry callback and telemetry events?
- How do you distinguish between retry attempt vs execution attempt in logs?
- What severity levels are used for different retry scenarios?
- How do you calculate retry success rate from telemetry data?

#### Practice Exercises
- Log structured JSON with attempt number, delay, exception type, timestamp
- Track total execution time including all retry delays
- Count success/failure rates per operation type
- Create metrics for average retry attempts per operation

### Integration Scenarios

#### Questions
- How do you register retry policies in DI container?
- What's the difference between IAsyncPolicy and ResiliencePipeline?
- How do you combine retry with HttpClient using AddPolicyHandler?
- When should you use policy registry vs inline policy creation?

#### Practice Exercises
- Configure HttpClient with typed client and retry policy in DI
- Create policy registry with named retry policies for different services
- Implement database repository with retry for transient SQL errors
- Build microservice client with retry, circuit breaker, and timeout combined

### Advanced Patterns

#### Questions
- How do you implement retry with different strategies for quick vs slow attempts?
- What's the anti-pattern of using retry for periodic execution?
- How do you prevent retry state machines using DelayGenerator?
- When should you split retry policies instead of combining conditions?

#### Practice Exercises
- Create dual retry: 3 quick attempts (100ms exponential) + 2 slow attempts (5s constant)
- Implement retry that switches strategy based on exception type
- Build context-aware retry that escalates attempts based on failure count
- Design retry policy that adapts delay based on time of day