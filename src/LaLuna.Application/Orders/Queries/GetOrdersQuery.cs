using LaLuna.Application.Common.Interfaces;
using LaLuna.Domain.Enums;
using LaLuna.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaLuna.Application.Orders.Queries;

public record OrderDto(
    int Id,
    string OrderNumber,
    OrderStatus Status,
    PaymentMethod PaymentMethod,
    decimal Subtotal,
    decimal ShippingCost,
    decimal Discount,
    decimal Total,
    string? CouponCode,
    string? TrackingCode,
    List<OrderItemDto> Items,
    OrderAddressDto ShippingAddress,
    DateTime CreatedAt
);

public record OrderItemDto(
    int ProductId,
    string ProductName,
    string ProductSlug,
    string? ProductImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal
);

public record OrderAddressDto(
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode
);

public record GetMyOrdersQuery(string CustomerKeycloakId) : IRequest<List<OrderDto>>;

public class GetMyOrdersQueryHandler(IAppDbContext db)
    : IRequestHandler<GetMyOrdersQuery, List<OrderDto>>
{
    public async Task<List<OrderDto>> Handle(
        GetMyOrdersQuery request,
        CancellationToken cancellationToken)
    {
        return await db.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerKeycloakId == request.CustomerKeycloakId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => OrderMapper.ToDto(o))
            .ToListAsync(cancellationToken);
    }
}

public static class OrderMapper
{
    public static OrderDto ToDto(Domain.Entities.Order o) => new(
        o.Id,
        o.OrderNumber,
        o.Status,
        o.PaymentMethod,
        o.Subtotal,
        o.ShippingCost,
        o.Discount,
        o.Total,
        o.CouponCode,
        o.TrackingCode,
        o.Items.Select(i => new OrderItemDto(
            i.ProductId,
            i.ProductName,
            i.ProductSlug,
            i.ProductImageUrl,
            i.UnitPrice,
            i.Quantity,
            i.Subtotal
        )).ToList(),
        new OrderAddressDto(
            o.ShippingStreet,
            o.ShippingNumber,
            o.ShippingComplement,
            o.ShippingNeighborhood,
            o.ShippingCity,
            o.ShippingState,
            o.ShippingZipCode
        ),
        o.CreatedAt
    );
}

public record GetOrderByIdQuery(int Id, string CustomerKeycloakId) : IRequest<OrderDto>;

public class GetOrderByIdQueryHandler(IAppDbContext db)
    : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<OrderDto> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(
                o => o.Id == request.Id && o.CustomerKeycloakId == request.CustomerKeycloakId,
                cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Order), request.Id);

        return OrderMapper.ToDto(order);
    }
}
