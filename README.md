# ReColor

Repaint your ReStory workbench. ReColor lets you paint your store table and switch between normal and tournament style whenever you want.

## Requirements

- MelonLoader, latest, or
- BepInEx 5.4.23.5.

## Installation (MelonLoader)

1. Download `MelonLoader.Installer.exe` from the
   [MelonLoader releases page](https://github.com/LavaGang/MelonLoader/releases/latest) and run it. Click
   **Select** and point it at your `Restory.exe`, leave the latest version selected, then hit **Install**.
   By default the game is at
   `(YourDrive):\Program Files (x86)\Steam\steamapps\common\Restory\Restory.exe`.

2. Launch the game once, then quit. MelonLoader creates its `Mods` and `UserData` folders on first run.

3. Extract this mod's archive and move `ReColor.dll` into `Mods\`, so it ends up at:

   ```
   ...\Restory\Mods\ReColor.dll
   ```

## Installation (BepInEx)

1. Download `BepInEx_win_x64_5.4.23.5.zip` from the
   [BepInEx releases page](https://github.com/BepInEx/BepInEx/releases) and extract it into your ReStory
   folder, the one containing `Restory.exe`. Any newer 5.4.x release works too. Just take the file with
   `win_x64` in its name.

   Check it extracted correctly: `BepInEx\core\BepInEx.dll` should now exist. By default that is
   `(YourDrive):\Program Files (x86)\Steam\steamapps\common\Restory\BepInEx\core`.

2. Launch the game once from Steam, then quit. BepInEx creates its `plugins` and `config` folders on first
   run.

3. Extract this mod's archive and move the `ReColor` folder into `BepInEx\plugins\`, so the DLL ends up at:

   ```
   ...\Restory\BepInEx\plugins\ReColor\ReColor.dll
   ```

## Controls

| Key     | Action                                                 |
|---------|--------------------------------------------------------|
| **F11** | Opens and closes the board while you are at the bench. |

Three buttons run down the left side of the board. Paint opens the editing page, Preset the grid of
ready-made looks, and Reset returns the selected surface to the game's own values.

## Configuration

BepInEx writes `...\BepInEx\config\com.archives.recolor.cfg` the first time you run the game with the mod
installed. **Edit it while the game is closed.** The mod reads it once at startup.

On MelonLoader the same settings live in `...\UserData\MelonPreferences.cfg`, shared with your other mods,
under `[ReColorGeneral]` and `[ReColorHotkeys]` instead of the two sections below.

### `[General]`

| Setting       | Default | Description                                            |
|---------------|---------|--------------------------------------------------------|
| `UpdateCheck` | `true`  | Checks once at startup whether a newer release exists. |

The check reads the latest release tag from `api.github.com`. Set it to `false` and the mod never touches
the network.

### `[Hotkeys]`

Takes any Unity KeyCode name, e.g. `F11`, `G`, `Tab`, `Keypad5`. `None` turns it off.

| Setting    | Default | Description                                              |
|------------|---------|----------------------------------------------------------|
| `BoardKey` | `F11`   | Opens the recolour board while you are at the workbench. |

## Building from source

Requires the .NET SDK. The project references the game's own assemblies, so set `GameDir` in
`ReColor.csproj` to your ReStory install path if it differs from the default.

```
dotnet build                            # Debug   - verbose per-action logging
dotnet build -c Release                 # Release - development logging compiled out
dotnet build -c Release -p:Loader=Melon # the MelonLoader build of the same source
```

