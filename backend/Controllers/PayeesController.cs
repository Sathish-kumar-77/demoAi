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

    [HttpGet("samples")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Samples()
    {
        var payees = await _db.Payees
            .OrderByDescending(p => p.Verified)
            .ThenBy(p => p.Name)
            .Take(12)
            .Select(p => new { name = p.Name, phone = p.Phone, upiId = p.UpiId, verified = p.Verified })
            .ToListAsync();

        return Ok(payees);
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

        var normalized = query.Trim().ToLowerInvariant();
        var isPhone = normalized.Length == 10 && normalized.All(char.IsDigit);
        var isUpi = normalized.Contains('@');

        var payee = isPhone
            ? await _db.Payees.FirstOrDefaultAsync(p => p.Phone == normalized)
            : isUpi
                ? await _db.Payees.FirstOrDefaultAsync(p => p.UpiId.ToLower() == normalized)
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
