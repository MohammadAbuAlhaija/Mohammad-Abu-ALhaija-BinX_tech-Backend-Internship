namespace MyFirstApi.Models;

public class CustomerPhone
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string PhoneType { get; set; } = string.Empty;

    public Customer Customer { get; set; } = null!;
}