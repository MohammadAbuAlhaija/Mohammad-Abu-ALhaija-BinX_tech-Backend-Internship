
Console.WriteLine("Async Operations Demo");

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
