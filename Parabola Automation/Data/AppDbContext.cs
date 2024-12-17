using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using Parabola_Automation.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Parabola_Automation.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Flow> Flows { get; set; } = null!;
        public DbSet<UserFlow> UserFlows { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure composite primary key for UserFlow
            modelBuilder.Entity<UserFlow>()
                .HasKey(uf => new { uf.UserId, uf.FlowId });

            // Optional: set up relationships
            modelBuilder.Entity<UserFlow>()
                .HasOne(uf => uf.User)
                .WithMany(u => u.UserFlows)
                .HasForeignKey(uf => uf.UserId);

            modelBuilder.Entity<UserFlow>()
                .HasOne(uf => uf.Flow)
                .WithMany(f => f.UserFlows)
                .HasForeignKey(uf => uf.FlowId);
        }
    }
}
