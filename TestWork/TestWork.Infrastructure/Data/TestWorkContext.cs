using Microsoft.EntityFrameworkCore;
using TestWork.Core.Models;
using TestWork.Infrastructure.Configurations;

namespace TestWork.Infrastructure.Data;

public class TestWorkContext : DbContext
{
    public TestWorkContext(DbContextOptions<TestWorkContext> options)
        : base(options)
    {
    }

    public DbSet<SubscriptionModel> Subscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
