using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using TimeTracker.Models;

namespace TimeTracker.Data
{
    public class TimeTrackerDbContext : DbContext
    {
        public DbSet<Models.Task> Tasks { get; set; }
        public DbSet<TimeLog> TimeLogs { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TimeTracker");
            
            // Ensure directory exists
            if (!Directory.Exists(dbPath))
                Directory.CreateDirectory(dbPath);
                
            string dbFile = Path.Combine(dbPath, "timetracker.db");
            optionsBuilder.UseSqlite($"Data Source={dbFile}");
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure cascade delete for TimeLogs when a Task is deleted
            modelBuilder.Entity<Models.Task>()
                .HasMany(t => t.TimeLogs)
                .WithOne(tl => tl.Task)
                .HasForeignKey(tl => tl.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 