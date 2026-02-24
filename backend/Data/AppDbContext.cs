using Microsoft.EntityFrameworkCore;
using UpiFraudApi.Entities;

namespace UpiFraudApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<UserLinkedAccount> UserLinkedAccounts => Set<UserLinkedAccount>();
    public DbSet<OtpRequest> OtpRequests => Set<OtpRequest>();
    public DbSet<Payee> Payees => Set<Payee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserLinkedAccount>()
            .HasOne(x => x.User)
            .WithMany(u => u.LinkedAccounts)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserLinkedAccount>()
            .HasOne(x => x.BankAccount)
            .WithMany(b => b.LinkedUsers)
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserLinkedAccount>()
            .HasIndex(x => x.UpiId)
            .IsUnique();

        modelBuilder.Entity<Payee>()
            .HasIndex(x => x.Phone)
            .IsUnique();

        modelBuilder.Entity<Payee>()
            .HasIndex(x => x.UpiId)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
