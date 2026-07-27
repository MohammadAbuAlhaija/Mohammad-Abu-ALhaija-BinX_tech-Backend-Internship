using System;
using System.Collections.Generic;

List<Customer> customers = new()
{
    new Customer { Id = 1, Name = "Ali" },
    new Customer { Id = 2, Name = "Sara" },
    new Customer { Id = 3, Name = "Ahmad" },
    new Customer { Id = 4, Name = "Lina" },
    new Customer { Id = 5, Name = "Omar" },
    new Customer { Id = 6, Name = "Noor" }
};

List<Order> orders = new()
{
    new Order
    {
        Id = 1,
        CustomerId = 1,
        Amount = 150,
        Items = new() { "Mouse", "Keyboard" }
    },

    new Order
    {
        Id = 2,
        CustomerId = 2,
        Amount = 300,
        Items = new() { "Monitor", "USB" }
    },

    new Order
    {
        Id = 3,
        CustomerId = 1,
        Amount = 200,
        Items = new() { "Headset" }
    },

    new Order
    {
        Id = 4,
        CustomerId = 3,
        Amount = 450,
        Items = new() { "Laptop", "Mouse" }
    },

    new Order
    {
        Id = 5,
        CustomerId = 5,
        Amount = 100,
        Items = new() { "USB" }
    },

    new Order
    {
        Id = 6,
        CustomerId = 2,
        Amount = 250,
        Items = new() { "Webcam", "Keyboard" }
    }
};


var ordersByCustomer = orders
    .GroupBy(o => o.CustomerId)
    .Select(g => new
    {
        CustomerId = g.Key,
        Total = g.Sum(o => o.Amount)
    });

foreach (var item in ordersByCustomer)
{
    Console.WriteLine($"Customer ID: {item.CustomerId}, Total: {item.Total}");
}

var customerOrders = customers.Join(
    orders,
    customer => customer.Id,
    order => order.CustomerId,
    (customer, order) => new
    {
        customer.Name,
        order.Amount
    });

    foreach (var item in customerOrders)
{
    Console.WriteLine($"{item.Name} - {item.Amount}");
}

var allItems = orders.SelectMany(o => o.Items);

foreach (var item in allItems)
{
    Console.WriteLine(item);
}

//////////////////////////////////////////////////////////////////
// Deferred Execution Example

var expensiveOrders = orders.Where(o => o.Amount >= 300);

// Add a new order after defining the query
orders.Add(new Order
{
    Id = 7,
    CustomerId = 4,
    Amount = 500,
    Items = new() { "Printer" }
});

// Enumerate the query
foreach (var order in expensiveOrders)
{
    Console.WriteLine($"Order ID: {order.Id}, Amount: {order.Amount}");
}
// Note:
// The query uses deferred execution.
// It is not executed when it is defined.
// It executes only when the foreach loop starts.
// Since a new order was added before enumeration,
// the new order is included in the result.
//////////////////////////////////////////////////////////



class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public double Amount { get; set; }

    public List<string> Items { get; set; } = new();
}


