// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WorkspaceLauncherForVSCode.Services.VisualStudio.Models.Json
{
    public class CodeContainer
    {
        public required string Key { get; set; }

        public required Value Value { get; set; }
    }
}