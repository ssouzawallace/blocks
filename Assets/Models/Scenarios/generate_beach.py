"""
generate_beach.py — Blender 3.x / 4.x Python script
=====================================================
Run from Blender's Script Editor (Alt+P / "Run Script").

Generates a sandy beach scenario and exports it as Scenarios/Beach.fbx.

The scene contains:
  - Sandy ground plane
  - A water edge strip (blue, for colour-sensor boundary detection)
  - Driftwood logs (box obstacles)
  - Shells and rocks (small obstacles)
  - Cliff wall at the back
  - Sun disc in the sky backdrop

Hierarchy created
-----------------
Beach (root)
  ├─ Sand
  ├─ WaterEdge
  ├─ WaterStrip         (deep-blue water area)
  ├─ Driftwood_0 … _3
  ├─ Shell_0 … _5
  ├─ Rock_0 … _5
  ├─ CliffWall
  └─ SkyBackdrop
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


def add_sphere(name, location=(0, 0, 0), radius=0.2, parent=None):
    deselect_all()
    bpy.ops.mesh.primitive_uv_sphere_add(
        radius=radius, location=location, segments=10, ring_count=6
    )
    obj = bpy.context.active_object
    obj.name = name
    obj.data.name = name + "_Mesh"
    if parent:
        obj.parent = parent
        obj.matrix_parent_inverse = parent.matrix_world.inverted()
    return obj


def new_material(name, base_color, metallic=0.0, roughness=0.85,
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
# Build
# ---------------------------------------------------------------------------

def build_beach():
    clear_scene()

    mats = {
        "sand":       new_material("MAT_Sand",        (0.90, 0.82, 0.60)),
        "wet_sand":   new_material("MAT_WetSand",     (0.65, 0.58, 0.40)),
        "water":      new_material("MAT_BeachWater",  (0.10, 0.45, 0.80),
                                   emission_color=(0.10, 0.40, 0.75), emission_strength=0.4),
        "water_edge": new_material("MAT_WaterEdge",   (0.55, 0.80, 0.95)),
        "driftwood":  new_material("MAT_Driftwood",   (0.45, 0.35, 0.22)),
        "shell":      new_material("MAT_Shell",       (0.92, 0.87, 0.78)),
        "rock":       new_material("MAT_BeachRock",   (0.50, 0.48, 0.45)),
        "cliff":      new_material("MAT_Cliff",       (0.60, 0.52, 0.40)),
        "sky":        new_material("MAT_Sky",         (0.60, 0.82, 1.00),
                                   emission_color=(0.60, 0.82, 1.00), emission_strength=0.5),
        "sun":        new_material("MAT_Sun",         (1.00, 0.95, 0.70),
                                   emission_color=(1.00, 0.95, 0.70), emission_strength=8.0),
    }

    SZ = 8.0

    # Root empty
    deselect_all()
    bpy.ops.object.empty_add(type="PLAIN_AXES", location=(0, 0, 0))
    root = bpy.context.active_object
    root.name = "Beach"

    # Dry sand (main navigable area)
    sand = add_box("Sand", location=(0, -2.0, -0.05),
                   dimensions=(SZ * 2, SZ - 2.0, 0.10), parent=root)
    assign_material(sand, mats["sand"])

    # Wet sand strip (transition zone, darker – good colour sensor boundary)
    wet = add_box("WetSandStrip", location=(0, SZ / 2 - 1.0, -0.03),
                  dimensions=(SZ * 2, 2.0, 0.06), parent=root)
    assign_material(wet, mats["wet_sand"])

    # Water edge (white foam line)
    edge = add_box("WaterEdge", location=(0, SZ / 2, 0.01),
                   dimensions=(SZ * 2, 0.30, 0.02), parent=root)
    assign_material(edge, mats["water_edge"])

    # Water strip (extends to the front)
    water = add_box("WaterStrip", location=(0, SZ / 2 + 1.5 + 0.30, -0.04),
                    dimensions=(SZ * 2, 3.3, 0.08), parent=root)
    assign_material(water, mats["water"])

    # Driftwood logs (tilted boxes – obstacles)
    driftwood_data = [
        ("Driftwood_0", (-2.5, -1.0, 0.12), (1.4, 0.20, 0.22)),
        ("Driftwood_1", ( 2.0, -2.5, 0.10), (1.8, 0.18, 0.20)),
        ("Driftwood_2", (-4.0, -3.5, 0.14), (1.2, 0.22, 0.28)),
        ("Driftwood_3", ( 3.5, -0.5, 0.12), (1.6, 0.20, 0.24)),
    ]
    for dname, dloc, ddim in driftwood_data:
        d = add_box(dname, location=dloc, dimensions=ddim, parent=root)
        assign_material(d, mats["driftwood"])

    # Shells (flattened spheres)
    shell_positions = [
        (-1.0, -0.5), (1.5, -1.5), (-3.0, -2.0),
        ( 3.5, -3.0), ( 0.5,  0.2), (-2.0,  1.0),
    ]
    for si, (sx, sy) in enumerate(shell_positions):
        sh = add_sphere(f"Shell_{si}", location=(sx, sy, 0.04), radius=0.08, parent=root)
        sh.scale = (1.0, 1.0, 0.35)
        bpy.ops.object.select_all(action="DESELECT")
        sh.select_set(True)
        bpy.context.view_layer.objects.active = sh
        bpy.ops.object.transform_apply(scale=True)
        assign_material(sh, mats["shell"])

    # Rocks (small boulders)
    rock_positions = [
        (-4.5, -1.0, 0.20, (0.45, 0.40, 0.40)),
        ( 4.5,  0.5, 0.25, (0.50, 0.45, 0.40)),
        (-1.5, -4.0, 0.30, (0.55, 0.50, 0.45)),
        ( 1.0, -3.0, 0.18, (0.40, 0.38, 0.38)),
        (-3.5,  0.5, 0.22, (0.48, 0.44, 0.42)),
        ( 3.0, -4.5, 0.28, (0.52, 0.48, 0.44)),
    ]
    for ri, (rx, ry, rh, _rcolor) in enumerate(rock_positions):
        rk = add_box(f"Rock_{ri}", location=(rx, ry, rh / 2),
                     dimensions=(rh * 2, rh * 1.5, rh), parent=root)
        assign_material(rk, mats["rock"])

    # Cliff wall at the back
    cliff = add_box("CliffWall", location=(0, -SZ + 0.4, 2.0),
                    dimensions=(SZ * 2, 0.8, 4.0), parent=root)
    assign_material(cliff, mats["cliff"])

    # Sky backdrop
    sky = add_box("SkyBackdrop", location=(0, SZ + 0.5, 5.0),
                  dimensions=(SZ * 2.5, 0.1, SZ), parent=root)
    assign_material(sky, mats["sky"])

    # Sun disc
    sun = add_sphere("SunDisc", location=(4.0, SZ + 0.6, 7.5), radius=1.2, parent=root)
    assign_material(sun, mats["sun"])

    print("Beach scenario built successfully.")
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
    build_beach()
    script_dir = os.path.dirname(bpy.data.filepath) or os.getcwd()
    export_fbx(os.path.join(script_dir, "Beach.fbx"))
