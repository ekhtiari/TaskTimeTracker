using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using TaskTimeTracker.Models;
using Task = System.Threading.Tasks.Task;

namespace TaskTimeTracker.Services
{
    public class DatabaseService
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public DatabaseService()
        {
            _dbPath = Path.Combine(FileSystem.AppDataDirectory, "tasks.db");
            _connectionString = $"Data Source={_dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Tasks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    Status INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    TotalTime TEXT NOT NULL,
                    LastStartTime TEXT
                )";
            command.ExecuteNonQuery();
        }

        public async Task<int> AddTaskAsync(string title)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Tasks (Title, Status, CreatedAt, TotalTime)
                VALUES (@Title, @Status, @CreatedAt, @TotalTime);
                SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("@Title", title);
            command.Parameters.AddWithValue("@Status", (int)Models.TaskStatus.Backlog);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("O"));
            command.Parameters.AddWithValue("@TotalTime", TimeSpan.Zero.ToString());

            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task<List<Models.Task>> GetAllTasksAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Tasks ORDER BY CreatedAt DESC";

            var tasks = new List<Models.Task>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tasks.Add(new Models.Task
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Status = (Models.TaskStatus)reader.GetInt32(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    TotalTime = TimeSpan.Parse(reader.GetString(4)),
                    LastStartTime = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5))
                });
            }

            return tasks;
        }

        public async Task UpdateTaskStatusAsync(int taskId, Models.TaskStatus status)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Tasks 
                SET Status = @Status,
                    LastStartTime = @LastStartTime,
                    TotalTime = @TotalTime
                WHERE Id = @Id";

            command.Parameters.AddWithValue("@Id", taskId);
            command.Parameters.AddWithValue("@Status", (int)status);

            if (status == Models.TaskStatus.Running)
            {
                command.Parameters.AddWithValue("@LastStartTime", DateTime.UtcNow.ToString("O"));
                command.Parameters.AddWithValue("@TotalTime", TimeSpan.Zero.ToString());
            }
            else if (status == Models.TaskStatus.Paused)
            {
                // Get current task
                var currentTask = await GetTaskAsync(taskId);
                if (currentTask != null && currentTask.LastStartTime.HasValue)
                {
                    var additionalTime = DateTime.UtcNow - currentTask.LastStartTime.Value;
                    var newTotalTime = currentTask.TotalTime + additionalTime;
                    command.Parameters.AddWithValue("@LastStartTime", DBNull.Value);
                    command.Parameters.AddWithValue("@TotalTime", newTotalTime.ToString());
                }
                else
                {
                    command.Parameters.AddWithValue("@LastStartTime", DBNull.Value);
                    command.Parameters.AddWithValue("@TotalTime", currentTask?.TotalTime.ToString() ?? TimeSpan.Zero.ToString());
                }
            }
            else
            {
                command.Parameters.AddWithValue("@LastStartTime", DBNull.Value);
                command.Parameters.AddWithValue("@TotalTime", TimeSpan.Zero.ToString());
            }

            await command.ExecuteNonQueryAsync();
        }

        public async Task<Models.Task> GetTaskAsync(int taskId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Tasks WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", taskId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Models.Task
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Status = (Models.TaskStatus)reader.GetInt32(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    TotalTime = TimeSpan.Parse(reader.GetString(4)),
                    LastStartTime = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5))
                };
            }

            return null;
        }

        public async Task<TimeSpan> GetTotalTimeAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT TotalTime FROM Tasks";

            var totalTime = TimeSpan.Zero;
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                totalTime += TimeSpan.Parse(reader.GetString(0));
            }

            return totalTime;
        }

        public async Task DeleteTaskAsync(int taskId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Tasks WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", taskId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateTaskTotalTimeAsync(int taskId, TimeSpan totalTime)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Tasks 
                SET TotalTime = @TotalTime
                WHERE Id = @Id";

            command.Parameters.AddWithValue("@Id", taskId);
            command.Parameters.AddWithValue("@TotalTime", totalTime.ToString());

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateTaskDescriptionAsync(int taskId, string description)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Tasks 
                SET Description = @Description
                WHERE Id = @Id";

            command.Parameters.AddWithValue("@Id", taskId);
            command.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? DBNull.Value : (object)description);

            await command.ExecuteNonQueryAsync();
        }
    }
} 