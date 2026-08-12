namespace MyFirstApi.DTOs;

public class CreateCarRequest
{
    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

   public string Color { get; set; } = string.Empty;

    public string VIN { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Status { get; set; } = string.Empty;
}