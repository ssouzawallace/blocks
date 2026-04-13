# Next Tasks

Ordered by priority. Items are sized S (< 1 day), M (1–3 days), L (> 3 days).

---

## High Priority

- [ ] **[M] Unity scenes & prefabs for robot variants** — Create at least one Unity scene (`Assets/Scenes/Main.unity`) and prefabs for each `RobotVariant` so `RobotController` / `BoardController` can be wired in the Inspector without code.
- [ ] **[M] Block → Python code generation for new commands** — Extend `BlocksPallete` / Python templates to emit `move_forward()`, `turn_left()`, `set_speed()`, `read_sensor()` calls that map to the `RobotController` API added in issue #22.
- [ ] **[S] Fix remaining gizmo color bug** — `ColorSensorController.OnDrawGizmos` uses the wrong `drawDistance` field (fixed in this PR); verify no similar issues in `UltrasonicSensorController`.
- [ ] **[M] PlayMode tests for new controllers** — Add NUnit tests in `Assets/Tests/PlayMode/` for `RobotController`, `BoardController`, `WheelController`, `ColorSensorController`, `ScenarioController` (following the `BlockTestHelper` pattern).

## Medium Priority

- [ ] **[L] iOS build pipeline on Bitrise** — Add a Bitrise workflow step for `Unity → Xcode → TestFlight` upload. Requires macOS Bitrise stack and Apple certificates.
- [ ] **[L] Android build pipeline on Bitrise** — Add workflow for `Unity → App Bundle → Google Play Internal Testing` upload via `google-play-deploy` step.
- [ ] **[M] WebGL build & GitHub Pages deployment** — Add a GitHub Actions workflow that builds WebGL and deploys to `gh-pages` branch on every push to `master`.
- [ ] **[M] Scenario prefabs** — Create `GameObject` prefabs for each `ScenarioType` (Classroom, Outdoors, LavaVolcano, Underwater, Beach, StoreMall) and wire them into `ScenarioController.scenarioRoots`.

## Low Priority / Backlog

- [ ] **[S] Update README badges** — Replace Bitrise badge with GitHub Actions badge once CI is migrated. Add WebGL live-demo link.
- [ ] **[M] Localization** — Add `ScenarioController.GetScenarioDisplayName` translations (Portuguese, English at minimum) via Unity Localization package.
- [ ] **[L] Physical robot integration** — Serial/BLE bridge between `BoardController` and real hardware (e.g., Br-GoGo board). Requires a native plugin or `System.IO.Ports` on desktop builds.
- [ ] **[S] `parsetab.py` cleanup** — Investigate whether `parsetab.py` (root level, 78 KB) belongs in the repo or should be `.gitignore`d as a generated PLY artifact.
- [ ] **[M] Accessibility pass** — Ensure block palette text/icons meet WCAG AA contrast ratios; add screen-reader-friendly labels for WebGL build.
- [ ] **[S] Version tagging** — Create a `v0.1.0` Git tag to mark the baseline release after issue #22 is merged.
