namespace TaskTimeTracker;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        window.Title = "Task Time Tracker";
        window.Width = 800;
        window.Height = 600;
        window.Destroying += Window_Destroying;
        return window;
    }

    private async void Window_Destroying(object sender, EventArgs e)
    {
        if (Shell.Current?.CurrentPage is MainPage mainPage)
        {
            await mainPage.SaveRunningTasksAsync();
        }
    }
}