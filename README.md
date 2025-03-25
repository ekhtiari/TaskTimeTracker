# Task Time Tracker

A .NET MAUI desktop application for tracking time spent on tasks. Built with modern UI and efficient time tracking capabilities.

## Features

- Create, start, pause, and complete tasks
- Accurate time tracking with automatic saving
- Clean and intuitive user interface
- Task details view with status and timing information
- Only one task can run at a time
- Persistent storage using SQLite database

## Requirements

- Windows 10 version 1809 or later
- .NET 9.0
- Windows App SDK runtime
- Visual Studio 2022 with .NET MAUI workload

## Setup

1. Clone the repository:
```bash
git clone https://github.com/ekhtiari/TaskTimeTracker.git
```

2. Open the solution in Visual Studio 2022

3. Install the required workload if not already installed:
```bash
dotnet workload install maui
```

4. Build and run the application:
```bash
dotnet run --project TaskTimeTracker/TaskTimeTracker.csproj
```

## Usage

1. Add a new task using the input field at the top
2. Select a task to view its details
3. Use the Start/Pause button to control the timer
4. Complete tasks when finished
5. Delete tasks if needed

## Technical Details

- Built with .NET MAUI
- Uses SQLite for data persistence
- MVVM architecture
- Real-time UI updates
- Automatic time tracking and saving 