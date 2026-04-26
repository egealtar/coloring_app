# Otsimo Coloring — SWE Intern Case Study

A children's coloring app prototype built in Unity 2022.3 LTS as part of the Otsimo Software Engineering Intern Study. Tap a region of a line-art image to flood fill it with the selected palette color; finish 90% of a level to trigger the celebration screen and unlock the completion mark on the main menu.

A short screen recording of the full flow is included as **`DEMO.mp4`** in the repository root.

## Demo

The included `DEMO.mp4` walks through:

- Launching into the main menu with three levels in the grid
- Tapping a level to enter the coloring scene (with a fade transition)
- Switching colors via the palette swatches (with bounce feedback)
- Filling several regions of the line-art
- Crossing the 90% completion threshold and seeing the celebration overlay
- Returning to the main menu with the completed level marked
- Entering a different level

## Features

- **Three levels** loaded from a `LevelDatabase` ScriptableObject; new levels can be added by creating a `LevelData` asset and dropping it into the database — no code changes.
- **Asynchronous scanline flood fill** with a dilation pass for clean anti-aliased edges. Outlines are checked against the original source texture so re-filling never bleeds through borders.
- **Alpha-aware outline detection** — any pixel the artist drew (alpha > 0) blocks the fill, so faint stroke ends still hold the line.
- **Six-color palette** with DOTween bounce on selection; the last selected color persists across sessions.
- **Completion detection** at a configurable threshold (default 90%) with a fade + scale-bounce celebration overlay and pulsing CTA button.
- **Per-level progress** persisted via `PlayerPrefs`, with a Unity Editor menu item (`Tools/OtsimoColoring/Reset All Progress`) for QA.
- **DOTween-driven scene transitions** (fade in/out) between MainScene and ColoringScene.
- **Visual polish**: pulsing "next-up" cell on the menu, tap punch feedback, animated diagonal completion line, and a bouncing intro for the title.

## Project Structure
Assets/_Project/
├── Art/
│   ├── Fonts/          Imported Google Font for the menu title
│   └── LineArts/       Source PNGs for each level
├── Config/             ScriptableObject assets (palette, fill, levels, database)
├── Prefabs/            LevelCell, SwatchTemplate
├── Scenes/             MainScene, ColoringScene
└── Scripts/
├── App/            SceneTransition
├── Coloring/       CanvasController, FloodFillService, PaletteView,
│                   SwatchView, CompletionDetector, CompletionView,
│                   ColoringSceneController, CanvasInputRelay
├── Config/         LevelData, LevelDatabase, PaletteConfig, FillConfig
├── Editor/         ResetProgressMenuItem, TextureAnalyzer
├── Main/           MainSceneController, LevelCell, TitleAnimator
└── Persistence/    ProgressStore, ColorStore, SelectedLevel

## Architecture Notes

- **No singletons.** Every dependency is wired via `[SerializeField]` inspector references. The one exception is `SelectedLevel`, a static handoff used purely to pass the chosen `LevelData` between scenes; it carries no behavior or lifecycle.
- **No magic numbers in gameplay logic.** Color tolerance, outline brightness threshold, completion percentage, pixels-per-frame, palette colors and animation timings all live in ScriptableObjects or `[SerializeField]` fields so artists and designers can tune them without touching code.
- **No `Lerp` in `Update`.** All animations — swatch selection, celebration overlay, scene transitions, title intro, completion line — go through DOTween.

See [DECISIONS.md](DECISIONS.md) for in-depth rationale on the fill algorithm, threading approach, persistence layer, and 2048×2048 scaling considerations.

## Running

1. Open the project in **Unity 2022.3 LTS**.
2. Open `Assets/_Project/Scenes/MainScene`.
3. Press Play. Game view is configured for **Portrait 1080×1920**.

## Adding a Level

1. Drop a new line-art PNG into `Assets/_Project/Art/LineArts/`. Set Texture Type to Default, enable **Read/Write**, set Filter Mode to **Point**, and Compression to **None**.
2. Right-click in `Assets/_Project/Config/` → **Create → OtsimoColoring → Level Data**. Set its level number, display name, and assign the line-art texture.
3. Open the `LevelDatabase` asset and add the new `LevelData` to the `Levels` list.

The MainScene will pick it up automatically on next play.