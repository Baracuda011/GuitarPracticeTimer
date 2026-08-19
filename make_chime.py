"""Generates Assets/chime.wav (pure stdlib: no numpy, no scipy).

The two-note chime that marks the end of a practice block: G5 then C6, the
same pair the original WPF build produced with Console.Beep. Console.Beep is
Windows-only, so on every other platform the app plays this file instead.

Each note is a fundamental plus two quiet harmonics under a short attack and
an exponential decay, which reads as a chime rather than a test tone and - more
importantly - starts and ends at zero, so there is no click at either edge.
"""
import math
import os
import struct
import wave

RATE = 44100
PEAK = 0.62  # leaves headroom; the chime should cut through, not startle

G5 = 783.99
C6 = 1046.50

NOTE_ONE = (G5, 0.150)
GAP = 0.070
NOTE_TWO = (C6, 0.280)

ATTACK = 0.006  # seconds of fade-in; anything shorter clicks
DECAY = 3.2     # higher decays faster, relative to the note's own length

# Relative levels of the fundamental and its next two harmonics.
HARMONICS = (1.0, 0.18, 0.06)


def note(freq, seconds):
    """Returns a list of floats in -1..1 for one enveloped note."""
    total = int(RATE * seconds)
    attack = max(1, int(RATE * ATTACK))
    samples = []

    for i in range(total):
        t = i / RATE
        wave_sum = sum(
            level * math.sin(2.0 * math.pi * freq * (n + 1) * t)
            for n, level in enumerate(HARMONICS)
        )
        wave_sum /= sum(HARMONICS)

        envelope = math.exp(-DECAY * (t / seconds))
        if i < attack:
            envelope *= i / attack
        # Force the last few samples to zero so the note cannot end mid-swing.
        if i > total - attack:
            envelope *= (total - i) / attack

        samples.append(wave_sum * envelope)

    return samples


def silence(seconds):
    return [0.0] * int(RATE * seconds)


def main():
    samples = note(*NOTE_ONE) + silence(GAP) + note(*NOTE_TWO)

    frames = b"".join(
        struct.pack("<h", max(-32768, min(32767, int(s * PEAK * 32767))))
        for s in samples
    )

    out_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Assets")
    os.makedirs(out_dir, exist_ok=True)
    path = os.path.join(out_dir, "chime.wav")

    with wave.open(path, "wb") as fh:
        fh.setnchannels(1)
        fh.setsampwidth(2)
        fh.setframerate(RATE)
        fh.writeframes(frames)

    print(f"wrote {path} ({os.path.getsize(path)} bytes, "
          f"{len(samples) / RATE:.3f}s)")


if __name__ == "__main__":
    main()
