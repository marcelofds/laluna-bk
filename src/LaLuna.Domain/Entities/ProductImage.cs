namespace LaLuna.Domain.Entities;

public class ProductImage : BaseEntity
{
    public string Url { get; private set; } = string.Empty;
    public string? AltText { get; private set; }
    public int SortOrder { get; private set; }
    public int ProductId { get; private set; }

    public Product? Product { get; private set; }

    private ProductImage() { }

    public static ProductImage Create(string url, int productId, string? altText = null, int sortOrder = 0) =>
        new()
        {
            Url = url,
            ProductId = productId,
            AltText = altText,
            SortOrder = sortOrder
        };
}
