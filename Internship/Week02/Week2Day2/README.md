## 📅 Week 2 — Day 2:Advanced LINQ & Deferred Execution

## Topics Covered
- Deferred vs. Immediate Execution
- Grouping data with `GroupBy`
- Joining data with `Join`
- Flattening nested collections with `SelectMany`

## GroupBy — Aggregating Orders per Customer
Groups orders by `CustomerId` and sums the amount per group.

```csharp
var ordersByCustomer = orders
    .GroupBy(o => o.CustomerId)
    .Select(g => new { CustomerId = g.Key, Total = g.Sum(o => o.Amount) });
```

## Join — Combining Customers with Their Orders
Matches `Customer.Id` with `Order.CustomerId` to produce a combined shape.

```csharp
var customerOrders = customers.Join(
    orders,
    customer => customer.Id,
    order => order.CustomerId,
    (customer, order) => new { customer.Name, order.Amount });
```

## SelectMany — Flattening Order Items
Flattens each order's `Items` list into a single sequence of all items across all orders.

```csharp
var allItems = orders.SelectMany(o => o.Items);
```

## Deferred Execution
A LINQ query isn't executed when defined — only when enumerated (e.g. by `foreach`). This means changes to the source collection *before* enumeration affect the result.

```csharp
var expensiveOrders = orders.Where(o => o.Amount >= 300);

orders.Add(new Order { Id = 7, CustomerId = 4, Amount = 500, Items = new() { "Printer" } });

foreach (var order in expensiveOrders) { ... }
// The new order (Id = 7) IS included, since the query only ran at the foreach.
```

**Key takeaway:** the query variable is just a definition, not a result — the data is pulled fresh at enumeration time.