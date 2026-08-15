# Guitar Practice Timer

[![Build](https://github.com/Baracuda011/GuitarPracticeTimer/actions/workflows/build.yml/badge.svg)](https://github.com/Baracuda011/GuitarPracticeTimer/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download)
[![Platform: Windows](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-0078D6?logo=windows&logoColor=white)](#requirements)

**A practice tool for guitarists.** It breaks your session into short, focused
blocks of playing separated by real rest — the structure that learning research
says gets you further than grinding a passage for an hour straight.

Work a lick, a scale shape, a bar of a solo. When the block ends the app beeps,
the window fades from cool mint to warm amber, and a **three-minute break starts
automatically** — one you can't skip out of for the first minute. That locked
minute is the whole design. Left to ourselves, mid-flow, we skip the break every
time. The research says the break is where a lot of the improvement actually
shows up.

Built with WPF on .NET 8. No installer, no settings file, no telemetry, no
network access. One window, one `.exe`.

<!-- Add a screenshot here once you have one:
![Guitar Practice Timer](docs/screenshot.png)
-->

---

## How to practise with it

The app is built around one loop, repeated:

1. **Pick one small thing.** A four-bar phrase, a chord change, one scale
   position, a tricky bit of picking. Not "practise the song."
2. **Set a short block.** 30 seconds to 10 minutes; the default is 3 minutes.
   Short is deliberate — the goal is many focused reps of one thing, not
   endurance.
3. **Play it, repeatedly, for the whole block.** Slowly enough to get it right.
4. **When the chime sounds, put the guitar down.** The window turns amber. Do
   not keep noodling — see the science below; playing something else through the
   break measurably eats into the gain.
5. **Come back and run it again.** The app returns to practice mode paused at
   your chosen length, so the next block is always a deliberate decision.

The next block is usually the interesting one. It is very common for a passage
to come back cleaner after the break than it was when you put the guitar down.

## The science behind it

The design follows a specific, well-replicated finding: during early skill
learning, **a large share of the improvement appears across the short rest
periods between practice attempts, not during the practice itself.**

### The primary source

> Bönstrup, M., Iturrate, I., Thompson, R., Cruciani, G., Censor, N., &
> Cohen, L. G. (2019). **A Rapid Form of Offline Consolidation in Skill
> Learning.** *Current Biology*, 29(8), 1346–1351.
> [doi:10.1016/j.cub.2019.02.049](https://doi.org/10.1016/j.cub.2019.02.049)
> · [full text](https://www.cell.com/current-biology/fulltext/S0960-9822(19)30219-2)

Leonardo Cohen's lab at the US National Institute of Neurological Disorders and
Stroke (NINDS) had participants learn a five-element finger-tapping sequence
with their non-dominant hand — the closest a lab gets to drilling a guitar lick.
Practice was structured as **36 trials of 10 seconds playing, 10 seconds rest**,
about 12 minutes in total, recorded under magnetoencephalography (MEG).

Splitting the learning curve into *micro-online* gains (improvement within a
10-second practice bout) and *micro-offline* gains (improvement across a
10-second rest), they found that **early learning was accounted for almost
entirely by the offline gains across rest** — people came back from each short
break faster than they left it, while performance within a bout was flat or
declining. Frontoparietal beta oscillations during rest predicted the size of
each break's gain, consistent with the brain rehearsing the sequence while the
hands were still. It put memory consolidation, previously discussed on a scale
of hours or a night's sleep, on a scale of **seconds**.

The same group replicated it at scale — 389 participants online plus an in-lab
group — in [Bönstrup et al. (2020), *npj Science of Learning*](https://doi.org/10.1038/s41539-020-0066-9).

### The music-specific study

> Simmons, A. L., Allen, S. E., Cash, C. D., & Duke, R. A. (2019). **Effects of
> early break intervals on musicians' and nonmusicians' skill learning.**
> *Psychology of Music*, 47(1), 83–95.
> [doi:10.1177/0305735617735373](https://doi.org/10.1177/0305735617735373)

Closer to home: 118 participants — 59 music majors, 59 non-musicians — learned a
five-element keypress sequence on a **digital piano** in a 12-minute session, with
a 5-minute break part way through. What they did *during* the break was varied.

The result that matters for how you use this app: participants who **practised a
different motor sequence** during the break made **significantly smaller gains
across it** than those who chatted with the proctor or memorised word pairs. A
competing motor task ate the benefit.

In guitar terms: when the break starts, actually stop playing. Don't switch to
warming up something else, don't drift into noodling. Put the guitar down.

### An honest caveat

This is live science, not settled fact. A 2025 paper —
[Das et al., *PNAS*, doi:10.1073/pnas.2509233122](https://doi.org/10.1073/pnas.2509233122)
— argues the micro-offline gains reflect recovery from fatigue and pre-planning
of the next attempt rather than genuine offline consolidation, and reports that
break and no-break groups reached similar skill levels on later tests. Note what
is and isn't disputed: **nobody disputes that you perform better coming out of a
break.** The argument is about the mechanism.

Two other honest notes:

- The strongest evidence is at the **seconds** timescale — 10 seconds of rest
  between 10 seconds of practice. This app enforces a **three-minute** break
  after a multi-minute block. That's an extrapolation, sitting closer to the
  distributed-practice literature and the 5-minute break in the Simmons study
  than to the Bönstrup design.
- The three-minute break also does a second, entirely non-controversial job:
  it gets your hands off the neck. Repetitive strain is a real way guitarists
  lose months, and no study is needed to justify unclenching your fretting hand
  every few minutes.

**Sources:** [Bönstrup et al. 2019, *Current Biology*](https://www.cell.com/current-biology/fulltext/S0960-9822(19)30219-2)
· [Bönstrup et al. 2020, *npj Science of Learning*](https://pmc.ncbi.nlm.nih.gov/articles/PMC7272649/)
· [Simmons et al. 2019, *Psychology of Music*](https://doi.org/10.1177/0305735617735373)
· [Das et al. 2025, *PNAS*](https://pmc.ncbi.nlm.nih.gov/articles/PMC12595466/)
· [NINDS press release](https://www.ninds.nih.gov/news-events/news/press-releases/want-learn-new-skill-take-some-short-breaks)

## Features

- **Adjustable practice block** — 30 seconds to 10 minutes, in 10-second steps.
- **Enforced 3-minute break** — starts on its own the moment a block ends.
- **60-second skip lock** — the break can't be cut short until a minute has
  passed, and the button shows exactly how long is left.
- **Audible end-of-block cue** — a two-note chime (G5 → C6), so you can keep your
  eyes on the fretboard instead of the clock.
- **Full-window colour shift** — practice is a cool dark mint, break is a warm
  amber, crossfading over 550 ms. Readable across the room, out of the corner of
  your eye, mid-phrase.
- **Circular progress dial** — an arc sweeping clockwise from 12 o'clock as the
  block drains.
- **Drift-free timing** — the timer stores a UTC deadline and subtracts from it
  rather than accumulating ticks, so it stays accurate even if the UI thread
  stutters.
- **Keyboard driven** — start, pause and reset without putting the guitar down.
- **Frameless window** — rounded custom chrome with a drop shadow; drag anywhere
  on the card to move it. Small enough to sit beside a tab or a score.

## Keyboard shortcuts

| Key     | Action                                                  |
| ------- | ------------------------------------------------------- |
| `Space` | Start / Pause / Resume — or skip the break once unlocked |
| `R`     | Reset to the full block length (ignored during a break)  |
| `Esc`   | Close the app                                            |

## What the app does, screen by screen

1. **Ready** — drag the slider to pick a block length. The dial and the label
   below the slider both update as you drag.
2. **Playing** — hit Start or `Space`. The slider locks so the block can't shift
   under you, and the arc unwinds.
3. **Chime** — the block hits zero, the two-note cue plays, and the window turns
   amber on its own.
4. **Cool down** — three minutes, "rest your hands". **Skip cooldown** is
   disabled for the first 60 seconds and the hint line counts down to the
   unlock; **Reset** and the slider are dead for the whole break.
5. **Back to ready** — whether you skip or wait it out, the app returns to
   practice mode, reset to your slider length and **paused**.

## Requirements

- Windows 10 or 11 (x64)
- [.NET 8 SDK](https://dotnet.microsoft.com/download) or newer to build
  *(the published `.exe` is self-contained and needs no runtime installed)*
- Python 3 — **only** if you want to regenerate the icon

## Install

**Download it:** grab the latest zip from the
[Releases page](https://github.com/Baracuda011/GuitarPracticeTimer/releases),
extract it, and run `GuitarPracticeTimer.exe`. Nothing to install and no .NET
runtime needed — it's all bundled.

The executable is unsigned, so Windows SmartScreen will warn you the first time:
*More info* → *Run anyway*. If you'd rather not trust an unsigned binary, build
it yourself — it takes about two seconds.

## Build and run

```powershell
git clone https://github.com/Baracuda011/GuitarPracticeTimer.git
cd GuitarPracticeTimer
dotnet run
```

### Publishing a standalone executable

```powershell
dotnet publish -c Release -o publish
```

Publish settings live in [`GuitarPracticeTimer.csproj`](GuitarPracticeTimer.csproj):
self-contained, single-file, `win-x64`, no debug symbols. The result is
`publish/GuitarPracticeTimer.exe` — it bundles the entire .NET runtime, so it's
large (~150 MB) but runs on a clean Windows machine with nothing installed.

A handful of native WPF libraries (`wpfgfx_cor3.dll`, `PresentationNative_cor3.dll`,
`D3DCompiler_47_cor3.dll`, `PenImc_cor3.dll`, `vcruntime140_cor3.dll`) are emitted
next to the `.exe`. They're extracted at runtime and can be left alongside it, or
shipped together as a folder.

### Regenerating the icon

[`make_icon.py`](make_icon.py) draws `Assets/app.ico` from scratch using nothing
but the Python standard library — no Pillow, no ImageMagick. It renders a dark
rounded square with a mint progress ring and a clock hand at 4× supersampling,
box-downsamples for anti-aliasing, then hand-assembles the PNG chunks (with CRCs)
and the multi-size `.ico` container itself.

```powershell
python make_icon.py
```

Sizes emitted: 16, 24, 32, 48, 64, 128, 256 px.

## Project structure

```
GuitarPracticeTimer/
├── App.xaml / App.xaml.cs          Application entry, mode colour palettes
├── MainWindow.xaml                 UI layout and control templates
├── MainWindow.xaml.cs              Timer state machine, dial geometry, input
├── GuitarPracticeTimer.csproj      Build + single-file publish settings
├── make_icon.py                    Generates Assets/app.ico (stdlib only)
├── Assets/
│   └── app.ico
└── .github/
    ├── workflows/build.yml         CI: build, publish, verify the icon
    ├── workflows/release.yml       Tagged releases with a SHA-256 checksum
    ├── ISSUE_TEMPLATE/             Bug report and feature request forms
    └── PULL_REQUEST_TEMPLATE.md
```

Roughly 300 lines of C# and 300 lines of XAML. No third-party packages.

## Customising it

Most of what you'd want to change is a constant or two. If you want to sit
closer to the Bönstrup protocol, try a 20–30 second block with a 15 second
break; for repertoire work, longer blocks with the full 3 minutes.

| What                     | Where                                                                             | Default          |
| ------------------------ | --------------------------------------------------------------------------------- | ---------------- |
| Break length             | `CooldownTotal` in [`MainWindow.xaml.cs`](MainWindow.xaml.cs)                      | `180` s          |
| Skip lock duration       | `CooldownLock` in [`MainWindow.xaml.cs`](MainWindow.xaml.cs)                       | `60` s           |
| Practice range and step  | `Minimum` / `Maximum` / `TickFrequency` on `DurationSlider` in `MainWindow.xaml`   | 30–600 s, 10 s   |
| Default block length     | `Value` on `DurationSlider` in `MainWindow.xaml`                                   | `180` s          |
| Mode colours             | `PracticeBg` / `PracticeAccent` / `CooldownBg` / `CooldownAccent` in `App.xaml.cs` | mint / amber     |
| Chime pitch and length   | `Beep()` in [`MainWindow.xaml.cs`](MainWindow.xaml.cs)                             | 784 Hz → 1047 Hz |

If you change the slider range in XAML, update the `0:30` / `10:00` end labels
just below it to match.

## Implementation notes

A few decisions that aren't obvious from a skim:

- **Deadline, not accumulation.** `_deadlineUtc` is set once when the timer
  starts; every tick just measures how far away it still is. A dropped or late
  frame costs display smoothness, never accuracy.
- **Brushes built in code, not XAML.** `BgBrush` and `AccentBrush` are created in
  `App.OnStartup` so they stay unfrozen and animatable. Every surface binds to
  them with `DynamicResource`, so one `ColorAnimation` repaints the entire
  window — background, dial, buttons, slider fill and thumb — in a single sweep.
- **The dial is drawn, not composed.** `BuildArc` emits a `StreamGeometry` with a
  single `ArcTo`, picking the large-arc flag past 180°, and clamps the sweep just
  short of a full turn so a completed circle doesn't collapse to a zero-length
  path.
- **A `_ready` guard.** `DurationSlider_ValueChanged` fires during
  `InitializeComponent()`, before the fields it touches are meaningful; the flag
  keeps that early call from writing garbage into the timer state.
- **The beep runs off-thread.** `Console.Beep` blocks for its full duration, so
  it's dispatched to a task pool thread and wrapped in a `try`/`catch` — on a
  machine with no usable speaker the colour change is still a complete signal.

## Roadmap ideas

Not implemented, but natural next steps for a practice tool:

- **Practice log** — blocks completed per day, and what you worked on
- **Named drills** — label the block ("Am pentatonic, position 3") and keep a history
- **Configurable long break** after N blocks
- **A metronome pane**, with the tempo recorded alongside each block
- Custom chime, or a sound file instead of `Console.Beep`
- Remembering the last-used block length between runs
- Always-on-top toggle, for practising alongside tab on screen

## Contributing

Contributions are welcome, with one standing caveat: **the enforced break stays
enforced.** A PR adding a way to switch the cooldown off is the one change that
won't be merged — it's the entire point of the app.

- [Contributing guidelines](CONTRIBUTING.md) — scope, setup, and what to check
  before opening a PR
- [Code of conduct](CODE_OF_CONDUCT.md)
- [Security policy](SECURITY.md) — please report vulnerabilities privately
- [Changelog](CHANGELOG.md)

Every push and pull request is built on `windows-latest` with warnings treated
as errors, and CI re-runs `make_icon.py` to confirm the committed icon still
matches its generator byte for byte.

## Licence

[MIT](LICENSE) — do what you like with it, including using it to actually
practise.

The research cited in [The science behind it](#the-science-behind-it) belongs to
its respective authors and publishers; the citations are provided so you can
check the claims yourself, and none of those authors are affiliated with or
endorse this app.
