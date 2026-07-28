using System.Diagnostics;

Console.WriteLine("Async Operations Demo");

Stopwatch stopwatch = Stopwatch.StartNew();

string users = await GetUsersAsync();
string orders = await GetOrdersAsync();
string products = await GetProductsAsync();

stopwatch.Stop();

Console.WriteLine(users);
Console.WriteLine(orders);
Console.WriteLine(products);

Console.WriteLine($"Total Time: {stopwatch.ElapsedMilliseconds} ms");

static async Task<string> GetUsersAsync()
{
    await Task.Delay(2000);
    return "Users";
}

static async Task<string> GetOrdersAsync()
{
    await Task.Delay(3000);
    return "Orders";
}

static async Task<string> GetProductsAsync()
{
    await Task.Delay(1000);
    return "Products";
}
