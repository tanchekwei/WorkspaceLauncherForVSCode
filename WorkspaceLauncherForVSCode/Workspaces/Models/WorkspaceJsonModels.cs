// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WorkspaceLauncherForVSCode.Workspaces.Models
{
    // Models for state.vscdb JSON content
    public class VscdbRoot
    {
        [JsonPropertyName("entries")]
        public List<VscdbEntry> Entries { get; set; } = new();
    }

    public class VscdbEntry
    {
        [JsonPropertyName("folderUri")]
        public string? FolderUri { get; set; }

        [JsonPropertyName("workspace")]
        public VscdbWorkspace? Workspace { get; set; }
    }

    public class VscdbWorkspace
    {
        [JsonPropertyName("configPath")]
        public string? ConfigPath { get; set; }
    }

    // Models for storage.json JSON content
    public class StorageJsonRoot
    {
        [JsonPropertyName("backupWorkspaces")]
        public BackupWorkspaces? BackupWorkspaces { get; set; }
    }

    public class BackupWorkspaces
    {
        [JsonPropertyName("workspaces")]
        public List<StorageJsonWorkspace> Workspaces { get; set; } = new();

        [JsonPropertyName("folders")]
        public List<StorageJsonFolder> Folders { get; set; } = new();
    }

    public class StorageJsonWorkspace
    {
        [JsonPropertyName("configURIPath")]
        public string? ConfigURIPath { get; set; }
    }

    public class StorageJsonFolder
    {
        [JsonPropertyName("folderUri")]
        public string? FolderUri { get; set; }
    }
}