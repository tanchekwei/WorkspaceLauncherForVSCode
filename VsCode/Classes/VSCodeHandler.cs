using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace CmdPalVsCode;

/// <summary>
/// Handles operations related to Visual Studio Code instances and workspaces.
/// </summary>
internal static class VSCodeHandler
{
    public static List<VSCodeInstance> Instances = new List<VSCodeInstance>();

    /// <summary>
    /// Loads all available VS Code instances (default and insiders, user and system installations).
    /// </summary> 
    public static void LoadInstances(string preferredEdition)
    {
        var appdataProgramFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var programsFolderPathBase = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var defaultStoragePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Code", "User", "globalStorage");
        var insiderStoragePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Code - Insiders", "User", "globalStorage");

        Instances.Clear();

        AddInstance("VS Code", Path.Combine(appdataProgramFilesPath, "Programs", "Microsoft VS Code", "Code.exe"), defaultStoragePath, VSCodeInstallationType.User, VSCodeType.Default);
        AddInstance("VS Code [System]", Path.Combine(programsFolderPathBase, "Microsoft VS Code", "Code.exe"), defaultStoragePath, VSCodeInstallationType.System, VSCodeType.Default);
        AddInstance("VS Code - Insiders", Path.Combine(appdataProgramFilesPath, "Programs", "Microsoft VS Code Insiders", "Code - Insiders.exe"), insiderStoragePath, VSCodeInstallationType.User, VSCodeType.Insider);
        AddInstance("VS Code - Insiders [System]", Path.Combine(programsFolderPathBase, "Microsoft VS Code Insiders", "Code - Insiders.exe"), insiderStoragePath, VSCodeInstallationType.System, VSCodeType.Insider);

        // search for custom installations in PATH environment variable
        try
        {
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                var paths = pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var dir in paths)
                {

                    // get parent directory of the current directory
                    if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                    {
                        continue;
                    }
                    var parentDir = Path.GetDirectoryName(dir) ?? dir;
                    try
                    {
                        var codeExe = Path.Combine(parentDir, "code.exe");
                        var codeInsidersExe = Path.Combine(parentDir, "Code - Insiders.exe");

                        if (File.Exists(codeExe))
                        {
                            AddInstance("VS Code [Custom]", codeExe, defaultStoragePath, VSCodeInstallationType.User, VSCodeType.Default);
                        }
                        if (File.Exists(codeInsidersExe))
                        {
                            AddInstance("VS Code - Insiders [Custom]", codeInsidersExe, insiderStoragePath, VSCodeInstallationType.User, VSCodeType.Insider);
                        }
                    }
                    catch
                    {
                        // ignore any errors while checking for custom installations
                    }
                }
            }
        }
        catch
        {
            // ignore invalid PATH entries
        }

        if (preferredEdition == "Insider")
        {
            // sort instances to have insiders first
            Instances.Sort((x, y) =>
            {
                if (x.VSCodeType == VSCodeType.Insider && y.VSCodeType != VSCodeType.Insider)
                {
                    return -1;
                }
                else if (x.VSCodeType != VSCodeType.Insider && y.VSCodeType == VSCodeType.Insider)
                {
                    return 1;
                }
                return 0;
            });
        }
        else
        {
            // sort instances to have default first
            Instances.Sort((x, y) =>
            {
                if (x.VSCodeType == VSCodeType.Default && y.VSCodeType != VSCodeType.Default)
                {
                    return -1;
                }
                else if (x.VSCodeType != VSCodeType.Default && y.VSCodeType == VSCodeType.Default)
                {
                    return 1;
                }
                return 0;
            });
        }
    }

    /// <summary>
    /// Adds a new VS Code instance to the list of instances if the executable path exists.
    /// /// </summary>
    /// <param name="name">Name of the instance.</param>
    /// <param name="path">Path to the executable.</param>
    /// <param name="storagePath">Path to the storage file.</param>
    /// <param name="type">Installation type (user/system).</param>
    /// <param name="codeType">Type of VS Code (default/insider).</param>
    private static void AddInstance(string name, string path, string storagePath, VSCodeInstallationType type, VSCodeType codeType)
    {
        if (File.Exists(path))
        {
            // check if there is already an instance with the same executable path
            if (Instances.Exists(instance => instance.ExecutablePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
            {
                return; // Instance already exists
            }
            Instances.Add(new VSCodeInstance(name, path, storagePath, type, codeType));
        }
    }

    /// <summary>
    /// Retrieves a list of workspaces from the loaded VS Code instances.
    /// </summary>
    /// <returns>List of VS Code workspaces.</returns>
    public static List<VSCodeWorkspace> GetWorkspaces()
    {
        var outWorkspaces = new List<VSCodeWorkspace>();

        foreach (var instance in Instances)
        {
            // check if storage file exists
            if (!File.Exists(instance.ExecutablePath))
            {
                continue;
            }

            SqliteConnection connection = null;
            // try getting data from state.vscdb 
            try
            {
                var stateFilePath = Path.Combine(instance.StoragePath, "state.vscdb");
                if (File.Exists(stateFilePath))
                {
                    connection = new SqliteConnection($"Data Source={Path.Combine(instance.StoragePath, "state.vscdb")};Mode=ReadOnly;");
                    connection.Open();

                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        var sqliteCommand = connection.CreateCommand();
                        sqliteCommand.CommandText = "SELECT value FROM ItemTable WHERE key LIKE 'history.recentlyOpenedPathsList'";

                        var sqliteDataReader = sqliteCommand.ExecuteReader();

                        if (sqliteDataReader.Read())
                        {
                            string json = sqliteDataReader.GetString(0);
                            if (!string.IsNullOrEmpty(json))
                            {
                                var jsonDocument = JsonDocument.Parse(json);
                                var rootElement = jsonDocument.RootElement;

                                if (rootElement.TryGetProperty("entries", out var entries))
                                {
                                    foreach (var entry in entries.EnumerateArray())
                                    {
                                        string? pathString = null;
                                        if (entry.TryGetProperty("folderUri", out var path))
                                        {
                                            pathString = path.GetString();

                                            if (pathString == null || pathString.Split('/').Length == 0)
                                            {
                                                continue;
                                            }

                                            outWorkspaces.Add(new VSCodeWorkspace(instance, pathString, VSCodeWorkspaceType.Folder));
                                        }
                                        else if (entry.TryGetProperty("workspace", out var workspace))
                                        {
                                            if (workspace.TryGetProperty("configPath", out var configPath))
                                            {
                                                pathString = configPath.GetString();
                                                if (pathString == null || pathString.Split('/').Length == 0)
                                                {
                                                    continue;
                                                }
                                                outWorkspaces.Add(new VSCodeWorkspace(instance, pathString, VSCodeWorkspaceType.Workspace));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                // Handle JSON parsing error
                Console.WriteLine($"Error parsing state.vscdb: {ex.Message}");
            }
            finally
            {
                connection?.Close();
            }

            // try getting data from storage.json
            try
            {
                var storageFilePath = Path.Combine(instance.StoragePath, "storage.json");

                if (File.Exists(storageFilePath))
                {
                    var jsonContent = File.ReadAllText(storageFilePath);
                    var jsonDocument = JsonDocument.Parse(jsonContent);
                    var rootElement = jsonDocument.RootElement;

                    if (rootElement.TryGetProperty("backupWorkspaces", out var
                        backupWorkspaces))
                    {
                        if (backupWorkspaces.TryGetProperty("workspaces", out var workspaces))
                        {
                            foreach (var workspace in workspaces.EnumerateArray())
                            {
                                if (workspace.TryGetProperty("configURIPath", out var path))
                                {
                                    var pathString = path.GetString();

                                    if (pathString == null || pathString.Split('/').Length == 0)
                                    {
                                        continue;
                                    }

                                    outWorkspaces.Add(new VSCodeWorkspace(instance, pathString, VSCodeWorkspaceType.Workspace));
                                }
                            }
                        }

                        if (backupWorkspaces.TryGetProperty("folders", out var folders))
                        {
                            foreach (var folder in folders.EnumerateArray())
                            {
                                if (folder.TryGetProperty("folderUri", out var path))
                                {
                                    var pathString = path.GetString();

                                    if (pathString == null || pathString.Split('/').Length == 0)
                                    {
                                        continue;
                                    }

                                    outWorkspaces.Add(new VSCodeWorkspace(instance, pathString, VSCodeWorkspaceType.Folder));
                                }
                            }
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                // Handle JSON parsing error
                Console.WriteLine($"Error parsing storage.json: {ex.Message}");
            }
        }

        // filter out workspaces with duplicate paths
        var uniqueWorkspaces = new HashSet<string>();
        outWorkspaces.RemoveAll(workspace =>
        {
            var path = workspace.Path;
            if (uniqueWorkspaces.Contains(path))
            {
                // Remove this workspace
                return true;
            }

            // Keep this workspace
            uniqueWorkspaces.Add(path);
            return false;
        });

        return outWorkspaces;
    }
}
