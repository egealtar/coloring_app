# Design Decisions

### Which fill algorithm did you use and why?

I used an iterative scanline flood fill (stack-based) followed by a small dilation pass. The scanline approach paints whole horizontal spans per iteration and only pushes vertical neighbors onto the stack, so memory stays low and recursion is avoided — a recursive 4-way fill on 1280×1280 art would risk stack overflow and run noticeably slower. The dilation pass after the main fill expands the painted region by one or two pixels into anti-aliased edge pixels that the strict color match would otherwise skip, so the result reaches the outline cleanly without leaking past it.

### How did you keep the fill from freezing the screen?

The fill runs as a Unity coroutine that yields after a configurable batch of pixels (`PixelsPerFrame` in `FillConfig`, default 4000). Each batch ends with a single `Texture2D.Apply()` so the GPU upload also amortizes, and the user sees the fill grow progressively rather than waiting for one big stall. The threshold lives in a ScriptableObject so it can be tuned per device without code changes.

### How is level progress stored and loaded?

Per-level completion and the last selected color index are persisted through small wrapper classes (`ProgressStore`, `ColorStore`) that sit on top of `PlayerPrefs`. The wrappers keep the persistence mechanism behind a narrow API, so switching to JSON-on-disk later would not touch any caller. Each level uses a stable `UniqueId` derived from its level number as the storage key, and a Unity Editor menu item (`Tools/OtsimoColoring/Reset All Progress`) clears all saved state for QA.

### What would break if the canvas were 2048×2048 instead of 512×512?

Total pixel count grows 16× (≈4.2M pixels), which has three consequences. First, `Texture2D.GetPixels32` and the working buffer copy in `CanvasController.Initialize` cost more time and memory at level load. Second, the completion-pixel precount, which scans the whole texture once, would also take 16× longer on the main thread — at 2048² it should be moved off-thread (Job System or `Task.Run`) or done incrementally. Third, the fill itself stays smooth because of the per-frame yielding, but `PixelsPerFrame` would likely need to be raised so a full-region fill doesn't drag on for too many frames. The dilation pass is the most exposed: it currently iterates over every pixel per pass, so at 2048² it should switch to walking only the boundary of the painted mask instead of the full grid.

### Additional decisions and trade-offs

A few choices in the project went beyond the brief and are worth calling out:

**Dual-buffer canvas.** The displayed RawImage is a separate `_displayPixels` buffer that preserves the original art (including transparency), while flood fill operates on a `_sourcePixels` logic buffer in which transparent pixels have been composited onto white. This way the canvas keeps its original look — line-art with see-through background — but the fill always has a well-defined paintable surface to work on. Painted pixels are written to both buffers so the visible canvas updates and subsequent fills see the painted region correctly.

**Alpha-aware outline detection.** Outline detection isn't only brightness-based. The original source texture's alpha is captured into an `_isInkMask` boolean array; any pixel the artist drew (alpha above a small cutoff) is treated as outline regardless of its color. This catches faint anti-aliased stroke ends that a pure brightness threshold would miss, so the fill respects thin lines instead of leaking around them. Brightness remains as a fallback for line-art that ships fully opaque.

**Fill quality limits.** The fill quality still depends on the source line-art being topologically closed — pixel-level gaps in an outline let the fill seep into adjacent regions even with the alpha-aware ink mask. A production version would add a gap-closing pre-pass (small-radius morphological dilation on the ink mask, similar to MyPaint's gap-closing fill) to handle hand-drawn strokes with rough edges.

**Tunable values.** Color tolerance, outline brightness threshold, completion percentage, and pixels-per-frame all live in `FillConfig` (a ScriptableObject) rather than as constants in code. The right values vary noticeably between line-art assets — for the bundled art a tolerance of 0.1 with an outline threshold of around 0.5 worked best — and keeping them in the inspector means an artist can re-tune per level without touching code.

**Scene transitions via DOTween.** Both scenes carry a small `TransitionOverlay` whose `SceneTransition` component fades from black on Start and fades back to black before `SceneManager.LoadScene`. This kept the brief's "all animations use DOTween" rule consistent across scene swaps, not just in-scene UI, and prevented the visible pop that an instant load would otherwise produce.

**No singletons; everything is wired in the inspector.** Both scene controllers, `CanvasController`, `FloodFillService`, `PaletteView`, `CompletionDetector` and `CompletionView` reference each other through `[SerializeField]` fields. Persistence wrappers (`ProgressStore`, `ColorStore`) are constructed locally in `Awake`. The `SelectedLevel` static class is the one exception, used purely as a typed handoff between scenes; it carries no behavior or lifecycle.