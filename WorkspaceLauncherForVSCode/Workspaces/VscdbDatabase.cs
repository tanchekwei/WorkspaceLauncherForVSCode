using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Workspaces.Models;

namespace WorkspaceLauncherForVSCode.Workspaces.Readers
{
    public partial class VscdbDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        public VscdbDatabase(string dbPath)
        {
            _connection = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly;");
        }

        public async Task OpenAsync(CancellationToken cancellationToken)
        {
            await _connection.OpenAsync(cancellationToken);
        }

        public async Task<string> ReadWorkspacesJsonAsync(CancellationToken cancellationToken)
        {
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT value FROM ItemTable WHERE key LIKE 'history.recentlyOpenedPathsList'";
            var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return reader.GetString(0);
            }
            return string.Empty;
        }

        public void Dispose()
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}