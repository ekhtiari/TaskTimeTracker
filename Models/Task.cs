using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTracker.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public required string Title { get; set; }
        
        public required string Description { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public TaskStatus Status { get; set; } = TaskStatus.Backlog;
        
        public TimeSpan TotalTime { get; set; } = TimeSpan.Zero;
        
        [NotMapped] // This property won't be stored in the database
        public TimeSpan DisplayTotalTime { get; set; } = TimeSpan.Zero;
        
        public DateTime? LastStartTime { get; set; }
        
        public bool IsRunning { get; set; }
        
        public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
    }
    
    public enum TaskStatus
    {
        Backlog,
        InProgress,
        Completed
    }
} 