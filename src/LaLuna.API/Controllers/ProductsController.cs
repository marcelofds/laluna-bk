using LaLuna.Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LaLuna.API.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ProductPageDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] string sortBy = "featured",
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetProductsQuery(category, search, sortBy, minPrice, maxPrice, page, pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductBySlugQuery(slug), cancellationToken);
        return Ok(result);
    }

    [HttpGet("/api/categories")]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("featured")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeatured(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetProductsQuery(null, null, "featured", PageSize: 8),
            cancellationToken);
        return Ok(result.Items);
    }

    [HttpGet("new-arrivals")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNewArrivals(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetProductsQuery(null, null, "newest", PageSize: 8),
            cancellationToken);
        return Ok(result.Items);
    }

    [HttpGet("best-sellers")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBestSellers(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetProductsQuery(null, null, "best-selling", PageSize: 8),
            cancellationToken);
        return Ok(result.Items);
    }
}
