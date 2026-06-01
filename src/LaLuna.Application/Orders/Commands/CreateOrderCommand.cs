using LaLuna.Application.Common.Interfaces;
using LaLuna.Domain.Entities;
using LaLuna.Domain.Enums;
using LaLuna.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaLuna.Application.Orders.Commands;

public record CreateOrderCommand(
    string CustomerKeycloakId,
    string CustomerEmail,
    PaymentMethod PaymentMethod,
    List<OrderItemRequest> Items,
    string ShippingStreet,
    string ShippingNumber,
    string ShippingNeighborhood,
    string ShippingCity,
    string ShippingState,
    string ShippingZipCode,
    string? ShippingComplement = null,
    string? CouponCode = null
) : IRequest<int>;

public record OrderItemRequest(int ProductId, int Quantity);

public class CreateOrderCommandHandler(IAppDbContext db)
    : IRequestHandler<CreateOrderCommand, int>
{
    private const decimal FreeShippingThreshold = 199.90m;
    private const decimal ShippingCost = 15.00m;

    public async Task<int> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .Include(p => p.Images)
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
            throw new NotFoundException("One or more products", "not found or inactive");

        // Validate stock
        foreach (var itemRequest in request.Items)
        {
            var product = products.First(p => p.Id == itemRequest.ProductId);
            if (product.Stock < itemRequest.Quantity)
                throw new InsufficientStockException(product.Name, itemRequest.Quantity, product.Stock);
        }

        var subtotal = request.Items.Sum(i =>
        {
            var product = products.First(p => p.Id == i.ProductId);
            return product.Price * i.Quantity;
        });

        var shipping = subtotal >= FreeShippingThreshold ? 0 : ShippingCost;

        var order = Order.Create(
            request.CustomerKeycloakId,
            request.CustomerEmail,
            request.PaymentMethod,
            subtotal,
            shipping,
            request.ShippingStreet,
            request.ShippingNumber,
            request.ShippingNeighborhood,
            request.ShippingCity,
            request.ShippingState,
            request.ShippingZipCode,
            request.ShippingComplement,
            couponCode: request.CouponCode
        );

        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var itemRequest in request.Items)
        {
            var product = products.First(p => p.Id == itemRequest.ProductId);
            var item = OrderItem.Create(
                order.Id,
                product.Id,
                product.Name,
                product.Slug,
                product.Price,
                itemRequest.Quantity,
                product.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url
            );
            db.OrderItems.Add(item);
            product.UpdateStock(product.Stock - itemRequest.Quantity);
        }

        await db.SaveChangesAsync(cancellationToken);
        return order.Id;
    }
}
