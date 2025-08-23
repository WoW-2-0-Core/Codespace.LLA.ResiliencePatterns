using Reactive.CircuitBreaker.N1_ThresholdConfiguration;
using Reactive.CircuitBreaker.N2_BreakDurationControl;
using Reactive.CircuitBreaker.N3_StateManagement;

// Threshold configuration examples
await FailureRatioExample.RunExampleAsync();
await MinimumThroughputExample.RunExampleAsync();
await SamplingDurationExample.RunExampleAsync();

// Break duration examples
await FixedBreakDurationExample.RunExampleAsync();
await DynamicBreakDurationExample.RunExampleAsync();

// State management examples
await StateMonitoringExample.RunExampleAsync();
await ManualControlExample.RunExampleAsync();
await StateBasedLogicExample.RunExampleAsync();