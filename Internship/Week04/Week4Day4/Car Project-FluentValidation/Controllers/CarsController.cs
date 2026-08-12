using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MyFirstApi.DTOs;
using MyFirstApi.Data;
using MyFirstApi.Models;

namespace MyFirstApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CarsController : ControllerBase
{
    // Gives access to the database through EF Core.
    private readonly AppDbContext _context;

    public CarsController(AppDbContext context)
    {
        _context = context;
    }
    
[Authorize(Policy = "CanManageCars")]
[HttpPost]
public async Task<IActionResult> Create(CreateCarRequest request)
{
    // Convert the request data into a Car entity.
    var car = new Car
    {
        Make = request.Make,
        Model = request.Model,
        Year = request.Year,
        Color = request.Color,
        VIN = request.VIN,
        Price = request.Price,
        Status = request.Status
    };

    // Add the car to the database.
    _context.Cars.Add(car);
    await _context.SaveChangesAsync();

    // Return the created car.
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
    public async Task<IActionResult> Update(int id, UpdateCarRequest request)
   {
    // Find the existing car.
    var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);

    if (car == null)
    {
        return NotFound(new
        {
            message = $"Car with ID {id} was not found."
        });
    }

    // Update the car data from the request.
    car.Make = request.Make;
    car.Model = request.Model;
    car.Year = request.Year;
    car.Color = request.Color;
    car.VIN = request.VIN;
    car.Price = request.Price;
    car.Status = request.Status;

    // Save the changes.
    await _context.SaveChangesAsync();

    return NoContent();
    }

    [Authorize(Roles = "Admin")]
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