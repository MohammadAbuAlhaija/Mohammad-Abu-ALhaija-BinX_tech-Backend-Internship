using System.Diagnostics;

Console.WriteLine("Async Operations Demo");
/*
Stopwatch stopwatch = Stopwatch.StartNew();

string users = await GetUsersAsync();
string orders = await GetOrdersAsync();
string products = await GetProductsAsync();

stopwatch.Stop();

Console.WriteLine(users);
Console.WriteLine(orders);
Console.WriteLine(products);

Console.WriteLine($"Total Time: {stopwatch.ElapsedMilliseconds} ms");

Stopwatch stopwatch = Stopwatch.StartNew();

Task<string> usersTask = GetUsersAsync();
Task<string> ordersTask = GetOrdersAsync();
Task<string> productsTask = GetProductsAsync();

await Task.WhenAll(usersTask, ordersTask, productsTask);

stopwatch.Stop();

Console.WriteLine(await usersTask);
Console.WriteLine(await ordersTask);
Console.WriteLine(await productsTask);

Console.WriteLine($"Total Time: {stopwatch.ElapsedMilliseconds} ms");
*/
CancellationTokenSource source = new();

Task<string> ordersTask = GetOrdersAsync(source.Token);

await Task.Delay(2000);

source.Cancel();

try
{
    string orders = await ordersTask;

    Console.WriteLine(orders);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Orders operation was cancelled");
}

static async Task<string> GetUsersAsync()
{
    await Task.Delay(2000);
    return "Users";
}

static async Task<string> GetOrdersAsync(CancellationToken token)
{
    await Task.Delay(5000, token);

    return "Orders";
}

static async Task<string> GetProductsAsync()
{
    await Task.Delay(1000);
    return "Products";
}
