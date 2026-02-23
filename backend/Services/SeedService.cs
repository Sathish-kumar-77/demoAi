using Microsoft.EntityFrameworkCore;
using UpiFraudApi.Data;
using UpiFraudApi.Entities;

namespace UpiFraudApi.Services;

public static class SeedService
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.BankAccounts.AnyAsync())
        {
            return;
        }

        var accounts = new List<BankAccount>
        {
            new() { BankName = "State Bank of India", AccountHolderName = "Arjun Kumar", PhoneNumber = "9876543210", AccountNumberMasked = "XXXXXX1023", Balance = 55000, UpiPin = "1111" },
            new() { BankName = "HDFC Bank", AccountHolderName = "Priya Sharma", PhoneNumber = "9123456780", AccountNumberMasked = "XXXXXX8891", Balance = 84500, UpiPin = "2222" },
            new() { BankName = "ICICI Bank", AccountHolderName = "Rahul Verma", PhoneNumber = "9988776655", AccountNumberMasked = "XXXXXX4500", Balance = 23400, UpiPin = "3333" },
            new() { BankName = "Axis Bank", AccountHolderName = "Sneha Reddy", PhoneNumber = "9090909090", AccountNumberMasked = "XXXXXX6782", Balance = 99210, UpiPin = "4444" }
        };

        db.BankAccounts.AddRange(accounts);
        await db.SaveChangesAsync();
    }
}
