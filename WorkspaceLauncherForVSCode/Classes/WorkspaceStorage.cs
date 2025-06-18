using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Classes
{
    public sealed partial class WorkspaceStorage : IDisposable
    {
        private readonly SqliteConnection _connection;
        private const string DbName = "workspaces.db";

        public WorkspaceStorage()
        {
            var dbPath = Path.Combine(Utilities.BaseSettingsPath("WorkspaceLauncherForVSCode"), DbName);
            _connection = new SqliteConnection($"Data Source={dbPath}");
            _connection.Open();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var command = _connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Workspaces (
                    Path TEXT PRIMARY KEY,
                    Name TEXT,
                    Type INTEGER,
                    Frequency INTEGER DEFAULT 0,
                    LastAccessed TEXT
                );
            ";
            command.ExecuteNonQuery();
        }

        public async Task<List<VisualStudioCodeWorkspace>> GetWorkspacesAsync()
        {
            var workspaces = new List<VisualStudioCodeWorkspace>();
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT Path, Name, Type, Frequency, LastAccessed FROM Workspaces";
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                workspaces.Add(new VisualStudioCodeWorkspace
                {
                    Path = reader.GetString(0),
                    Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                    WorkspaceType = (Enums.WorkspaceType)reader.GetInt32(2),
                    Frequency = reader.GetInt32(3),
                    LastAccessed = reader.IsDBNull(4) ? DateTime.MinValue : DateTime.Parse(reader.GetString(4), CultureInfo.InvariantCulture)
                });
            }
            return workspaces;
        }

        public async Task SaveWorkspacesAsync(IEnumerable<VisualStudioCodeWorkspace> workspaces)
        {
            // using var logger = new TimeLogger();
            using var transaction = _connection.BeginTransaction();
            foreach (var workspace in workspaces)
            {
                if (workspace.Path == null || workspace.Path == null)
                {
                    continue;
                }
                var command = _connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT OR REPLACE INTO Workspaces (Path, Name, Type, Frequency, LastAccessed)
                    VALUES (@Path, @Name, @Type, COALESCE((SELECT Frequency FROM Workspaces WHERE Path = @Path), 0), @LastAccessed)";
                command.Parameters.AddWithValue("@Path", workspace.Path);
                command.Parameters.AddWithValue("@Name", workspace.Name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Type", workspace.WorkspaceType);
                command.Parameters.AddWithValue("@LastAccessed", workspace.LastAccessed.ToString("o"));
                await command.ExecuteNonQueryAsync();
            }
            transaction.Commit();
        }

        public async Task UpdateWorkspaceFrequencyAsync(string path)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "UPDATE Workspaces SET Frequency = Frequency + 1, LastAccessed = @LastAccessed WHERE Path = @path";
            command.Parameters.AddWithValue("@path", path);
            command.Parameters.AddWithValue("@LastAccessed", DateTime.Now.ToString("o"));
            await command.ExecuteNonQueryAsync();
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}