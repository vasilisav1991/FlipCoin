// WebApplicationFactory's host resolution relies on a process-wide diagnostic
// listener and is not safe to run concurrently. Serialize the integration tests.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
