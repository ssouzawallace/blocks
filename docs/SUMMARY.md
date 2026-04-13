# Project Summary — Blocks Programming (Unity 6.2)

## What Was Built

This is a **2D visual block-programming editor** (Scratch-like) for educational robots, built with Unity 6000.2.10f1 and IronPython 2.7. Users drag-and-drop blocks to generate Python code that controls a simulated (or physical) robot.

---

## Accomplishments (Issue #22)

### Robot Layer (`Assets/Scripts/Robot/`)

| File | Purpose |
|---|---|
| `RobotConfiguration.cs` | `ScriptableObject` defining each robot variant (wheel count, sensors, LEDs, speaker, max speed). Supports 4 variants: TwoWheeler, +ColorSensor, +FrontUltrasonic, +UltrasonicArray. |
| `RobotController.cs` | High-level API: `MoveForward`, `MoveBackward`, `TurnLeft`, `TurnRight`, `Brake`, `SetSpeed`, `TurnLEDOn/Off`, `SetLEDColor`, `Beep`, `PlayTone`, `ReadUltrasonicSensor`, `ReadColorSensor`, `ReadLightLevel`. |
| `BoardController.cs` | Bridges the programming interface to the robot hardware. Manages power state, connection status, and serial-like execute-program flow. |
| `WheelController.cs` | Rotates a wheel GameObject around its local X-axis at a speed-scaled rate each `Update`. Supports `SetSpeed` and `Brake`. |
| `LEDController.cs` | Drives a `Light` component and/or material emission. Exposes `TurnOn`, `TurnOff`, `SetColor`. |
| `SpeakerController.cs` | Generates procedural `AudioClip` tones in-editor via `AudioSource`. Exposes `Beep` and `PlayTone(freq, duration)`. |
| `UltrasonicSensorController.cs` | Raycasts forward to measure distance; returns `Distance` property; draws gizmos. |
| `ColorSensorController.cs` | Raycasts downward, reads `Renderer.sharedMaterial.color`; computes `LightLevel`; draws gizmos. |

### Scenario Layer (`Assets/Scripts/Scenario/`)

| File | Purpose |
|---|---|
| `ScenarioController.cs` | Manages 6 world environments (Classroom, Outdoors, LavaVolcano, Underwater, Beach, StoreMall). `LoadScenario(type)` activates one root GameObject and deactivates the rest. |

### Code Quality Improvements

- Removed a redundant ternary in `BoardController.cs`
- Added array bounds check in `RobotController.SetSpeed`
- Fixed undeclared `drawDistance` variable in `ColorSensorController.OnDrawGizmos` → replaced with `maxRange`

### Infrastructure

- `.gitignore` updated with standard Unity ignore patterns
- `.idea` / `.vs` workspace files committed for Rider and Visual Studio
- `git gc --aggressive --prune=now` run to compact pack files (`.git/` = 12 MB)

---

## Architecture Summary

```
Assets/
  Scripts/
    Programming/        ← Block palette, Python editor window, IronPython bridge
    Robot/              ← Per-component controllers + RobotConfiguration SO
    Scenario/           ← World/environment manager
  Tests/
    PlayMode/           ← NUnit / Unity Test Framework tests for block classes
```

**Data flow:** Block palette → Python code generation (IronPython 2.7) → `BoardController` → `RobotController` → individual component controllers (wheels, sensors, LEDs, speaker).

---

## Build / Test Status

- **Unity Editor:** Unity 6000.2.10f1 (Unity 6.2)
- **CI:** Bitrise (badge in README) — master branch
- **Test Framework:** `com.unity.test-framework 1.1.33`, NUnit, PlayMode tests in `Assets/Tests/PlayMode/`
- **Known manual test:** Open project in Unity Editor, enter Play mode, drag blocks in the palette, observe Python output in `PythonEditorWindow`, confirm robot simulation responds.

> See [`docs/run-evidences/`](./run-evidences/) for screenshots and run logs.
