# Command Palette
On its own Command Palette doesn't do much besides displaying a window.  
There are samples included that showcase how to develop plugins for Command Palette.

## Samples

* **Basic Commands**: Example of setting up simple commands that can be executed, such as opening a scene, creating a new folder, and more.
* **Math Engine**: Perform math calculations without ever leaving the editor. (Based on [PowerToys](https://github.com/Microsoft/PowerToys)' implementation)
* **Color Converter**: Easily convert colors between HEX, RGB(A), HSV(A), and HSL(A).

## Usage

* By default `Alt+]` opens the command palette. (can be configured in `Edit > Shortcuts...`)
* Type a command name and press `Enter`

Other keybinds:

* `Escape` - closes the window
* `Alt+Backspace` - goes back to the main window in case of commands with parameters

## Creating a Plugin for Command Palette
```csharp
using CommandPalette.Core;
using CommandPalette.Plugins;

public class MyPlugin : IPlugin {
    [UnityEditor.InitializeOnLoadMethod]
    private static void InitializePlugin() {
        CommandPalette.RegisterPlugin(new MyPlugin());
    }

    // This is an overall plugin multiplier. Controls the order in which entries are displayed
    public string PriorityMultiplier => 1.0f;
    public string Name => "My Plugin";
    public CommandPaletteWindow Window { get; set; }

    // Validate input here
    public bool IsValid(Query query) {
        return true;
    }
    
    // Generate entries based on query
    public IEnumerable<ResultEntry> GetResults(Query query) {
        yield return new ResultEntry(
            new ResultDisplaySettings("Title", ...),
            100, // Per-entry priority
            () => {
                UnityEngine.Debug.Log("Selected entry!");
            }
        );
    }
}
```