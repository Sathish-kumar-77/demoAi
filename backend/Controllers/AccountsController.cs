using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpiFraudApi.Data;
using UpiFraudApi.DTOs;
using UpiFraudApi.Entities;

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

    [HttpGet("bank-directory")]
    public async Task<IActionResult> BankDirectory()
    {
        var items = await _db.BankAccounts.Select(a => new
        {
            a.Id,
            a.BankName,
            a.AccountHolderName,
            a.PhoneNumber,
            a.AccountNumberMasked
        }).ToListAsync();

        return Ok(items);
    }

    [HttpGet("upi-directory")]
    public async Task<IActionResult> UpiDirectory()
    {
        var upis = await _db.UserLinkedAccounts
            .Include(x => x.BankAccount)
            .Select(x => new
            {
                x.UpiId,
                HolderName = x.BankAccount!.AccountHolderName,
                BankName = x.BankAccount.BankName
            })
            .Distinct()
            .ToListAsync();

        return Ok(upis);
    }

    [HttpPost("request-otp")]
    public async Task<IActionResult> RequestOtp(RequestOtpRequest request)
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

        var otp = Random.Shared.Next(100000, 999999).ToString();
        _db.OtpRequests.Add(new OtpRequest
        {
            UserId = userId,
            PhoneNumber = request.PhoneNumber,
            Code = otp,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });
        await _db.SaveChangesAsync();

        // Demo SMS simulation (normally send via SMS provider)
        return Ok(new { message = "OTP sent to phone number", demoOtp = otp });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        var otp = await _db.OtpRequests
            .Where(x => x.UserId == userId && x.PhoneNumber == request.PhoneNumber && !x.IsUsed)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp == null || otp.ExpiresAt < DateTime.UtcNow || otp.Code != request.OtpCode)
        {
            return BadRequest("Invalid or expired OTP");
        }

        var account = await _db.BankAccounts.FirstOrDefaultAsync(a => a.PhoneNumber == request.PhoneNumber);
        if (account == null)
        {
            return NotFound("Bank account not found");
        }

        var existingLink = await _db.UserLinkedAccounts
            .FirstOrDefaultAsync(x => x.UserId == userId && x.BankAccountId == account.Id);

        otp.IsUsed = true;

        if (existingLink == null)
        {
            var isFirst = !await _db.UserLinkedAccounts.AnyAsync(x => x.UserId == userId);
            var prefix = account.AccountHolderName.Replace(" ", string.Empty).ToLowerInvariant();
            var upiId = $"{prefix}{userId}{account.Id}@upi";

            _db.UserLinkedAccounts.Add(new UserLinkedAccount
            {
                UserId = userId,
                BankAccountId = account.Id,
                UpiId = upiId,
                IsPrimary = isFirst
            });

            await _db.SaveChangesAsync();
            return Ok(new { message = "Account linked successfully", upiId, account.BankName, account.AccountNumberMasked });
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Account already linked", upiId = existingLink.UpiId, account.BankName, account.AccountNumberMasked });
    }

    [HttpGet("linked")]
    public async Task<IActionResult> LinkedAccounts()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var linked = await _db.UserLinkedAccounts
            .Where(x => x.UserId == userId)
            .Include(x => x.BankAccount)
            .Select(x => new
            {
                x.Id,
                x.UpiId,
                x.IsPrimary,
                x.BankAccount!.BankName,
                x.BankAccount.AccountHolderName,
                x.BankAccount.AccountNumberMasked
            })
            .ToListAsync();

        return Ok(linked);
    }

    [HttpDelete("linked/{linkedId:int}")]
    public async Task<IActionResult> RemoveLinkedAccount(int linkedId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var link = await _db.UserLinkedAccounts.FirstOrDefaultAsync(x => x.Id == linkedId && x.UserId == userId);
        if (link == null)
        {
            return NotFound("Linked account not found");
        }

        var wasPrimary = link.IsPrimary;
        _db.UserLinkedAccounts.Remove(link);
        await _db.SaveChangesAsync();

        if (wasPrimary)
        {
            var first = await _db.UserLinkedAccounts.FirstOrDefaultAsync(x => x.UserId == userId);
            if (first != null)
            {
                first.IsPrimary = true;
                await _db.SaveChangesAsync();
            }
        }

        return Ok(new { message = "Linked account removed" });
    }

    [HttpPost("balance")]
    public async Task<IActionResult> Balance([FromBody] BalanceRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var linked = await _db.UserLinkedAccounts
            .Include(x => x.BankAccount)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.IsPrimary);

        if (linked?.BankAccount == null)
        {
            return BadRequest("No linked account found. Link account from Profile.");
        }

        if (linked.BankAccount.UpiPin != request.UpiPin)
        {
            return Unauthorized("Invalid UPI PIN");
        }

        return Ok(new
        {
            linked.UpiId,
            linked.BankAccount.BankName,
            linked.BankAccount.AccountHolderName,
            linked.BankAccount.AccountNumberMasked,
            linked.BankAccount.Balance
        });
    }
}
