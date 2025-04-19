# Command Palette for Visual Studio Code

## Overview

This project provides a command palette extension for opening Visual Studio Code workspaces.

![Command Palette for Visual Studio Code](./Assets/screenshot.png)

## Features

- **Workspace Management**: Retrieve and display a list of available workspaces, including their paths and types (e.g., Local, WSL, Remote).
- **Command Execution**: Open workspaces in Visual Studio Code using a dedicated command.
- **Multi-Installation Support**: Works for multiple installations of Visual Studio Code, including Insider and system installations.

## Installation

### Windows Store

<a href="https://apps.microsoft.com/detail/9PKCGVQ05TG1?mode=direct">
	<img src="https://get.microsoft.com/images/en-us%20light.svg" width="300"/>
</a>

### Manual Installation

1. Make sure you use the latest version of PowerToys.
2. Download the current Version and the certificate from [releases](https://github.com/JonahFintzDev/CommandPaletteVSCode/releases/).
3. Install the application by double-clicking the `.msix` file.

## Settings

- **Preferred Edition**: Determines which edition (Default or Insider) is used when a folder or workspace has been opened in both editions of VS Code.
- **Use Strict Search**: Enables or disables strict search for workspaces.  
  - **Strict Search**: Matches items where the search text appears as a contiguous substring in the item's title or subtitle. For example, searching for "abc" will match "abc" or "abc123" but not "a1b2c3".
- **Show Details Panel**: Toggles the visibility of the details panel in the UI.

## Contributing

Contributions are welcome! If you have suggestions for improvements or new features, please open an issue or submit a pull request.
