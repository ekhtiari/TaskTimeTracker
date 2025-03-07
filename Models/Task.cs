using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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
        
        /// <summary>
        /// Gets the time spent on this task for a specific day
        /// </summary>
        /// <param name="date">The day to calculate time for</param>
        /// <returns>TimeSpan representing the total time for that day</returns>
        public TimeSpan GetTimeForDay(DateOnly date)
        {
            var timeForDay = TimeSpan.Zero;
            
            foreach (var log in TimeLogs.Where(l => DateOnly.FromDateTime(l.StartTime) == date))
            {
                timeForDay += log.Duration;
            }
            
            // If the task is currently running and was started today, add the current session
            if (IsRunning && LastStartTime.HasValue && DateOnly.FromDateTime(LastStartTime.Value) == date)
            {
                timeForDay += DateTime.Now - LastStartTime.Value;
            }
            
            return timeForDay;
        }
        
        /// <summary>
        /// Gets a dictionary of time spent per day
        /// </summary>
        /// <returns>Dictionary with date as key and time spent as value</returns>
        public Dictionary<DateOnly, TimeSpan> GetTimeByDay()
        {
            var timeByDay = new Dictionary<DateOnly, TimeSpan>();
            
            // Group logs by date and sum up durations
            var logsByDate = TimeLogs.GroupBy(l => DateOnly.FromDateTime(l.StartTime));
            
            foreach (var group in logsByDate)
            {
                timeByDay[group.Key] = TimeSpan.FromTicks(group.Sum(l => l.Duration.Ticks));
            }
            
            // Add today's running time if applicable
            if (IsRunning && LastStartTime.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var currentSessionTime = DateTime.Now - LastStartTime.Value;
                
                if (timeByDay.ContainsKey(today))
                {
                    timeByDay[today] += currentSessionTime;
                }
                else
                {
                    timeByDay[today] = currentSessionTime;
                }
            }
            
            return timeByDay;
        }
    }
    
    public enum TaskStatus
    {
        Backlog,
        InProgress,
        Completed
    }
} 