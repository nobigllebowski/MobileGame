# NATION: WORLD ORDER — Phase 0 Foundation and Phase 1 Plan

Status: awaiting owner confirmation before Phase 1 implementation begins.

---

## 1. Game Name Evaluation

Candidate: **NATION: WORLD ORDER** — tagline "LEAD A NATION. CHANGE THE WORLD."

| Criterion | Assessment |
|---|---|
| App Store discoverability | "Nation" is a crowded keyword (Conflict of Nations, Rise of Nations, Nation Simulator, Age of Civilizations). The colon format gives two keyword lanes, which helps ranking. |
| Memorability | Medium. Two-part titles are harder to recall than one word. The tagline is strong and carries the fantasy. |
| Commercial appeal | Good. Reads as serious geopolitics, not casual. Fits the premium visual direction. |
| Genre clarity | Excellent. Nobody will mistake it for a puzzle or idle game. |
| Trademark risk | Medium. "World Order" is a common phrase used in other titles. "New World Order" carries conspiracy-theory baggage in search results. "Nation" alone is unprotectable. |

Alternatives considered:

1. **STATECRAFT** — the most accurate one-word description of the gameplay. Already used by an educational political simulation and a card game. Medium-high collision.
2. **SOVEREIGN** — premium, distinctive, looks trademarkable. Weak genre signal in store search. Used by fintech products and an older MMO.
3. **HEAD OF STATE** — instant role fantasy. Generic phrase, low distinctiveness, film association.
4. **WORLD ORDER** — cleaner, but loses the "nation" keyword lane and worsens the political baggage.

**Decision: keep NATION: WORLD ORDER as the working title.** It has the best combination of genre clarity, role fantasy, and tagline. Each alternative trades away genre clarity or carries a harder collision.

Engineering consequence: the code namespace root is `Nation`, the product name is `Nation`, and the bundle id is `com.<studio>.nation`. All three survive a later change of the subtitle. Before store submission, run a trademark search in classes 9 and 41 (USPTO, EUIPO) and use Google Play store-listing experiments on the title. Not now.

---

## 2. One-Page Game Vision

NATION: WORLD ORDER is a portrait-mode mobile grand strategy game. You take charge of a real country in the present day and steer it into an alternate future. The world starts from real-world-inspired data and then diverges based on what you and the AI-led nations do.

**Player fantasy:** "I run a real country from my phone, and the world responds to me."

**What you actually do:** You read the state of your nation. You make a handful of high-leverage decisions: adjust taxes, reallocate the budget, start a project, sign a trade deal, improve a relationship. You let time run and watch the consequences unfold in the economy, the population, and the news feed. Then you decide again.

**Five pillars:**

1. **A real nation.** Starting data grounded in reality. Familiar names, believable numbers, plausible problems. Germany really does have an energy dependency.
2. **A living world.** AI nations build, trade, negotiate, and compete visibly. Every news item is generated from something that actually happened in the simulation.
3. **Decisions with tradeoffs.** No free lunches. Every lever pulls something else. The interface always answers three questions: what changed, why, and what can I do.
4. **Premium mobile.** Command-center aesthetic, one-handed portrait play, bottom sheets and cards instead of PC menus.
5. **Short sessions, long arcs.** Two to ten minute sessions. Construction, research, and diplomacy arcs span many sessions and give a reason to come back.

**Differentiation:** existing mobile "nation" games are war-first or idle-first. This game is economy-and-diplomacy-first, uses real countries, looks expensive, and is built so those same countries can be player-controlled in shared asynchronous worlds later.

**Session shape:** open the app, see what changed while away (news, completed projects), make one to three decisions, advance time, close. First session: a meaningful decision within 60 seconds.

**Commercial model:** free-to-play. Cosmetics, premium scenarios, an optional subscription, optional rewarded ads. No pay-to-win, no forced ads, no timers that block play.

---

## 3. Core Game Loop

```
        ┌────────────────────────────────────────────────────────────┐
        │                                                            │
        ▼                                                            │
   OBSERVE      HUD, dashboard, news feed, map                       │
        │                                                            │
        ▼                                                            │
   DECIDE       tax · budget · build · trade · diplomacy             │
        │       (every decision is a Command)                        │
        ▼                                                            │
   ADVANCE TIME play at a chosen speed; one tick = one game day      │
        │                                                            │
        ▼                                                            │
   SIMULATE     population → economy → budget → energy → … → AI      │
        │                                                            │
        ▼                                                            │
   CONSEQUENCES metrics move, projects progress, events fire         │
        │                                                            │
        ▼                                                            │
   NEWS         generated from what actually happened ───────────────┘
```

Nested loops:

- **Micro (seconds):** read a number, pull a lever, see it move.
- **Session (minutes):** make decisions, advance a month, answer an event card.
- **Arc (days of real time):** a 120-day power plant, a research track, an alliance built from +20 to +80.
- **Meta (later):** scenarios, achievements, seasons.

Time model: one simulation tick equals one game day. Speeds: PAUSED, SLOW (one day per 2 s), NORMAL (one day per second), FAST (three days per second), VERY FAST (ten days per second). A year at VERY FAST takes about 36 seconds. Nothing in the simulation runs per frame.

---

## 4. Mobile UX Plan

### UI technology decision: UI Toolkit (runtime)

Why:
- USS theming means sellable UI themes without duplicating prefabs.
- UXML and USS map closely to XAML and CSS, which suits a .NET developer.
- Retained-mode rendering. Only dirty elements repaint. Panels of numbers and lists are its best case.
- Vector charts are drawn with `generateVisualContent` and `Painter2D`, no chart package needed.

Costs accepted: safe area is applied manually as root padding (one method), there is no world-space UI (not needed), and there are fewer asset-store widgets (we build our own for a custom look anyway).

The world map is a normal URP scene rendered underneath a transparent UI Toolkit panel.

### Main Menu

Portrait. A full-screen, slowly rotating Earth: textured sphere, cloud layer, night lights blended by a day/night term. Quality tiers drop clouds and lights on low-end devices. Title lockup in the upper third. Five stacked buttons with PLAY as the primary. MULTIPLAYER, SCENARIOS, PROFILE, and SETTINGS open a polished COMING SOON card in MVP. A CONTINUE button appears above PLAY once a save exists.

### Country Selection

One screen with a MAP / LIST segmented control. Search bar on top. Filter chips: Region, Population, Economy, Technology, Difficulty, Influence. Tapping a country opens the Country Preview card: flag, name, four stat rows, Strengths, Challenges, and a START LEADERSHIP button pinned to the bottom inside the safe area.

### World Map

A 2D projected map, not a 3D globe, for gameplay. The globe lives only in the main menu. Reason: pan, zoom bands, layer coloring, and tap selection are all simpler and cheaper on a flat map, and portrait phones show more useful map area that way.

- Country polygons from Natural Earth (public domain) are converted into one mesh per country in an editor import step.
- Pan is one-finger drag. Zoom is pinch. Double-tap zooms to a country. A home button recenters on the player's country.
- Three zoom bands: FAR (colored fills only), MID (borders, names, layer data), NEAR (country detail, capitals). Nothing is rendered outside its band.
- Layer switcher is a small pill at the top left. POLITICAL and ECONOMY in MVP; the architecture holds all ten.
- Selection uses a highlight and a subtle glow.

### Bottom Navigation

Five tabs: NATION, ECONOMY, BUILD, WORLD, POWER. Icon plus label. Active state is the accent color with an indicator bar. WORLD is the map and the default tab. The other four open as full-height panels sliding over the map. The map keeps running underneath.

### Bottom Sheets

Tap a country on the map and a sheet snaps to peek height (about 35 percent) showing flag, name, four key numbers, and four actions: VIEW COUNTRY, DIPLOMACY, TRADE, COMPARE. Drag up for the expanded state (about 85 percent). Drag down or tap the map to dismiss. Spring animation, drag-follow, velocity-based snapping. One bottom sheet component is reused for build details, event cards, and trade offers.

### Main HUD

A top bar of two rows that respects the notch and Dynamic Island.

- Row 1: flag and country name on the left, date on the right.
- Row 2: three stat chips (Money, GDP, Population) and the speed control (⏸ ▶ ▶▶ ▶▶▶) on the right.

Tapping a chip jumps to its panel. Nothing else lives in the HUD. Warnings such as an energy deficit appear as a single badge on the NATION tab.

### Responsive rules

Panel Settings use scale-with-screen-size on a 390 by 844 reference. Safe-area padding is applied on the root element. Tablets get wider max-width columns, not a different layout. Minimum tap target is 48 by 48 dp; primary buttons are 56 dp tall.

---

## 5. Core Architecture

### Foundational decision: the simulation is engine-free C#

The `Nation.Core` assembly has no reference to UnityEngine. It holds models, state, simulation systems, commands, and signals. Consequences:

- EditMode tests of the simulation run in milliseconds with no scene.
- The same assembly runs on a .NET server later, which is what server-authoritative multiplayer requires.
- UI code cannot leak into the simulation, because the compiler forbids the reference direction.

### Static data

Immutable definitions versioned with the app: `CountryDefinition`, `BuildingDefinition`, `TechnologyDefinition`, `EventDefinition`, string tables.

- Stored as JSON `TextAsset` files under `Assets/Data`, referenced by a `GameDataCatalog` ScriptableObject. Loading is synchronous and works on every platform. StreamingAssets is deliberately avoided: on Android it sits inside the APK and needs asynchronous `UnityWebRequest` reads. Addressables come later only if content size demands it.
- Loaded through providers: `ICountryDataProvider`, `IMapDataProvider`, `IWorldDataProvider`. Static JSON providers in MVP. A downloaded-data provider later, behind the same interface. Core gameplay never touches a network API.
- ScriptableObjects are used only for Unity-facing presentation config: quality tiers, map palette, theme.

### Dynamic state

`WorldState` is the single root aggregate: seed, clock, player country id, the list of `CountryState`, and, in later phases, projects, agreements, relationships, active events, and the news log.

Plain serializable classes with no Unity types. The serialized `WorldState` is the save file and, later, the network snapshot. One object, three uses.

### Simulation

- `SimulationEngine.Tick(WorldState)` advances the clock one day and runs an ordered list of `ISimulationSystem`. The order lives in one place and is documented there.
- `TickContext` carries the current date, a deterministic random generator (our own xorshift, identical on Mono, IL2CPP, and .NET), and a signal sink.
- Systems never touch the UI. Expensive systems run on their own cadence inside the tick: AI evaluates weekly, events are checked monthly.
- The engine measures its own tick duration with a `Stopwatch`, which feeds the performance overlay in Phase 17.

### Commands: the mutation seam

Every player and AI intent is an `ICommand`: `SetTaxRateCommand`, `StartProjectCommand`, `ProposeTradeCommand`. `CommandProcessor.Execute` validates the command against the state, applies it, and returns a `CommandResult` carrying a localization key when refused.

The UI never mutates `WorldState`. This is exactly what the server validates later. Introduced in Phase 6, when the first real command exists.

### Signals versus Events

Two words for two things, to avoid a naming collision:

- **Signals** are in-memory notifications published by the simulation: `TickCompleted`, `ProjectCompleted`, `RelationChanged`. News and UI consume them. `SignalBus` is typed and allocates nothing per publish.
- **Events** are player-facing content: an `EventDefinition` such as "Energy Shortage" with choices and consequences. They are data, not messages.

### Services (Unity side, `Nation.Game`)

- `GameBootstrap` is the composition root. Manual constructor injection through a `GameContext` object. No DI framework, because the object graph is small and the developer knows constructor injection.
- `GameSession` owns one `WorldState`, one `SimulationEngine`, and one `CommandProcessor`.
- `TimeService` converts real time and the chosen speed into tick calls. Nothing simulates per frame. It pauses itself when the app is backgrounded.
- `SaveService`, `LocalizationService`, `MapService`, `SceneNavigator`, `UIService`.
- Interfaces exist only where substitution is real: data providers, save storage, world session, localization.

### UI (`Nation.UI`)

UI Toolkit. One controller class per screen or panel. Controllers read state through read-only views and send commands. They refresh on `TickCompleted` and on specific signals only, never by polling. `UIService` manages the screen stack and the bottom sheet.

### Save

`SaveService` writes a `SaveFile` containing `SaveMetadata` and `WorldState` as JSON through Newtonsoft (the official `com.unity.nuget.newtonsoft-json` package). `JsonUtility` is not used because it cannot serialize dictionaries or polymorphic lists. `ISaveStorage` is a local file now and cloud or platform storage later. A `saveVersion` field and a migration hook exist from the first save. Autosave triggers on app pause and every N game months.

### Future multiplayer

`IWorldSession` has two implementations. `LocalWorldSession` (MVP) sends commands straight to the local processor. `RemoteWorldSession` (later) sends commands to a server that runs the same `Nation.Core` simulation and returns snapshots. Rankings, ownership, and seasons are server services that never exist as truth on the client. Nothing in the UI changes between the two.

### Technical decisions summary

| Decision | Choice | Why |
|---|---|---|
| Engine version | Unity 6 LTS, pinned exact patch in `ProjectVersion.txt` | Current LTS, URP and UI Toolkit mature |
| Render pipeline | URP | Mobile performance, Shader Graph for the Earth and map |
| UI | UI Toolkit runtime | Theming, data-heavy panels, .NET-friendly |
| Input | Unity Input System, EnhancedTouch | Pinch and drag gestures, official |
| JSON | Newtonsoft (official Unity package) | Dictionaries, polymorphism, server parity |
| DI | Manual composition root | Small graph, no extra dependency |
| Simulation | Engine-free assembly, tick = one day | Tests, server reuse, battery |
| Map | 2D projected map, Natural Earth meshes | Performance, selection, portrait fit |
| Static data | JSON TextAssets via catalog ScriptableObject | Synchronous, Android-safe, server-shareable |
| Random | Own xorshift generator | Deterministic across runtimes |
| Localization | Own key and table service backed by JSON | Light, testable, engine-free |

---

## 6. System Diagram

```
┌────────────────────────────────────────────────────────────────────┐
│  UI  (Nation.UI · UI Toolkit)                                      │
│  Screens · Panels · Bottom Sheets · HUD · Map View                 │
└───────────────┬────────────────────────────────────────▲───────────┘
                │ intents (tap, drag, slider)            │ read-only views
                ▼                                        │ + signals
┌────────────────────────────────┐        ┌──────────────┴───────────┐
│  GAME COMMANDS  (Nation.Core)  │        │  SIGNAL BUS (Nation.Core)│
│  SetTaxRate · StartProject ·   │        │  TickCompleted ·         │
│  ProposeTrade · Diplomacy …    │        │  ProjectCompleted ·      │
└───────────────┬────────────────┘        │  RelationChanged · News  │
                │ validate + apply         └──────────────▲───────────┘
                ▼                                        │ publish
┌────────────────────────────────────────────────────────┴───────────┐
│  SERVICES  (Nation.Game · Unity side)                              │
│  GameSession · TimeService · SaveService · Localization ·          │
│  MapService · SceneNavigator · Data Providers                      │
└───────────────┬────────────────────────────────────────────────────┘
                │ Tick() on schedule, never per frame
                ▼
┌────────────────────────────────────────────────────────────────────┐
│  SIMULATION  (Nation.Core · engine-free)                           │
│  Population → Economy → Budget → Energy → Resources →              │
│  Construction → Trade → Technology → Diplomacy → Events → AI → News│
└───────────────┬────────────────────────────────────────────────────┘
                │ mutates
                ▼
┌────────────────────────────────────────────────────────────────────┐
│  WORLD STATE  (Nation.Core · one serializable root)                │
│  Clock · Countries · Projects · Agreements · Relations · Events    │
│  = save file = future network snapshot                             │
└───────────────┬────────────────────────────────────────────────────┘
                │ what changed
                ▼
   SIGNALS  ──►  NEWS SYSTEM  ──►  UI refresh (event-driven only)
```

Future multiplayer inserts one hop and changes nothing else:

```
MOBILE CLIENT                      SERVER (.NET, same Nation.Core)
UI → Commands → RemoteWorldSession ──► CommandProcessor → Simulation → WorldState
UI ◄─ snapshot / delta ◄────────────── state publisher
```

---

## 7. MVP Scope

### Included

1. Cinematic main menu with the rotating Earth and quality tiers.
2. New game flow: menu → country selection → preview → world.
3. Country selection by list, search, filters, and map tap.
4. Interactive 2D world map: pan, zoom, select, home button, POLITICAL and ECONOMY layers, three zoom bands.
5. All country shapes drawn on the map so the planet looks complete. Ten countries are simulated and selectable: Germany, United States, United Kingdom, France, Japan, China, India, Brazil, Turkey, Australia. The others are dimmed and show a "not yet available" toast. The data format supports all countries from day one.
6. Country dashboard (NATION tab) with the six primary metrics and secondary metrics revealed on expand.
7. Portrait UI with safe area, bottom navigation, bottom sheets, cards.
8. Time system with five speeds and pause.
9. Economy: eight aggregated sectors, GDP model, budget with ten categories, three taxes, charts.
10. Population groups, happiness, stability.
11. Energy as abstract production and consumption. Four resources: Oil, Gas, Food, Iron.
12. Construction: about eight building types with cost, duration, maintenance, effects, and status.
13. Trade: import and export of Energy, Food, Raw Materials, and Industrial Goods; agreements; tariffs.
14. Diplomacy: relationship scale, five actions (Improve Relations, Trade Agreement, Economic Partnership, Aid, Sanctions).
15. AI for the ten countries with profiles, priorities, and scheduled decisions through the same command system.
16. About ten event definitions with choices. Global news feed generated from signals.
17. Save: three manual slots plus autosave, metadata, continue from menu.
18. Localization architecture with 100 percent key coverage. English complete. One additional language partially translated to prove the pipeline.
19. Performance overlay (frame time, tick time, memory) and profiling on two real devices.

### Excluded from MVP

- Multiplayer, alliances, seasons, rankings, accounts, cloud save.
- Technology tree (Phase 18). Technology exists in MVP only as a static tier that affects productivity.
- Military and conflict. The POWER tab shows Influence and a COMING SOON state for the rest.
- Regions, cities, and the other eight map layers.
- In-app purchases, ads, cosmetics, subscription.
- Audio.
- Achievements beyond the architecture hook.
- Tablet-specific layouts. Tablets scale the phone layout.
- Landscape orientation.
- Live data APIs.

### MVP acceptance test

Launch, select Germany, enter the world, open the dashboard, see the economy, change a tax, build an energy project, advance time, see construction progress, trade with another country, improve a relationship, observe AI countries acting, read generated news, save, close, load, continue. If all of this works on a mid-range Android phone and an iPhone, the MVP is done.

---

## 8. Unity Project Structure

The owner's suggested list is a domain list. The top level is grouped by assembly instead, because the engine-free simulation boundary must be a compiler-enforced folder boundary. The domain folders live inside `Core`. Folders are created only when their phase arrives.

```
MobileGame/
├── .gitignore
├── docs/
│   └── design/
├── Assets/
│   ├── Art/                       (Phase 4: Earth textures, map palette, icons)
│   ├── Data/
│   │   ├── GameDataCatalog.asset
│   │   ├── Localization/          (en.json, de.json, …)
│   │   ├── Countries/             (Phase 3)
│   │   ├── Buildings/             (Phase 10)
│   │   └── Events/                (Phase 14)
│   ├── Materials/                 (Phase 4)
│   ├── Prefabs/                   (Phase 4)
│   ├── Scenes/
│   │   ├── Bootstrap.unity
│   │   ├── MainMenu.unity
│   │   └── World.unity
│   ├── ScriptableObjects/         (Phase 2: theme, quality tiers)
│   ├── Scripts/
│   │   ├── Core/                  Nation.Core.asmdef  (no engine references)
│   │   │   ├── Models/
│   │   │   ├── Simulation/
│   │   │   ├── Signals/
│   │   │   ├── Localization/
│   │   │   ├── World/
│   │   │   ├── Utilities/
│   │   │   ├── Commands/          (Phase 6)
│   │   │   ├── Countries/         (Phase 3)
│   │   │   ├── Economy/           (Phase 7)
│   │   │   ├── Population/        (Phase 8)
│   │   │   ├── Energy/            (Phase 9)
│   │   │   ├── Construction/      (Phase 10)
│   │   │   ├── Trade/             (Phase 11)
│   │   │   ├── Diplomacy/         (Phase 12)
│   │   │   ├── AI/                (Phase 13)
│   │   │   ├── Events/            (Phase 14)
│   │   │   ├── News/              (Phase 14)
│   │   │   ├── Technology/        (Phase 18)
│   │   │   └── Power/             (Phase 20)
│   │   ├── Game/                  Nation.Game.asmdef  (Unity runtime glue)
│   │   │   ├── Bootstrap/
│   │   │   ├── Config/
│   │   │   ├── Scenes/
│   │   │   ├── Session/
│   │   │   ├── Localization/
│   │   │   ├── UI/                (Phase 1 placeholders only; moves to Nation.UI in Phase 2)
│   │   │   ├── Data/              (Phase 3: JSON providers)
│   │   │   ├── Map/               (Phase 4)
│   │   │   ├── Time/              (Phase 6)
│   │   │   └── Save/              (Phase 15)
│   │   ├── UI/                    Nation.UI.asmdef    (Phase 2)
│   │   └── Editor/                Nation.Editor.asmdef (Phase 4: map import tool)
│   ├── UI/
│   │   ├── NationPanelSettings.asset
│   │   ├── Base.uss
│   │   └── *.uxml
│   └── Tests/
│       └── EditMode/              Nation.Tests.EditMode.asmdef
├── Packages/
│   └── manifest.json
└── ProjectSettings/
```

Assembly dependency direction, enforced by asmdef references:

```
Nation.Tests.EditMode ─► Nation.Core
Nation.UI ─► Nation.Game ─► Nation.Core
Nation.Editor ─► Nation.Game
```

Nothing references `Nation.UI`. `Nation.Core` references nothing.

---

## 9. Phase Roadmap

Every phase ends with a build that opens without console errors and passes its test steps.

| Phase | Deliverable | Testable result |
|---|---|---|
| 0 | Architecture, design foundation, technical decisions, MVP plan | This document |
| 1 | Unity project, assemblies, core state, simulation engine skeleton, bootstrap, scene navigation | Menu → world placeholder, advance a day, EditMode tests green |
| 2 | UI foundation: theme tokens, safe area, responsive root, screen stack, bottom sheet, bottom nav shell, transitions, COMING SOON cards | All five tabs and the sheet work with placeholder content on phone and tablet |
| 3 | Static country data, ten countries, `ICountryDataProvider`, dynamic state creation, country list, search, filters, preview | Choose Germany, read its preview, start leadership |
| 4 | World map MVP: Natural Earth import tool, meshes, camera pan and zoom, selection, POLITICAL and ECONOMY layers, zoom bands, menu Earth globe | Pan and zoom at 60 fps on a mid-range phone, tap Germany |
| 5 | Main gameplay screen: HUD, bottom nav wired to panels, country bottom sheet, NATION dashboard | Full screen composition works end to end |
| 6 | Time: `TimeService`, speeds, pause, background pause, `CommandProcessor` | Speed control advances the date; app pause stops ticks |
| 7 | Economy: sectors, GDP model, budget categories, revenue and expenses, taxes, ECONOMY panel with charts | Change a tax, see revenue and happiness react over months |
| 8 | Population groups, happiness, stability | Metrics move plausibly over a year |
| 9 | Energy balance, four resources, effects on economy and happiness | Energy deficit shows and hurts |
| 10 | Construction: building definitions, projects, progress, maintenance, effects, BUILD panel | Build a solar plant, watch 120 days, see +energy |
| 11 | Trade: goods, agreements, tariffs, prices, effects | Import gas, see budget and energy change |
| 12 | Diplomacy: relationships, actions with cost, duration, consequences | Improve relations with France from +40 to +60 |
| 13 | AI: profiles, priorities, weekly scheduled decisions through commands | AI countries build and trade visibly |
| 14 | Events and news: definitions, triggers, choices, cooldowns, news generation from signals | Event card appears, choice has consequences, feed fills |
| 15 | Save: slots, autosave, metadata, migration, continue | Save, kill app, load, continue |
| 16 | Localization completion: locale switching, numbers, dates, currency, plurals, RTL, localized country names, second language | Switch language at runtime, nothing untranslated |
| 17 | Testing, performance overlay, profiling, quality tiers, Android and iOS builds, device matrix | MVP acceptance test passes on device |
| 18 | Technology tree | Research provides real benefits |
| 19 | Advanced map: regions, cities, remaining layers | All ten layers |
| 20 | Strategic power: military budget, power, readiness, defense, POWER panel | Abstract, no battles |
| 20b | Monetization: IAP, cosmetics, optional subscription, rewarded ads | Store-compliant, no gameplay power |
| 21 | Multiplayer architecture: .NET server project reusing `Nation.Core`, command protocol, auth abstraction, `RemoteWorldSession` | Server ticks the same world |
| 22 | Multiplayer MVP: two players, two countries, shared async world | Two phones, one world |
| 23 | PvP: trade, diplomacy, rankings | Async proposals, accept, reject, counter |
| 24 | Alliances | Create, join, alliance power |
| 25 | Seasons | 30-day season with cosmetic rewards |

Phase 20b is inserted deliberately: monetization is built only after the game is fun and before multiplayer, which needs it to pay for servers.

---

## 10. Phase 1 Plan

### Goals

1. A Unity 6 LTS project that opens without errors and is configured for portrait mobile from the first commit.
2. Assembly boundaries in place: `Nation.Core` compiles with no engine references.
3. The core state skeleton: `WorldState`, `CountryState`, `GameDate`, the simulation engine, the signal bus, the deterministic random generator.
4. Bootstrap → Main Menu → World placeholder navigation.
5. Every visible string goes through a localization key from day one.
6. EditMode tests green.
7. Optional but recommended: one Android build of the placeholder to validate the toolchain early.

### Unity concepts used in this phase

- **Assembly definition (asmdef):** a file that turns a folder into its own compiled assembly with explicit references. This is how the engine-free boundary is enforced.
- **ScriptableObject:** a data asset that lives in the project, editable in the Inspector. Used here for the `GameDataCatalog`.
- **UIDocument and PanelSettings:** the UI Toolkit runtime host component and its shared display settings (scale mode, reference resolution).
- **Bootstrap scene:** a first scene containing only the composition root. It creates services once, marks them persistent, and loads the menu.

### Project setup steps (performed in Unity, not written as files)

1. Create the project with the Universal 3D template on Unity 6 LTS. Add Android and iOS build support modules.
2. Packages: Input System, Newtonsoft JSON, Test Framework. URP comes with the template. UI Toolkit is built in.
3. Player settings: product name `Nation`, bundle id, portrait only, IL2CPP, ARM64, Android minimum API 26, iOS minimum 15, Active Input Handling set to Input System Package.
4. Scenes in Build Settings in order: Bootstrap, MainMenu, World.
5. Editor settings: force text serialization, visible meta files (defaults).
6. Add a Unity `.gitignore` at the repository root.

### Files

Assembly definitions:

| File | Responsibility |
|---|---|
| `Assets/Scripts/Core/Nation.Core.asmdef` | Engine-free assembly, `noEngineReferences: true` |
| `Assets/Scripts/Game/Nation.Game.asmdef` | Unity runtime assembly, references Core and Input System |
| `Assets/Tests/EditMode/Nation.Tests.EditMode.asmdef` | Editor-only test assembly, references Core and NUnit |

Core (`Nation.Core`):

| File | Responsibility |
|---|---|
| `Core/Models/GameDate.cs` | Immutable struct storing a day number from 1 Jan 2026; year, month, day via `System.DateTime`; `AddDays`; ISO string for logs. Display formatting is left to localization. |
| `Core/Models/GameSpeed.cs` | Enum: Paused, Slow, Normal, Fast, VeryFast. Session state, not world state. |
| `Core/Models/CountryState.cs` | Dynamic state of one country: ISO3 id and the six primary metrics (Treasury, GDP, Population, Happiness, Energy production and consumption, Influence). Later phases add sub-objects. |
| `Core/Models/WorldState.cs` | Root aggregate: seed, current date, tick count, player country id, country list, id lookup rebuilt after load. |
| `Core/Simulation/ISimulationSystem.cs` | Contract: name plus `Tick(WorldState, TickContext)`. |
| `Core/Simulation/TickContext.cs` | Per-tick data: date, random generator, signal publisher. |
| `Core/Simulation/SimulationEngine.cs` | Advances the clock, runs systems in order, measures tick duration, publishes `TickCompleted`. |
| `Core/Signals/ISignal.cs` | Marker interface for bus messages. |
| `Core/Signals/TickCompletedSignal.cs` | First signal: date and tick count. |
| `Core/Signals/SignalBus.cs` | Typed publish and subscribe with unsubscribe tokens, no allocation on publish. |
| `Core/Utilities/DeterministicRandom.cs` | Seeded xorshift generator: `NextInt`, `NextDouble`, `Chance`. Same output on every runtime. |
| `Core/Localization/ILocalizationService.cs` | `Get(key)`, `Get(key, args)`, current locale. |
| `Core/Localization/StringTableLocalizationService.cs` | Dictionary-backed tables per locale, fallback to English, missing key renders as `[key]` so gaps are visible. |
| `Core/World/WorldFactory.cs` | Builds a fresh `WorldState` from seed, initial countries, and player id. Phase 3 feeds it from the data provider. |

Game (`Nation.Game`):

| File | Responsibility |
|---|---|
| `Game/Bootstrap/GameBootstrap.cs` | MonoBehaviour in the Bootstrap scene. Sets target frame rate and orientation, builds `GameContext`, persists it, loads MainMenu. |
| `Game/Bootstrap/GameContext.cs` | Composition root output: holds the signal bus, localization, scene navigator, and the current session. One documented static accessor. |
| `Game/Config/GameDataCatalog.cs` | ScriptableObject listing data `TextAsset` references. Phase 1: localization tables only. |
| `Game/Localization/LocalizationTableLoader.cs` | Parses a locale JSON `TextAsset` into the Core service. |
| `Game/Scenes/SceneNavigator.cs` | Scene name constants and asynchronous single-scene loading. |
| `Game/Session/GameSession.cs` | Owns `WorldState`, `SimulationEngine`, and the bus. `StartNewGame` and `TickOnce`. Contains a small placeholder country seed that Phase 3 deletes. |
| `Game/UI/MainMenuController.cs` | Binds the menu UXML: PLAY starts a session and loads World, the other buttons show a COMING SOON label. All text from keys. |
| `Game/UI/WorldPlaceholderController.cs` | Shows date, country id, treasury and an ADVANCE DAY button. Refreshes on `TickCompleted`. Replaced by the real HUD in Phase 5. |

UI and data assets:

| File | Responsibility |
|---|---|
| `Assets/UI/NationPanelSettings.asset` | Panel Settings: scale with screen size, 390 by 844 reference. Created in Unity. |
| `Assets/UI/Base.uss` | Minimal dark palette and 56 dp buttons. Phase 2 replaces it with the theme system. |
| `Assets/UI/MainMenu.uxml` | Five buttons and a COMING SOON label. |
| `Assets/UI/WorldPlaceholder.uxml` | Three labels and one button. |
| `Assets/Data/Localization/en.json` | Keys: `menu.play`, `menu.multiplayer`, `menu.scenarios`, `menu.profile`, `menu.settings`, `common.coming_soon`, `debug.advance_day`, `hud.date`. |
| `Assets/Data/GameDataCatalog.asset` | Instance of the catalog referencing `en.json`. Created in Unity. |
| `Assets/Scenes/Bootstrap.unity`, `MainMenu.unity`, `World.unity` | Scenes wired in Build Settings. |

Tests (`Nation.Tests.EditMode`):

| File | Covers |
|---|---|
| `Tests/EditMode/GameDateTests.cs` | Month and year rollover, leap day, ISO formatting |
| `Tests/EditMode/SimulationEngineTests.cs` | One tick advances one day, systems run in declared order, `TickCompleted` published exactly once |
| `Tests/EditMode/SignalBusTests.cs` | Subscribe, publish, unsubscribe, no delivery after unsubscribe |
| `Tests/EditMode/LocalizationServiceTests.cs` | Key lookup, fallback locale, missing-key marker, format arguments |

Total: 3 asmdefs, 14 Core files, 8 Game files, 6 assets, 3 scenes, 4 test files.

### How the files connect

`GameBootstrap` creates a `SignalBus`, loads `en.json` through `LocalizationTableLoader` into `StringTableLocalizationService`, creates `SceneNavigator`, stores all of them in `GameContext`, and loads MainMenu. `MainMenuController` reads button labels from the localization service. PLAY calls `GameSession.StartNewGame`, which uses `WorldFactory` to build a `WorldState` and a `SimulationEngine` with an empty system list, then navigates to World. `WorldPlaceholderController` renders the state and subscribes to `TickCompleted`. ADVANCE DAY calls `GameSession.TickOnce`, the engine advances the date and publishes the signal, and the label updates. No code path touches the simulation per frame.

### Dependencies

- Unity 6 LTS with Android and iOS modules.
- Packages: `com.unity.inputsystem`, `com.unity.nuget.newtonsoft-json`, `com.unity.test-framework`, `com.unity.render-pipelines.universal` (template).
- No third-party assets. No network.

### Test plan

Steps:

1. Open the project in Unity. Wait for compilation.
2. Window → General → Test Runner → EditMode → Run All.
3. Open `Bootstrap.unity`. Press Play.
4. Observe the automatic transition to the main menu.
5. Tap MULTIPLAYER. Observe the COMING SOON label.
6. Tap PLAY. Observe the World scene with date `2026-01-01`.
7. Tap ADVANCE DAY three times.
8. Switch the Game view to a 390 by 844 portrait aspect and repeat steps 3 to 7.
9. Optional: File → Build Settings → Android → Build and Run on a connected phone.

Expected result:

- All EditMode tests pass.
- Zero console errors and zero warnings from our assemblies.
- Date reads `2026-01-04` after three taps.
- Buttons show localized text, never raw keys in square brackets.
- Layout is readable at 390 by 844 and at a tablet aspect.

Failure checklist:

- `Nation.Core` fails to compile with a UnityEngine reference error: a Core file imported a Unity namespace. Remove it.
- Buttons show `[menu.play]`: the catalog asset is not assigned on `GameBootstrap`, or the JSON key is missing.
- Nothing happens on PLAY: the World scene is not in Build Settings.
- UI is tiny or huge: Panel Settings is not assigned to the `UIDocument`, or the scale mode is wrong.
- Touch does nothing in the build: Active Input Handling is not set to the Input System package.

### Commit milestones

```
chore: initialize unity project for portrait mobile
feat: add core assemblies, world state and simulation engine
feat: add bootstrap, scene navigation and placeholder menu
test: add edit mode tests for date, engine, signals, localization
```

### Explicitly not in Phase 1

No commands, no time service, no country data, no map, no theme, no save. Those arrive in their own phases.

---

## Phase 1 implementation notes

Decisions taken while building Phase 1 that refine the plan above:

- **Catalog location.** `GameDataCatalog` lives in `Assets/Resources` and is loaded by the bootstrap before the first scene, so pressing Play in any scene works and no scene needs Inspector wiring. It remains the only `Resources` lookup in the project.
- **Packages deferred.** The Input System, URP, and Newtonsoft JSON packages are not yet in the manifest. Phase 1 has no gestures, no 3D rendering, and no save files, and each of those packages needs editor-side setup that is better done in the phase that uses it (4, 4, and 15 respectively). Localization tables are read with `JsonUtility`, so the file format is an array of key/value entries.
- **Theme.** UI Toolkit's default runtime theme is imported through `Assets/UI Toolkit/UnityThemes/UnityDefaultRuntimeTheme.tss` and referenced by `Assets/UI/NationPanelSettings.asset` (390 by 844 reference, shrink-to-fit so the PC Game view works at any size).
- **Menu backdrop.** A vector-drawn wireframe globe (`GlobeBackdropElement`, Painter2D) stands in for the textured Earth until Phase 4.
- **Placeholders to delete in later phases.** `PlaceholderCountries.cs` (Phase 3), the static German flag stripes in `WorldPlaceholder.uxml` (Phase 3), and `WorldPlaceholderController` (Phase 5).

## Phase 2 and 3 implementation notes

- **UI is built in C#, styled by USS.** Screens and components compose reusable elements (`Typography`, `Buttons`, `StatCard`, `InfoCard`, `CountryCard`, `FilterChip`, `SearchField`, `FlagElement`, `IconElement`, `ChartElement`, `GaugeBar`, `Badge`, `EmptyState`, `LoadingIndicator`) and read tokens from `Assets/UI/Theme.uss`. No UXML is used; this keeps every layout testable by the compiler and avoids hand-authored asset references.
- **One persistent UI document.** `UIService` owns the layers (screens, sheets, modals, loading, toasts) inside a `SafeAreaElement`. Scenes only host 3D content; switching scenes never rebuilds the UI.
- **Navigation.** `ScreenStack` provides Push, Pop, Replace and ReplaceAll with 260 ms fade-and-slide transitions; `ModalLayer` and `SheetLayer` provide ShowModal/Dismiss and a draggable three-state bottom sheet with velocity snapping.
- **Data.** `Nation.Core.Data.JsonParser` is a small strict JSON reader so country, building, map and localization files load identically in Unity, in tests and on a future server. Vector flags are described in the country data and drawn with Painter2D until textures arrive.
- **Time.** `TickScheduler` converts elapsed real time at the chosen speed into whole days (0.5, 1, 3 or 10 per second); `GameRunner` is the only per-frame hook and feeds `GameSession.AdvanceRealTime`.
- **Placeholders still to replace.** `EconomyOverview` and `PowerOverview` derive display figures from state until their simulation phases; `WorldMapView` draws coarse silhouettes and capital markers until the Natural Earth map; CONSTRUCT shows a toast until the construction phase.
