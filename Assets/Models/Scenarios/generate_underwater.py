"""
generate_underwater.py — Blender 3.x / 4.x Python script
==========================================================
Run from Blender's Script Editor (Alt+P / "Run Script").

Generates an underwater ocean-floor scenario and exports it as
Scenarios/Underwater.fbx.

The scene contains:
  - A sandy ocean floor with varied dark/light patches (colour sensor)
  - Coral formations (obstacle clusters)
  - Rock pillars (ultrasonic obstacles)
  - Seaweed patches (tall flat obstacles)
  - A blue-tinted water-surface plane above
  - Scattered shells and stones for detail

Hierarchy created
-----------------
Underwater (root)
  ├─ OceanFloor
  ├─ WaterSurface
  ├─ SandPatch_0 … _3   (colour variation areas)
  ├─ Coral_0 … Coral_7
  ├─ RockPillar_0 … _3
  ├─ Seaweed_0 … _5
  └─ Shell_0 … _5
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


def add_cylinder(name, location=(0, 0, 0), radius=0.3, depth=1.0, parent=None):
    deselect_all()
    bpy.ops.mesh.primitive_cylinder_add(
        radius=radius, depth=depth, location=location, vertices=12
    )
    obj = bpy.context.active_object
    obj.name = name
    obj.data.name = name + "_Mesh"
    if parent:
        obj.parent = parent
        obj.matrix_parent_inverse = parent.matrix_world.inverted()
    return obj


def add_sphere(name, location=(0, 0, 0), radius=0.3, parent=None):
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


def new_material(name, base_color, metallic=0.0, roughness=0.8,
                 emission_color=None, emission_strength=0.0, alpha=1.0):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*base_color, 1.0)
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    if alpha < 1.0:
        bsdf.inputs["Alpha"].default_value = alpha
        mat.blend_method = "BLEND"
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

def build_underwater():
    clear_scene()

    mats = {
        "sand":       new_material("MAT_Sand",       (0.76, 0.70, 0.50)),
        "dark_sand":  new_material("MAT_DarkSand",   (0.50, 0.45, 0.30)),
        "coral_red":  new_material("MAT_CoralRed",   (0.90, 0.30, 0.20),
                                   emission_color=(0.90, 0.30, 0.20), emission_strength=0.5),
        "coral_pink": new_material("MAT_CoralPink",  (0.95, 0.55, 0.60),
                                   emission_color=(0.95, 0.55, 0.60), emission_strength=0.3),
        "rock":       new_material("MAT_UWRock",     (0.35, 0.32, 0.28)),
        "seaweed":    new_material("MAT_Seaweed",    (0.10, 0.45, 0.15),
                                   emission_color=(0.10, 0.50, 0.18), emission_strength=0.3),
        "shell":      new_material("MAT_Shell",      (0.90, 0.85, 0.75)),
        "water":      new_material("MAT_Water",      (0.05, 0.30, 0.70),
                                   emission_color=(0.05, 0.25, 0.65), emission_strength=0.4,
                                   alpha=0.35),
    }

    SZ = 8.0

    # Root empty
    deselect_all()
    bpy.ops.object.empty_add(type="PLAIN_AXES", location=(0, 0, 0))
    root = bpy.context.active_object
    root.name = "Underwater"

    # Ocean floor
    floor = add_box("OceanFloor", location=(0, 0, -0.05),
                    dimensions=(SZ * 2, SZ * 2, 0.10), parent=root)
    assign_material(floor, mats["sand"])

    # Water surface (semi-transparent, high above)
    water = add_box("WaterSurface", location=(0, 0, 5.0),
                    dimensions=(SZ * 2, SZ * 2, 0.10), parent=root)
    assign_material(water, mats["water"])

    # Sand colour patches (for colour-sensor variation)
    sand_patches = [
        ("SandPatch_0", (-3.0,  3.0, 0.005), (3.5, 3.5, 0.01)),
        ("SandPatch_1", ( 3.0,  3.0, 0.005), (3.0, 3.0, 0.01)),
        ("SandPatch_2", (-3.0, -3.0, 0.005), (3.0, 3.5, 0.01)),
        ("SandPatch_3", ( 3.0, -3.0, 0.005), (3.5, 3.0, 0.01)),
    ]
    for pname, ploc, pdim in sand_patches:
        p = add_box(pname, location=ploc, dimensions=pdim, parent=root)
        assign_material(p, mats["dark_sand"])

    # Coral formations (alternating red/pink spheres on cylinders)
    coral_positions = [
        (-2.5,  2.0), ( 2.5,  2.0),
        (-1.5,  4.5), ( 1.5,  4.5),
        (-3.5,  0.0), ( 3.5,  0.0),
        (-2.0, -3.0), ( 2.0, -3.0),
    ]
    for ci, (cx, cy) in enumerate(coral_positions):
        # Stem
        stem = add_cylinder(f"Coral_{ci}",
                            location=(cx, cy, 0.35),
                            radius=0.06, depth=0.7, parent=root)
        assign_material(stem, mats["coral_red"] if ci % 2 == 0 else mats["coral_pink"])
        # Top bulb
        bulb = add_sphere(f"CoralBulb_{ci}",
                          location=(cx, cy, 0.75),
                          radius=0.18, parent=root)
        assign_material(bulb, mats["coral_pink"] if ci % 2 == 0 else mats["coral_red"])

    # Rock pillars (large cylinders – good ultrasonic obstacles)
    rock_positions = [(-4.0, 3.5), (4.0, 3.5), (-4.0, -3.5), (4.0, -3.5)]
    for ri, (rx, ry) in enumerate(rock_positions):
        pillar = add_cylinder(f"RockPillar_{ri}",
                              location=(rx, ry, 1.0),
                              radius=0.45, depth=2.0, parent=root)
        assign_material(pillar, mats["rock"])

    # Seaweed (thin tall boxes, swaying effect via rotation)
    seaweed_positions = [
        (-1.0,  3.0), (1.0,  3.0),
        (-3.0,  1.5), (3.0, -1.5),
        ( 0.5, -2.5), (-2.5, -1.0),
    ]
    for si, (sx, sy) in enumerate(seaweed_positions):
        sw = add_box(f"Seaweed_{si}",
                     location=(sx, sy, 0.6),
                     dimensions=(0.06, 0.04, 1.2), parent=root)
        assign_material(sw, mats["seaweed"])
        # Tilt slightly
        sw.rotation_euler[0] = math.radians(8 * (1 if si % 2 == 0 else -1))
        bpy.ops.object.select_all(action="DESELECT")
        sw.select_set(True)
        bpy.context.view_layer.objects.active = sw
        bpy.ops.object.transform_apply(rotation=True)

    # Shells (flattened spheres on the floor)
    shell_positions = [
        ( 0.5,  1.0), (-1.5,  2.5), ( 2.5,  1.0),
        (-0.5, -1.0), ( 1.5, -2.0), (-2.0, -0.5),
    ]
    for shi, (shx, shy) in enumerate(shell_positions):
        shell = add_sphere(f"Shell_{shi}",
                           location=(shx, shy, 0.06),
                           radius=0.10, parent=root)
        shell.scale = (1.0, 1.0, 0.4)
        bpy.ops.object.select_all(action="DESELECT")
        shell.select_set(True)
        bpy.context.view_layer.objects.active = shell
        bpy.ops.object.transform_apply(scale=True)
        assign_material(shell, mats["shell"])

    print("Underwater scenario built successfully.")
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
    build_underwater()
    script_dir = os.path.dirname(bpy.data.filepath) or os.getcwd()
    export_fbx(os.path.join(script_dir, "Underwater.fbx"))
