# Profiling Notes

## Goal

Target for gameplay hot path: `0 B/frame` GC allocations.

## Allocation Strategy

- No LINQ in gameplay loop.
- No boxing in the main tick path.
- No per-frame `new` allocations inside `GameSession.Tick` and animation tick.
- Pair processing uses preallocated ring-buffer arrays.
- Card visuals use a prewarmed pool (`CardViewPool`) based on max layout card count.

## Instantiate/Destroy Policy

- Card GameObjects are created once at startup (pool prewarm).
- Gameplay loop does not call `Instantiate`/`Destroy`.
- Pool return/reuse is used for board rebuilds/layout switches.

## Suggested Profiler Verification

1. Open Unity Profiler (CPU + GC Alloc columns).
2. Start the game, repeatedly flip cards quickly for 60+ seconds.
3. Verify `GC Alloc` is `0 B` on steady-state gameplay frames.
4. Verify spikes only during startup and scene/bootstrap initialization.

## Notes

- Save operations serialize JSON on change (debounced); these are event-driven writes and not per-frame.
- Procedural audio clips are generated once at startup.
