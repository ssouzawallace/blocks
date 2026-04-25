"""
generate_board.py — Blender 3.x / 4.x Python script
======================================================
Run from Blender's Script Editor (open the Text Editor, paste this script,
then press Alt+P or click "Run Script").

Generates the circuit-board model for the Blocks robotics project and exports
it as FBX next to this script file.

Hierarchy created
-----------------
Board (root – attach BoardController.cs)
  ├─ BoardBody
  ├─ CPU
  ├─ LED_Green          ← status LED green  (attach LEDController.cs)
  ├─ LED_Red            ← status LED red    (attach LEDController.cs)
  ├─ LED_Yellow         ← status LED yellow (attach LEDController.cs)
  ├─ LED_Blue           ← status LED blue   (attach LEDController.cs)
  ├─ SensorPort0        ← sensor connector 0
  ├─ SensorPort1
  ├─ SensorPort2
  ├─ SensorPort3
  ├─ ActuatorPort0      ← actuator/motor connector 0
  ├─ ActuatorPort1
  ├─ ActuatorPort2
  ├─ ActuatorPort3
  ├─ EthernetPort
  ├─ WiFiAntenna
  ├─ BluetoothModule
  ├─ BLEModule
  ├─ DisplayConnector
  ├─ IOHeader
  ├─ RXPin
  ├─ TXPin
  ├─ SerialConnector
  └─ WireTerminals
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
    for collection in bpy.data.collections:
        bpy.data.collections.remove(collection)


def deselect_all():
    bpy.ops.object.select_all(action="DESELECT")


def add_box(name, location=(0, 0, 0), dimensions=(1, 1, 1), parent=None):
    """Create a cube and scale it to the requested dimensions."""
    deselect_all()
    bpy.ops.mesh.primitive_cube_add(size=1, location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.data.name = name + "_Mesh"
    obj.scale = dimensions
    bpy.ops.object.transform_apply(scale=True)
    if parent is not None:
        obj.parent = parent
        obj.matrix_parent_inverse = parent.matrix_world.inverted()
    return obj


def add_cylinder(name, location=(0, 0, 0), radius=0.1, depth=0.1, parent=None):
    deselect_all()
    bpy.ops.mesh.primitive_cylinder_add(
        radius=radius, depth=depth, location=location, vertices=16
    )
    obj = bpy.context.active_object
    obj.name = name
    obj.data.name = name + "_Mesh"
    if parent is not None:
        obj.parent = parent
        obj.matrix_parent_inverse = parent.matrix_world.inverted()
    return obj


def add_sphere(name, location=(0, 0, 0), radius=0.05, parent=None):
    deselect_all()
    bpy.ops.mesh.primitive_uv_sphere_add(
        radius=radius, location=location, segments=16, ring_count=8
    )
    obj = bpy.context.active_object
    obj.name = name
    obj.data.name = name + "_Mesh"
    if parent is not None:
        obj.parent = parent
        obj.matrix_parent_inverse = parent.matrix_world.inverted()
    return obj


def new_material(name, base_color, metallic=0.0, roughness=0.6,
                 emission_color=None, emission_strength=0.0):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    bsdf = nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*base_color, 1.0)
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    if emission_color is not None:
        bsdf.inputs["Emission Color"].default_value = (*emission_color, 1.0)
        bsdf.inputs["Emission Strength"].default_value = emission_strength
    return mat


def assign_material(obj, mat):
    if obj.data.materials:
        obj.data.materials[0] = mat
    else:
        obj.data.materials.append(mat)


# ---------------------------------------------------------------------------
# Materials
# ---------------------------------------------------------------------------

def create_materials():
    mats = {}
    mats["pcb"]          = new_material("MAT_PCB",        (0.04, 0.25, 0.08), roughness=0.8)
    mats["chip"]         = new_material("MAT_Chip",       (0.05, 0.05, 0.05), metallic=0.3)
    mats["connector"]    = new_material("MAT_Connector",  (0.15, 0.15, 0.15), metallic=0.8, roughness=0.3)
    mats["antenna"]      = new_material("MAT_Antenna",    (0.9,  0.9,  0.9 ), metallic=0.9, roughness=0.2)
    mats["led_green"]    = new_material("MAT_LED_Green",  (0.0,  1.0,  0.0 ),
                                        emission_color=(0.0, 1.0, 0.0),
                                        emission_strength=2.0)
    mats["led_red"]      = new_material("MAT_LED_Red",    (1.0,  0.0,  0.0 ),
                                        emission_color=(1.0, 0.0, 0.0),
                                        emission_strength=2.0)
    mats["led_yellow"]   = new_material("MAT_LED_Yellow", (1.0,  0.8,  0.0 ),
                                        emission_color=(1.0, 0.8, 0.0),
                                        emission_strength=2.0)
    mats["led_blue"]     = new_material("MAT_LED_Blue",   (0.0,  0.3,  1.0 ),
                                        emission_color=(0.0, 0.3, 1.0),
                                        emission_strength=2.0)
    mats["gold"]         = new_material("MAT_GoldPin",    (1.0,  0.8,  0.0 ), metallic=1.0, roughness=0.2)
    return mats


# ---------------------------------------------------------------------------
# Board construction
# ---------------------------------------------------------------------------

def build_board():
    clear_scene()
    mats = create_materials()

    # ── Root empty (attach BoardController.cs here) ─────────────────────────
    deselect_all()
    bpy.ops.object.empty_add(type="PLAIN_AXES", location=(0, 0, 0))
    root = bpy.context.active_object
    root.name = "Board"

    # ── PCB body ─────────────────────────────────────────────────────────────
    #   160 mm × 100 mm × 2 mm  →  0.16 × 0.10 × 0.002 m
    body = add_box("BoardBody", location=(0, 0, 0),
                   dimensions=(0.16, 0.10, 0.002), parent=root)
    assign_material(body, mats["pcb"])

    # ── CPU chip (centre) ────────────────────────────────────────────────────
    cpu = add_box("CPU", location=(0, 0, 0.003),
                  dimensions=(0.02, 0.02, 0.004), parent=root)
    assign_material(cpu, mats["chip"])

    # ── Status LEDs (4 indicators in a row near the top-right corner) ────────
    # Green = powered/ready, Red = error state, Yellow = busy/processing,
    # Blue = connected to programming interface.
    # Spacing: 7 mm between centres, starting at x = 0.035.
    LED_RADIUS  = 0.004
    LED_HEIGHT  = 0.004   # height above PCB surface
    LED_ROW_Y   = 0.040   # Y position of the LED row
    led_configs = [
        ("LED_Green",  0.035, mats["led_green"]),
        ("LED_Red",    0.042, mats["led_red"]),
        ("LED_Yellow", 0.049, mats["led_yellow"]),
        ("LED_Blue",   0.056, mats["led_blue"]),
    ]
    for led_name, lx, mat in led_configs:
        led_obj = add_sphere(led_name, location=(lx, LED_ROW_Y, LED_HEIGHT),
                             radius=LED_RADIUS, parent=root)
        assign_material(led_obj, mat)

    # ── Sensor ports (right edge, top half) ─────────────────────────────────
    sensor_y_positions = [0.035, 0.015, -0.015, -0.035]
    for i, sy in enumerate(sensor_y_positions):
        port = add_box(f"SensorPort{i}", location=(0.072, sy, 0.004),
                       dimensions=(0.010, 0.008, 0.006), parent=root)
        assign_material(port, mats["connector"])

    # ── Actuator ports (left edge) ───────────────────────────────────────────
    actuator_y_positions = [0.035, 0.015, -0.015, -0.035]
    for i, ay in enumerate(actuator_y_positions):
        port = add_box(f"ActuatorPort{i}", location=(-0.072, ay, 0.004),
                       dimensions=(0.010, 0.008, 0.006), parent=root)
        assign_material(port, mats["connector"])

    # ── Ethernet port (front edge, large) ────────────────────────────────────
    eth = add_box("EthernetPort", location=(0.02, -0.053, 0.007),
                  dimensions=(0.016, 0.006, 0.013), parent=root)
    assign_material(eth, mats["connector"])

    # ── WiFi antenna (small flat paddle on front edge) ───────────────────────
    wifi = add_box("WiFiAntenna", location=(-0.02, -0.053, 0.005),
                   dimensions=(0.018, 0.002, 0.010), parent=root)
    assign_material(wifi, mats["antenna"])

    # ── Bluetooth module ─────────────────────────────────────────────────────
    bt = add_box("BluetoothModule", location=(-0.05, 0.0, 0.004),
                 dimensions=(0.012, 0.010, 0.003), parent=root)
    assign_material(bt, mats["chip"])

    # ── BLE module (beside Bluetooth) ────────────────────────────────────────
    ble = add_box("BLEModule", location=(-0.05, 0.015, 0.004),
                  dimensions=(0.010, 0.008, 0.003), parent=root)
    assign_material(ble, mats["chip"])

    # ── Display connector ─────────────────────────────────────────────────────
    disp = add_box("DisplayConnector", location=(0.04, 0.053, 0.004),
                   dimensions=(0.030, 0.005, 0.006), parent=root)
    assign_material(disp, mats["connector"])

    # ── I/O header (2×8 pin strip) ────────────────────────────────────────────
    io_header = add_box("IOHeader", location=(-0.02, 0.053, 0.005),
                        dimensions=(0.032, 0.005, 0.008), parent=root)
    assign_material(io_header, mats["connector"])

    # ── RX pin ───────────────────────────────────────────────────────────────
    rx = add_cylinder("RXPin", location=(0.055, 0.053, 0.005),
                      radius=0.002, depth=0.008, parent=root)
    assign_material(rx, mats["gold"])

    # ── TX pin ───────────────────────────────────────────────────────────────
    tx = add_cylinder("TXPin", location=(0.060, 0.053, 0.005),
                      radius=0.002, depth=0.008, parent=root)
    assign_material(tx, mats["gold"])

    # ── Serial connector ─────────────────────────────────────────────────────
    serial = add_box("SerialConnector", location=(0.05, -0.053, 0.005),
                     dimensions=(0.012, 0.006, 0.009), parent=root)
    assign_material(serial, mats["connector"])

    # ── Wire terminals (screw terminal block) ─────────────────────────────────
    wire_term = add_box("WireTerminals", location=(-0.03, -0.053, 0.006),
                        dimensions=(0.024, 0.006, 0.010), parent=root)
    assign_material(wire_term, mats["connector"])

    print("Board model built successfully.")
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


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

if __name__ == "__main__":
    build_board()

    script_dir = os.path.dirname(bpy.data.filepath) or os.getcwd()
    fbx_path = os.path.join(script_dir, "Board.fbx")
    export_fbx(fbx_path)
