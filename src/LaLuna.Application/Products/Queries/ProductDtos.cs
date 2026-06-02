namespace LaLuna.Application.Products.Queries;

public record ProductDto(
    int Id,
    string Name,
    string Slug,
    string Description,
    decimal Price,
    decimal? OriginalPrice,
    int? DiscountPercent,
    List<string> Images,
    int CategoryId,
    string CategoryName,
    int Stock,
    string? Sku,
    bool IsFeatured,
    bool IsNew,
    bool FreeShipping,
    DateTime CreatedAt
);

public record CategoryDto(
    int Id,
    string Name,
    string Slug,
    string? ImageUrl,
    int? ParentId,
    List<CategoryDto>? Children = null
);

public record ProductPageDto(
    List<ProductDto> Items,
    int Total,
    int Page,
    int PageSize,
    int TotalPages
);
