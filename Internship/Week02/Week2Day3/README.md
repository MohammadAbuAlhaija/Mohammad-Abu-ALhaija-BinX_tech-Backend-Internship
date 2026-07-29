# Week 2 - Day 3: Async/Await Deep Dive & Concurrency Basics

## Overview
Explored the Task-based asynchronous pattern, sequential vs concurrent execution, and cancellation tokens.

## Key Concepts

### 1. Sequential Awaits (slow)
Awaiting one call after another runs them one at a time, even when they're independent:
```csharp
string users = await GetUsersAsync();
string orders = await GetOrdersAsync();
string products = await GetProductsAsync();
// Total time ≈ sum of all delays
```

### 2. Concurrent Execution with Task.WhenAll
Starting all tasks first, then awaiting them together, runs them in parallel:
```csharp
Task<string> usersTask = GetUsersAsync();
Task<string> ordersTask = GetOrdersAsync();
Task<string> productsTask = GetProductsAsync();

await Task.WhenAll(usersTask, ordersTask, productsTask);
// Total time ≈ slowest single operation
```

### 3. Cancellation Tokens
A `CancellationToken` lets a running task be stopped early instead of waiting for it to finish:
```csharp
CancellationTokenSource source = new();
Task<string> ordersTask = GetOrdersAsync(source.Token);

await Task.Delay(2000);
source.Cancel();

try
{
    string orders = await ordersTask;
}
catch (OperationCanceledException)
{
    Console.WriteLine("Orders operation was cancelled");
}
```
`Task.Delay(5000, token)` respects the token, so cancelling after 2s throws `OperationCanceledException` instead of waiting the full 5s.

## Lab Summary
- Wrote 3 async methods simulating different data sources (`Task.Delay`)
- Measured sequential vs `Task.WhenAll` execution time
- Added a `CancellationToken` to demonstrate cancelling a long-running operation mid-flight

## Takeaway
`Task.WhenAll` is the right tool when operations are independent — it cuts total wait time from "sum of delays" to "slowest delay." Cancellation tokens prevent wasted work when an operation is no longer needed.