"""
generate_lava_volcano.py — Blender 3.x / 4.x Python script
============================================================
Run from Blender's Script Editor (Alt+P / "Run Script").

Generates a volcanic lava landscape scenario and exports it as
Scenarios/LavaVolcano.fbx.

The scene contains:
  - A dark rocky ground plane
  - Bright red/orange lava "rivers" (great for colour-sensor testing)
  - Rock formations (cylindrical / box obstacles for ultrasonic sensors)
  - Volcano cone in the background
  - Glowing lava pool at the base of the volcano

Hierarchy created
-----------------
LavaVolcano (root)
  ├─ GroundBase
  ├─ LavaRiver_0 … _3
  ├─ Rock_0 … Rock_7
  ├─ VolcanoCone
  ├─ LavaPool
  └─ EmberGlow_0 … _5   (small glowing spheres to convey ambient lighting)
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


def add_cylinder(name, location=(0, 0, 0), radius=0.5, depth=1.0, parent=None):
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


def add_cone(name, location=(0, 0, 0), radius1=3.0, radius2=0.5, depth=5.0, parent=None):
    deselect_all()
    bpy.ops.mesh.primitive_cone_add(
        radius1=radius1, radius2=radius2, depth=depth,
        location=location, vertices=16
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
        radius=radius, location=location, segments=8, ring_count=4
    )
    obj = bpy.context.active_object
    obj.name = name
    obj.data.name = name + "_Mesh"
    if parent:
        obj.parent = parent
        obj.matrix_parent_inverse = parent.matrix_world.inverted()
    return obj


def new_material(name, base_color, metallic=0.0, roughness=0.9,
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

def build_lava_volcano():
    clear_scene()

    mats = {
        "rock":       new_material("MAT_Rock",       (0.15, 0.12, 0.10)),
        "dark_rock":  new_material("MAT_DarkRock",   (0.08, 0.06, 0.05)),
        "lava":       new_material("MAT_Lava",       (0.90, 0.25, 0.02),
                                   emission_color=(1.00, 0.35, 0.00), emission_strength=3.0),
        "lava_hot":   new_material("MAT_LavaHot",    (1.00, 0.60, 0.00),
                                   emission_color=(1.00, 0.70, 0.10), emission_strength=5.0),
        "volcano":    new_material("MAT_Volcano",    (0.20, 0.15, 0.12), roughness=0.95),
        "ember":      new_material("MAT_Ember",      (1.00, 0.45, 0.00),
                                   emission_color=(1.00, 0.55, 0.05), emission_strength=4.0),
    }

    SZ = 8.0

    # Root empty
    deselect_all()
    bpy.ops.object.empty_add(type="PLAIN_AXES", location=(0, 0, 0))
    root = bpy.context.active_object
    root.name = "LavaVolcano"

    # Ground base (dark volcanic rock)
    ground = add_box("GroundBase", location=(0, 0, -0.05),
                     dimensions=(SZ * 2, SZ * 2, 0.10), parent=root)
    assign_material(ground, mats["dark_rock"])

    # Lava rivers (bright, navigable obstacle for colour sensor)
    rivers = [
        ("LavaRiver_0", (0,    2.0, 0.01), (1.2, 8.0, 0.02)),   # centre channel
        ("LavaRiver_1", (-3.0, 0.0, 0.01), (0.8, 6.0, 0.02)),   # left branch
        ("LavaRiver_2", ( 3.0, 1.5, 0.01), (0.6, 5.0, 0.02)),   # right branch
        ("LavaRiver_3", ( 0.0,-3.0, 0.01), (2.5, 1.0, 0.02)),   # pool at base
    ]
    for rname, rloc, rdim in rivers:
        r = add_box(rname, location=rloc, dimensions=rdim, parent=root)
        assign_material(r, mats["lava"])

    # Rock formations (box + cylinder obstacles)
    rock_data = [
        ("Rock_0", (-2.0,  3.0, 0.3), (0.6, 0.5, 0.6)),
        ("Rock_1", ( 2.5,  3.5, 0.4), (0.8, 0.6, 0.8)),
        ("Rock_2", (-4.0,  1.0, 0.5), (1.0, 0.8, 1.0)),
        ("Rock_3", ( 4.0,  0.5, 0.35),(0.7, 0.5, 0.7)),
        ("Rock_4", (-3.5, -2.0, 0.4), (0.9, 0.6, 0.8)),
        ("Rock_5", ( 3.5, -1.5, 0.3), (0.6, 0.6, 0.6)),
        ("Rock_6", (-1.5, -3.5, 0.25),(0.5, 0.7, 0.5)),
        ("Rock_7", ( 1.5, -3.0, 0.35),(0.7, 0.5, 0.7)),
    ]
    for rname, rloc, rdim in rock_data:
        r = add_box(rname, location=rloc, dimensions=rdim, parent=root)
        assign_material(r, mats["rock"])

    # Volcano cone (background, large)
    volcano = add_cone("VolcanoCone",
                       location=(0, SZ - 1.0, 2.5),
                       radius1=4.0, radius2=0.8, depth=5.0, parent=root)
    assign_material(volcano, mats["volcano"])

    # Lava pool at the front of the volcano
    lava_pool = add_cylinder("LavaPool",
                             location=(0, SZ - 2.5, 0.01),
                             radius=1.5, depth=0.05, parent=root)
    assign_material(lava_pool, mats["lava_hot"])

    # Ember glow spheres (scattered for atmosphere)
    ember_positions = [
        (-1.0, 1.0, 0.15), (1.5, 0.5, 0.10),
        (-2.5, 2.5, 0.20), (2.0, 3.0, 0.12),
        ( 0.5,-1.5, 0.08), (-1.5,-2.0, 0.18),
    ]
    for ei, (ex, ey, ez) in enumerate(ember_positions):
        emb = add_sphere(f"EmberGlow_{ei}", location=(ex, ey, ez),
                         radius=0.06, parent=root)
        assign_material(emb, mats["ember"])

    print("LavaVolcano scenario built successfully.")
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
    build_lava_volcano()
    script_dir = os.path.dirname(bpy.data.filepath) or os.getcwd()
    export_fbx(os.path.join(script_dir, "LavaVolcano.fbx"))
