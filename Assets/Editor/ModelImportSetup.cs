using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Asset post-processor that automatically attaches controller MonoBehaviours to
/// GameObjects inside imported robot and board FBX models.
///
/// When an FBX from Assets/Models/Robots/ or Assets/Models/Board/ is (re)imported,
/// Unity calls <see cref="OnPostprocessGameObjectWithUserProperties"/> and
/// <see cref="OnPostprocessModel"/> so that we can walk the hierarchy and add the
/// correct components.
///
/// Naming conventions expected in the FBX hierarchy:
/// <list type="table">
///   <item><term>Root object named "Board"</term><description>Add <see cref="BoardController"/>.</description></item>
///   <item><term>Root object matching a robot variant name</term><description>Add <see cref="RobotController"/>.</description></item>
///   <item><term>"LeftWheel" or "RightWheel"</term><description>Add <see cref="WheelController"/>.</description></item>
///   <item><term>Starts with "LED"</term><description>Add <see cref="LEDController"/>.</description></item>
///   <item><term>"Speaker"</term><description>Add <see cref="SpeakerController"/> and <see cref="AudioSource"/>.</description></item>
///   <item><term>Starts with "ColorSensor"</term><description>Add <see cref="ColorSensorController"/>.</description></item>
///   <item><term>Starts with "UltrasonicSensor"</term><description>Add <see cref="UltrasonicSensorController"/>.</description></item>
///   <item><term>"StatusLED"</term><description>Add <see cref="LEDController"/> (treated as a named LED).</description></item>
/// </list>
///
/// This processor only runs when the model is located under <c>Assets/Models/Robots/</c>
/// or <c>Assets/Models/Board/</c>.
/// </summary>
public class ModelImportSetup : AssetPostprocessor
{
    // Prefixes that identify the asset paths this processor handles.
    private const string RobotModelPath   = "Assets/Models/Robots/";
    private const string BoardModelPath   = "Assets/Models/Board/";

    /// <summary>
    /// Called by Unity after the model GameObject hierarchy has been constructed.
    /// Walks every transform in the imported prefab and attaches controller scripts.
    /// </summary>
    private void OnPostprocessModel(GameObject root)
    {
        string assetPath = assetImporter.assetPath;

        bool isRobot = assetPath.StartsWith(RobotModelPath, StringComparison.OrdinalIgnoreCase);
        bool isBoard = assetPath.StartsWith(BoardModelPath, StringComparison.OrdinalIgnoreCase);

        if (!isRobot && !isBoard)
            return;

        Debug.Log($"[ModelImportSetup] Post-processing model: {assetPath}");

        foreach (Transform t in root.GetComponentsInChildren<Transform>(includeInactive: true))
        {
            ProcessTransform(t, isBoard, isRobot);
        }
    }

    // -------------------------------------------------------------------------

    private static void ProcessTransform(Transform t, bool isBoard, bool isRobot)
    {
        string name = t.gameObject.name;

        // ── Board root ────────────────────────────────────────────────────────
        if (isBoard && name.Equals("Board", StringComparison.OrdinalIgnoreCase))
        {
            EnsureComponent<BoardController>(t.gameObject);
            return;
        }

        // ── Robot root (any recognised variant name) ──────────────────────────
        if (isRobot && IsRobotRootName(name))
        {
            EnsureComponent<RobotController>(t.gameObject);
            return;
        }

        // ── Wheels ────────────────────────────────────────────────────────────
        if (name.Equals("LeftWheel", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("RightWheel", StringComparison.OrdinalIgnoreCase))
        {
            EnsureComponent<WheelController>(t.gameObject);
            return;
        }

        // ── StatusLED / generic LEDs ──────────────────────────────────────────
        if (name.Equals("StatusLED", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("LED", StringComparison.OrdinalIgnoreCase))
        {
            EnsureComponent<LEDController>(t.gameObject);
            return;
        }

        // ── Speaker ───────────────────────────────────────────────────────────
        if (name.Equals("Speaker", StringComparison.OrdinalIgnoreCase))
        {
            EnsureComponent<AudioSource>(t.gameObject);
            EnsureComponent<SpeakerController>(t.gameObject);
            return;
        }

        // ── Color / Light sensors ─────────────────────────────────────────────
        if (name.StartsWith("ColorSensor", StringComparison.OrdinalIgnoreCase))
        {
            EnsureComponent<ColorSensorController>(t.gameObject);
            return;
        }

        // ── Ultrasonic sensors ────────────────────────────────────────────────
        if (name.StartsWith("UltrasonicSensor", StringComparison.OrdinalIgnoreCase))
        {
            EnsureComponent<UltrasonicSensorController>(t.gameObject);
            return;
        }
    }

    // -------------------------------------------------------------------------

    /// <summary>Returns true if the name matches a known robot root object name.</summary>
    private static bool IsRobotRootName(string name)
    {
        return name.Equals("TwoWheeler",                       StringComparison.OrdinalIgnoreCase) ||
               name.Equals("TwoWheelerWithLCD",               StringComparison.OrdinalIgnoreCase) ||
               name.Equals("TwoWheelerWithColorSensors",      StringComparison.OrdinalIgnoreCase) ||
               name.Equals("TwoWheelerWithFrontUltrasonic",   StringComparison.OrdinalIgnoreCase) ||
               name.Equals("TwoWheelerWithUltrasonicArray",   StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Adds <typeparamref name="T"/> to <paramref name="go"/> if it does not already
    /// have one, and logs the action.
    /// </summary>
    private static T EnsureComponent<T>(GameObject go) where T : Component
    {
        T existing = go.GetComponent<T>();
        if (existing != null)
            return existing;

        T added = go.AddComponent<T>();
        Debug.Log($"[ModelImportSetup] Added {typeof(T).Name} to '{go.name}'");
        return added;
    }
}
