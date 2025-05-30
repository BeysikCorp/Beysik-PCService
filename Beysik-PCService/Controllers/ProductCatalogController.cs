//using RabbitMQ.Client;
using Microsoft.AspNetCore.Mvc;
using Beysik_PCService.Models;
using Beysik_PCService.Services;
using System.Text.Json;

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
    public async Task<ActionResult<Product>> Get([FromRoute] string id)
    {
        var product = await _productService.GetAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        return product;
    }

    [HttpGet("/products/tag")]

    public async Task<ActionResult<Product>> GetByTag([FromQuery] string tag)
    {
        var product = await _productService.GetByTagAsync(tag);

        if (product is null)
        {
            return NotFound();
        }

        return product;
    }

    [HttpPost("/products")]
    public async Task<IActionResult> Post([FromBody] Product newProduct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _productService.CreateAsync(newProduct);

        return CreatedAtAction(nameof(Get), new { id = newProduct.Id }, newProduct);
    }

    //This will be publish to rabbitMQ  if the item is created
    //    PublishProductCreated(newProduct);

    //        return CreatedAtAction(nameof(Get), new { id = newProduct.Id
    //}, newProduct);
    //    }

    //    private void PublishProductCreated(Product product)
    //{
    //    using var channel = _rabbitConnection.CreateModel();
    //    channel.QueueDeclare(queue: "product_created", durable: false, exclusive: false, autoDelete: false, arguments: null);

    //    var body = JsonSerializer.SerializeToUtf8Bytes(product);
    //    channel.BasicPublish(exchange: "", routingKey: "product_created", basicProperties: null, body: body);
    //}

    [HttpPut("/products/{id:length(24)}")]
    public async Task<IActionResult> Update([FromRoute] string id, Product updatedProduct)
    {
        var product = await _productService.GetAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        updatedProduct.Id = product.Id;

        await _productService.UpdateAsync(id, updatedProduct);

        return NoContent();
    }

    [HttpDelete("/products/{id:length(24)}")]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        var product = await _productService.GetAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        await _productService.RemoveAsync(id);

        return NoContent();
    }

}
