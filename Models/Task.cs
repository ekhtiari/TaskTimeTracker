using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public TaskStatus Status { get; set; } = TaskStatus.Backlog;
        
        public TimeSpan TotalTime { get; set; } = TimeSpan.Zero;
        
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