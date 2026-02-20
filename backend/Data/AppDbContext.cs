using Microsoft.EntityFrameworkCore;
using UpiFraudApi.Entities;

namespace UpiFraudApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
}
