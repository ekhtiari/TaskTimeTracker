using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTracker.Models
{
    public class TimeLog
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int TaskId { get; set; }
        
        [ForeignKey("TaskId")]
        public Task Task { get; set; }
        
        public DateTime StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        public TimeSpan Duration => EndTime.HasValue 
            ? EndTime.Value - StartTime 
            : DateTime.Now - StartTime;
        
        public string Notes { get; set; }
    }
} 