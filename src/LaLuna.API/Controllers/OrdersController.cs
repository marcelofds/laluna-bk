using LaLuna.Application.Orders.Commands;
using LaLuna.Application.Orders.Queries;
using LaLuna.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LaLuna.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
[Produces("application/json")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var keycloakId = GetKeycloakId();
        var result = await mediator.Send(new GetMyOrdersQuery(keycloakId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var keycloakId = GetKeycloakId();
        var result = await mediator.Send(new GetOrderByIdQuery(id, keycloakId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var keycloakId = GetKeycloakId();
        var email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email")
            ?? string.Empty;

        var command = new CreateOrderCommand(
            keycloakId,
            email,
            request.PaymentMethod,
            request.Items.Select(i => new OrderItemRequest(i.ProductId, i.Quantity)).ToList(),
            request.ShippingStreet,
            request.ShippingNumber,
            request.ShippingNeighborhood,
            request.ShippingCity,
            request.ShippingState,
            request.ShippingZipCode,
            request.ShippingComplement,
            request.CouponCode
        );

        var orderId = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = orderId }, new { id = orderId });
    }

    private string GetKeycloakId() =>
        User.FindFirstValue("sub")
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();
}

public record CreateOrderRequest(
    PaymentMethod PaymentMethod,
    List<OrderItemInput> Items,
    string ShippingStreet,
    string ShippingNumber,
    string ShippingNeighborhood,
    string ShippingCity,
    string ShippingState,
    string ShippingZipCode,
    string? ShippingComplement = null,
    string? CouponCode = null
);

public record OrderItemInput(int ProductId, int Quantity);
