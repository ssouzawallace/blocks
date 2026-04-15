using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity Editor window for interactively controlling and monitoring the robot simulation.
/// Open via <b>Window &gt; Blocks &gt; Robot Simulation</b>, then enter Play Mode.
///
/// The window automatically discovers the <see cref="RobotController"/>,
/// <see cref="BoardController"/>, <see cref="ScenarioController"/>, and all sensor/LED
/// components present in the active scene. Sensor readings refresh every editor frame
/// while in Play Mode.
/// </summary>
public class RobotSimulationEditorWindow : EditorWindow
{
    // ─── Scene references ────────────────────────────────────────────────────

    private RobotController _robot;
    private BoardController _board;
    private ScenarioController _scenario;
    private LEDController[] _leds;
    private UltrasonicSensorController[] _ultrasonicSensors;
    private ColorSensorController[] _colorSensors;

    // ─── UI state ────────────────────────────────────────────────────────────

    private Vector2 _logScroll;
    private readonly StringBuilder _log = new StringBuilder();
    private string _commandInput = "";
    private float _speedSlider = 3f;

    private bool _showConfig = true;
    private bool _showControls = true;
    private bool _showSensors = true;
    private bool _showLEDs = true;
    private bool _showCommand = true;

    // ─── Open ────────────────────────────────────────────────────────────────

    /// <summary>Opens (or focuses) the Robot Simulation editor window.</summary>
    [MenuItem("Window/Blocks/Robot Simulation")]
    public static void Open()
    {
        var window = GetWindow<RobotSimulationEditorWindow>("Robot Simulation");
        window.minSize = new Vector2(320, 520);
        window.Show();
    }

    // ─── Lifecycle ───────────────────────────────────────────────────────────

    private void OnEnable()
    {
        EditorApplication.update += RepaintIfPlaying;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.update -= RepaintIfPlaying;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void RepaintIfPlaying()
    {
        if (Application.isPlaying)
            Repaint();
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
            RefreshSceneReferences();
        else if (state == PlayModeStateChange.ExitingPlayMode)
            ClearSceneReferences();
    }

    // ─── Scene discovery ────────────────────────────────────────────────────

    private void RefreshSceneReferences()
    {
        _robot = FindFirstObjectByType<RobotController>();
        _board = FindFirstObjectByType<BoardController>();
        _scenario = FindFirstObjectByType<ScenarioController>();
        _leds = FindObjectsByType<LEDController>(FindObjectsSortMode.None);
        _ultrasonicSensors = FindObjectsByType<UltrasonicSensorController>(FindObjectsSortMode.None);
        _colorSensors = FindObjectsByType<ColorSensorController>(FindObjectsSortMode.None);

        AppendLog("Scene references refreshed.");
    }

    private void ClearSceneReferences()
    {
        _robot = null;
        _board = null;
        _scenario = null;
        _leds = null;
        _ultrasonicSensors = null;
        _colorSensors = null;
    }

    // ─── OnGUI ───────────────────────────────────────────────────────────────

    private void OnGUI()
    {
        // Lazy first-frame discovery in play mode
        if (Application.isPlaying && _robot == null)
            RefreshSceneReferences();

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Robot Simulation", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Enter Play Mode to control the robot simulation.",
                MessageType.Info);
            return;
        }

        if (_robot == null)
        {
            EditorGUILayout.HelpBox(
                "No RobotController found in the scene. " +
                "Add a robot GameObject with a RobotController component.",
                MessageType.Warning);
            if (GUILayout.Button("Refresh Scene"))
                RefreshSceneReferences();
            return;
        }

        DrawBoardSection();
        DrawConfigSection();
        DrawControlsSection();
        DrawSensorsSection();
        DrawLEDsSection();
        DrawCommandSection();
        DrawLogSection();
    }

    // ─── Board power ─────────────────────────────────────────────────────────

    private void DrawBoardSection()
    {
        if (_board == null) return;

        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

        // Power indicator dot
        Color prevColor = GUI.color;
        GUI.color = _board.IsPowered ? Color.green : Color.gray;
        EditorGUILayout.LabelField(
            _board.IsPowered ? "● Powered" : "○ Off",
            EditorStyles.boldLabel,
            GUILayout.Width(78));
        GUI.color = prevColor;

        // Power toggle button
        if (GUILayout.Button(_board.IsPowered ? "Power Off" : "Power On", GUILayout.Height(22)))
        {
            if (_board.IsPowered)
            {
                _board.PowerOff();
                AppendLog("Board powered OFF.");
            }
            else
            {
                _board.PowerOn();
                AppendLog("Board powered ON.");
            }
        }

        GUILayout.Space(4);
        if (GUILayout.Button("Refresh", GUILayout.Width(60), GUILayout.Height(22)))
            RefreshSceneReferences();

        EditorGUILayout.EndHorizontal();
    }

    // ─── Configuration ───────────────────────────────────────────────────────

    private void DrawConfigSection()
    {
        _showConfig = EditorGUILayout.Foldout(_showConfig, "Configuration", true, EditorStyles.foldoutHeader);
        if (!_showConfig) return;

        EditorGUI.indentLevel++;

        if (_robot.Configuration != null)
        {
            EditorGUILayout.LabelField("Variant", _robot.Configuration.variant.ToString());
            EditorGUILayout.LabelField("Display Name", _robot.Configuration.displayName);
            EditorGUILayout.LabelField(
                "Max Speed",
                $"{_robot.Configuration.maxSpeed:F1} u/s");
        }
        else
        {
            EditorGUILayout.HelpBox("No RobotConfiguration assigned to RobotController.", MessageType.Warning);
        }

        if (_scenario != null)
        {
            EditorGUILayout.Space(2);
            string[] scenarioNames = System.Enum.GetNames(typeof(ScenarioController.ScenarioType));
            int current = (int)_scenario.ActiveScenario;
            int selected = EditorGUILayout.Popup("Scenario", current, scenarioNames);
            if (selected != current)
            {
                var newType = (ScenarioController.ScenarioType)selected;
                _scenario.LoadScenario(newType);
                AppendLog($"Scenario → {ScenarioController.GetScenarioDisplayName(newType)}");
            }
        }

        EditorGUI.indentLevel--;
    }

    // ─── Movement controls ───────────────────────────────────────────────────

    private void DrawControlsSection()
    {
        _showControls = EditorGUILayout.Foldout(_showControls, "Controls", true, EditorStyles.foldoutHeader);
        if (!_showControls) return;

        EditorGUI.indentLevel++;

        // Speed slider
        float maxSpeed = _robot.Configuration != null ? _robot.Configuration.maxSpeed : 5f;
        EditorGUI.BeginChangeCheck();
        _speedSlider = EditorGUILayout.Slider("Speed", _speedSlider, 0f, maxSpeed);
        if (EditorGUI.EndChangeCheck())
        {
            _robot.SetSpeed(_speedSlider);
            AppendLog($"Speed → {_speedSlider:F1}");
        }

        // Status row
        EditorGUILayout.BeginHorizontal();
        Color prevColor = GUI.color;
        GUI.color = _robot.IsMoving ? Color.green : Color.gray;
        EditorGUILayout.LabelField(
            _robot.IsMoving ? "● Moving" : "○ Stopped",
            EditorStyles.boldLabel,
            GUILayout.Width(90));
        GUI.color = prevColor;
        EditorGUILayout.LabelField(
            $"Speed: {_robot.CurrentSpeed:F1}",
            EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        // Forward
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("▲  Forward", GUILayout.Width(110), GUILayout.Height(28)))
            IssueMovement(() => { _robot.SetSpeed(_speedSlider); _robot.MoveForward(); }, "MoveForward");
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        // Left / Brake / Right
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("◄ Left", GUILayout.Width(78), GUILayout.Height(28)))
            IssueMovement(() => { _robot.SetSpeed(_speedSlider); _robot.TurnLeft(); }, "TurnLeft");
        GUILayout.Space(4);
        Color savedBg = GUI.backgroundColor;
        GUI.backgroundColor = new Color(1f, 0.45f, 0.45f);
        if (GUILayout.Button("■  Brake", GUILayout.Width(78), GUILayout.Height(28)))
            IssueMovement(_robot.Brake, "Brake");
        GUI.backgroundColor = savedBg;
        GUILayout.Space(4);
        if (GUILayout.Button("Right ►", GUILayout.Width(78), GUILayout.Height(28)))
            IssueMovement(() => { _robot.SetSpeed(_speedSlider); _robot.TurnRight(); }, "TurnRight");
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        // Backward
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("▼  Backward", GUILayout.Width(110), GUILayout.Height(28)))
            IssueMovement(() => { _robot.SetSpeed(_speedSlider); _robot.MoveBackward(); }, "MoveBackward");
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(2);

        // Beep
        if (GUILayout.Button("♪  Beep", GUILayout.Height(24)))
        {
            _robot.Beep();
            AppendLog("Beep");
        }

        EditorGUI.indentLevel--;
    }

    private void IssueMovement(System.Action action, string label)
    {
        action.Invoke();
        AppendLog($"→ {label}");
    }

    // ─── Sensor readings ─────────────────────────────────────────────────────

    private void DrawSensorsSection()
    {
        _showSensors = EditorGUILayout.Foldout(_showSensors, "Sensor Readings", true, EditorStyles.foldoutHeader);
        if (!_showSensors) return;

        EditorGUI.indentLevel++;

        bool hasUS = _ultrasonicSensors != null && _ultrasonicSensors.Length > 0;
        bool hasCS = _colorSensors != null && _colorSensors.Length > 0;

        if (!hasUS && !hasCS)
        {
            EditorGUILayout.LabelField("No sensors found in scene.", EditorStyles.miniLabel);
        }
        else
        {
            if (hasUS)
            {
                EditorGUILayout.LabelField("Ultrasonic", EditorStyles.boldLabel);
                for (int i = 0; i < _ultrasonicSensors.Length; i++)
                {
                    var sensor = _ultrasonicSensors[i];
                    if (sensor == null) continue;

                    float dist = sensor.Distance;
                    float maxR = sensor.MaxRange;
                    float norm = Mathf.Clamp01(dist / maxR);
                    string label = sensor.IsDetecting ? $"{dist:F2} m" : "no obstacle";

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"US [{i}]", GUILayout.Width(46));
                    Rect bar = EditorGUILayout.GetControlRect(GUILayout.Height(14));
                    EditorGUI.ProgressBar(bar, norm, label);
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (hasCS)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField("Color / Light", EditorStyles.boldLabel);
                for (int i = 0; i < _colorSensors.Length; i++)
                {
                    var sensor = _colorSensors[i];
                    if (sensor == null) continue;

                    Color c = sensor.DetectedColor;
                    float light = sensor.LightLevel;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"CS [{i}]", GUILayout.Width(46));
                    Rect swatch = EditorGUILayout.GetControlRect(GUILayout.Width(18), GUILayout.Height(14));
                    EditorGUI.DrawRect(swatch, c);
                    GUILayout.Space(2);
                    Rect lightBar = EditorGUILayout.GetControlRect(GUILayout.Height(14));
                    EditorGUI.ProgressBar(lightBar, light, $"Light {light:P0}");
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        EditorGUI.indentLevel--;
    }

    // ─── LED controls ────────────────────────────────────────────────────────

    private void DrawLEDsSection()
    {
        _showLEDs = EditorGUILayout.Foldout(_showLEDs, "LEDs", true, EditorStyles.foldoutHeader);
        if (!_showLEDs) return;

        EditorGUI.indentLevel++;

        if (_leds == null || _leds.Length == 0)
        {
            EditorGUILayout.LabelField("No LEDs found in scene.", EditorStyles.miniLabel);
        }
        else
        {
            for (int i = 0; i < _leds.Length; i++)
            {
                var led = _leds[i];
                if (led == null) continue;

                EditorGUILayout.BeginHorizontal();

                Color prevColor = GUI.color;
                GUI.color = led.IsOn ? Color.yellow : Color.gray;
                EditorGUILayout.LabelField(led.IsOn ? "●" : "○", GUILayout.Width(16));
                GUI.color = prevColor;

                EditorGUILayout.LabelField($"LED {i}", GUILayout.Width(44));

                if (GUILayout.Button(led.IsOn ? "Turn OFF" : "Turn ON", GUILayout.Width(68), GUILayout.Height(20)))
                {
                    if (led.IsOn)
                    {
                        _robot.TurnLEDOff(i);
                        AppendLog($"LED {i} → OFF");
                    }
                    else
                    {
                        _robot.TurnLEDOn(i);
                        AppendLog($"LED {i} → ON");
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUI.indentLevel--;
    }

    // ─── Raw command input ───────────────────────────────────────────────────

    private void DrawCommandSection()
    {
        _showCommand = EditorGUILayout.Foldout(_showCommand, "Send Command", true, EditorStyles.foldoutHeader);
        if (!_showCommand) return;

        EditorGUI.indentLevel++;

        EditorGUILayout.BeginHorizontal();
        GUI.SetNextControlName("cmd_input");
        _commandInput = EditorGUILayout.TextField(_commandInput);
        bool sendClicked = GUILayout.Button("Send", GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();

        bool enterPressed = Event.current.type == EventType.KeyDown
                            && Event.current.keyCode == KeyCode.Return
                            && GUI.GetNameOfFocusedControl() == "cmd_input";

        if (sendClicked || enterPressed)
        {
            DispatchCommand(_commandInput);
            _commandInput = "";
            GUI.FocusControl(null);
        }

        EditorGUILayout.LabelField(
            "e.g.  forward  backward  turnleft  setpower 3  ledon 0  beep",
            EditorStyles.miniLabel);

        EditorGUI.indentLevel--;
    }

    private void DispatchCommand(string cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd)) return;

        if (_board != null)
        {
            _board.ExecuteCommand(cmd);
            AppendLog($"> {cmd}");
        }
        else
        {
            AppendLog($"[no board] > {cmd}");
        }
    }

    // ─── Log ─────────────────────────────────────────────────────────────────

    private void DrawLogSection()
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Log", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
        if (GUILayout.Button("Clear", GUILayout.Width(46), GUILayout.Height(16)))
            _log.Clear();
        EditorGUILayout.EndHorizontal();

        _logScroll = EditorGUILayout.BeginScrollView(_logScroll, GUILayout.Height(110));
        EditorGUILayout.TextArea(_log.ToString(), GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private void AppendLog(string message)
    {
        _log.AppendLine($"[{System.DateTime.Now:HH:mm:ss}] {message}");
        _logScroll.y = float.MaxValue;
        Repaint();
    }
}
