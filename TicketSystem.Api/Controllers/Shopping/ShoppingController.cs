using Microsoft.AspNetCore.Mvc;
using TicketSystem.Api.Contracts.Shopping;
using TicketSystem.Services.Shopping;

namespace TicketSystem.Api.Controllers.Shopping;

[ApiController]
[Route("api/cart")]
public sealed class ShoppingController : ControllerBase
{
    private readonly ShoppingService _shoppingService;

    public ShoppingController(ShoppingService shoppingService)
    {
        _shoppingService = shoppingService;
    }

    [HttpPost("items")]
    public ActionResult<AddToCartResult> AddToNewCart(AddToCartRequest request)
    {
        return Ok(_shoppingService.AddToCart(
            request.ProductId,
            request.Quantity,
            request.Year,
            request.Month,
            request.Day));
    }

    [HttpPost("{cartId:guid}/items")]
    public ActionResult<AddToCartResult> AddToExistingCart(Guid cartId, AddToCartRequest request)
    {
        return Ok(_shoppingService.AddToCart(
            cartId,
            request.ProductId,
            request.Quantity,
            request.Year,
            request.Month,
            request.Day));
    }

    [HttpGet("{cartId:guid}/items")]
    public ActionResult<IReadOnlyList<CartItemDto>> GetCartItems(Guid cartId)
    {
        return Ok(_shoppingService.GetCartItems(cartId));
    }

    [HttpPost("{cartId:guid}/items/{cartItemId:guid}/holder")]
    public ActionResult<AssignTicketHolderResult> AssignTicketHolder(
        Guid cartId,
        Guid cartItemId,
        AssignTicketHolderRequest request)
    {
        return Ok(_shoppingService.AssignTicketHolder(
            cartId,
            cartItemId,
            request.FirstName,
            request.LastName,
            request.Email));
    }

    [HttpPost("{cartId:guid}/paid")]
    public async Task<ActionResult<MarkPaidResult>> MarkPaid(
        Guid cartId,
        CancellationToken cancellationToken)
    {
        return Ok(await _shoppingService.MarkPaidAsync(cartId, cancellationToken));
    }

    [HttpDelete("{cartId:guid}/items/{productId}/{year:int}/{month:int}/{day:int}")]
    public ActionResult<RemoveFromCartResult> RemoveFromCart(
        Guid cartId,
        string productId,
        int year,
        int month,
        int day)
    {
        return Ok(_shoppingService.RemoveFromCart(cartId, productId, year, month, day));
    }
}
