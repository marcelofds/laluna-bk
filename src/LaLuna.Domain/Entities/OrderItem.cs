namespace LaLuna.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string ProductSlug { get; private set; } = string.Empty;
    public string? ProductImageUrl { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal Subtotal => UnitPrice * Quantity;

    public Order? Order { get; private set; }
    public Product? Product { get; private set; }

    private OrderItem() { }

    public static OrderItem Create(
        int orderId,
        int productId,
        string productName,
        string productSlug,
        decimal unitPrice,
        int quantity,
        string? productImageUrl = null) =>
        new()
        {
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            ProductSlug = productSlug,
            UnitPrice = unitPrice,
            Quantity = quantity,
            ProductImageUrl = productImageUrl
        };
}
