# Project Guide: Workspace Launcher for VS Code

This document provides an overview of the project's architecture, focusing on its design principles and key components.

## Architecture Overview

The extension is designed following SOLID principles to ensure it is maintainable, scalable, and testable.

### Key Principles

*   **Single Responsibility Principle (SRP):** Each class has a single, well-defined responsibility. This is exemplified by the service layer, where responsibilities are highly granular. For instance, `VisualStudioCodeInstanceProvider` is solely responsible for discovering Visual Studio Code installations. `VisualStudioCodeWorkspaceProvider` orchestrates workspace discovery, which is further delegated to specialized reader classes like `VscdbWorkspaceReader` and `StorageJsonWorkspaceReader`, each handling a specific data source.
*   **Dependency Inversion Principle (DIP):** High-level modules do not depend on low-level modules; both depend on abstractions. This is achieved through the use of interfaces like `IVisualStudioCodeService`. The `WorkspaceLauncherForVSCodeCommandsProvider` and `VisualStudioCodePage` depend on the `IVisualStudioCodeService` interface, not the concrete `VisualStudioCodeService` implementation. This decoupling allows for easier testing and maintenance.

## Project Structure

The project is organized into the following key directories within the `VisualStudioCode` project:

- **`/`**:
  - [`WorkspaceLauncherForVSCode.cs`](./WorkspaceLauncherForVSCode.cs) - The main extension implementation.
- **`/Classes`**: Contains core data models and helper classes.
  - [`SettingsManager.cs`](./Classes/SettingsManager.cs) - Manages user-configurable settings.
  - `VisualStudioCodeInstance.cs`, `VisualStudioCodeWorkspace.cs` - Core data models.
- **`/Commands`**: Contains command implementations that are executed by the user.
    - `OpenVisualStudioCodeCommand.cs` - The primary command for launching a selected workspace.
    - `CopyPathCommand.cs` - Copies the workspace path to the clipboard.
    - `RemoveWorkspaceCommand.cs` - Removes a workspace from the recently opened list.
- **`/Interfaces`**: Defines the contracts for services.
  - `IVisualStudioCodeService.cs` - The contract for the main Visual Studio Code service.
- **`/Pages`**: Contains UI components.
  - [`VisualStudioCodePage.cs`](./Pages/VisualStudioCodePage.cs) - A dynamic list page that displays discovered workspaces.
- **`/Services`**: Contains the primary service implementations.
  - [`WorkspaceLauncherForVSCodeCommandsProvider.cs`](./Services/WorkspaceLauncherForVSCodeCommandsProvider.cs) - The entry point for providing commands to the Command Palette.
  - [`VisualStudioCodeService.cs`](./Services/VisualStudioCodeService.cs) - Acts as a facade, orchestrating calls to more specialized provider classes.
  - [`VisualStudioCodeInstanceProvider.cs`](./Services/VisualStudioCodeInstanceProvider.cs) - Discovers all installed instances of Visual Studio Code.
  - [`VisualStudioCodeWorkspaceProvider.cs`](./Services/VisualStudioCodeWorkspaceProvider.cs) - Orchestrates the process of finding recent workspaces.
- **`/Workspaces`**: Contains workspace-related logic.
    - **`/Models`**: C# models that map to the JSON structures of Visual Studio Code's workspace storage.
    - **`/Readers`**: Specialized classes for reading and parsing workspace data.
        - [`VscdbWorkspaceReader.cs`](./Workspaces/Readers/VscdbWorkspaceReader.cs) - Reads workspace data from the `state.vscdb` SQLite database.
        - [`StorageJsonWorkspaceReader.cs`](./Workspaces/Readers/StorageJsonWorkspaceReader.cs) - Reads workspace data from the `storage.json` file.

## Core Components

### Services and Providers

*   **`IVisualStudioCodeService` / `VisualStudioCodeService`**: This service acts as the primary entry point for interacting with local Visual Studio Code data. It delegates the complex tasks of instance and workspace discovery to specialized provider classes.
*   **`VisualStudioCodeInstanceProvider`**: A static provider class responsible for discovering all installed instances of Visual Studio Code (Stable, Insiders, User, System) by scanning known locations and the system's PATH environment variable.
*   **`VisualStudioCodeWorkspaceProvider`**: This static provider orchestrates the process of finding recently opened workspaces. It doesn't perform the reading itself but delegates the task to specialized reader classes.

### Workspace Readers and Performance

To ensure high performance and low memory usage, the extension uses the `System.Text.Json` source generator for deserializing workspace data. This avoids runtime reflection and minimizes allocations.

*   **`VscdbWorkspaceReader` & `StorageJsonWorkspaceReader`**: These static reader classes are responsible for retrieving workspace information from Visual Studio Code's two primary data sources: the `state.vscdb` SQLite database and the `storage.json` file. Each reader is optimized to read its specific source and deserialize the data efficiently using source-generated models.

### Settings

*   **`SettingsManager`**: Manages all user-configurable settings for the extension. It loads settings from a JSON file and provides them to the rest of the application. It raises an event when settings are changed, allowing other components to react accordingly.

### UI and Commands

*   **`WorkspaceLauncherForVSCodeCommandsProvider`**: The main entry point for providing commands to the Command Palette. It initializes the required services (`SettingsManager`, `VisualStudioCodeService`) and creates the top-level command items. It also listens for settings changes to reload Visual Studio Code instances when necessary.
*   **`VisualStudioCodePage`**: A dynamic list page that displays the discovered Visual Studio Code workspaces. It receives the `IVisualStudioCodeService` to fetch workspace data asynchronously and handles user interactions like searching and scrolling.
*   **`OpenVisualStudioCodeCommand`**: The primary command responsible for launching a selected Visual Studio Code workspace.

#### Secondary Commands
Beyond the primary action of opening a workspace, the extension provides secondary commands for managing the workspace list:

*   **`CopyPathCommand`**: This command allows the user to copy the full file path of a workspace or folder directly to the clipboard, providing a quick way to access the location of the project.
*   **`RemoveWorkspaceCommand`**: This command provides the functionality to remove a workspace or folder entry from the Visual Studio Code's list of recently opened items. It first presents a confirmation dialog to prevent accidental removals. The command directly modifies the `state.vscdb` or `storage.json` files where VS Code stores this information.

This architecture ensures a clean separation of concerns, making the codebase easier to understand, extend, and debug.