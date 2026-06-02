using LaLuna.Application.Common.Interfaces;
using LaLuna.Application.Products.Queries;
using LaLuna.Domain.Entities;
using LaLuna.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaLuna.Application.Products.Commands;

public record CreateCategoryCommand(
    string Name,
    string Slug,
    string? ImageUrl,
    int? ParentId
) : IRequest<CategoryDto>;

public record UpdateCategoryCommand(
    int Id,
    string Name,
    string Slug,
    string? ImageUrl,
    int? ParentId
) : IRequest<CategoryDto>;

public record DeleteCategoryCommand(int Id) : IRequest;

public class CreateCategoryCommandHandler(IAppDbContext db)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        ValidateRequired(request.Name, request.Slug);

        var slug = NormalizeSlug(request.Slug);
        await EnsureSlugIsAvailable(slug, null, cancellationToken);
        await EnsureParentExists(request.ParentId, cancellationToken);

        var category = Category.Create(
            request.Name.Trim(),
            slug,
            NormalizeOptional(request.ImageUrl),
            request.ParentId);

        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(category);
    }

    private async Task EnsureSlugIsAvailable(
        string slug,
        int? currentCategoryId,
        CancellationToken cancellationToken)
    {
        var exists = await db.Categories.AnyAsync(
            c => c.Slug == slug && c.Id != currentCategoryId,
            cancellationToken);

        if (exists)
            throw new ValidationException($"Category slug '{slug}' already exists.");
    }

    private async Task EnsureParentExists(int? parentId, CancellationToken cancellationToken)
    {
        if (parentId is null) return;

        var exists = await db.Categories.AnyAsync(c => c.Id == parentId, cancellationToken);
        if (!exists)
            throw new ValidationException("Parent category does not exist.");
    }

    private static string NormalizeSlug(string slug) => slug.Trim().ToLowerInvariant();
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void ValidateRequired(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(slug))
            throw new ValidationException("Category name and slug are required.");
    }

    private static CategoryDto ToDto(Category category) =>
        new(category.Id, category.Name, category.Slug, category.ImageUrl, category.ParentId);
}

public class UpdateCategoryCommandHandler(IAppDbContext db)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        ValidateRequired(request.Name, request.Slug);

        var category = await db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.Id);

        if (request.ParentId == request.Id)
            throw new ValidationException("A category cannot be its own parent.");

        var slug = NormalizeSlug(request.Slug);
        await EnsureSlugIsAvailable(slug, request.Id, cancellationToken);
        await EnsureParentExists(request.ParentId, cancellationToken);

        category.Update(
            request.Name.Trim(),
            slug,
            NormalizeOptional(request.ImageUrl),
            request.ParentId);

        await db.SaveChangesAsync(cancellationToken);

        return ToDto(category);
    }

    private async Task EnsureSlugIsAvailable(
        string slug,
        int currentCategoryId,
        CancellationToken cancellationToken)
    {
        var exists = await db.Categories.AnyAsync(
            c => c.Slug == slug && c.Id != currentCategoryId,
            cancellationToken);

        if (exists)
            throw new ValidationException($"Category slug '{slug}' already exists.");
    }

    private async Task EnsureParentExists(int? parentId, CancellationToken cancellationToken)
    {
        if (parentId is null) return;

        var exists = await db.Categories.AnyAsync(c => c.Id == parentId, cancellationToken);
        if (!exists)
            throw new ValidationException("Parent category does not exist.");
    }

    private static string NormalizeSlug(string slug) => slug.Trim().ToLowerInvariant();
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void ValidateRequired(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(slug))
            throw new ValidationException("Category name and slug are required.");
    }

    private static CategoryDto ToDto(Category category) =>
        new(category.Id, category.Name, category.Slug, category.ImageUrl, category.ParentId);
}

public class DeleteCategoryCommandHandler(IAppDbContext db)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.Id);

        var hasProducts = await db.Products
            .AnyAsync(p => p.CategoryId == request.Id, cancellationToken);

        if (hasProducts)
            throw new ValidationException("Cannot delete a category with products.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);
    }
}
