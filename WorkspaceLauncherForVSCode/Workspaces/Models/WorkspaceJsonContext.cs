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