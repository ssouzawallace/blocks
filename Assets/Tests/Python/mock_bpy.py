"""
Minimal mock of the `bpy` Blender Python API so that generate_board.py
can be imported and its logic tested without Blender being installed.
"""

import types

# ── bpy.data ────────────────────────────────────────────────────────────────

class _Materials:
    def __init__(self):
        self._store = {}

    def new(self, name):
        mat = types.SimpleNamespace(
            name=name,
            use_nodes=False,
            node_tree=types.SimpleNamespace(
                nodes=_Nodes()
            )
        )
        self._store[name] = mat
        return mat

    def __iter__(self):
        return iter(self._store.values())


class _Nodes:
    def __init__(self):
        self._nodes = {
            "Principled BSDF": types.SimpleNamespace(
                inputs=_Inputs()
            )
        }

    def get(self, key):
        return self._nodes.get(key)


class _Inputs:
    def __init__(self):
        self._data = {}

    def __getitem__(self, key):
        if key not in self._data:
            self._data[key] = types.SimpleNamespace(default_value=None)
        return self._data[key]


class _Collections:
    def __init__(self):
        self._store = []

    def remove(self, c):
        if c in self._store:
            self._store.remove(c)

    def __iter__(self):
        return iter(list(self._store))


_data = types.SimpleNamespace(
    materials=_Materials(),
    collections=_Collections(),
    filepath=""
)

# ── Scene objects ────────────────────────────────────────────────────────────

class _FakeObject:
    def __init__(self, name, location=(0, 0, 0)):
        self.name = name
        self.location = list(location)
        self.scale = [1, 1, 1]
        self.data = types.SimpleNamespace(name=name + "_Mesh", materials=[])
        self.parent = None
        self.matrix_parent_inverse = None
        self.rotation_euler = [0, 0, 0]

    def matrix_world_inverted(self):
        return None

    @property
    def matrix_world(self):
        return types.SimpleNamespace(inverted=lambda: None)


_scene_objects = []
_active_object = None


def _make_object(name, location):
    global _active_object
    obj = _FakeObject(name, location)
    _scene_objects.append(obj)
    _active_object = obj
    return obj


# ── bpy.ops ──────────────────────────────────────────────────────────────────

class _OpsObject:
    @staticmethod
    def select_all(action="SELECT"):
        pass

    @staticmethod
    def delete(use_global=False):
        pass

    @staticmethod
    def empty_add(type="PLAIN_AXES", location=(0, 0, 0)):
        _make_object("__empty__", location)

    @staticmethod
    def transform_apply(scale=False):
        pass


class _OpsMesh:
    @staticmethod
    def primitive_cube_add(size=1, location=(0, 0, 0)):
        _make_object("__cube__", location)

    @staticmethod
    def primitive_cylinder_add(radius=0.1, depth=0.1, location=(0, 0, 0), vertices=16):
        _make_object("__cylinder__", location)

    @staticmethod
    def primitive_uv_sphere_add(radius=0.05, location=(0, 0, 0), segments=16, ring_count=8):
        _make_object("__sphere__", location)


class _OpsExportScene:
    @staticmethod
    def fbx(**kwargs):
        pass


_ops = types.SimpleNamespace(
    object=_OpsObject,
    mesh=_OpsMesh,
    export_scene=_OpsExportScene,
)

# ── bpy.context ──────────────────────────────────────────────────────────────

class _Context:
    @property
    def active_object(self):
        return _active_object


_context = _Context()

# ── Module assembly ──────────────────────────────────────────────────────────

bpy = types.ModuleType("bpy")
bpy.data = _data
bpy.ops = _ops
bpy.context = _context
