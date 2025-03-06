using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeTracker.Models;

namespace TimeTracker.Data
{
    public class TaskService
    {
        private readonly TimeTrackerDbContext _context;
        
        public TaskService()
        {
            _context = new TimeTrackerDbContext();
            _context.Database.EnsureCreated();
        }
        
        public async Task<List<Models.Task>> GetAllTasksAsync()
        {
            return await _context.Tasks
                .Include(t => t.TimeLogs)
                .ToListAsync();
        }
        
        public async Task<Models.Task?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.TimeLogs)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        
        public async Task<Models.Task> AddTaskAsync(Models.Task task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }
        
        public async Task<Models.Task> UpdateTaskAsync(Models.Task task)
        {
            _context.Entry(task).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return task;
        }
        
        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return false;
                
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<Models.Task?> StartTaskTimerAsync(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.TimeLogs)
                .FirstOrDefaultAsync(t => t.Id == taskId);
                
            if (task == null)
                return null;
                
            // Create a new time log entry
            var timeLog = new TimeLog
            {
                TaskId = taskId,
                Task = task,
                StartTime = DateTime.Now,
                Notes = string.Empty // Initialize Notes property to avoid null reference
            };
            
            task.TimeLogs.Add(timeLog);
            task.Status = Models.TaskStatus.InProgress;
            task.IsRunning = true;
            task.LastStartTime = timeLog.StartTime;
            
            await _context.SaveChangesAsync();
            return task;
        }
        
        public async Task<Models.Task?> StopTaskTimerAsync(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.TimeLogs)
                .FirstOrDefaultAsync(t => t.Id == taskId);
                
            if (task == null || !task.IsRunning)
                return task;
                
            // Find the active time log and stop it
            var activeTimeLog = task.TimeLogs.FirstOrDefault(tl => tl.EndTime == null);
            if (activeTimeLog != null)
            {
                activeTimeLog.EndTime = DateTime.Now;
                task.TotalTime += activeTimeLog.Duration;
            }
            
            task.IsRunning = false;
            task.LastStartTime = null;
            
            await _context.SaveChangesAsync();
            return task;
        }
        
        public async System.Threading.Tasks.Task CloseAsync()
        {
            await _context.DisposeAsync();
        }
    }
} 