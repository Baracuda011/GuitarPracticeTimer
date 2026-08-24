# Contributing to Guitar Practice Timer

Thanks for taking an interest. This is a small, deliberately simple app, and the
best contributions keep it that way.

By participating you agree to abide by the [Code of Conduct](CODE_OF_CONDUCT.md).

## What this project is trying to be

Before opening a PR, it helps to know what the project is optimising for:

- **One window, no configuration.** There is no settings file, no account, no
  network access. Features that need a preferences dialog are a hard sell.
- **The break is not optional.** The 60-second skip lock is the point of the
  app, not an inconvenience to be smoothed away. PRs that add a "disable
  cooldown" switch will be declined - fork it if you want that.
- **Very few dependencies.** Avalonia for the UI and SoundFlow for audio, and
  that is the whole list. `GuitarPracticeTimer.Core` has none at all and should
  stay that way. The asset generators use only the Python standard library.
  Adding a dependency needs a strong justification.
- **The rules live in Core.** Anything governing the practice/cooldown cycle
  belongs in `PracticeSession`, not in a view. That is what keeps desktop and
  mobile from drifting apart, and it is why that class carries most of the
  tests.
- **Small enough to read in one sitting.** Keep it that way.

## Getting set up

```bash
git clone https://github.com/Baracuda011/GuitarPracticeTimer.git
cd GuitarPracticeTimer
dotnet run --project src/GuitarPracticeTimer.Desktop
```

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download). The desktop
app builds and runs on Windows and Linux; macOS should work but is not tested.
Python 3 is only needed if you change the icon or the chime.

### The website

The site in `docs/` is plain static HTML, but its stylesheet is generated.
`docs/app.main.css` is compiled from the Sass sources in `styles/` and must not
be edited directly:

```bash
npm install
npm run css        # compile once
npm run css:watch  # recompile on save
```

`docs/` is uploaded to the host verbatim, so the compiled CSS is committed
alongside its source. CI recompiles and fails if the two have drifted, so
commit the regenerated `docs/app.main.css` with any change to `styles/`.

## Before you open a pull request

1. **Open an issue first** for anything beyond a typo or an obvious bug fix.
   It saves you writing code that gets turned down on scope grounds.
2. **Build cleanly and pass the tests:**
   ```bash
   dotnet build -c Release -warnaserror
   dotnet test
   ```
   CI runs both on Windows and Linux, so a change that only compiles on one of
   them will be caught. `Nullable` is enabled - keep it warning-free rather than
   silencing it with `!`.
3. **Cover changes to the rules with a test.** If you touch `PracticeSession`,
   `PracticeSessionTests` is where it gets proven. The cooldown enforcement
   tests in particular should never be weakened to make a change pass.
4. **Check the app actually runs** and that both modes still work: start a
   short block, let it elapse, confirm the chime fires, the colours cross-fade,
   the skip button stays locked for 60 seconds, and `Reset` and the slider are
   disabled for the whole break.
5. **Regenerate assets if you changed a generator.** `python verify_assets.py`
   must pass; CI runs it on every push.
6. **Match the surrounding style.** [`.editorconfig`](.editorconfig) covers the
   mechanics; beyond that, mirror what is already there - file-scoped
   namespaces, `_camelCase` private fields, expression-bodied one-liners, and
   the section banner comments in
   [`PracticeSession.cs`](src/GuitarPracticeTimer.Core/PracticeSession.cs).
7. **Keep commits focused.** One logical change per commit, present-tense
   subject line under ~72 characters.

## Things that would genuinely help

- **Linux testing** on anything that isn't Ubuntu - window chrome, the audio
  backend and font rendering are the likely trouble spots.
- **The Android and iOS heads**, built on the existing `Core` and `Ui`
  projects. A short GIF of the colour cross-fade for the README would help too.
- Accessibility: screen-reader announcements for mode changes, keyboard focus
  visuals, honouring the OS reduced-motion setting for the colour transition.
- High-DPI and multi-monitor testing.
- A practice log, if it can be done without a settings UI.

## Reporting bugs

Use the [bug report template](https://github.com/Baracuda011/GuitarPracticeTimer/issues/new?template=bug_report.yml).
Include your OS and version, how you're running it (`dotnet run` or a published
binary), and what the window looked like when it went wrong - a screenshot of
the timer mid-failure is worth a lot here.

## A note on the science

The README makes claims about interval practice and cites sources. If you think
a claim is overstated, understated, or has been superseded, that's a legitimate
issue to open - please bring the citation with you.
