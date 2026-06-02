using LaLuna.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaLuna.Application.Products.Queries;

public record GetCategoriesQuery : IRequest<List<CategoryDto>>;
public record GetFlatCategoriesQuery : IRequest<List<CategoryDto>>;

public class GetCategoriesQueryHandler(IAppDbContext db)
    : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Slug, c.ImageUrl, c.ParentId, null))
            .ToListAsync(cancellationToken);

        var childrenByParent = categories
            .Where(c => c.ParentId.HasValue)
            .GroupBy(c => c.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        CategoryDto WithChildren(CategoryDto category)
        {
            var children = childrenByParent.TryGetValue(category.Id, out var value)
                ? value.Select(WithChildren).ToList()
                : [];

            return category with { Children = children };
        }

        return categories
            .Where(c => c.ParentId is null)
            .Select(WithChildren)
            .ToList();
    }
}

public class GetFlatCategoriesQueryHandler(IAppDbContext db)
    : IRequestHandler<GetFlatCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(
        GetFlatCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        return await db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Slug, c.ImageUrl, c.ParentId, null))
            .ToListAsync(cancellationToken);
    }
}
