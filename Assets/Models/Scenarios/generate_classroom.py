"""
generate_classroom.py — Blender 3.x / 4.x Python script
=========================================================
Run from Blender's Script Editor (Alt+P / "Run Script").

Generates a classroom scenario environment and exports it as
Scenarios/Classroom.fbx.

The scene contains:
  - A flat floor (suitable for wheeled robot navigation and color-sensor testing)
  - Perimeter walls
  - Rows of desks and chairs (obstacles for ultrasonic-sensor testing)
  - A whiteboard on the front wall
  - Coloured floor markings (lines for line-following exercises)
  - A ceiling with recessed light fixtures

Hierarchy created
-----------------
Classroom (root)
  ├─ Floor
  ├─ Ceiling
  ├─ WallFront
  ├─ WallBack
  ├─ WallLeft
  ├─ WallRight
  ├─ Whiteboard
  ├─ Desk_0 … Desk_7          (two rows of 4 desks)
  ├─ Chair_0 … Chair_7        (one chair per desk)
  ├─ FloorLine_0 … FloorLine_4 (black guide lines)
  ├─ Light_0 … Light_3        (ceiling fixture boxes)
  └─ Teacher_Desk
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
# Build
# ---------------------------------------------------------------------------

def build_classroom():
    clear_scene()

    mats = {
        "floor":      new_material("MAT_Floor",      (0.85, 0.82, 0.70), roughness=0.9),
        "wall":       new_material("MAT_Wall",        (0.92, 0.90, 0.85), roughness=0.95),
        "ceiling":    new_material("MAT_Ceiling",     (0.95, 0.95, 0.95), roughness=1.0),
        "whiteboard": new_material("MAT_Whiteboard",  (0.98, 0.98, 0.98), roughness=0.85),
        "desk":       new_material("MAT_Desk",        (0.55, 0.40, 0.25), roughness=0.8),
        "chair":      new_material("MAT_Chair",       (0.20, 0.35, 0.60), roughness=0.8),
        "line_black": new_material("MAT_LineBlack",   (0.02, 0.02, 0.02), roughness=1.0),
        "light":      new_material("MAT_Light",       (1.00, 1.00, 0.90),
                                   emission_color=(1.00, 1.00, 0.90), emission_strength=3.0),
        "teacher_desk": new_material("MAT_TeacherDesk", (0.40, 0.28, 0.15), roughness=0.75),
    }

    # Room dimensions  (6 m wide × 8 m deep × 3 m tall)
    RW, RD, RH = 6.0, 8.0, 3.0
    WALL_T = 0.15   # wall thickness

    # Root empty
    deselect_all()
    bpy.ops.object.empty_add(type="PLAIN_AXES", location=(0, 0, 0))
    root = bpy.context.active_object
    root.name = "Classroom"

    # Floor
    floor = add_box("Floor", location=(0, 0, -WALL_T / 2),
                    dimensions=(RW, RD, WALL_T), parent=root)
    assign_material(floor, mats["floor"])

    # Ceiling
    ceiling = add_box("Ceiling", location=(0, 0, RH + WALL_T / 2),
                      dimensions=(RW, RD, WALL_T), parent=root)
    assign_material(ceiling, mats["ceiling"])

    # Walls (front = +Y, back = -Y, left = -X, right = +X)
    wf = add_box("WallFront", location=(0, RD / 2, RH / 2),
                 dimensions=(RW, WALL_T, RH), parent=root)
    assign_material(wf, mats["wall"])

    wb = add_box("WallBack", location=(0, -RD / 2, RH / 2),
                 dimensions=(RW, WALL_T, RH), parent=root)
    assign_material(wb, mats["wall"])

    wl = add_box("WallLeft", location=(-RW / 2, 0, RH / 2),
                 dimensions=(WALL_T, RD, RH), parent=root)
    assign_material(wl, mats["wall"])

    wr = add_box("WallRight", location=(RW / 2, 0, RH / 2),
                 dimensions=(WALL_T, RD, RH), parent=root)
    assign_material(wr, mats["wall"])

    # Whiteboard (front wall)
    wb_board = add_box("Whiteboard", location=(0, RD / 2 - WALL_T / 2 - 0.02, 1.5),
                       dimensions=(3.0, 0.04, 1.2), parent=root)
    assign_material(wb_board, mats["whiteboard"])

    # Desks (2 rows × 4 columns)
    DESK_W, DESK_D, DESK_H = 0.65, 0.45, 0.75
    desk_x_positions = [-1.8, -0.6, 0.6, 1.8]
    desk_y_positions  = [1.5, -0.5]   # two rows
    desk_idx = 0
    for ry in desk_y_positions:
        for rx in desk_x_positions:
            desk = add_box(f"Desk_{desk_idx}",
                           location=(rx, ry, DESK_H / 2),
                           dimensions=(DESK_W, DESK_D, DESK_H), parent=root)
            assign_material(desk, mats["desk"])

            # Chair slightly behind each desk
            chair = add_box(f"Chair_{desk_idx}",
                            location=(rx, ry - 0.55, 0.45),
                            dimensions=(0.45, 0.45, 0.90), parent=root)
            assign_material(chair, mats["chair"])

            desk_idx += 1

    # Teacher's desk (front, near whiteboard)
    t_desk = add_box("Teacher_Desk", location=(0, RD / 2 - 1.2, 0.40),
                     dimensions=(1.4, 0.60, 0.80), parent=root)
    assign_material(t_desk, mats["teacher_desk"])

    # Floor guide lines (5 parallel lines for line-following)
    LINE_T = 0.005   # thickness above floor
    for i in range(5):
        lx = -1.0 + i * 0.5
        fl = add_box(f"FloorLine_{i}",
                     location=(lx, 0, LINE_T / 2),
                     dimensions=(0.04, RD - 1.0, LINE_T), parent=root)
        assign_material(fl, mats["line_black"])

    # Ceiling light fixtures
    light_positions = [(-1.5, 2.5), (1.5, 2.5), (-1.5, -1.5), (1.5, -1.5)]
    for li, (lx, ly) in enumerate(light_positions):
        light = add_box(f"Light_{li}",
                        location=(lx, ly, RH - 0.05),
                        dimensions=(0.6, 0.3, 0.06), parent=root)
        assign_material(light, mats["light"])

    print("Classroom scenario built successfully.")
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
    build_classroom()
    script_dir = os.path.dirname(bpy.data.filepath) or os.getcwd()
    export_fbx(os.path.join(script_dir, "Classroom.fbx"))
