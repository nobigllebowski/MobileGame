# NATION: WORLD ORDER

*Lead a nation. Change the world.*

A premium portrait-mode mobile grand strategy game built with Unity 6 and C#.
The design foundation and phase roadmap live in [`docs/design/phase-0-foundation.md`](docs/design/phase-0-foundation.md).

## Current state: Phase 1

Project skeleton, engine-free simulation core, bootstrap, scene navigation, main menu, and a world placeholder
screen that advances the simulation one day at a time.

## Open the project

1. Install **Unity 6 LTS** (any `6000.x` release) through Unity Hub. No extra modules are needed to run in the editor.
2. In Unity Hub choose **Add → Add project from disk** and select this repository folder.
3. Open the project. If Hub reports that `6000.0.23f1` is not installed, pick your installed 6000.x editor; the upgrade is safe.
4. Wait for the first import to finish (packages resolve and scripts compile).

## Run it

1. Open `Assets/Scenes/Bootstrap.unity` (or `MainMenu.unity`; every scene bootstraps itself).
2. In the Game view aspect dropdown choose a portrait size, for example add **390x844** or pick **9:16 Portrait**.
3. Press **Play**. The main menu appears; **PLAY** starts a new game as Germany and opens the world screen.
4. Click **ADVANCE DAY** to step the simulation. The date moves from January 1, 2026 to January 2, 2026 and onward.

## Tests

**Window → General → Test Runner → EditMode → Run All.** The simulation core is engine-free, so its tests run in milliseconds.

## Layout

```
Assets/Scripts/Core   Nation.Core   engine-free models, simulation, signals, localization
Assets/Scripts/Game   Nation.Game   Unity runtime: bootstrap, session, scenes, UI controllers
Assets/Tests/EditMode Nation.Tests  NUnit tests for Nation.Core
Assets/UI             UXML layouts and USS styles
Assets/Data           JSON string tables (static data)
Assets/Resources      GameDataCatalog, the single asset the bootstrap loads
Assets/Scenes         Bootstrap, MainMenu, World
```
