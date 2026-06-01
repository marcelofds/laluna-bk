using LaLuna.Domain.Enums;

namespace LaLuna.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; private set; } = string.Empty;
    public string CustomerKeycloakId { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public PaymentMethod PaymentMethod { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal ShippingCost { get; private set; }
    public decimal Discount { get; private set; }
    public decimal Total => Subtotal + ShippingCost - Discount;
    public string? CouponCode { get; private set; }
    public string? TrackingCode { get; private set; }

    // Shipping address (denormalized snapshot)
    public string ShippingStreet { get; private set; } = string.Empty;
    public string ShippingNumber { get; private set; } = string.Empty;
    public string? ShippingComplement { get; private set; }
    public string ShippingNeighborhood { get; private set; } = string.Empty;
    public string ShippingCity { get; private set; } = string.Empty;
    public string ShippingState { get; private set; } = string.Empty;
    public string ShippingZipCode { get; private set; } = string.Empty;

    public ICollection<OrderItem> Items { get; private set; } = [];

    private Order() { }

    public static Order Create(
        string customerKeycloakId,
        string customerEmail,
        PaymentMethod paymentMethod,
        decimal subtotal,
        decimal shippingCost,
        string shippingStreet,
        string shippingNumber,
        string shippingNeighborhood,
        string shippingCity,
        string shippingState,
        string shippingZipCode,
        string? shippingComplement = null,
        decimal discount = 0,
        string? couponCode = null)
    {
        return new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerKeycloakId = customerKeycloakId,
            CustomerEmail = customerEmail,
            PaymentMethod = paymentMethod,
            Subtotal = subtotal,
            ShippingCost = shippingCost,
            Discount = discount,
            CouponCode = couponCode,
            ShippingStreet = shippingStreet,
            ShippingNumber = shippingNumber,
            ShippingComplement = shippingComplement,
            ShippingNeighborhood = shippingNeighborhood,
            ShippingCity = shippingCity,
            ShippingState = shippingState,
            ShippingZipCode = shippingZipCode
        };
    }

    public void Confirm() => UpdateStatus(OrderStatus.Confirmed);
    public void Process() => UpdateStatus(OrderStatus.Processing);
    public void Ship(string trackingCode)
    {
        TrackingCode = trackingCode;
        UpdateStatus(OrderStatus.Shipped);
    }
    public void Deliver() => UpdateStatus(OrderStatus.Delivered);
    public void Cancel() => UpdateStatus(OrderStatus.Cancelled);

    private void UpdateStatus(OrderStatus status)
    {
        Status = status;
        SetUpdatedAt();
    }

    private static string GenerateOrderNumber() =>
        $"LLN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
}
