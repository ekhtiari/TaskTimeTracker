# TimeTracker

A WPF time tracking application with system tray support and SQLite database.

## Features

1. **Minimize to Tray**: When minimized, the application continues running in the system tray
2. **Task Backlog**: Add new tasks to track
3. **Time Tracking**: Start and pause tasks with a built-in timer
4. **Task States**: View task status (Backlog, In Progress, Completed)
5. **Reporting**: See time spent on each task, including daily reports
6. **System Tray**: Minimizes to system tray with custom application icon

## Requirements

- .NET 9.0 Runtime
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

### Daily Time Reports
1. The application shows time spent on tasks for the selected day
2. Use the "Today" and "Yesterday" buttons to switch between dates
3. View the total time spent across all tasks for that day

### Minimizing to Tray
- The application will minimize to the system tray when you click the minimize button
- Click on the tray icon to restore the application

### Exiting the Application
- Click the "X" button to close the application
- Alternatively, right-click the tray icon and select "Exit"

## Technical Details

- Built with WPF and .NET 9.0
- Uses SQLite for local database storage
- Uses Entity Framework Core for data access
- Implements MVVM architectural pattern

## Deployment and Release

This project uses GitHub Actions for automated building and publishing:

### Automatic Builds

When you push to the master branch, GitHub Actions will:
1. Build the application
2. Create a self-contained, single-file executable for Windows
3. Upload the build artifacts to GitHub

### Creating a Release

To create an official release:

1. Create a new tag:
   ```
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. GitHub Actions will:
   - Build the application
   - Create a new GitHub Release with the packaged application
   - Attach the built application to the release

### Notes for Cross-Platform

- This application is Windows-only as it uses WPF
- To support macOS, you would need to migrate to a cross-platform UI framework like .NET MAUI

## Note About Icon File

The application uses an `.ico` file in the `Resources` folder named `app.ico`. Replace this with your preferred icon if desired. 