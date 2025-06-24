// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

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