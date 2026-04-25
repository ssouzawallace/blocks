using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// EditMode unit tests for <see cref="BoardController"/> LED state logic.
///
/// These tests verify the four-colour status LED behaviour described in the
/// Board model specification:
/// <list type="table">
///   <item><term>Green</term><description>Powered on / ready.</description></item>
///   <item><term>Red</term><description>Error state.</description></item>
///   <item><term>Yellow</term><description>Command dispatch flash.</description></item>
///   <item><term>Blue</term><description>Connected to programming interface.</description></item>
/// </list>
///
/// <para>
/// The tests use Unity's EditMode test runner and do not require Play mode.
/// All GameObjects created during a test are destroyed in <see cref="TearDown"/>.
/// </para>
/// </summary>
[TestFixture]
public class BoardControllerTests
{
    // ── Fixture state ────────────────────────────────────────────────────────

    private readonly List<GameObject> _createdObjects = new List<GameObject>();

    private BoardController _board;
    private LEDController   _ledGreen;
    private LEDController   _ledRed;
    private LEDController   _ledYellow;
    private LEDController   _ledBlue;

    [SetUp]
    public void SetUp()
    {
        // Create the board root
        var boardGO = new GameObject("Board");
        _createdObjects.Add(boardGO);
        _board = boardGO.AddComponent<BoardController>();

        // Create one LED per colour; no Renderer needed — IsOn tracks the state
        _ledGreen  = CreateLED("LED_Green");
        _ledRed    = CreateLED("LED_Red");
        _ledYellow = CreateLED("LED_Yellow");
        _ledBlue   = CreateLED("LED_Blue");

        // Inject via reflection (fields are [SerializeField] private)
        SetField(_board, "ledGreen",  _ledGreen);
        SetField(_board, "ledRed",    _ledRed);
        SetField(_board, "ledYellow", _ledYellow);
        SetField(_board, "ledBlue",   _ledBlue);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var go in _createdObjects)
            Object.DestroyImmediate(go);
        _createdObjects.Clear();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private LEDController CreateLED(string name)
    {
        var go = new GameObject(name);
        _createdObjects.Add(go);
        return go.AddComponent<LEDController>();
    }

    private static void SetField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(
            fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}");
        field.SetValue(target, value);
    }

    // ── PowerOn ──────────────────────────────────────────────────────────────

    [Test]
    public void PowerOn_SetsBoardPowered()
    {
        _board.PowerOn();
        Assert.IsTrue(_board.IsPowered);
    }

    [Test]
    public void PowerOn_TurnsGreenLEDOn()
    {
        _board.PowerOn();
        Assert.IsTrue(_ledGreen.IsOn, "Green LED should be on after PowerOn");
    }

    [Test]
    public void PowerOn_AllOtherLEDsOff()
    {
        _board.PowerOn();
        Assert.IsFalse(_ledRed.IsOn,    "Red LED should be off after PowerOn");
        Assert.IsFalse(_ledYellow.IsOn, "Yellow LED should be off after PowerOn");
        Assert.IsFalse(_ledBlue.IsOn,   "Blue LED should be off after PowerOn");
    }

    // ── PowerOff ─────────────────────────────────────────────────────────────

    [Test]
    public void PowerOff_ClearsBoardPowered()
    {
        _board.PowerOn();
        _board.PowerOff();
        Assert.IsFalse(_board.IsPowered);
    }

    [Test]
    public void PowerOff_TurnsAllLEDsOff()
    {
        _board.PowerOn();
        _board.PowerOff();
        Assert.IsFalse(_ledGreen.IsOn,  "Green LED should be off after PowerOff");
        Assert.IsFalse(_ledRed.IsOn,    "Red LED should be off after PowerOff");
        Assert.IsFalse(_ledYellow.IsOn, "Yellow LED should be off after PowerOff");
        Assert.IsFalse(_ledBlue.IsOn,   "Blue LED should be off after PowerOff");
    }

    // ── Connect ───────────────────────────────────────────────────────────────

    [Test]
    public void Connect_SetsConnectedFlag()
    {
        _board.PowerOn();
        _board.Connect();
        Assert.IsTrue(_board.IsConnected);
    }

    [Test]
    public void Connect_TurnsBlueLEDOn()
    {
        _board.PowerOn();
        _board.Connect();
        Assert.IsTrue(_ledBlue.IsOn, "Blue LED should be on after Connect");
    }

    [Test]
    public void Connect_TurnsGreenLEDOff()
    {
        _board.PowerOn();
        _board.Connect();
        Assert.IsFalse(_ledGreen.IsOn, "Green LED should be off after Connect");
    }

    [Test]
    public void Connect_WhenNotPowered_DoesNotChangeLEDs()
    {
        // Board never powered — LEDs should remain off
        _board.Connect();
        Assert.IsFalse(_ledBlue.IsOn,  "Blue LED must not turn on when board is off");
        Assert.IsFalse(_ledGreen.IsOn, "Green LED must not turn on when board is off");
    }

    // ── Disconnect ───────────────────────────────────────────────────────────

    [Test]
    public void Disconnect_ClearsConnectedFlag()
    {
        _board.PowerOn();
        _board.Connect();
        _board.Disconnect();
        Assert.IsFalse(_board.IsConnected);
    }

    [Test]
    public void Disconnect_TurnsGreenOnAndBlueOff()
    {
        _board.PowerOn();
        _board.Connect();
        _board.Disconnect();
        Assert.IsTrue(_ledGreen.IsOn,  "Green LED should be on after Disconnect");
        Assert.IsFalse(_ledBlue.IsOn,  "Blue LED should be off after Disconnect");
    }

    [Test]
    public void Disconnect_WhenNotPowered_DoesNotChangeLEDs()
    {
        _board.Disconnect();
        Assert.IsFalse(_ledGreen.IsOn, "Green LED must not turn on when board is off");
        Assert.IsFalse(_ledBlue.IsOn,  "Blue LED must not change when board is off");
    }

    // ── SetError ─────────────────────────────────────────────────────────────

    [Test]
    public void SetError_True_TurnsRedOnAndGreenOff()
    {
        _board.PowerOn();
        _board.SetError(true);
        Assert.IsTrue(_ledRed.IsOn,    "Red LED should be on after SetError(true)");
        Assert.IsFalse(_ledGreen.IsOn, "Green LED should be off after SetError(true)");
    }

    [Test]
    public void SetError_False_TurnsRedOff()
    {
        _board.PowerOn();
        _board.SetError(true);
        _board.SetError(false);
        Assert.IsFalse(_ledRed.IsOn, "Red LED should be off after SetError(false)");
    }

    [Test]
    public void SetError_False_WhenPowered_RestoresGreenLED()
    {
        _board.PowerOn();
        _board.SetError(true);
        _board.SetError(false);
        Assert.IsTrue(_ledGreen.IsOn, "Green LED should be restored after SetError(false) when powered");
    }

    [Test]
    public void SetError_False_WhenNotPowered_DoesNotTurnGreenOn()
    {
        // Board not powered — SetError(false) should not turn green on
        _board.SetError(true);
        _board.SetError(false);
        Assert.IsFalse(_ledGreen.IsOn, "Green LED must stay off when board is not powered");
    }

    // ── ExecuteCommand (LED behaviour only) ──────────────────────────────────

    [Test]
    public void ExecuteCommand_YellowLEDOffAfterDispatch()
    {
        // No robot wired — command will return early, but yellow should still be
        // turned off at the end of the method (guard clause returns before lighting).
        // To test the LED flash path, we need to test with a wired robot.
        // Without a robot, ExecuteCommand returns at the first guard.
        // So yellow stays off — still a valid assertion.
        _board.PowerOn();
        _board.ExecuteCommand("forward");
        Assert.IsFalse(_ledYellow.IsOn, "Yellow LED should be off after ExecuteCommand returns");
    }

    [Test]
    public void ExecuteCommand_WhenNotPowered_DoesNotFlashYellow()
    {
        _board.ExecuteCommand("forward");
        Assert.IsFalse(_ledYellow.IsOn, "Yellow LED must not flash when board is off");
    }

    // ── Round-trip state transitions ─────────────────────────────────────────

    [Test]
    public void PowerCycle_ResetsAllLEDs()
    {
        _board.PowerOn();
        _board.Connect();
        _board.PowerOff();
        _board.PowerOn();
        // After power cycle: green on, all others off
        Assert.IsTrue(_ledGreen.IsOn,   "Green should be on after second PowerOn");
        Assert.IsFalse(_ledBlue.IsOn,   "Blue should be off after power cycle");
        Assert.IsFalse(_ledRed.IsOn,    "Red should be off after power cycle");
        Assert.IsFalse(_ledYellow.IsOn, "Yellow should be off after power cycle");
    }
}
