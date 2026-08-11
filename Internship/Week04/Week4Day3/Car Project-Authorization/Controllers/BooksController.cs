using Microsoft.AspNetCore.Mvc;

namespace MyFirstApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    [HttpGet]
    public IActionResult GetBooks()
    {
        var books = new List<string>
        {
            "Clean Code",
            "C# Programming",
            "Python Programming"
        };

        return Ok(books);
    }

    [HttpGet("{id}")]
public IActionResult GetBookById(int id)
{
    var books = new List<string>
    {
        "Clean Code",
        "C# Programming",
        "Python Programming"
    };

    if (id < 1 || id > books.Count)
    {
        return NotFound();
    }

    return Ok(books[id - 1]);
}
}