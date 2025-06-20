// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WorkspaceLauncherForVSCode.Services.VisualStudio.Models.Json
{
    [JsonSerializable(typeof(List<CodeContainer>))]
    internal sealed partial class CodeContainerSerializerContext : JsonSerializerContext
    {
    }
}