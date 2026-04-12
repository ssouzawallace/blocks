# 3D Model Specifications for Blocks Project

This document describes the 3D models that need to be created in Blender and imported into Unity.
Refer to [Issue #22](https://github.com/ssouzawallace/blocks/issues/22) for the original requirements.

## Directory Structure

```
Assets/Models/
├── Board/           # Circuit board model
├── Robots/          # Robot variant models
└── Scenarios/       # World/environment models
```

## Export Guidelines

- Export from Blender as **FBX** format for Unity compatibility.
- Use a consistent scale (1 Blender unit = 1 meter in Unity).
- Apply all transforms before exporting (Ctrl+A in Blender).
- Name all meshes, bones, and materials descriptively.
- Keep poly counts reasonable for real-time rendering.

---

## 1. Board

A circuit board model representing the robot's controller board.

**Requirements:**
- Visible microcontroller chip area.
- Connectors / ports for motors and sensors.
- Status LED placeholder (named `StatusLED` in hierarchy).
- Attach `BoardController.cs` to the root GameObject.

---

## 2. Robot Variants

All robots are two-wheeled differential-drive robots. Each variant builds upon the previous one.

### 2.1 Basic Two-Wheeler

**Components:**
- Chassis / body.
- 2 wheels (named `LeftWheel` and `RightWheel`).
- Caster or support point at the rear.
- Power switch area.

**Unity Setup:**
- Attach `WheelController.cs` to each wheel object.
- Attach `RobotController.cs` to the root.

### 2.2 Two-Wheeler with Color/Light Sensors

Same as 2.1, plus:
- 2 color/light sensors mounted underneath the chassis (named `ColorSensor0`, `ColorSensor1`).
- Sensors should face downward (-Y direction).

**Unity Setup:**
- Attach `ColorSensorController.cs` to each sensor object.

### 2.3 Two-Wheeler with Color Sensors + Front Ultrasonic

Same as 2.2, plus:
- 1 ultrasonic distance sensor mounted on the front of the robot (named `UltrasonicSensor0`).
- Sensor should face forward (+Z direction).
- Visible transducer elements (the two "eyes" of a typical ultrasonic sensor).

**Unity Setup:**
- Attach `UltrasonicSensorController.cs` to the sensor object.

### 2.4 Two-Wheeler with Color Sensors + Ultrasonic Array

Same as 2.2, plus:
- 6 to 8 ultrasonic sensors arranged evenly around the robot (named `UltrasonicSensor0` through `UltrasonicSensor7`).
- Each sensor faces outward from the robot center.
- Suggested placement: front, front-left, left, rear-left, rear, rear-right, right, front-right.

**Unity Setup:**
- Attach `UltrasonicSensorController.cs` to each sensor object.

### Common Robot Features

All variants should include:
- **LEDs** (named `LED0`, `LED1`, etc.) — attach `LEDController.cs`.
- **Speaker** area (named `Speaker`) — attach `SpeakerController.cs` with an `AudioSource`.
- **Wheels must rotate** around their local X-axis when the robot moves.
- Materials should support emission for LEDs (use Standard or URP Lit shader).

---

## 3. Scenarios (Worlds)

Each scenario is a self-contained environment that the robot operates in.
Scenarios are activated/deactivated via `ScenarioController.cs`.

### 3.1 Classroom
- Indoor classroom with desks, chairs, and a whiteboard.
- Flat floor suitable for wheeled robot navigation.
- Obstacles: table legs, backpacks, books.

### 3.2 Outdoors
- Outdoor park or yard setting.
- Grass terrain with paths, trees, and benches.
- Varied surfaces for color sensor testing.

### 3.3 Lava / Volcano
- Volcanic terrain with lava flows and rock formations.
- Bright red/orange lava surfaces (good for color sensor testing).
- Dark rock paths for navigation.

### 3.4 Underwater
- Ocean floor environment with coral, sand, and seaweed.
- Blue-tinted lighting and particle effects for water.
- Underwater obstacles for ultrasonic sensor testing.

### 3.5 Beach
- Sandy beach with water edge, shells, and driftwood.
- Transition between sand and water surfaces.
- Open spaces with scattered obstacles.

### 3.6 Store / Mall
- Indoor shopping environment with shelves and aisles.
- Tiled floor with lines for line-following exercises.
- Structured layout good for navigation challenges.

---

## Interactive Behavior Reference

| Component | Behavior | Controller Script |
|-----------|----------|-------------------|
| Wheels | Rotate based on speed/direction | `WheelController.cs` |
| LEDs | Turn on/off, change color with emission | `LEDController.cs` |
| Speaker | Play sounds, beeps, and tones | `SpeakerController.cs` |
| Ultrasonic Sensor | Raycast forward, report distance | `UltrasonicSensorController.cs` |
| Color Sensor | Raycast downward, detect surface color | `ColorSensorController.cs` |
| Board | Command interpreter, status LED | `BoardController.cs` |
| Robot (root) | Coordinates all components | `RobotController.cs` |
| Scenario | Switches between environments | `ScenarioController.cs` |
