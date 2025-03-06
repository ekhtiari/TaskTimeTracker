# TimeTracker

A WPF time tracking application with system tray support and SQLite database.

## Features

1. **Minimize to Tray**: When minimized, the application continues running in the system tray
2. **Task Backlog**: Add new tasks to track
3. **Time Tracking**: Start and pause tasks with a built-in timer
4. **Task States**: View task status (Backlog, In Progress, Completed)
5. **Reporting**: See time spent on each task

## Requirements

- .NET 6.0 Runtime
- Windows Operating System

## Setup Instructions

1. Open the solution in Visual Studio
2. Make sure to restore NuGet packages
3. Build and run the application

## Usage

### Adding Tasks
1. Click the "Add New Task" button
2. Enter a title and optional description
3. Click "Add Task"

### Tracking Time
1. Select a task from the list
2. Click "Start" to begin tracking time
3. Click "Pause" to pause the timer
4. Click "Complete" to mark the task as completed

### Minimizing to Tray
- The application will minimize to the system tray when you click the minimize button
- Click on the tray icon to restore the application

### Exiting the Application
- Click the "X" button to close the application
- Alternatively, right-click the tray icon and select "Exit"

## Technical Details

- Built with WPF and .NET 6.0
- Uses SQLite for local database storage
- Uses Entity Framework Core for data access
- Implements MVVM architectural pattern

## Note About Icon File

The application requires a valid `.ico` file in the `Resources` folder named `timer.ico`. The current file is a placeholder that you'll need to replace with a real icon file. 