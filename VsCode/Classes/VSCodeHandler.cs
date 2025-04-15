using System;
using System.Collections.Generic;
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
    public static void LoadInstances()
    {
        var appdataProgramFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var programsFolderPathBase = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var defaultStoragePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Code", "User", "globalStorage", "storage.json");
        var insiderStoragePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Code - Insiders", "User", "globalStorage", "storage.json");


        AddInstance("VS Code", Path.Combine(appdataProgramFilesPath, "Programs", "Microsoft VS Code", "Code.exe"), defaultStoragePath, VSCodeInstallationType.User, VSCodeType.Default);
        AddInstance("VS Code [System]", Path.Combine(programsFolderPathBase, "Microsoft VS Code", "Code.exe"), defaultStoragePath, VSCodeInstallationType.System, VSCodeType.Default);
        AddInstance("VS Code - Insiders", Path.Combine(appdataProgramFilesPath, "Programs", "Microsoft VS Code Insiders", "Code - Insiders.exe"), insiderStoragePath, VSCodeInstallationType.User, VSCodeType.Insider);
        AddInstance("VS Code - Insiders [System]", Path.Combine(programsFolderPathBase, "Microsoft VS Code Insiders", "Code - Insiders.exe"), insiderStoragePath, VSCodeInstallationType.System, VSCodeType.Insider);
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
            if (!File.Exists(instance.StoragePath) || !File.Exists(instance.ExecutablePath))
            {
                continue;
            }

            try
            {
                var jsonContent = File.ReadAllText(instance.StoragePath);
                var jsonDocument = JsonDocument.Parse(jsonContent);
                var rootElement = jsonDocument.RootElement;
                // Navigate to the profileAssociations.workspaces property
                if (rootElement.TryGetProperty("backupWorkspaces", out var profileAssociations))
                {
                    if (profileAssociations.TryGetProperty("workspaces", out var workspaces))
                    {
                        // workspaces: key-value pairs
                        foreach (var workspace in workspaces.EnumerateArray())
                        {
                            if (workspace.TryGetProperty("configURIPath", out var path))
                            {
                                var pathString = path.GetString();

                                if (pathString == null || pathString.Split('/').Length == 0)
                                {
                                    continue;
                                }

                                outWorkspaces.Add(new VSCodeWorkspace(instance, pathString));
                            }
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                // Handle JSON parsing error
                Console.WriteLine($"Error parsing JSON: {ex.Message}");
            }
        }
        return outWorkspaces;
    }
}
