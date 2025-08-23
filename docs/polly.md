## Resilience Context

### Getting and Using Resilience Context

Resilience context is a thread-safe object for storing request-specific data. Since it uses a shared pool, always return it after use with a try-finally block:

```csharp
var resilienceContext = ResilienceContextPool.Shared.Get();
try
{
    // Use the context here
}
finally
{
    ResilienceContextPool.Shared.Return(resilienceContext);
}
```