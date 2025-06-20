// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Text.Json.Serialization;

namespace WorkspaceLauncherForVSCode.Services.VisualStudio.Models.Json
{
    public class VisualStudioInstance
    {
        [JsonPropertyName("instanceId")]
        public string InstanceId { get; set; } = string.Empty;

        [JsonPropertyName("isPrerelease")]
        public bool IsPrerelease { get; set; }

        [JsonPropertyName("catalog")]
        public Catalog Catalog { get; set; } = new();
    }
}