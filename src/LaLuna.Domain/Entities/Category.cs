namespace LaLuna.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    public int? ParentId { get; private set; }

    public Category? Parent { get; private set; }
    public ICollection<Category> Children { get; private set; } = [];
    public ICollection<Product> Products { get; private set; } = [];

    private Category() { }

    public static Category Create(string name, string slug, string? imageUrl = null, int? parentId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Category
        {
            Name = name,
            Slug = slug.ToLowerInvariant(),
            ImageUrl = imageUrl,
            ParentId = parentId
        };
    }

    public void Update(string name, string slug, string? imageUrl = null)
    {
        Name = name;
        Slug = slug.ToLowerInvariant();
        ImageUrl = imageUrl;
        SetUpdatedAt();
    }
}
