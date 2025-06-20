// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Text.Json.Serialization;

namespace WorkspaceLauncherForVSCode.Workspaces.Models
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(VscdbRoot))]
    [JsonSerializable(typeof(StorageJsonRoot))]
    internal sealed partial class WorkspaceJsonContext : JsonSerializerContext
    {
    }
}