namespace GuitarPracticeTimer.Core;

/// <summary>
/// One frame's worth of display state. Produced by <see cref="PracticeSession.Describe"/>
/// so that every platform renders the same words and enables the same controls.
/// </summary>
/// <param name="Time">Countdown for the current segment, m:ss.</param>
/// <param name="Duration">Selected practice length, m:ss.</param>
/// <param name="Mode">PRACTICE or COOL DOWN.</param>
/// <param name="Status">Sub-caption under the clock.</param>
/// <param name="PrimaryLabel">Text on the main pill button.</param>
/// <param name="Hint">Small line under the buttons; empty when there is nothing to say.</param>
/// <param name="PrimaryEnabled">False while the cooldown skip is still locked.</param>
/// <param name="ResetEnabled">False for the whole cooldown.</param>
/// <param name="DurationEnabled">False while practice runs and for the whole cooldown.</param>
/// <param name="RemainingFraction">Dial fill, 0 to 1.</param>
/// <param name="IsCooldown">Drives which palette the UI fades to.</param>
public readonly record struct SessionView(
    string Time,
    string Duration,
    string Mode,
    string Status,
    string PrimaryLabel,
    string Hint,
    bool PrimaryEnabled,
    bool ResetEnabled,
    bool DurationEnabled,
    double RemainingFraction,
    bool IsCooldown);
