using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpiFraudApi.Data;
using UpiFraudApi.DTOs;
using UpiFraudApi.Entities;
using UpiFraudApi.Services;

namespace UpiFraudApi.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly MlService _ml;
    private readonly EmailService _email;

    public TransactionsController(AppDbContext db, MlService ml, EmailService email)
    {
        _db = db;
        _ml = ml;
        _email = email;
    }

    [HttpPost("pay")]
    public async Task<ActionResult<PayResponse>> Pay(PayRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        var now = DateTime.UtcNow;
        var user = await _db.Users.Include(u => u.BankAccount).FirstAsync(u => u.Id == userId);
        if (user.BankAccount == null)
        {
            return BadRequest("No bank account linked. Please link account in Profile first.");
        }

        if (user.BankAccount.Balance < request.Amount)
        {
            return BadRequest("Insufficient balance");
        }

        var hour = now.Hour;
        var noteLength = request.Note?.Length ?? 0;

        var hasDevice = await _db.Transactions.AnyAsync(t => t.UserId == userId && t.DeviceId == request.DeviceId);
        var hasLocation = await _db.Transactions.AnyAsync(t => t.UserId == userId && t.City == request.City);
        var velocityCount = await _db.Transactions.CountAsync(t => t.UserId == userId && t.CreatedAt >= now.AddMinutes(-10));

        var deviceRisk = hasDevice ? 0.0 : 1.0;
        var locationRisk = hasLocation ? 0.0 : 1.0;
        var velocityRisk = Math.Min(1.0, velocityCount / 5.0);

        var features = new Dictionary<string, double>
        {
            ["amount"] = (double)request.Amount,
            ["hour"] = hour,
            ["device_risk"] = deviceRisk,
            ["location_risk"] = locationRisk,
            ["velocity_risk"] = velocityRisk,
            ["note_length"] = noteLength
        };

        var prediction = await _ml.PredictAsync(features);

        var reasons = new List<string>();
        if (deviceRisk > 0) reasons.Add("New device detected");
        if (locationRisk > 0) reasons.Add("New location detected");
        if (velocityRisk > 0.6) reasons.Add("High transaction velocity");
        if (request.Amount > 5000) reasons.Add("High amount transaction");
        if (reasons.Count == 0) reasons.Add("Behavior within normal range");

        var tx = new Transaction
        {
            UserId = userId,
            UpiId = request.UpiId,
            Amount = request.Amount,
            Note = request.Note,
            DeviceId = request.DeviceId,
            City = request.City,
            FraudProbability = prediction.FraudProbability,
            IsFraud = prediction.IsFraud,
            Reasons = string.Join("|", reasons),
            CreatedAt = now
        };

        user.BankAccount.Balance -= request.Amount;
        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync();

        if (prediction.IsFraud)
        {
            _email.SendFraudAlert(email, "UPI Fraud Alert", $"Transaction {tx.Id} flagged as fraud. Probability: {prediction.FraudProbability:P2}.");
        }

        return Ok(new PayResponse(tx.Id, prediction.IsFraud, prediction.FraudProbability, reasons));
    }

    [HttpGet("history")]
    public async Task<IActionResult> History()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var history = await _db.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.UpiId,
                t.Amount,
                t.IsFraud,
                t.FraudProbability,
                t.CreatedAt
            })
            .ToListAsync();

        return Ok(history);
    }
}
