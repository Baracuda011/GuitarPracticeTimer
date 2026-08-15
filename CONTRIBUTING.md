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
  cooldown" switch will be declined — fork it if you want that.
- **No third-party packages.** The app is plain WPF on .NET 8 with zero NuGet
  dependencies, and the icon generator uses only the Python standard library.
  Adding a dependency needs a strong justification.
- **Small enough to read in one sitting.** Roughly 300 lines of C# and 300 of
  XAML. Keep it that way.

## Getting set up

```powershell
git clone https://github.com/Baracuda011/GuitarPracticeTimer.git
cd GuitarPracticeTimer
dotnet run
```

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) or newer, and
Windows 10/11 — this is a WPF app and does not run on Linux or macOS. Python 3
is only needed if you change the icon.

## Before you open a pull request

1. **Open an issue first** for anything beyond a typo or an obvious bug fix.
   It saves you writing code that gets turned down on scope grounds.
2. **Build cleanly:**
   ```powershell
   dotnet build -c Release
   ```
   No new warnings. `Nullable` is enabled — keep it warning-free rather than
   silencing it with `!`.
3. **Check the app actually runs** and that both modes still work: start a
   short block, let it elapse, confirm the chime fires, the colours cross-fade,
   the skip button stays locked for 60 seconds, and `Reset` and the slider are
   disabled for the whole break.
4. **Match the surrounding style.** [`.editorconfig`](.editorconfig) covers the
   mechanics; beyond that, mirror what is already there — file-scoped
   namespaces, `_camelCase` private fields, expression-bodied one-liners, and
   the section banner comments in
   [`MainWindow.xaml.cs`](MainWindow.xaml.cs).
5. **Keep commits focused.** One logical change per commit, present-tense
   subject line under ~72 characters.

## Things that would genuinely help

- A screenshot or short GIF for the README (there's a placeholder waiting).
- Accessibility: screen-reader announcements for mode changes, keyboard focus
  visuals, honouring the OS reduced-motion setting for the colour animation.
- High-DPI and multi-monitor testing.
- A practice log, if it can be done without a settings UI.
- Replacing `Console.Beep` with something that works on machines where it is
  silent, without adding a dependency.

## Reporting bugs

Use the [bug report template](https://github.com/Baracuda011/GuitarPracticeTimer/issues/new?template=bug_report.yml).
Include your Windows version, how you're running it (`dotnet run` or the
published `.exe`), and what the window looked like when it went wrong — a
screenshot of the timer mid-failure is worth a lot here.

## A note on the science

The README makes claims about interval practice and cites sources. If you think
a claim is overstated, understated, or has been superseded, that's a legitimate
issue to open — please bring the citation with you.
