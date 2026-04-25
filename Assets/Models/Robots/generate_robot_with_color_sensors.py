"""
generate_robot_with_color_sensors.py — Blender 3.x / 4.x Python script
=======================================================================
Run from Blender's Script Editor (Alt+P / "Run Script").

Generates the two-wheeled robot with two color/light sensors underneath the
chassis, then exports it as Robots/TwoWheelerWithColorSensors.fbx.

Hierarchy created
-----------------
TwoWheelerWithColorSensors (root – attach RobotController.cs)
  ├─ Chassis
  ├─ LeftWheel           ← attach WheelController.cs
  ├─ RightWheel          ← attach WheelController.cs
  ├─ CasterWheel
  ├─ LED0                ← attach LEDController.cs
  ├─ LED1                ← attach LEDController.cs
  ├─ Speaker             ← attach SpeakerController.cs + AudioSource
  ├─ ColorSensor0        ← attach ColorSensorController.cs (faces -Y / downward)
  └─ ColorSensor1        ← attach ColorSensorController.cs (faces -Y / downward)
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


# ---------------------------------------------------------------------------
# Build
# ---------------------------------------------------------------------------

def build_robot_with_color_sensors():
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
    }

    # Root
    deselect_all()
    bpy.ops.object.empty_add(type="PLAIN_AXES", location=(0, 0, 0))
    root = bpy.context.active_object
    root.name = "TwoWheelerWithColorSensors"

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

    # ── Color/Light sensors mounted underneath the chassis, facing downward ──
    # The sensors are placed at the bottom of the chassis and oriented so their
    # local -Z axis points toward the ground.  ColorSensorController casts a
    # ray in the -transform.up direction, which is -Y in world space when the
    # object's up axis is +Y.  We therefore rotate them 90° around X so that
    # their forward direction becomes downward.
    sensor_bottom_z = chassis_z - CHASSIS_H / 2 - 0.006   # just below chassis

    # Left sensor (sensor 0)
    cs0 = add_box("ColorSensor0",
                  location=(-0.03, 0.02, sensor_bottom_z),
                  dimensions=(0.012, 0.012, 0.006), parent=root)
    assign_material(cs0, mats["sensor"])
    # Rotate 90° around X so the sensor faces -Z (downward)
    cs0.rotation_euler[0] = math.pi / 2
    bpy.ops.object.select_all(action="DESELECT")
    cs0.select_set(True)
    bpy.context.view_layer.objects.active = cs0
    bpy.ops.object.transform_apply(rotation=True)

    # Right sensor (sensor 1)
    cs1 = add_box("ColorSensor1",
                  location=(0.03, 0.02, sensor_bottom_z),
                  dimensions=(0.012, 0.012, 0.006), parent=root)
    assign_material(cs1, mats["sensor"])
    cs1.rotation_euler[0] = math.pi / 2
    bpy.ops.object.select_all(action="DESELECT")
    cs1.select_set(True)
    bpy.context.view_layer.objects.active = cs1
    bpy.ops.object.transform_apply(rotation=True)

    print("TwoWheelerWithColorSensors built successfully.")
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
    build_robot_with_color_sensors()
    script_dir = os.path.dirname(bpy.data.filepath) or os.getcwd()
    export_fbx(os.path.join(script_dir, "TwoWheelerWithColorSensors.fbx"))
