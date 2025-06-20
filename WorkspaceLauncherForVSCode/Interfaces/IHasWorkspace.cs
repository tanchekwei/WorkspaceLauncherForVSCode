// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

namespace WorkspaceLauncherForVSCode
{
    public interface IHasWorkspace
    {
        public VisualStudioCodeWorkspace Workspace { get; set; }
    }
}