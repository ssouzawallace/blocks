"""
generate_robot_with_ultrasonic_array.py — Blender 3.x / 4.x Python script
==========================================================================
Run from Blender's Script Editor (Alt+P / "Run Script").

Generates the two-wheeled robot with color sensors AND an array of 8
ultrasonic distance sensors arranged evenly around the chassis, then exports
it as Robots/TwoWheelerWithUltrasonicArray.fbx.

Sensor placement (top view, robot faces +Y):
    US6 (left)     US0 (front)    US2 (right)
         US7    US5          US1    US3
                    US4 (rear)

Named UltrasonicSensor0 … UltrasonicSensor7 in this order:
  0 – front          (0°   / +Y)
  1 – front-right    (45°)
  2 – right          (90°  / +X)
  3 – rear-right     (135°)
  4 – rear           (180° / -Y)
  5 – rear-left      (225°)
  6 – left           (270° / -X)
  7 – front-left     (315°)

Hierarchy created
-----------------
TwoWheelerWithUltrasonicArray (root – attach RobotController.cs)
  ├─ Chassis
  ├─ LeftWheel               ← attach WheelController.cs
  ├─ RightWheel              ← attach WheelController.cs
  ├─ CasterWheel
  ├─ LED0                    ← attach LEDController.cs
  ├─ LED1                    ← attach LEDController.cs
  ├─ Speaker                 ← attach SpeakerController.cs + AudioSource
  ├─ ColorSensor0            ← attach ColorSensorController.cs
  ├─ ColorSensor1            ← attach ColorSensorController.cs
  ├─ UltrasonicSensor0       ← attach UltrasonicSensorController.cs
  │    ├─ Transducer0
  │    └─ Transducer1
  ├─ UltrasonicSensor1  …
  └─ UltrasonicSensor7
"""

import bpy
import math
import os


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def deselect_all():
    bpy.ops.object.select_all(action="DESELECT")


def add_box(name, location=(0, 0, 0), dimensions=(1, 1, 1), parent=None):
    deselect_all()
    bpy.ops.mesh.primitive_cube_add(size=1, location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.data.name = name + "_Mesh"
    obj.scale = dimensions
    bpy.ops.object.transform_apply(scale=True)
    if parent:
        obj.parent = parent
        obj.matrix_parent_inverse = parent.matrix_world.inverted()
    return obj


def add_cylinder(name, location=(0, 0, 0), radius=0.05, depth=0.04,
                 rot_x=0.0, parent=None):
    deselect_all()
    bpy.ops.mesh.primitive_cylinder_add(
        radius=radius, depth=depth,
        location=location, vertices=24,
        rotation=(rot_x, 0, 0)
    )
    obj = bpy.context.active_object
    obj.name = name
    obj.data.name = name + "_Mesh"
    bpy.ops.object.transform_apply(rotation=True)
    if parent:
        obj.parent = parent
        obj.matrix_parent_inverse = parent.matrix_world.inverted()
    return obj


def add_sphere(name, location=(0, 0, 0), radius=0.03, parent=None):
    deselect_all()
    bpy.ops.mesh.primitive_uv_sphere_add(
        radius=radius, location=location, segments=16, ring_count=8
    )
    obj = bpy.context.active_object
    obj.name = name
    obj.data.name = name + "_Mesh"
    if parent:
        obj.parent = parent
        obj.matrix_parent_inverse = parent.matrix_world.inverted()
    return obj


def new_material(name, base_color, metallic=0.0, roughness=0.6,
                 emission_color=None, emission_strength=0.0):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*base_color, 1.0)
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    if emission_color:
        bsdf.inputs["Emission Color"].default_value = (*emission_color, 1.0)
        bsdf.inputs["Emission Strength"].default_value = emission_strength
    return mat


def assign_material(obj, mat):
    if obj.data.materials:
        obj.data.materials[0] = mat
    else:
        obj.data.materials.append(mat)


# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------

WHEEL_RADIUS = 0.04
WHEEL_WIDTH  = 0.015
CHASSIS_W    = 0.14
CHASSIS_D    = 0.12
CHASSIS_H    = 0.05
WHEEL_Y      = CHASSIS_W / 2 + WHEEL_WIDTH / 2 + 0.002
WHEEL_Z      = 0.0

# Radius at which US sensors are placed around the chassis centre
US_ORBIT_RADIUS = 0.085   # slightly outside the chassis


# ---------------------------------------------------------------------------
# Ultrasonic sensor helper
# ---------------------------------------------------------------------------

def add_ultrasonic_sensor(index, angle_deg, us_z, mats, root):
    """Place one ultrasonic sensor (body + 2 transducer eyes) around the chassis.

    angle_deg=0 → front (+Y).  Increases clockwise when viewed from above.
    The sensor body faces outward and rotates to match the angle.
    """
    angle_rad = math.radians(angle_deg)
    # Position on orbit circle
    sx = math.sin(angle_rad) * US_ORBIT_RADIUS
    sy = math.cos(angle_rad) * US_ORBIT_RADIUS   # cos(0)=1 → front

    # Sensor body
    sensor = add_box(
        f"UltrasonicSensor{index}",
        location=(sx, sy, us_z),
        dimensions=(0.045, 0.010, 0.020),
        parent=root,
    )
    assign_material(sensor, mats["us_body"])

    # Rotate so the sensor faces outward (away from centre)
    # The sensor's "front" is along its local +Y, which should point outward.
    sensor.rotation_euler[2] = angle_rad
    deselect_all()
    sensor.select_set(True)
    bpy.context.view_layer.objects.active = sensor
    bpy.ops.object.transform_apply(rotation=True)

    # Transducer eyes
    t0 = add_cylinder(
        f"Transducer{index}_0",
        location=(sx - math.cos(angle_rad) * 0.012,
                  sy + math.sin(angle_rad) * 0.012 + math.cos(angle_rad) * 0.006,
                  us_z),
        radius=0.007, depth=0.006,
        rot_x=math.pi / 2, parent=sensor,
    )
    assign_material(t0, mats["us_eye"])

    t1 = add_cylinder(
        f"Transducer{index}_1",
        location=(sx + math.cos(angle_rad) * 0.012,
                  sy + math.sin(angle_rad) * 0.012 + math.cos(angle_rad) * 0.006,
                  us_z),
        radius=0.007, depth=0.006,
        rot_x=math.pi / 2, parent=sensor,
    )
    assign_material(t1, mats["us_eye"])

    return sensor


# ---------------------------------------------------------------------------
# Build
# ---------------------------------------------------------------------------

def build_robot_with_ultrasonic_array():
    clear_scene()

    mats = {
        "chassis":   new_material("MAT_Chassis",   (0.18, 0.18, 0.22), roughness=0.7),
        "wheel":     new_material("MAT_Wheel",     (0.08, 0.08, 0.08), roughness=0.9),
        "led_green": new_material("MAT_LED_Green", (0.1,  1.0,  0.1 ),
                                  emission_color=(0.1, 1.0, 0.1), emission_strength=2.0),
        "led_red":   new_material("MAT_LED_Red",   (1.0,  0.1,  0.1 ),
                                  emission_color=(1.0, 0.1, 0.1), emission_strength=2.0),
        "speaker":   new_material("MAT_Speaker",   (0.05, 0.05, 0.05), roughness=0.95),
        "caster":    new_material("MAT_Caster",    (0.5,  0.5,  0.5 ), metallic=0.6),
        "sensor":    new_material("MAT_ColorSensor", (0.1, 0.1, 0.8),
                                  emission_color=(0.2, 0.2, 1.0), emission_strength=0.8),
        "us_body":   new_material("MAT_USSensor",  (0.85, 0.85, 0.2), roughness=0.6),
        "us_eye":    new_material("MAT_USEye",     (0.1,  0.1,  0.1), roughness=0.5),
    }

    # Root
    deselect_all()
    bpy.ops.object.empty_add(type="PLAIN_AXES", location=(0, 0, 0))
    root = bpy.context.active_object
    root.name = "TwoWheelerWithUltrasonicArray"

    chassis_z = WHEEL_RADIUS + CHASSIS_H / 2

    # Chassis
    chassis = add_box("Chassis",
                      location=(0, 0, chassis_z),
                      dimensions=(CHASSIS_W, CHASSIS_D, CHASSIS_H),
                      parent=root)
    assign_material(chassis, mats["chassis"])

    # Wheels
    lw = add_cylinder("LeftWheel",
                      location=(-WHEEL_Y, 0, WHEEL_Z),
                      radius=WHEEL_RADIUS, depth=WHEEL_WIDTH,
                      rot_x=math.pi / 2, parent=root)
    assign_material(lw, mats["wheel"])

    rw = add_cylinder("RightWheel",
                      location=(WHEEL_Y, 0, WHEEL_Z),
                      radius=WHEEL_RADIUS, depth=WHEEL_WIDTH,
                      rot_x=math.pi / 2, parent=root)
    assign_material(rw, mats["wheel"])

    # Caster
    caster = add_sphere("CasterWheel",
                        location=(0, -(CHASSIS_D / 2 - 0.01), WHEEL_RADIUS * 0.5),
                        radius=WHEEL_RADIUS * 0.4, parent=root)
    assign_material(caster, mats["caster"])

    # LEDs
    led_z = chassis_z + CHASSIS_H / 2 + 0.005
    led0 = add_sphere("LED0", location=(-0.025, CHASSIS_D / 2 - 0.01, led_z),
                      radius=0.005, parent=root)
    assign_material(led0, mats["led_green"])

    led1 = add_sphere("LED1", location=(0.025, CHASSIS_D / 2 - 0.01, led_z),
                      radius=0.005, parent=root)
    assign_material(led1, mats["led_red"])

    # Speaker
    speaker = add_box("Speaker",
                      location=(0, CHASSIS_D / 2 + 0.001, chassis_z),
                      dimensions=(0.04, 0.002, 0.02), parent=root)
    assign_material(speaker, mats["speaker"])

    # Color sensors (underneath)
    sensor_bottom_z = chassis_z - CHASSIS_H / 2 - 0.006
    cs0 = add_box("ColorSensor0",
                  location=(-0.03, 0.02, sensor_bottom_z),
                  dimensions=(0.012, 0.012, 0.006), parent=root)
    assign_material(cs0, mats["sensor"])

    cs1 = add_box("ColorSensor1",
                  location=(0.03, 0.02, sensor_bottom_z),
                  dimensions=(0.012, 0.012, 0.006), parent=root)
    assign_material(cs1, mats["sensor"])

    # ── Ultrasonic sensor array (8 sensors, evenly spaced at 45° intervals) ──
    # Sensors are placed at mid-chassis height.
    us_z = chassis_z
    # Angles: 0=front, 45=front-right, 90=right, 135=rear-right,
    #         180=rear, 225=rear-left, 270=left, 315=front-left
    us_angles = [0, 45, 90, 135, 180, 225, 270, 315]
    for i, angle in enumerate(us_angles):
        add_ultrasonic_sensor(i, angle, us_z, mats, root)

    print("TwoWheelerWithUltrasonicArray built successfully.")
    return root


# ---------------------------------------------------------------------------
# Export
# ---------------------------------------------------------------------------

def export_fbx(output_path):
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.export_scene.fbx(
        filepath=output_path,
        use_selection=False,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_NONE",
        bake_space_transform=False,
        mesh_smooth_type="FACE",
        add_leaf_bones=False,
    )
    print(f"Exported FBX → {output_path}")


if __name__ == "__main__":
    build_robot_with_ultrasonic_array()
    script_dir = os.path.dirname(bpy.data.filepath) or os.getcwd()
    export_fbx(os.path.join(script_dir, "TwoWheelerWithUltrasonicArray.fbx"))
