"""
generate_outdoors.py — Blender 3.x / 4.x Python script
========================================================
Run from Blender's Script Editor (Alt+P / "Run Script").

Generates an outdoor park scenario and exports it as Scenarios/Outdoors.fbx.

The scene contains:
  - A large flat ground plane with varied surface colour patches (grass, path)
  - Boundary hedges / fences
  - Tree stumps (cylindrical obstacles for ultrasonic testing)
  - Benches (more obstacles)
  - A paved path with guide lines
  - A sky-blue backdrop plane

Hierarchy created
-----------------
Outdoors (root)
  ├─ Ground
  ├─ PathStrip
  ├─ PathLine_0 … PathLine_2   (white edge markings)
  ├─ GrassPatch_0 … _3        (coloured ground areas)
  ├─ Tree_0 … Tree_5          (cylindrical trunks)
  ├─ Bench_0 … Bench_3
  ├─ HedgeN / HedgeS / HedgeE / HedgeW  (perimeter)
  └─ SkyBackdrop
"""

import bpy
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


def add_cylinder(name, location=(0, 0, 0), radius=0.2, depth=1.0, parent=None):
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

def build_outdoors():
    clear_scene()

    mats = {
        "grass":      new_material("MAT_Grass",    (0.15, 0.50, 0.10)),
        "path":       new_material("MAT_Path",     (0.70, 0.65, 0.55)),
        "dark_grass": new_material("MAT_DarkGrass",(0.08, 0.35, 0.06)),
        "light_path": new_material("MAT_LightPath",(0.88, 0.82, 0.72)),
        "tree":       new_material("MAT_Tree",     (0.35, 0.22, 0.10)),
        "bench":      new_material("MAT_Bench",    (0.55, 0.38, 0.20)),
        "hedge":      new_material("MAT_Hedge",    (0.10, 0.40, 0.08)),
        "sky":        new_material("MAT_Sky",      (0.45, 0.72, 0.95),
                                   emission_color=(0.45, 0.72, 0.95), emission_strength=0.4),
        "line_white": new_material("MAT_LineWhite",(0.95, 0.95, 0.95)),
    }

    SZ = 10.0   # scene half-size

    # Root empty
    deselect_all()
    bpy.ops.object.empty_add(type="PLAIN_AXES", location=(0, 0, 0))
    root = bpy.context.active_object
    root.name = "Outdoors"

    # Ground
    ground = add_box("Ground", location=(0, 0, -0.05),
                     dimensions=(SZ * 2, SZ * 2, 0.10), parent=root)
    assign_material(ground, mats["grass"])

    # Central paved path (runs along Y axis)
    path = add_box("PathStrip", location=(0, 0, 0.01),
                   dimensions=(1.6, SZ * 2, 0.02), parent=root)
    assign_material(path, mats["path"])

    # Path edge lines
    line_x = [-0.75, 0.0, 0.75]
    for li, lx in enumerate(line_x):
        fl = add_box(f"PathLine_{li}",
                     location=(lx, 0, 0.021),
                     dimensions=(0.04, SZ * 2, 0.002), parent=root)
        assign_material(fl, mats["line_white"])

    # Grass colour patches (for colour-sensor testing)
    patch_data = [
        ("GrassPatch_0", (-3.5,  3.0, 0.005), (3.0, 3.0, 0.01), mats["dark_grass"]),
        ("GrassPatch_1", ( 3.5,  3.0, 0.005), (3.0, 3.0, 0.01), mats["light_path"]),
        ("GrassPatch_2", (-3.5, -3.0, 0.005), (3.0, 3.0, 0.01), mats["light_path"]),
        ("GrassPatch_3", ( 3.5, -3.0, 0.005), (3.0, 3.0, 0.01), mats["dark_grass"]),
    ]
    for name, loc, dim, mat in patch_data:
        p = add_box(name, location=loc, dimensions=dim, parent=root)
        assign_material(p, mat)

    # Tree trunks (cylindrical obstacles)
    tree_positions = [
        (-3.0,  4.0), (3.0,  4.0),
        (-3.0,  0.0), (3.0,  0.0),
        (-3.0, -4.0), (3.0, -4.0),
    ]
    for ti, (tx, ty) in enumerate(tree_positions):
        tree = add_cylinder(f"Tree_{ti}",
                            location=(tx, ty, 0.75),
                            radius=0.18, depth=1.5, parent=root)
        assign_material(tree, mats["tree"])

    # Benches
    bench_data = [
        ("Bench_0", (-2.0,  5.5)),
        ("Bench_1", ( 2.0,  5.5)),
        ("Bench_2", (-2.0, -5.5)),
        ("Bench_3", ( 2.0, -5.5)),
    ]
    for bname, (bx, by) in bench_data:
        bench = add_box(bname, location=(bx, by, 0.22),
                        dimensions=(1.2, 0.40, 0.44), parent=root)
        assign_material(bench, mats["bench"])

    # Perimeter hedges
    HEDGE_H = 1.2
    hedges = [
        ("HedgeN", (0,  SZ, HEDGE_H / 2), (SZ * 2, 0.5, HEDGE_H)),
        ("HedgeS", (0, -SZ, HEDGE_H / 2), (SZ * 2, 0.5, HEDGE_H)),
        ("HedgeE", ( SZ, 0, HEDGE_H / 2), (0.5, SZ * 2, HEDGE_H)),
        ("HedgeW", (-SZ, 0, HEDGE_H / 2), (0.5, SZ * 2, HEDGE_H)),
    ]
    for hname, hloc, hdim in hedges:
        h = add_box(hname, location=hloc, dimensions=hdim, parent=root)
        assign_material(h, mats["hedge"])

    # Sky backdrop
    sky = add_box("SkyBackdrop", location=(0, SZ + 0.5, SZ),
                  dimensions=(SZ * 2.5, 0.1, SZ * 2), parent=root)
    assign_material(sky, mats["sky"])

    print("Outdoors scenario built successfully.")
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
    build_outdoors()
    script_dir = os.path.dirname(bpy.data.filepath) or os.getcwd()
    export_fbx(os.path.join(script_dir, "Outdoors.fbx"))
