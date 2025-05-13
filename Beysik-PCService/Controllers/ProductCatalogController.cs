using RabbitMQ.Client;
using Microsoft.AspNetCore.Mvc;
using Beysik_PCService.Models;
using Beysik_PCService.Services;

namespace Beysik_PCService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductCatalogController : ControllerBase
{
    private readonly ProductService _productService;
    public ProductCatalogController(ProductService productService) =>
        _productService = productService;


    [HttpGet("/products")]
    public async Task<List<Product>> Get() =>
        await _productService.GetAsync();

    [HttpGet("/products/{id:length(24)}")]
    public async Task<ActionResult<Product>> Get(string id)
    {
        var product = await _productService.GetAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        return product;
    }

    [HttpPost("/products")]
    public async Task<IActionResult> Post(Product newProduct)
    {
        await _productService.CreateAsync(newProduct);

        return CreatedAtAction(nameof(Get), new { id = newProduct.Id }, newProduct);
    }

    [HttpPut("/products/{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Product updatedProduct)
    {
        var book = await _productService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        updatedProduct.Id = book.Id;

        await _productService.UpdateAsync(id, updatedProduct);

        return NoContent();
    }
    //not active delete
    [HttpDelete("/products/{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var book = await _productService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        await _productService.RemoveAsync(id);

        return NoContent();
    }
}
