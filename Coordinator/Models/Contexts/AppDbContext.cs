using Microsoft.EntityFrameworkCore;

namespace Coordinator.Models.Contexts
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }

        public DbSet<Node> Nodes { get; set; }
        public DbSet<NodeState> NodesState { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Node>().HasData(
                new Node("Order.API") { Id = Guid.NewGuid() },
                new Node("Stock.API") { Id = Guid.NewGuid() },
                new Node("Payment.API") { Id = Guid.NewGuid() }
                );
        }
    }
}
