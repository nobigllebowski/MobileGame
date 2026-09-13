# NATION: WORLD ORDER

*Lead a nation. Change the world.*

A premium portrait-mode mobile grand strategy game built with Unity 6 and C#.
The design foundation and phase roadmap live in [`docs/design/phase-0-foundation.md`](docs/design/phase-0-foundation.md).

## Current state: Phases 1 to 3

- Engine-free simulation core (`Nation.Core`): world state, one-day ticks, deterministic random, signals, JSON data, localization.
- Premium mobile UI system (UI Toolkit, code-built): design tokens, screen stack with transitions, safe area, bottom sheets, modal dialogs, toasts, bottom navigation.
- Ten playable countries with hand-curated starting data, vector flags, search, filters, data-derived difficulty.
- Game flow: Main menu → Choose your nation → Country briefing → Start leadership → Game shell with NATION, ECONOMY, WORLD, BUILD and POWER tabs, live date and five simulation speeds.

## Open the project

1. Install **Unity 6 LTS** (any `6000.x` release) through Unity Hub. No extra modules are needed to run in the editor.
2. In Unity Hub choose **Add → Add project from disk** and select this repository folder.
3. Open the project. If Hub reports that `6000.0.23f1` is not installed, pick your installed 6000.x editor; the upgrade is safe.
4. Wait for the first import to finish (packages resolve and scripts compile).

## Run it

1. Open `Assets/Scenes/Bootstrap.unity` (any scene bootstraps itself; `World.unity` starts a game directly).
2. In the Game view aspect dropdown choose a portrait size, for example add **390x844**. Any size works, the UI scales to fit.
3. Press **Play**. Main menu → **PLAY** → search or filter → tap a country → **START LEADERSHIP**.
4. In the game: **WORLD** tab shows the map, HUD and speed controls (pause, slow, normal, fast, very fast). Drag the map to pan, tap a marker for a country sheet, drag the sheet up or down. **NATION** is the command center; the menu icon there returns to the main menu.

Mouse notes for the editor: scroll lists with the mouse wheel, drag the bottom sheet by its handle or header, drag the map with the left button.
To preview notches and safe areas use **Window → General → Device Simulator**.

## Tests

**Window → General → Test Runner → EditMode → Run All.** The core is engine-free, so its 56 tests run in milliseconds.

## Layout

```
Assets/Scripts/Core   Nation.Core   engine-free models, simulation, data parsers, countries, session
Assets/Scripts/Game   Nation.Game   Unity runtime: bootstrap, tick runner, scenes, UI system, screens, tabs
Assets/Tests/EditMode Nation.Tests  NUnit tests over Nation.Core and the shipped data files
Assets/UI             Theme.uss (design system) and panel settings
Assets/Data           countries.json, buildings.json, continents.json, localization tables (en, ru)
Assets/Resources      GameDataCatalog, the single asset the bootstrap loads
Assets/Scenes         Bootstrap, MainMenu, World
```

## Editing data

- Countries: `Assets/Data/Countries/countries.json`. Values are approximate; the file header documents every field and unit.
- Text: `Assets/Data/Localization/<locale>.json`. Every visible string is a key; missing keys render as `[key]`.
- Projects: `Assets/Data/Buildings/buildings.json`.
