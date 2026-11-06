using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScreenAdBooking.Api.Auth;
using ScreenAdBooking.Application.Contracts.Credits;
using ScreenAdBooking.Application.Credits;

namespace ScreenAdBooking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/credits")]
public sealed class CreditsController(CreditService creditService) : ControllerBase
{
    [HttpGet("packs")]
    public ActionResult<IReadOnlyList<CreditPackResponse>> Packs() => Ok(CreditService.Packs);

    [HttpGet]
    public async Task<ActionResult<WalletResponse>> Wallet(CancellationToken cancellationToken) =>
        User.GetUserId() is { } userId
            ? ToActionResult(await creditService.GetWalletAsync(userId, cancellationToken))
            : Unauthorized();

    [HttpPost("top-up")]
    public async Task<ActionResult<WalletResponse>> TopUp(TopUpRequest request, CancellationToken cancellationToken) =>
        User.GetUserId() is { } userId
            ? ToActionResult(await creditService.TopUpAsync(userId, request, cancellationToken))
            : Unauthorized();

    private ActionResult<WalletResponse> ToActionResult(WalletResult result) => result.Status switch
    {
        WalletStatus.Ok => Ok(result.Wallet),
        WalletStatus.UserNotFound => Unauthorized(),
        WalletStatus.NotAdvertiser => Problem(
            statusCode: StatusCodes.Status403Forbidden,
            title: "Only advertisers have a credits wallet."),
        _ => this.InvalidRequest(result.Errors),
    };
}
