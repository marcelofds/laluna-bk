using LaLuna.Application.Common.Interfaces;
using LaLuna.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaLuna.Application.Products.Queries;

public record GetProductBySlugQuery(string Slug) : IRequest<ProductDto>;

public class GetProductBySlugQueryHandler(IAppDbContext db)
    : IRequestHandler<GetProductBySlugQuery, ProductDto>
{
    public async Task<ProductDto> Handle(
        GetProductBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var product = await db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Slug == request.Slug && p.IsActive, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Product), request.Slug);

        return new ProductDto(
            product.Id,
            product.Name,
            product.Slug,
            product.Description,
            product.Price,
            product.OriginalPrice,
            product.DiscountPercent,
            product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),
            product.CategoryId,
            product.Category?.Name ?? string.Empty,
            product.Stock,
            product.Sku,
            product.IsFeatured,
            product.IsNew,
            product.FreeShipping,
            product.CreatedAt
        );
    }
}
