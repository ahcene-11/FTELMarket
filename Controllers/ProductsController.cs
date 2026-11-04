
using FTELMarket.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FTELMarket.Models;
using FTELMarket.DTOs;
namespace FTELMarket.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly FtelMarketDbContext _context;

    public ProductsController(FtelMarketDbContext context)
    {
        _context = context;
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProducts()
    {
        var products = await _context.Products
            .Select(p=> new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.StockQuantity,
                CategoryId = p.CategoryId,
                CreatedAt = p.CreatedAt,
                CategoryName = p.Category.Name                
            })
            .ToListAsync();
        return Ok(products);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> CreateProduct(ProductCreateRequest request)
    {
        var category = await _context.Categories.FindAsync(request.CategoryId);
        if (category == null)
        {
            return BadRequest($"Category with ID {request.CategoryId} does not exist.");
        }
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.Stock,
            CategoryId = request.CategoryId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();
        
        var response = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.StockQuantity,
            CategoryId = product.CategoryId,
            CreatedAt = product.CreatedAt,
            CategoryName = category.Name
        };
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(int id, ProductCreateRequest request)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound($"Produit introuvable");
        }

        var category = await _context.Categories.FindAsync(request.CategoryId);
        if (category == null)
        {
            return BadRequest($"La catégorie spécifiée n'existe pas.");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.StockQuantity = request.Stock;
        product.CategoryId = request.CategoryId;

        await _context.SaveChangesAsync();
        var response = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.StockQuantity,
            CategoryId = product.CategoryId,
            CreatedAt = product.CreatedAt,
            CategoryName = category.Name
        };
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if( product == null)
        {
            return NotFound("Produit introuvable");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return NoContent();

    }
}