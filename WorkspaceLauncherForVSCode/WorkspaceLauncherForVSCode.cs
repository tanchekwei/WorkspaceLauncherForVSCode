using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CommandPalette.Extensions;

namespace WorkspaceLauncherForVSCode;

[Guid("c6506a70-a0c8-4a96-9bc8-29714d6b2e34")]
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
