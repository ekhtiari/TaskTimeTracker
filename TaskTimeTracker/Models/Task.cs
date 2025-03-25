using System;

namespace TaskTimeTracker.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public TimeSpan TotalTime { get; set; }
        public DateTime? LastStartTime { get; set; }
    }

    public enum TaskStatus
    {
        Backlog,
        Running,
        Paused,
        Completed
    }
} 