using System;
using System.ComponentModel;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using TimeTracker.ViewModels;

namespace TimeTracker.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;
        private TaskbarIcon _trayIcon;
        
        public MainWindow()
        {
            InitializeComponent();
            
            // Get the tray icon from resources
            _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
            
            // Handle minimize to tray
            StateChanged += MainWindow_StateChanged;
        }
        
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                // Hide the window when minimized
                Hide();
            }
        }
        
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Clean up resources
            _trayIcon.Dispose();
            
            // Clean up the ViewModel resources
            ViewModel.CloseAsync().ConfigureAwait(false);
        }
        
        private void TrayIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            // Show the window when the tray icon is clicked
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }
        
        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Show the window when the Open menu item is clicked
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }
        
        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Close the application when the Exit menu item is clicked
            Application.Current.Shutdown();
        }
    }
} 