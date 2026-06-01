namespace LaLuna.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal? OriginalPrice { get; private set; }
    public string? Sku { get; private set; }
    public int Stock { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsFeatured { get; private set; }
    public bool IsNew { get; private set; }
    public bool FreeShipping { get; private set; }
    public int CategoryId { get; private set; }

    public Category? Category { get; private set; }
    public ICollection<ProductImage> Images { get; private set; } = [];

    public int? DiscountPercent =>
        OriginalPrice.HasValue && OriginalPrice > 0
            ? (int)Math.Round((1 - Price / OriginalPrice.Value) * 100)
            : null;

    private Product() { }

    public static Product Create(
        string name,
        string slug,
        string description,
        decimal price,
        int categoryId,
        int stock,
        string? sku = null,
        decimal? originalPrice = null,
        bool isFeatured = false,
        bool isNew = true,
        bool freeShipping = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

        return new Product
        {
            Name = name,
            Slug = slug.ToLowerInvariant(),
            Description = description,
            Price = price,
            OriginalPrice = originalPrice,
            CategoryId = categoryId,
            Stock = stock,
            Sku = sku,
            IsFeatured = isFeatured,
            IsNew = isNew,
            FreeShipping = freeShipping
        };
    }

    public void UpdateStock(int quantity) => Stock = quantity;

    public void Deactivate() { IsActive = false; SetUpdatedAt(); }
    public void Activate() { IsActive = true; SetUpdatedAt(); }
}
