// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CommandPalette.Extensions;

namespace WorkspaceLauncherForVSCode;

#if DEBUG
[Guid("4c23de8f-6bdd-41a1-92f0-744d7af84659")]
#else
    [Guid("c6506a70-a0c8-4a96-9bc8-29714d6b2e34")]
#endif
public sealed partial class WorkspaceLauncherForVSCode : IExtension, IDisposable
{
    private readonly ManualResetEvent _extensionDisposedEvent;

    private readonly WorkspaceLauncherForVSCodeCommandsProvider _provider = new();

    public WorkspaceLauncherForVSCode(ManualResetEvent extensionDisposedEvent)
    {
        this._extensionDisposedEvent = extensionDisposedEvent;
    }

    public object? GetProvider(ProviderType providerType)
    {
        return providerType switch
        {
            ProviderType.Commands => _provider,
            _ => null,
        };
    }

    public void Dispose() => this._extensionDisposedEvent.Set();
}
