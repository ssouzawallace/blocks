"""
test_generate_board.py
======================
Validates that generate_board.py builds the expected Blender object hierarchy
without requiring Blender to be installed.

Run from the repo root:
    python3 -m pytest Assets/Tests/Python/ -v
"""

import sys, os, importlib

# ── Inject mock bpy before importing the board script ─────────────────────
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import mock_bpy
sys.modules["bpy"] = mock_bpy.bpy

# Repo root: Assets/Tests/Python/ is 3 levels deep
_REPO_ROOT  = os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))))
_SCRIPT_PATH = os.path.join(_REPO_ROOT, "Assets", "Models", "Board", "generate_board.py")


def _fresh_build():
    """Re-run build_board() with a clean mock scene; return object list."""
    mock_bpy._scene_objects.clear()
    mock_bpy._data.materials   = mock_bpy._Materials()
    mock_bpy._data.collections = mock_bpy._Collections()

    spec = importlib.util.spec_from_file_location("generate_board", _SCRIPT_PATH)
    mod  = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    mod.build_board()
    return mock_bpy._scene_objects[:]


import pytest


@pytest.fixture(scope="module")
def objects():
    return _fresh_build()


def _names(objs):
    return [o.name for o in objs]


# ── Hierarchy tests ────────────────────────────────────────────────────────

class TestBoardHierarchy:

    def test_board_root_exists(self, objects):
        assert "Board" in _names(objects)

    def test_board_body_exists(self, objects):
        assert "BoardBody" in _names(objects)

    def test_cpu_exists(self, objects):
        assert "CPU" in _names(objects)

    # Four status LEDs ──────────────────────────────────────────────────────

    def test_led_green_exists(self, objects):
        assert "LED_Green" in _names(objects), "LED_Green missing"

    def test_led_red_exists(self, objects):
        assert "LED_Red" in _names(objects), "LED_Red missing"

    def test_led_yellow_exists(self, objects):
        assert "LED_Yellow" in _names(objects), "LED_Yellow missing"

    def test_led_blue_exists(self, objects):
        assert "LED_Blue" in _names(objects), "LED_Blue missing"

    def test_no_old_status_led(self, objects):
        assert "StatusLED" not in _names(objects), "Old StatusLED should be removed"

    def test_exactly_four_led_objects(self, objects):
        led_names = [n for n in _names(objects) if n.startswith("LED_")]
        assert len(led_names) == 4, f"Expected 4 LED_ objects, got: {led_names}"

    # Connectors ────────────────────────────────────────────────────────────

    def test_four_sensor_ports(self, objects):
        ports = [n for n in _names(objects) if n.startswith("SensorPort")]
        assert len(ports) == 4

    def test_four_actuator_ports(self, objects):
        ports = [n for n in _names(objects) if n.startswith("ActuatorPort")]
        assert len(ports) == 4

    def test_ethernet_port(self, objects):
        assert "EthernetPort" in _names(objects)

    def test_wifi_antenna(self, objects):
        assert "WiFiAntenna" in _names(objects)

    def test_bluetooth_module(self, objects):
        assert "BluetoothModule" in _names(objects)

    def test_ble_module(self, objects):
        assert "BLEModule" in _names(objects)

    def test_display_connector(self, objects):
        assert "DisplayConnector" in _names(objects)

    def test_io_header(self, objects):
        assert "IOHeader" in _names(objects)

    def test_rx_tx_pins(self, objects):
        assert "RXPin" in _names(objects)
        assert "TXPin" in _names(objects)

    def test_serial_connector(self, objects):
        assert "SerialConnector" in _names(objects)

    def test_wire_terminals(self, objects):
        assert "WireTerminals" in _names(objects)


# ── Material tests ─────────────────────────────────────────────────────────

class TestBoardMaterials:

    @pytest.fixture(scope="class")
    def mats(self):
        _fresh_build()
        return {m.name: m for m in mock_bpy._data.materials}

    def test_green_material_exists(self, mats):
        assert "MAT_LED_Green" in mats

    def test_red_material_exists(self, mats):
        assert "MAT_LED_Red" in mats

    def test_yellow_material_exists(self, mats):
        assert "MAT_LED_Yellow" in mats

    def test_blue_material_exists(self, mats):
        assert "MAT_LED_Blue" in mats

    def test_green_emission_color(self, mats):
        bsdf = mats["MAT_LED_Green"].node_tree.nodes.get("Principled BSDF")
        e = bsdf.inputs["Emission Color"].default_value
        assert e[0] < 0.1 and e[1] > 0.9 and e[2] < 0.1

    def test_red_emission_color(self, mats):
        bsdf = mats["MAT_LED_Red"].node_tree.nodes.get("Principled BSDF")
        e = bsdf.inputs["Emission Color"].default_value
        assert e[0] > 0.9 and e[1] < 0.1 and e[2] < 0.1

    def test_yellow_emission_color(self, mats):
        bsdf = mats["MAT_LED_Yellow"].node_tree.nodes.get("Principled BSDF")
        e = bsdf.inputs["Emission Color"].default_value
        assert e[0] > 0.9 and e[1] > 0.7 and e[2] < 0.1

    def test_blue_emission_color(self, mats):
        bsdf = mats["MAT_LED_Blue"].node_tree.nodes.get("Principled BSDF")
        e = bsdf.inputs["Emission Color"].default_value
        assert e[0] < 0.1 and e[2] > 0.9

    def test_all_led_emission_strengths(self, mats):
        for name in ("MAT_LED_Green", "MAT_LED_Red", "MAT_LED_Yellow", "MAT_LED_Blue"):
            bsdf = mats[name].node_tree.nodes.get("Principled BSDF")
            strength = bsdf.inputs["Emission Strength"].default_value
            assert strength >= 2.0, f"{name}: strength {strength} < 2.0"


# ── Spatial layout tests ───────────────────────────────────────────────────

class TestBoardLEDPositions:
    """Verify LEDs are in a row (same Y, increasing X)."""

    @pytest.fixture(scope="class")
    def led_objects(self):
        return {o.name: o for o in _fresh_build() if o.name.startswith("LED_")}

    def test_all_leds_same_row_y(self, led_objects):
        ys = {n: o.location[1] for n, o in led_objects.items()}
        vals = list(ys.values())
        assert all(abs(y - vals[0]) < 1e-6 for y in vals), \
            f"LED Y positions differ: {ys}"

    def test_led_x_positions_increasing(self, led_objects):
        order = ["LED_Green", "LED_Red", "LED_Yellow", "LED_Blue"]
        xs = [led_objects[n].location[0] for n in order]
        assert xs == sorted(xs), f"LED X positions not increasing: {xs}"
