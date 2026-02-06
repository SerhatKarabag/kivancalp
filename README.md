# kivancalp

Unity 6 (`6000.3.3f1`) card-match prototype focused on gameplay-first architecture.

## Implemented Requirements

- Smooth gameplay with card flip/match/mismatch flow.
- Continuous input: player can keep flipping cards while previous pairs are still waiting for comparison/hide timing.
- Multiple layouts from data (`2x2`, `2x3`, `5x6`) with responsive scaling to board container.
- Save/load persistence (`Application.persistentDataPath/memory_checkpoint.json`) and auto-restore on restart.
- Scoring + combo logic.
- Four basic sound effects: flip, match, mismatch, game over.

## Architecture

`Assets/Game` is separated with assembly definitions:

- `Core`: DI container + shared contracts.
- `Gameplay`: domain/application logic, strongly typed events, no Unity dependency.
- `Infrastructure`: persistence/audio/config/random/logging adapters.
- `UI`: bootstrap/composition root, presenters, runtime-generated UI, card pool, animations.
- `Tests`: EditMode tests for DI and gameplay core behavior.

### Layer Boundaries

- `Gameplay` references only `Core`.
- `Infrastructure` references `Core + Gameplay`.
- `UI` references `Core + Gameplay + Infrastructure`.
- No `FindObjectOfType`, `GameObject.Find`, or script execution order dependencies.

## DI Container (Custom)

Implemented in `Assets/Game/Core/DI/DiContainer.cs` with:

- Explicit registration APIs (`RegisterSingleton`, `RegisterScoped`, `RegisterTransient`, `RegisterInstance`).
- Constructor injection support.
- Scope creation (`CreateScope`) and deterministic disposal chain (`IDisposable`).
- AOT/IL2CPP-compatible minimal API (no dynamic codegen).

## Data-Driven Setup

Config is loaded from:

- `Assets/Resources/Game/card_match_config.json`

Includes layouts, scoring, animation timings, save debounce, and RNG seed.

## Runtime Flow

- Entry point: `Assets/Game/UI/Bootstrap/RuntimeEntryPoint.cs`
- Composition root: `Assets/Game/UI/Bootstrap/GameBootstrapper.cs`
- Main gameplay service: `Assets/Game/Gameplay/Application/GameSession.cs`

## Save/Load

- Auto-load on startup.
- Debounced auto-save during play.
- Manual Save/Load buttons in HUD.

## Profiling Notes

See `Docs/PROFILING_NOTES.md`.

## Tests

EditMode tests:

- `Assets/Game/Tests/EditMode/DiContainerTests.cs`
- `Assets/Game/Tests/EditMode/GameSessionTests.cs`
