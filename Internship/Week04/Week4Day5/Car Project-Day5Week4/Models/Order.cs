namespace MyFirstApi.Models;

public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int CarId { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal SalePrice { get; set; }

    public string Status { get; set; } = string.Empty;

    public string PaymentMethod { get; set; } = string.Empty;

    public Customer Customer { get; set; } = null!;

    public Car Car { get; set; } = null!;
}