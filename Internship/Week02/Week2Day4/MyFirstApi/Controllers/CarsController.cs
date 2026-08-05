using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;
using MyFirstApi.Models;

namespace MyFirstApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarsController : ControllerBase
{
    // Gives access to the database through EF Core.
    private readonly AppDbContext _context;

    public CarsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
public async Task<IActionResult> Create(Car car)
{
    // Check the required car data before saving.
    if (string.IsNullOrWhiteSpace(car.Make) ||
        string.IsNullOrWhiteSpace(car.Model) ||
        string.IsNullOrWhiteSpace(car.VIN) ||
        car.Year <= 0 ||
        car.Price <= 0)
    {
        return BadRequest(new
        {
            message = "Invalid car data."
        });
    }

    // Add the new car to the DbContext.
    _context.Cars.Add(car);

    // Save the new car to the database.
    await _context.SaveChangesAsync();

    // Return 201 Created with the URL of the new car.
    return CreatedAtAction(
        nameof(GetById),
        new { id = car.Id },
        car
    );
}

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Read all cars without tracking since this is a read-only request.
        var cars = await _context.Cars
            .AsNoTracking()
            .ToListAsync();

        return Ok(cars);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // Find a car by its ID.
        var car = await _context.Cars
            .AsNoTracking()
            .FirstOrDefaultAsync(car => car.Id == id);

        if (car == null)
        {
            return NotFound(new
            {
                message = $"Car with ID {id} was not found."
            });
        }

        return Ok(car);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Car updatedCar)
    {
        // Check if the input data is valid.
        if (string.IsNullOrWhiteSpace(updatedCar.Make) ||
            string.IsNullOrWhiteSpace(updatedCar.Model) ||
            string.IsNullOrWhiteSpace(updatedCar.VIN) ||
            updatedCar.Year <= 0 ||
            updatedCar.Price <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid car data."
            });
        }

        // Find the existing car.
        var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);

        if (car == null)
        {
            return NotFound(new
            {
                message = $"Car with ID {id} was not found."
            });
        }

        // Update the car properties.
        car.Make = updatedCar.Make;
        car.Model = updatedCar.Model;
        car.Year = updatedCar.Year;
        car.Color = updatedCar.Color;
        car.VIN = updatedCar.VIN;
        car.Price = updatedCar.Price;
        car.Status = updatedCar.Status;

        // Save the changes.
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Find the car by its ID.
        var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);

        if (car == null)
        {
            return NotFound(new
            {
                message = $"Car with ID {id} was not found."
            });
        }

        // Remove the car from the DbContext.
        _context.Cars.Remove(car);

        // Save the changes.
        await _context.SaveChangesAsync();

        return NoContent();
    }
}