# Polly Circuit Breaker Strategy

## Overview

Circuit breaker strategy prevents cascading failures by breaking the circuit when failure rate exceeds threshold. Blocks executions during break period, then gradually tests recovery.

### When to Use
- External service dependencies
- Database connections with potential overload
- API calls to unreliable endpoints
- Protection against downstream service failures

### When NOT to Use
- Local, in-memory operations
- One-time operations
- When immediate failure feedback is required
- Operations where blocking is worse than potential failure

## Key Concepts

### Circuit States
- **Closed**: Normal operation, monitoring failures
- **Open**: Circuit broken, blocking all executions
- **HalfOpen**: Testing recovery with probe calls
- **Isolated**: Manually held open (emergency mode)

### Core Properties
```csharp
var options = new CircuitBreakerStrategyOptions
{
    FailureRatio = 0.1,                     // Default: 10% failure rate
    MinimumThroughput = 100,                // Default: 100 calls
    SamplingDuration = TimeSpan.FromSeconds(30), // Default: 30s
    BreakDuration = TimeSpan.FromSeconds(5),     // Default: 5s
    ShouldHandle = /* predicate */,         // What failures to track
    OnOpened = /* callback */,              // Circuit opened event
    OnClosed = /* callback */,              // Circuit closed event
    OnHalfOpened = /* callback */           // Circuit half-opened event
};
```


## Anti-Pattern Examples

### ❌ Overly Sensitive Circuit Breaker
```csharp
// BAD: Breaks after just 1-2 failures
var oversensitive = new ResiliencePipelineBuilder()
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,
        MinimumThroughput = 1,  // ❌ Breaks on first failure
        SamplingDuration = TimeSpan.FromMinutes(5)
    })
    .Build();

// This will break immediately on any single failure
```


### ❌ No Fallback Strategy
```csharp
// BAD: Just throws exceptions when circuit is open
try 
{
    await circuitBreakerPipeline.ExecuteAsync(() => CallExternalAPI());
}
catch (BrokenCircuitException)
{
    throw; // ❌ No graceful degradation
}
```


### ❌ Circuit Breaker as Retry Mechanism
```csharp
// BAD: Using very short break duration as retry
var retryLikeCircuitBreaker = new ResiliencePipelineBuilder()
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        BreakDuration = TimeSpan.FromMilliseconds(100), // ❌ Too short
        FailureRatio = 0.9
    })
    .Build();
```


### ❌ Ignoring Circuit State
```csharp
// BAD: Not checking circuit state for expensive operations
public async Task<Data> GetDataAsync()
{
    // ❌ Always try expensive operation even when circuit is open
    return await pipeline.ExecuteAsync(async () =>
    {
        // Expensive database query that will just get blocked
        return await database.ComplexQuery();
    });
}
```


### ❌ Wrong Pipeline Order
```csharp
// BAD: Circuit breaker inside retry
var wrongOrder = new ResiliencePipelineBuilder()
    .AddCircuitBreaker(circuitBreakerOptions)  // ❌ Inner
    .AddRetry(retryOptions)                    // ❌ Outer
    .Build();

// This can cause retry to keep hitting open circuit
```
---

## Configuration Examples

### Threshold Configuration

#### [**Failure ratio**](../src/Reactive.CircuitBreaker/N1_ThresholdConfiguration/FailureRatioExample.cs)
Shows how failure ratio threshold determines when circuit opens. Demonstrates 50% failure ratio with different request patterns.

#### [**Minimum throughput**](../src/Reactive.CircuitBreaker/N1_ThresholdConfiguration/MinimumThroughputCircuitBreakerExample.cs)
Compares circuit behavior with insufficient vs sufficient call volume before breaking.

#### [**Sampling duration**](../src/Reactive.CircuitBreaker/N1_ThresholdConfiguration/SamplingDurationCircuitBreakerExample.cs)
Shows how time window affects failure tracking with requests fired within and beyond sampling period.

### Break Duration Control

#### [**Fixed break duration**](../src/Reactive.CircuitBreaker/N2_BreakDurationControl/FixedBreakDurationExample.cs)
Demonstrates fixed 2-second break with state transitions from open to half-open to closed.

#### [**Dynamic break duration**](../src/Reactive.CircuitBreaker/N2_BreakDurationControl/DynamicBreakDurationExample.cs)
Uses BreakDurationGenerator to calculate break time based on failure count (failureCount × 2 seconds).

### State Management

#### [**State monitoring**](../src/Reactive.CircuitBreaker/N3_StateManagement/StateMonitoringExample.cs)
Shows how to monitor circuit state using CircuitBreakerStateProvider during different operation phases.

#### [**Manual control**](../src/Reactive.CircuitBreaker/N3_StateManagement/ManualControlExample.cs)
Demonstrates manual isolation and closing of circuit using CircuitBreakerManualControl.

#### [**State-based logic**](../src/Reactive.CircuitBreaker/N3_StateManagement/StateBasedLogicExample.cs)
Implements different execution logic based on current circuit state with fallback for isolated state.

### Manual Control
```csharp
var manualControl = new CircuitBreakerManualControl();
await manualControl.IsolateAsync();  // Emergency stop
await manualControl.CloseAsync();    // Force recovery
```



---

## Exception Handling

### Built-in Exceptions
- **BrokenCircuitException** : thrown when circuit is open
- **IsolatedCircuitException** : thrown when manually isolated

Both exceptions provide:
- `RetryAfter`: minimum time before retry
- `TelemetrySource`: source information for debugging

## Event Callbacks

### Telemetry Events
- **OnCircuitOpened**: Error severity
- **OnCircuitClosed**: Information severity
- **OnCircuitHalfOpened**: Warning severity

## Advanced Patterns

### Dynamic Break Duration
```csharp
BreakDurationGenerator = args => {
    // Exponential backoff based on failure count
    var duration = TimeSpan.FromSeconds(Math.Pow(2, args.FailureCount));
    return new ValueTask<TimeSpan>(duration);
}
```

### Result-Based Circuit Breaking
```csharp
var options = new CircuitBreakerStrategyOptions<HttpResponseMessage>
{
    ShouldHandle = args => ValueTask.FromResult(
        args.Outcome.Result?.StatusCode >= HttpStatusCode.InternalServerError)
};
```

## Integration Examples

### HttpClient Integration
```csharp
services.AddHttpClient<ApiService>()
    .AddResilienceHandler("circuit-breaker", builder => 
        builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions()));
```

### Pipeline Composition
```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddRetry(retryOptions)           // Inner: Retry transient failures
    .AddCircuitBreaker(cbOptions)     // Outer: Break on persistent failures
    .Build();
```

### Health Monitoring
```csharp
var stateProvider = new CircuitBreakerStateProvider();
// Register health check that monitors stateProvider.CircuitState
```

---

## Questions & Exercises

[Rest of the content remains exactly the same...]

---

## Questions & Exercises

### ThresholdConfiguration

#### Questions
- How does FailureRatio interact with MinimumThroughput?
- What happens if MinimumThroughput isn't reached during SamplingDuration?
- When should you use conservative vs aggressive failure ratios?
- How does SamplingDuration affect circuit sensitivity?

#### Practice Exercises
- Configure circuit breaker that breaks after 3 failures in 5 seconds
- Create aggressive circuit breaker for cache operations (low tolerance)
- Build conservative circuit breaker for critical payment service
- Implement different thresholds for peak vs off-peak hours

### BreakDurationControl

#### Questions
- When should you use BreakDurationGenerator vs fixed BreakDuration?
- How do you implement exponential backoff for break duration?
- What are the trade-offs of longer vs shorter break durations?
- How does break duration affect service recovery time?

#### Practice Exercises
- Implement progressive break duration: 1s, 5s, 15s, 30s
- Create dynamic break duration based on time of day
- Build Fibonacci sequence break duration generator
- Design break duration that adapts to service load

### StateManagement

#### Questions
- How do you safely check circuit state without race conditions?
- When should you use manual control vs automatic recovery?
- How do you implement emergency stop functionality?
- What's the difference between Isolated and Open states?

#### Practice Exercises
- Build health check endpoint that reports circuit state
- Implement emergency isolation during deployment
- Create state-aware fallback logic
- Design circuit state dashboard for monitoring

### EventCallbacks

#### Questions
- What's the order of execution for OnOpened vs telemetry events?
- How do you correlate circuit events with application metrics?
- When should you use OnRetry vs circuit breaker events?
- How do you implement alerting based on circuit state?

#### Practice Exercises
- Log circuit events to structured logging system
- Create metrics for circuit open/close frequency
- Implement Slack alerts for circuit state changes
- Build circuit breaker event correlation with business metrics

### Integration Scenarios

#### Questions
- How do you configure per-service circuit breakers in microservices?
- When should circuit breaker be inside vs outside retry policy?
- How do you handle circuit breaker with caching strategies?
- What's the relationship between circuit breaker and load balancing?

#### Practice Exercises
- Configure HttpClient with per-endpoint circuit breakers
- Implement database circuit breaker with connection pooling
- Build Redis circuit breaker with fallback to in-memory cache
- Create circuit breaker registry for microservice communication

### Advanced Patterns

#### Questions
- How do you implement cascading failure prevention across services?
- When should you use shared vs isolated circuit breakers?
- How do you handle circuit breaker in pub/sub scenarios?
- What's the impact of circuit breaker on distributed transactions?

#### Practice Exercises
- Build circuit breaker that escalates to upstream services
- Implement circuit breaker with automatic dependency health checking
- Create circuit breaker with business rule integration
- Design circuit breaker for event-driven architecture