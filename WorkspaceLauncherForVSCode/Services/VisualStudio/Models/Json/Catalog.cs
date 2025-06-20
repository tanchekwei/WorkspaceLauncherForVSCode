// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Text.Json.Serialization;

namespace WorkspaceLauncherForVSCode.Services.VisualStudio.Models.Json
{
    public class Catalog
    {
        [JsonPropertyName("productLineVersion")]
        public string ProductLineVersion { get; set; } = string.Empty;
    }
}