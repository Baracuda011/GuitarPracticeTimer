# Security Policy

## Supported versions

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

Only the latest release on the default branch receives fixes.

## Threat surface

Guitar Practice Timer is a single-window desktop app with a deliberately small
attack surface, which is worth stating plainly:

- **No network access.** It makes no HTTP requests, opens no sockets, and
  contacts no telemetry or update service.
- **No file I/O at runtime.** It reads and writes nothing on disk - no settings
  file, no log, no practice history.
- **No credentials, accounts, or personal data**, so there is nothing stored to
  leak.
- **No elevation.** It runs as a normal user process and requires no
  administrator rights.
- **No third-party runtime dependencies.** Zero NuGet packages; the only code
  shipped beyond the app itself is the bundled .NET runtime.

The realistic concerns are therefore supply chain (a tampered release binary),
the bundled .NET runtime in the self-contained build, and anything in the CI
workflow.

## Reporting a vulnerability

**Please do not open a public issue for a security problem.**

Report it privately through GitHub's
[private vulnerability reporting](https://github.com/Baracuda011/GuitarPracticeTimer/security/advisories/new)
- the **Security** tab → **Report a vulnerability**. That is the only supported
private channel; there is no security contact email for this project.

Please include:

- What the issue is and how it can be triggered
- The version, and whether you're running a published `.exe` or a local build
- Your Windows version
- Anything you've worked out about impact

### What to expect

This is a hobby project maintained by one person in their spare time, so an
honest timeline rather than an aspirational one:

- **Acknowledgement:** within 7 days
- **Initial assessment:** within 30 days
- **Fix or public disclosure:** by agreement, once a fix exists or it's clear
  one won't be written

You'll be credited in the release notes unless you'd rather not be.

## Verifying a download

Release binaries are built by the GitHub Actions workflow in
[`.github/workflows/build.yml`](.github/workflows/build.yml) and published from
this repository. If you obtained `GuitarPracticeTimer.exe` from anywhere other
than this repository's Releases page, treat it as untrusted.

The published executable is unsigned - Windows SmartScreen will warn on first
run. That warning is expected and is not itself evidence of tampering, but it
also means the binary carries no cryptographic proof of origin. If that matters
to you, build it yourself from source: `dotnet publish -c Release -o publish`.
