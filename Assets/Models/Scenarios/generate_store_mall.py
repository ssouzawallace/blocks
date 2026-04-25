"""
generate_store_mall.py — Blender 3.x / 4.x Python script
==========================================================
Run from Blender's Script Editor (Alt+P / "Run Script").

Generates an indoor store/mall scenario and exports it as
Scenarios/StoreMall.fbx.

The scene contains:
  - Tiled floor with black aisle guide lines (line-following exercises)
  - Perimeter walls
  - Ceiling with overhead light panels
  - Rows of product shelves (tall box obstacles)
  - Checkout counter at one end
  - Shopping cart obstacles

Hierarchy created
-----------------
StoreMall (root)
  ├─ Floor
  ├─ Ceiling
  ├─ WallFront / WallBack / WallLeft / WallRight
  ├─ AisleGuide_0 … _3   (black floor lines)
  ├─ TilePattern_0 … _3  (light-coloured floor tile rows for sensor testing)
  ├─ Shelf_0 … Shelf_7   (two aisles of 4 shelves each)
  ├─ ShelfDivider_0 … _3 (cross-aisle dividers)
  ├─ CheckoutCounter
  ├─ Cart_0 … Cart_3
  └─ OverheadLight_0 … _5
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


def new_material(name, base_color, metallic=0.0, roughness=0.7,
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

def build_store_mall():
    clear_scene()

    mats = {
        "floor":      new_material("MAT_StoreFloor",   (0.88, 0.88, 0.85), roughness=0.5),
        "tile_light": new_material("MAT_TileLight",    (0.95, 0.95, 0.92)),
        "tile_dark":  new_material("MAT_TileDark",     (0.70, 0.70, 0.68)),
        "wall":       new_material("MAT_StoreWall",    (0.92, 0.90, 0.88)),
        "ceiling":    new_material("MAT_StoreCeiling", (0.96, 0.96, 0.96)),
        "shelf":      new_material("MAT_Shelf",        (0.60, 0.58, 0.55), metallic=0.4, roughness=0.5),
        "shelf_item": new_material("MAT_ShelfItem",    (0.85, 0.30, 0.20)),
        "counter":    new_material("MAT_Counter",      (0.25, 0.25, 0.25), metallic=0.2),
        "cart":       new_material("MAT_Cart",         (0.80, 0.80, 0.80), metallic=0.7, roughness=0.3),
        "guide_line": new_material("MAT_GuideLine",    (0.05, 0.05, 0.05)),
        "light":      new_material("MAT_OverheadLight",(1.00, 1.00, 0.95),
                                   emission_color=(1.00, 1.00, 0.95), emission_strength=4.0),
    }

    # Room: 10 m wide × 14 m deep × 3.5 m tall
    RW, RD, RH = 10.0, 14.0, 3.5
    WALL_T = 0.15

    # Root empty
    deselect_all()
    bpy.ops.object.empty_add(type="PLAIN_AXES", location=(0, 0, 0))
    root = bpy.context.active_object
    root.name = "StoreMall"

    # Floor
    floor = add_box("Floor", location=(0, 0, -WALL_T / 2),
                    dimensions=(RW, RD, WALL_T), parent=root)
    assign_material(floor, mats["floor"])

    # Ceiling
    ceiling = add_box("Ceiling", location=(0, 0, RH + WALL_T / 2),
                      dimensions=(RW, RD, WALL_T), parent=root)
    assign_material(ceiling, mats["ceiling"])

    # Walls
    for wname, wloc, wdim in [
        ("WallFront", (0,  RD / 2, RH / 2), (RW, WALL_T, RH)),
        ("WallBack",  (0, -RD / 2, RH / 2), (RW, WALL_T, RH)),
        ("WallLeft",  (-RW / 2, 0, RH / 2), (WALL_T, RD, RH)),
        ("WallRight", ( RW / 2, 0, RH / 2), (WALL_T, RD, RH)),
    ]:
        w = add_box(wname, location=wloc, dimensions=wdim, parent=root)
        assign_material(w, mats["wall"])

    # Alternating floor tile strips (for colour-sensor differentiation)
    for ti in range(4):
        ty = -5.0 + ti * 2.5
        tile = add_box(f"TilePattern_{ti}",
                       location=(0, ty, 0.002),
                       dimensions=(RW, 1.0, 0.004), parent=root)
        assign_material(tile, mats["tile_dark"] if ti % 2 == 0 else mats["tile_light"])

    # Aisle guide lines (black, centre lines)
    aisle_x = [-3.5, -1.5, 1.5, 3.5]
    for li, lx in enumerate(aisle_x):
        gl = add_box(f"AisleGuide_{li}",
                     location=(lx, 0, 0.003),
                     dimensions=(0.06, RD - 1.0, 0.003), parent=root)
        assign_material(gl, mats["guide_line"])

    # Shelves – two aisles × 4 units each
    #   Aisle 1: x = -2.5, Aisle 2: x = 2.5
    SHELF_W, SHELF_D, SHELF_H = 0.40, 1.80, 2.20
    aisle_x_pos = [-2.5, 2.5]
    shelf_y_pos = [-4.5, -1.5, 1.5, 4.5]
    shelf_idx = 0
    for ax in aisle_x_pos:
        for sy in shelf_y_pos:
            shelf = add_box(f"Shelf_{shelf_idx}",
                            location=(ax, sy, SHELF_H / 2),
                            dimensions=(SHELF_W, SHELF_D, SHELF_H), parent=root)
            assign_material(shelf, mats["shelf"])
            shelf_idx += 1

    # Shelf dividers (cross braces between the two aisles per row)
    for di, sy in enumerate(shelf_y_pos):
        div = add_box(f"ShelfDivider_{di}",
                      location=(0, sy, 1.1),
                      dimensions=(4.6, 0.08, 0.06), parent=root)
        assign_material(div, mats["shelf"])

    # Checkout counter (near front wall)
    counter = add_box("CheckoutCounter",
                      location=(0, RD / 2 - 1.2, 0.50),
                      dimensions=(3.0, 0.80, 1.00), parent=root)
    assign_material(counter, mats["counter"])

    # Shopping carts (small obstacles in the aisle)
    cart_data = [
        ("Cart_0", (-4.0,  2.0, 0.35)),
        ("Cart_1", ( 4.0,  2.0, 0.35)),
        ("Cart_2", (-4.0, -3.5, 0.35)),
        ("Cart_3", ( 4.0, -3.5, 0.35)),
    ]
    for cname, cloc in cart_data:
        cart = add_box(cname, location=cloc, dimensions=(0.55, 0.80, 0.70), parent=root)
        assign_material(cart, mats["cart"])

    # Overhead lights
    light_positions = [
        (-3.5,  4.0), (0.0,  4.0), (3.5,  4.0),
        (-3.5, -4.0), (0.0, -4.0), (3.5, -4.0),
    ]
    for oli, (olx, oly) in enumerate(light_positions):
        ol = add_box(f"OverheadLight_{oli}",
                     location=(olx, oly, RH - 0.06),
                     dimensions=(0.80, 0.40, 0.08), parent=root)
        assign_material(ol, mats["light"])

    print("StoreMall scenario built successfully.")
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
    build_store_mall()
    script_dir = os.path.dirname(bpy.data.filepath) or os.getcwd()
    export_fbx(os.path.join(script_dir, "StoreMall.fbx"))
