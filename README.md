# NATION: WORLD ORDER

*Lead a nation. Change the world.*

A premium portrait-mode mobile grand strategy game built with Unity 6 and C#.
The design foundation and phase roadmap live in [`docs/design/phase-0-foundation.md`](docs/design/phase-0-foundation.md).

## Current state: Phases 1 to 4

- Engine-free simulation core (`Nation.Core`): world state, one-day ticks, deterministic random, signals, JSON data, localization.
- Premium mobile UI system (UI Toolkit, code-built): design tokens, screen stack with transitions, safe area, bottom sheets, modal dialogs, toasts, bottom navigation.
- Ten playable countries with hand-curated starting data, vector flags, search, filters, data-derived difficulty.
- Game flow: Main menu → Choose your nation → Country briefing → Start leadership → Game shell with NATION, ECONOMY, WORLD, BUILD and POWER tabs, live date and five simulation speeds.
- Real world map from Natural Earth (242 territories, three zoom bands), pan, pinch zoom, double tap, tap selection with camera focus, POLITICAL and ECONOMY layers (population, stability, influence ready; resources and diplomacy marked upcoming), capital markers, adaptive labels, and a real-geography menu globe with quality tiers.

## Open the project

1. Install **Unity 6 LTS** (any `6000.x` release) through Unity Hub. No extra modules are needed to run in the editor.
2. In Unity Hub choose **Add → Add project from disk** and select this repository folder.
3. Open the project. If Hub reports that `6000.0.23f1` is not installed, pick your installed 6000.x editor; the upgrade is safe.
4. Wait for the first import to finish (packages resolve and scripts compile).

## Run it

1. Open `Assets/Scenes/Bootstrap.unity` (any scene bootstraps itself; `World.unity` starts a game directly).
2. In the Game view aspect dropdown choose a portrait size, for example add **390x844**. Any size works, the UI scales to fit.
3. Press **Play**. Main menu → **PLAY** → search or filter → tap a country → **START LEADERSHIP**.
4. In the game: **WORLD** tab shows the real map, HUD and speed controls (pause, slow, normal, fast, very fast). Drag to pan, pinch (or mouse wheel) to zoom, tap a country to select it and open its sheet, double tap to zoom in, use the pin button to return home. POLITICAL / ECONOMY chips switch the layer; LAYERS lists every layer with its availability. **NATION** is the command center; the menu icon there returns to the main menu.
5. Press **F3** in the editor or a development build to toggle the performance overlay (frame time, tick time, map vertices, labels).

Mouse notes for the editor: scroll lists with the mouse wheel, drag the bottom sheet by its handle or header, drag the map with the left button, zoom with the wheel.
To preview notches and safe areas use **Window → General → Device Simulator**.

## Tests

**Window → General → Test Runner → EditMode → Run All.** The core is engine-free, so its 80 tests run in milliseconds.

## Map data pipeline

Source files (Natural Earth, public domain) live in `Tools/MapSource`. Regenerate the map with **Nation → Map → Import Natural Earth** in the editor, or `Tools/MapImport/run.sh` from a shell with Mono. Both run the same engine-free `MapBuildPipeline` and write `Assets/Data/Map/world.map.bytes`, the generated `map-names.<locale>.json` tables and `import-report.txt`. **Nation → Map → Validate Map Data** re-checks the shipped catalog against `countries.json`. See `Tools/MapSource/README.md` for licensing, identifiers and polygon conversion.

## Layout

```
Assets/Scripts/Core   Nation.Core   engine-free models, simulation, data parsers, countries, session
Assets/Scripts/Game   Nation.Game   Unity runtime: bootstrap, tick runner, scenes, UI system, screens, tabs, map, globe
Assets/Scripts/Editor Nation.Editor Natural Earth importer and map validation menu
Assets/Tests/EditMode Nation.Tests  NUnit tests over Nation.Core and the shipped data files
Assets/UI             Theme.uss (design system) and panel settings
Assets/Data           countries.json, buildings.json, Map/world.map.bytes, localization tables (curated en, ru; generated names in 8 locales)
Assets/Resources/Shaders  map and globe shaders (built-in pipeline, vertex-color unlit)
Tools/MapSource       Natural Earth GeoJSON sources and license notes
Tools/MapImport       command-line map importer
Assets/Resources      GameDataCatalog, the single asset the bootstrap loads
Assets/Scenes         Bootstrap, MainMenu, World
```

## Editing data

- Countries: `Assets/Data/Countries/countries.json`. Values are approximate; the file header documents every field and unit.
- Text: `Assets/Data/Localization/<locale>.json`. Every visible string is a key; missing keys render as `[key]`.
- Projects: `Assets/Data/Buildings/buildings.json`.
