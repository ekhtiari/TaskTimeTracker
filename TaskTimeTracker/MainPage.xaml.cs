using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TaskTimeTracker.Models;
using TaskTimeTracker.Services;
using Task = System.Threading.Tasks.Task;

namespace TaskTimeTracker;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly ObservableCollection<TaskViewModel> _tasks;
    private IDispatcherTimer _timer;
    private TaskViewModel _selectedTask;

    public MainPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
        _tasks = new ObservableCollection<TaskViewModel>();
        TasksListView.ItemsSource = _tasks;

        _timer = Application.Current.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
        _timer.Start();

        LoadTasksAsync();
    }

    private async void LoadTasksAsync()
    {
        var tasks = await _databaseService.GetAllTasksAsync();
        _tasks.Clear();
        foreach (var task in tasks)
        {
            _tasks.Add(new TaskViewModel(task, _databaseService));
        }
    }

    private async void OnAddTaskClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NewTaskEntry.Text))
            return;

        await _databaseService.AddTaskAsync(NewTaskEntry.Text);
        NewTaskEntry.Text = string.Empty;
        LoadTasksAsync();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        CurrentTimeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
        UpdateRunningTasks();
        UpdateSelectedTaskDetails();
    }

    private void UpdateRunningTasks()
    {
        foreach (var task in _tasks)
        {
            task.UpdateRunningTime();
        }
    }

    private void UpdateSelectedTaskDetails()
    {
        if (_selectedTask != null)
        {
            SelectedTaskTitle.Text = _selectedTask.Title;
            SelectedTaskStatus.Text = _selectedTask.Status;
            SelectedTaskTotalTime.Text = _selectedTask.TotalTime.ToString(@"hh\:mm\:ss");
            SelectedTaskTimerStatus.Text = _selectedTask.TimerStatus;
            SelectedTaskCreatedAt.Text = _selectedTask.CreatedAt.ToString("M/d/yyyy h:mm tt");

            PauseButton.IsVisible = _selectedTask.Status != "Completed";
            PauseButton.Text = _selectedTask.Status == "Running" ? "Pause" : "Start";
            CompleteButton.IsVisible = _selectedTask.Status != "Completed";
        }
    }

    private void OnTaskSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is TaskViewModel selectedTask)
        {
            _selectedTask = selectedTask;
            TaskDetailsPanel.IsVisible = true;
            UpdateSelectedTaskDetails();
        }
    }

    private async void OnPauseClicked(object sender, EventArgs e)
    {
        if (_selectedTask == null) return;

        if (_selectedTask.Status == "Running")
        {
            await _selectedTask.PauseTaskAsync();
        }
        else
        {
            await _selectedTask.StartTaskAsync();
        }
        UpdateSelectedTaskDetails();
    }

    private async void OnCompleteClicked(object sender, EventArgs e)
    {
        if (_selectedTask == null) return;

        await _selectedTask.CompleteTaskAsync();
        UpdateSelectedTaskDetails();
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_selectedTask == null) return;

        bool answer = await DisplayAlert("Confirm Delete", 
            "Are you sure you want to delete this task?", "Yes", "No");

        if (answer)
        {
            await _databaseService.DeleteTaskAsync(_selectedTask.Id);
            _tasks.Remove(_selectedTask);
            TaskDetailsPanel.IsVisible = false;
            _selectedTask = null;
        }
    }
}

public class TaskViewModel : INotifyPropertyChanged
{
    private readonly Models.Task _task;
    private readonly DatabaseService _databaseService;
    private TimeSpan _totalTime;
    private DateTime? _lastStartTime;

    public TaskViewModel(Models.Task task, DatabaseService databaseService)
    {
        _task = task;
        _databaseService = databaseService;
        _totalTime = task.TotalTime;
        _lastStartTime = task.LastStartTime;
    }

    public int Id => _task.Id;
    public string Title => _task.Title;
    public string Status => _task.Status.ToString();
    public DateTime CreatedAt => _task.CreatedAt;
    public string TimerStatus => _task.Status == Models.TaskStatus.Running ? "Running" : "Stopped";
    public TimeSpan TotalTime
    {
        get => _totalTime;
        private set
        {
            if (_totalTime != value)
            {
                _totalTime = value;
                OnPropertyChanged();
            }
        }
    }

    public bool CanStart => _task.Status == Models.TaskStatus.Backlog || _task.Status == Models.TaskStatus.Paused;
    public bool CanPause => _task.Status == Models.TaskStatus.Running;

    public async Task StartTaskAsync()
    {
        await _databaseService.UpdateTaskStatusAsync(_task.Id, Models.TaskStatus.Running);
        _task.Status = Models.TaskStatus.Running;
        _lastStartTime = DateTime.Now;
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(TimerStatus));
        OnPropertyChanged(nameof(CanStart));
        OnPropertyChanged(nameof(CanPause));
    }

    public async Task PauseTaskAsync()
    {
        if (_task.Status == Models.TaskStatus.Running && _lastStartTime.HasValue)
        {
            var currentTime = DateTime.Now;
            var additionalTime = currentTime - _lastStartTime.Value;
            _totalTime += additionalTime;
            await _databaseService.UpdateTaskTotalTimeAsync(_task.Id, _totalTime);
        }
        
        await _databaseService.UpdateTaskStatusAsync(_task.Id, Models.TaskStatus.Paused);
        _task.Status = Models.TaskStatus.Paused;
        _lastStartTime = null;
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(TimerStatus));
        OnPropertyChanged(nameof(CanStart));
        OnPropertyChanged(nameof(CanPause));
    }

    public async Task CompleteTaskAsync()
    {
        if (_task.Status == Models.TaskStatus.Running)
        {
            await PauseTaskAsync();
        }
        await _databaseService.UpdateTaskStatusAsync(_task.Id, Models.TaskStatus.Completed);
        _task.Status = Models.TaskStatus.Completed;
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(TimerStatus));
        OnPropertyChanged(nameof(CanStart));
        OnPropertyChanged(nameof(CanPause));
    }

    public void UpdateRunningTime()
    {
        if (_task.Status == Models.TaskStatus.Running && _lastStartTime.HasValue)
        {
            try
            {
                var currentTime = DateTime.Now;
                var runningTime = currentTime - _lastStartTime.Value;
                _totalTime = _totalTime.Add(TimeSpan.FromSeconds(1));
                OnPropertyChanged(nameof(TotalTime));
            }
            catch (Exception)
            {
                // Ignore any errors during time update
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}