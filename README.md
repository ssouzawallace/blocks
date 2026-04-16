# Unity3D Blocks Programming

![GitHub top language](https://img.shields.io/github/languages/top/ssouzawallace/blocks)
![GitHub language count](https://img.shields.io/github/languages/count/ssouzawallace/blocks?style=flat-square)
![GitHub issues](https://img.shields.io/github/issues/ssouzawallace/blocks?style=flat-square)
![CI](https://github.com/ssouzawallace/blocks/actions/workflows/ci.yml/badge.svg)
![YouTube Video](https://img.shields.io/youtube/views/uAIc0vqwZjI?style=social)


<img src="https://github.com/ssouzawallace/blocks/assets/6471118/ef2a3a59-7fe7-4c77-8bc9-a62d4143950c" alt="UnityBlender" width="256">
<img src="https://github.com/ssouzawallace/blocks/assets/6471118/2ef8a0aa-02aa-4759-a4c6-48b0744e3677" alt="Python" width="256">

[Image credits](https://twitter.com/sawaratsuki1004)

# YouTube Video

[![Blocks Programming Unity3D](https://img.youtube.com/vi/uAIc0vqwZjI/maxresdefault.jpg)](https://youtu.be/uAIc0vqwZjI)

# Bitrise

Reference:

[Br-GoGo](https://br-gogo.sourceforge.net)


## Pre-requisites

- [Python 3](https://www.python.org/downloads/)

- [IronPython 2.7](https://ironpython.net)

- [UnityHub](https://unity.com/download#how-get-started)

- _Using Unity Editor 6000.2.10f1 (Unity 6.2)_

- [git-lfs](https://git-lfs.github.com)

### Useful Tools
- Windows:
- [Visual Studio](https://visualstudio.microsoft.com/pt-br/downloads)

- linux:
- [Mono Framework](https://www.mono-project.com/download)

- macOS:
- [Mono Framework](https://www.mono-project.com/download)


## Architecture Overview

### Block System (`Assets/Scripts/Programming/Blocks/`)

The editor uses a connection-based visual block system inspired by Scratch/Logo:

- **`Block`** — Abstract base class for all blocks. Manages drag-and-drop, a list of `Connection` objects (each with a socket type, connection type, and relative position), and code generation via `GetCode()`.
- **`Connection`** — Inner class of `Block`. Represents a slot that can attach to another block's complementary slot (Male↔Female, matching `ConnectionType`). Types: `Regular`, `Logic`, `Number`, `Forbidden`.
- **`SimpleInstructionBlock`** — Linear block with a top input and a next output. Generates a single instruction string.
- **`BlockWithArgument`** — Extends `SimpleInstructionBlock` to add a numeric argument slot.
- **`StartBlock`** — Entry-point block; generates `to start ... end` wrapper.
- **`IfThenBlock` / `IfThenElseBlock` / `WhileBlock` / `ForeverBlock`** — Control-flow blocks with dynamic height/width based on nested content.
- **`NumberBlock`** — Base for numeric value blocks (constant, sensor read, operation).
- **`ConditionBlock` / `ConditionOperatorBlock`** — Logic/comparison blocks.

#### Code Generation Pipeline

Each block's `GetCode()` recursively calls `GetCode()` on attached blocks to produce a Logo-like string. The `IndentLogoCode()` method in `Block` post-processes the flat string into indented output using `[` / `]` markers.

```
StartBlock.GetCode()
  → "to start"
  → next.GetCode()          (e.g. IfThenBlock)
      → "if (...) [\n"
      → then.GetCode()      (e.g. SimpleInstructionBlock chain)
      → "\n]"
  → "end"
```

### Robot System (`Assets/Scripts/Robot/`)

- **`RobotController`** — High-level robot API (move, turn, LED, sensor read). Delegates to component controllers.
- **`BoardController`** — Parses command strings (e.g. `"abcd, setpower 50"`) and dispatches to the robot API.
- **`WheelController`** — Animates wheel rotation based on speed.
- **`LEDController`** — Toggles LED emission via `MaterialPropertyBlock`.
- **`UltrasonicSensorController`** — Distance measurement via downward `Physics.Raycast`.
- **`ColorSensorController`** — Surface color/light detection via downward `Physics.Raycast`.
- **`SpeakerController`** — Audio playback and procedural tone generation.
- **`RobotConfiguration`** — `ScriptableObject` defining robot variants and enabled sensors/actuators.

### Scenario System (`Assets/Scripts/Scenario/`)

- **`ScenarioController`** — Activates one of several pre-built scenario `GameObject`s by `ScenarioType` enum.

### Adding New Block Types

1. Create a new `MonoBehaviour` class extending `Block` (or an existing subclass).
2. In `Start()`, call `base.Start()` and populate `this.connections` with `Connection` objects.
3. Implement `GetCode()` to return the block's Logo-like code string, appending `connectionNext.GetAttachedBlock()?.GetCode()` for sequential chaining.
4. Create a prefab with `RectTransform`, `LayoutElement`, and (optionally) `Shadow` components.
5. Register the prefab in the appropriate `BlocksPallete` section in `BlocksPallete.cs`.

## Tests

EditMode unit tests live in `Assets/Tests/EditMode/`. Run them from **Window → General → Test Runner → EditMode** in the Unity Editor.

The tests cover code generation for all block types and the `Connection` attach/detach behaviour.

## Robot Simulation Editor

A live control panel for testing the robot simulation without leaving the Unity Editor.

**Open:** `Window > Blocks > Robot Simulation`

**Features:**
- **Board power toggle** — powers the `BoardController` on/off; commands are only dispatched when the board is powered.
- **Configuration panel** — displays the active robot variant, max speed, and lets you switch the active scenario from a dropdown.
- **D-pad movement controls** — Forward / Backward / Turn Left / Turn Right / Brake buttons, with a speed slider.
- **Live sensor readings** — ultrasonic sensors shown as distance progress bars; color sensors shown as color swatches with light-level bars. Updates every frame.
- **LED controls** — per-LED on/off toggle showing current state.
- **Raw command input** — type any `BoardController` command (e.g. `setpower 3`, `ledon 0`, `beep`) and press Enter or Send.
- **Log** — timestamped history of all issued commands and events.

> The window requires Play Mode to be active; it shows an info message otherwise.
