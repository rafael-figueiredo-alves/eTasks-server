using eTasks_server.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace eTasks_server.Core.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        // Define DbSet properties for your entities here
        // Example:
        // public DbSet<User> Users { get; set; }

        public DbSet<eTasksVersion> DbVersion { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<eTasksVersion>().ToTable("version").HasNoKey();

            base.OnModelCreating(modelBuilder);
            // Configure your entity relationships and constraints here
            // Example:
            // modelBuilder.Entity<User>()
            //     .HasMany(u => u.Tasks)
            //     .WithOne(t => t.User)
            //     .HasForeignKey(t => t.UserId);
        }
    }
}
