# Run Evidences

This folder stores screenshots, logs, and recordings that prove the project builds and runs correctly.

## How to Add Evidence

1. **Build log** — Export the Unity build log (Console → copy or save to file) and place it here as `build-log-<date>.txt`.
2. **Play mode screenshot** — Use Unity's Screenshot utility or any screen-capture tool. Name files `playmode-<feature>-<date>.png`.
3. **Test results** — Export the Unity Test Runner XML report (`Window → General → Test Runner → Export`) and save as `test-results-<date>.xml`.
4. **Video** — Short `.mp4` / `.gif` screen recordings showing features working end-to-end.

## Evidence Index

| Date | Type | Description | File |
|------|------|-------------|------|
| *(pending)* | Build log | Unity 6.2 — successful build | *(add here)* |
| *(pending)* | Screenshot | Block palette in Play mode | *(add here)* |
| *(pending)* | Test results | PlayMode NUnit run | *(add here)* |
| *(pending)* | Screenshot | RobotController moving forward | *(add here)* |
| *(pending)* | Screenshot | ColorSensor detecting line | *(add here)* |
| *(pending)* | Screenshot | ScenarioController switching world | *(add here)* |

## Build Instructions (Quick Reference)

```bash
# Open in Unity Hub → select Unity 6000.2.10f1
# File → Build Settings → choose platform → Build
# OR via command line:
Unity -batchmode -quit -projectPath . -buildTarget StandaloneWindows64 -logFile build.log
```

## Notes

- Unity does not run in the CI sandbox (no GPU); builds must be triggered from Unity Hub or a Bitrise pipeline with a macOS/Windows runner.
- Bitrise badge status: see README.md at the root of the project.
