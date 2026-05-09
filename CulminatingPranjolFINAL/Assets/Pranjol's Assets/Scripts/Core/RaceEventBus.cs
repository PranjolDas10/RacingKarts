using System;

/// <summary>
/// Decoupled publish/subscribe event bus for race game events.
/// Game systems publish state changes here without holding direct references
/// to their consumers, making each system independently testable and reusable.
/// </summary>
public static class RaceEventBus
{
    // ── Race lifecycle ────────────────────────────────────────────────────────
    public static event Action OnRaceStarted;
    public static event Action<string, float> OnRaceFinished;  // winner name, elapsed seconds

    // ── Player state (published every FixedUpdate) ────────────────────────────
    public static event Action<float, float> OnSpeedChanged;   // current KPH, max KPH
    public static event Action<float>        OnNitroChanged;   // 0–1 normalized charge

    // ── Race progress ─────────────────────────────────────────────────────────
    public static event Action<int, int> OnLapCompleted;       // current lap, total laps
    public static event Action<int, int> OnPositionChanged;    // current place, total racers

    // ── Publish helpers (null-conditional invoke keeps call-sites clean) ──────
    public static void PublishRaceStarted()                        => OnRaceStarted?.Invoke();
    public static void PublishRaceFinished(string winner, float t) => OnRaceFinished?.Invoke(winner, t);
    public static void PublishSpeedChanged(float kph, float max)   => OnSpeedChanged?.Invoke(kph, max);
    public static void PublishNitroChanged(float normalized)       => OnNitroChanged?.Invoke(normalized);
    public static void PublishLapCompleted(int lap, int total)     => OnLapCompleted?.Invoke(lap, total);
    public static void PublishPositionChanged(int pos, int total)  => OnPositionChanged?.Invoke(pos, total);

    /// <summary>
    /// Clears all subscribers. Call on scene unload to prevent stale delegates
    /// from leaking across scene reloads.
    /// </summary>
    public static void ClearAllListeners()
    {
        OnRaceStarted    = null;
        OnRaceFinished   = null;
        OnSpeedChanged   = null;
        OnNitroChanged   = null;
        OnLapCompleted   = null;
        OnPositionChanged = null;
    }
}
