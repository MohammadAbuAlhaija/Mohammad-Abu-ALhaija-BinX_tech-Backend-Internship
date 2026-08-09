namespace MyFirstApi.Models;

public class Customer
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public ICollection<CustomerPhone> CustomerPhones { get; set; }
        = new List<CustomerPhone>();

    public ICollection<Order> Orders { get; set; }
        = new List<Order>();
}