# Baytak Custom Toolbar

Custom main toolbar elements for Unity 6.3+ using the official `MainToolbarElement` API.

Features:

- **Scene Selector Dropdown**

  - Lists all scenes under `Assets/Scenes`.
  - Scenes directly under `Assets/Scenes` appear at the top level.
  - Scenes in subfolders appear in nested menus (e.g. `Chapter1/Intro`, `Chapter2/Boss/Room1`).
  - All other scenes under `Assets` appear in an `Other/` submenu.
- **Time Scale Slider**

  - Controls `Time.timeScale` from 0 to 4.
  - Useful for slow motion / fast forward testing.
- **Compile Scripts Button**

  - Calls `CompilationPipeline.RequestScriptCompilation()`.
  - Forces a script compilation when Unity does not automatically recompile.

## Requirements

- Unity **6000.3** or newer (Unity 6.3+).

## Installation (via Git URL)

1. Open **Unity**.
2. Go to **Window > Package Manager**.
3. Click the **+** button → **Add package from git URL...**
4. Paste:

   ```text
   https://github.com/FurkanBaytak/unity-baytak-custom-main-toolbar.git#main
   ```
5. Click **Add**.

## Enabling the toolbar elements

1. After installation, go to the **main toolbar** in Unity (top bar).
2. Right-click on an empty area of the toolbar (or use the "More" / customize menu).
3. Look for the **"Custom Toolbar"** group.
4. Enable:
   - `Scene Selector`
   - `Time Scale`
   - `Compile Scripts`

You can move them around using the toolbar customization tools in Unity 6.3 (drag in Edit mode).

## Folder conventions

- Scenes under `Assets/Scenes`:
  - `Assets/Scenes/MyScene.unity` → menu: `MyScene`
  - `Assets/Scenes/Chapter1/Intro.unity` → menu: `Chapter1/Intro`
  - `Assets/Scenes/Chapter1/Boss/BossRoom.unity` → menu: `Chapter1/Boss/BossRoom`
- Scenes outside `Assets/Scenes`:
  - `Assets/OtherScenes/TestScene.unity` → menu: `Other/TestScene`