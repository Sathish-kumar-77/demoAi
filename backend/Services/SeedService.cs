using Microsoft.EntityFrameworkCore;
using UpiFraudApi.Data;
using UpiFraudApi.Entities;

namespace UpiFraudApi.Services;

public static class SeedService
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (!await db.BankAccounts.AnyAsync())
        {
            var accounts = new List<BankAccount>
            {
                new() { BankName = "State Bank of India", AccountHolderName = "Arjun Kumar", PhoneNumber = "9876543210", AccountNumberMasked = "XXXXXX1023", Balance = 55000, UpiPin = "1111" },
                new() { BankName = "HDFC Bank", AccountHolderName = "Priya Sharma", PhoneNumber = "9123456780", AccountNumberMasked = "XXXXXX8891", Balance = 84500, UpiPin = "2222" },
                new() { BankName = "ICICI Bank", AccountHolderName = "Rahul Verma", PhoneNumber = "9988776655", AccountNumberMasked = "XXXXXX4500", Balance = 23400, UpiPin = "3333" },
                new() { BankName = "Axis Bank", AccountHolderName = "Sneha Reddy", PhoneNumber = "9090909090", AccountNumberMasked = "XXXXXX6782", Balance = 99210, UpiPin = "4444" }
            };
            db.BankAccounts.AddRange(accounts);
        }

        var seededPayees = new List<Payee>
        {
            new() { Name = "Akhil Menon", Phone = "9000011111", UpiId = "akhil@okaxis", Verified = true },
            new() { Name = "Divya Nair", Phone = "9000011112", UpiId = "divya@okhdfc", Verified = true },
            new() { Name = "Ramesh Iyer", Phone = "9000011113", UpiId = "ramesh@oksbi", Verified = false },
            new() { Name = "Kavya Rao", Phone = "9000011114", UpiId = "kavya@okicici", Verified = true },
            new() { Name = "Nitin Shah", Phone = "9000011115", UpiId = "nitin@paytm", Verified = false }
        };

        foreach (var payee in seededPayees)
        {
            var exists = await db.Payees.AnyAsync(p => p.Phone == payee.Phone || p.UpiId == payee.UpiId);
            if (!exists)
            {
                db.Payees.Add(payee);
            }
        }

        await db.SaveChangesAsync();
    }
}
