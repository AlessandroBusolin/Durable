using Xunit;

// The Durable Touchstone cases share a single process-wide database provider, so they must not run in
// parallel. Disable xUnit parallelization to keep execution sequential and deterministic.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
