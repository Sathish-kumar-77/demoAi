using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpiFraudApi.Data;

namespace UpiFraudApi.Controllers;

[ApiController]
[Route("api/payees")]
[Authorize]
public class PayeesController : ControllerBase
{
    private readonly AppDbContext _db;

    public PayeesController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Resolve payee by phone (10 digits) or UPI ID.
    /// </summary>
    [HttpGet("resolve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Resolve([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return NotFound(new { message = "Payee not found" });
        }

        var trimmed = query.Trim();
        var isPhone = trimmed.Length == 10 && trimmed.All(char.IsDigit);
        var isUpi = trimmed.Contains('@');

        var payee = isPhone
            ? await _db.Payees.FirstOrDefaultAsync(p => p.Phone == trimmed)
            : isUpi
                ? await _db.Payees.FirstOrDefaultAsync(p => p.UpiId.ToLower() == trimmed.ToLower())
                : null;

        if (payee == null)
        {
            return NotFound(new { message = "Payee not found" });
        }

        return Ok(new
        {
            name = payee.Name,
            phone = payee.Phone,
            upiId = payee.UpiId,
            verified = payee.Verified
        });
    }
}
