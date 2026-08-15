# What this changes

<!-- A sentence or two. What does the app do differently after this? -->

Fixes #<!-- issue number, or delete this line -->

## Why

<!--
For anything beyond a typo or an obvious bug fix, there should be an issue
already agreeing the change is in scope. Link it above.
-->

## How it was tested

Building is not enough on its own — this is a timer, so it has to be watched
running at least once.

- [ ] `dotnet build -c Release` succeeds with no new warnings
- [ ] Started a short block and watched it elapse
- [ ] The chime fired and the window cross-faded to amber
- [ ] **Skip cooldown** stayed disabled for the first 60 seconds, with the hint
      line counting down
- [ ] **Reset** and the duration slider stayed disabled for the whole break
- [ ] The app returned to practice mode paused at the slider length
- [ ] `Space`, `R` and `Esc` all still do what the README says

<!-- Note anything you couldn't test, and why. -->

## Scope check

- [ ] No new NuGet packages or Python dependencies
- [ ] No new settings screen or configuration file
- [ ] Does not weaken or add a way around the enforced cooldown
- [ ] Style matches the surrounding code and `.editorconfig`
- [ ] README updated if behaviour, shortcuts or the customisation table changed

<!--
If you're ticking "no" on any of these deliberately, say why here — the rules
have exceptions, they just need arguing for.
-->

## Screenshots

<!-- Required for anything that changes what the window looks like. -->
