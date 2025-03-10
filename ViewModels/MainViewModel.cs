using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TimeTracker.Data;
using TimeTracker.Models;

namespace TimeTracker.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly TaskService _taskService;
        private readonly DispatcherTimer _timer;
        
        private ObservableCollection<Models.Task> _tasks = new();
        private Models.Task? _selectedTask;
        private string _newTaskTitle = string.Empty;
        private string _newTaskDescription = string.Empty;
        private string _currentTime = string.Empty;
        private bool _isAddingNewTask;
        private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Today);
        private ObservableCollection<DailyTimeReport> _dailyTimeReports = new();
        
        public MainViewModel()
        {
            _taskService = new TaskService();
            
            // Create and configure timer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
            
            // Initialize commands
            AddTaskCommand = new RelayCommand(_ => AddTask());
            StartTaskCommand = new RelayCommand(_ => StartTask(), _ => CanStartTask());
            PauseTaskCommand = new RelayCommand(_ => PauseTask(), _ => CanPauseTask());
            CompleteTaskCommand = new RelayCommand(_ => CompleteTask(), _ => CanCompleteTask());
            DeleteTaskCommand = new RelayCommand(_ => DeleteTask(), _ => CanDeleteTask());
            ShowAddTaskCommand = new RelayCommand(_ => ShowAddTask());
            CancelAddTaskCommand = new RelayCommand(_ => CancelAddTask());
            ShowTodayCommand = new RelayCommand(_ => ShowToday());
            ShowYesterdayCommand = new RelayCommand(_ => ShowYesterday());
            
            // Load tasks
            LoadTasksAsync();
        }
        
        public ObservableCollection<Models.Task> Tasks
        {
            get => _tasks;
            private set => SetProperty(ref _tasks, value);
        }
        
        public Models.Task? SelectedTask
        {
            get => _selectedTask;
            set => SetProperty(ref _selectedTask, value);
        }
        
        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set => SetProperty(ref _newTaskTitle, value);
        }
        
        public string NewTaskDescription
        {
            get => _newTaskDescription;
            set => SetProperty(ref _newTaskDescription, value);
        }
        
        public string CurrentTime
        {
            get => _currentTime;
            private set => SetProperty(ref _currentTime, value);
        }
        
        public bool IsAddingNewTask
        {
            get => _isAddingNewTask;
            set => SetProperty(ref _isAddingNewTask, value);
        }
        
        public DateOnly SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    UpdateDailyTimeReports();
                }
            }
        }
        
        public ObservableCollection<DailyTimeReport> DailyTimeReports
        {
            get => _dailyTimeReports;
            private set => SetProperty(ref _dailyTimeReports, value);
        }
        
        public string SelectedDateFormatted => SelectedDate.ToString("yyyy-MM-dd");
        
        public TimeSpan TotalTimeForSelectedDate => CalculateTotalTimeForDate(SelectedDate);
        
        public ICommand AddTaskCommand { get; }
        public ICommand StartTaskCommand { get; }
        public ICommand PauseTaskCommand { get; }
        public ICommand CompleteTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ShowAddTaskCommand { get; }
        public ICommand CancelAddTaskCommand { get; }
        public ICommand ShowTodayCommand { get; }
        public ICommand ShowYesterdayCommand { get; }
        
        private async void LoadTasksAsync()
        {
            try
            {
                var tasks = await _taskService.GetAllTasksAsync();
                Tasks = new ObservableCollection<Models.Task>(tasks);
                
                // Initial update of daily time reports
                UpdateDailyTimeReports();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void Timer_Tick(object? sender, EventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            
            // Update all tasks
            if (Tasks != null)
            {
                foreach (var task in Tasks)
                {
                    // For running tasks, include current session time
                    if (task.IsRunning && task.LastStartTime.HasValue)
                    {
                        TimeSpan currentSessionTime = DateTime.Now - task.LastStartTime.Value;
                        task.DisplayTotalTime = task.TotalTime + currentSessionTime;
                    }
                    else
                    {
                        // For non-running tasks, just use the stored total time
                        task.DisplayTotalTime = task.TotalTime;
                    }
                }
                
                // Force refresh of task items
                OnPropertyChanged(nameof(Tasks));
                
                // Also refresh the selected task if there is one
                if (SelectedTask != null)
                {
                    OnPropertyChanged(nameof(SelectedTask));
                }
                
                // Update daily time reports if they're being displayed
                UpdateDailyTimeReports();
            }
        }
        
        private void ShowAddTask()
        {
            NewTaskTitle = string.Empty;
            NewTaskDescription = string.Empty;
            IsAddingNewTask = true;
        }
        
        private void CancelAddTask()
        {
            IsAddingNewTask = false;
        }
        
        private async void AddTask()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                MessageBox.Show("Task title is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                var task = new Models.Task
                {
                    Title = NewTaskTitle,
                    Description = NewTaskDescription,
                    CreatedAt = DateTime.Now,
                    Status = Models.TaskStatus.Backlog
                };
                
                await _taskService.AddTaskAsync(task);
                Tasks.Add(task);
                IsAddingNewTask = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void StartTask()
        {
            if (SelectedTask == null) return;
            
            try
            {
                // First pause any running tasks
                foreach (var task in Tasks.Where(t => t.IsRunning && t.Id != SelectedTask.Id))
                {
                    await _taskService.StopTaskTimerAsync(task.Id);
                    task.IsRunning = false;
                    task.LastStartTime = null;
                }
                
                var updatedTask = await _taskService.StartTaskTimerAsync(SelectedTask.Id);
                
                // Update the task in the collection
                if (updatedTask != null)
                {
                    int index = Tasks.IndexOf(SelectedTask);
                    if (index >= 0)
                    {
                        Tasks[index] = updatedTask;
                        SelectedTask = updatedTask;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void PauseTask()
        {
            if (SelectedTask == null || !SelectedTask.IsRunning) return;
            
            try
            {
                var updatedTask = await _taskService.StopTaskTimerAsync(SelectedTask.Id);
                
                // Update the task in the collection
                if (updatedTask != null)
                {
                    int index = Tasks.IndexOf(SelectedTask);
                    if (index >= 0)
                    {
                        Tasks[index] = updatedTask;
                        SelectedTask = updatedTask;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error pausing task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void CompleteTask()
        {
            if (SelectedTask == null) return;
            
            try
            {
                // First stop the timer if it's running
                if (SelectedTask.IsRunning)
                {
                    await _taskService.StopTaskTimerAsync(SelectedTask.Id);
                }
                
                SelectedTask.Status = Models.TaskStatus.Completed;
                await _taskService.UpdateTaskAsync(SelectedTask);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error completing task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void DeleteTask()
        {
            if (SelectedTask == null) return;
            
            try
            {
                var result = MessageBox.Show($"Are you sure you want to delete task '{SelectedTask.Title}'?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                if (result == MessageBoxResult.Yes)
                {
                    await _taskService.DeleteTaskAsync(SelectedTask.Id);
                    Tasks.Remove(SelectedTask);
                    SelectedTask = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private bool CanStartTask()
        {
            return SelectedTask != null && !SelectedTask.IsRunning && SelectedTask.Status != Models.TaskStatus.Completed;
        }
        
        private bool CanPauseTask()
        {
            return SelectedTask != null && SelectedTask.IsRunning;
        }
        
        private bool CanCompleteTask()
        {
            return SelectedTask != null && SelectedTask.Status != Models.TaskStatus.Completed;
        }
        
        private bool CanDeleteTask()
        {
            return SelectedTask != null;
        }
        
        private void ShowToday()
        {
            SelectedDate = DateOnly.FromDateTime(DateTime.Today);
        }
        
        private void ShowYesterday()
        {
            SelectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        }
        
        private void UpdateDailyTimeReports()
        {
            if (Tasks == null || !Tasks.Any()) return;
            
            var reports = new ObservableCollection<DailyTimeReport>();
            
            foreach (var task in Tasks)
            {
                var timeForDay = task.GetTimeForDay(SelectedDate);
                
                // Only add tasks that have time logged for the selected day
                if (timeForDay > TimeSpan.Zero)
                {
                    reports.Add(new DailyTimeReport
                    {
                        TaskId = task.Id,
                        TaskTitle = task.Title,
                        TimeSpent = timeForDay,
                        Status = task.Status
                    });
                }
            }
            
            DailyTimeReports = reports;
            OnPropertyChanged(nameof(TotalTimeForSelectedDate));
            OnPropertyChanged(nameof(SelectedDateFormatted));
        }
        
        private TimeSpan CalculateTotalTimeForDate(DateOnly date)
        {
            if (Tasks == null || !Tasks.Any()) return TimeSpan.Zero;
            
            TimeSpan total = TimeSpan.Zero;
            
            foreach (var task in Tasks)
            {
                total += task.GetTimeForDay(date);
            }
            
            return total;
        }
        
        public async System.Threading.Tasks.Task CloseAsync()
        {
            _timer.Stop();
            await _taskService.CloseAsync();
        }
    }
    
    public class DailyTimeReport
    {
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public TimeSpan TimeSpent { get; set; }
        public Models.TaskStatus Status { get; set; }
    }
} 