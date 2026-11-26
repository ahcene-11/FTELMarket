using FTELMarket.Services;
using Microsoft.AspNetCore.Mvc;

namespace FTELMarket.Controllers;

[ApiController]
[Route("api/flaky")]
public class FlakyTestController(FlakyService flakyService) : ControllerBase
{
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers()
    {
        var customers = await flakyService.GetAllCustomersAsync();

        return Ok(customers);
    }

    [HttpPost("synchronize-customers")]
    public async Task<IActionResult> SynchronizeCustomers()
    {
        await flakyService.SynchronizeCustomersAsync();

        return Ok("Synchronisation des clients terminée avec succès.");
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts()
    {
        var products = await flakyService.GetAllProductsAsync();
        
        return Ok(products);
    }

    [HttpPost("synchronize-products")]
    public async Task<IActionResult> SynchronizeProducts()
    {
        await flakyService.SynchronizeProductsAsync();

        return Ok("Synchronisation des produits terminée avec succès.");
    }
}