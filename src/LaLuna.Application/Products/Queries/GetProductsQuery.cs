using LaLuna.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaLuna.Application.Products.Queries;

public record GetProductsQuery(
    string? CategorySlug,
    string? Search,
    string SortBy = "featured",
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int Page = 1,
    int PageSize = 24
) : IRequest<ProductPageDto>;

public class GetProductsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetProductsQuery, ProductPageDto>
{
    public async Task<ProductPageDto> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(request.CategorySlug))
            query = query.Where(p => p.Category!.Slug == request.CategorySlug);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p =>
                p.Name.Contains(request.Search) ||
                p.Description.Contains(request.Search));

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        query = request.SortBy switch
        {
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "price-asc" => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            "best-selling" => query.OrderByDescending(p => p.Id),
            _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new ProductPageDto(
            items.Select(MapToDto).ToList(),
            total,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling((double)total / request.PageSize)
        );
    }

    private static ProductDto MapToDto(Domain.Entities.Product p) =>
        new(
            p.Id,
            p.Name,
            p.Slug,
            p.Description,
            p.Price,
            p.OriginalPrice,
            p.DiscountPercent,
            p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),
            p.CategoryId,
            p.Category?.Name ?? string.Empty,
            p.Stock,
            p.Sku,
            p.IsFeatured,
            p.IsNew,
            p.FreeShipping,
            p.CreatedAt
        );
}
