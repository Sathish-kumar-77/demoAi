using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpiFraudApi.Data;
using UpiFraudApi.DTOs;

namespace UpiFraudApi.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AccountsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("directory")]
    public async Task<IActionResult> Directory()
    {
        var items = await _db.BankAccounts.Select(a => new
        {
            a.BankName,
            a.AccountHolderName,
            a.PhoneNumber,
            a.AccountNumberMasked
        }).ToListAsync();

        return Ok(items);
    }

    [HttpPost("link")]
    public async Task<IActionResult> Link(LinkAccountRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        var account = await _db.BankAccounts.FirstOrDefaultAsync(a => a.PhoneNumber == request.PhoneNumber);
        if (account == null)
        {
            return NotFound("No bank account found for this phone number");
        }

        if (account.UpiPin != request.UpiPin)
        {
            return BadRequest("Invalid UPI PIN");
        }

        var user = await _db.Users.FirstAsync(u => u.Id == userId);
        user.BankAccountId = account.Id;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Bank account linked", account.BankName, account.AccountHolderName, account.AccountNumberMasked });
    }

    [HttpPost("balance")]
    public async Task<IActionResult> Balance([FromBody] LinkAccountRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var user = await _db.Users.Include(u => u.BankAccount).FirstAsync(u => u.Id == userId);

        if (user.BankAccount == null)
        {
            return BadRequest("No bank account linked. Link account first from Profile.");
        }

        if (user.BankAccount.UpiPin != request.UpiPin)
        {
            return Unauthorized("Invalid UPI PIN");
        }

        return Ok(new
        {
            user.BankAccount.BankName,
            user.BankAccount.AccountHolderName,
            user.BankAccount.AccountNumberMasked,
            user.BankAccount.Balance
        });
    }
}
