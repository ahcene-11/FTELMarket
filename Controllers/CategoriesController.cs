using FTELMarket.Data;
using Microsoft.AspNetCore.Mvc;
using FTELMarket.DTOs;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly FtelMarketDbContext _context;

    public CategoriesController(FtelMarketDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategories()
    {
        var categories = await _context.Categories
            .Select(c=> new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync();

        return Ok(categories);
    }
}