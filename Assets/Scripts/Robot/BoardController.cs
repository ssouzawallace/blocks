using UnityEngine;

/// <summary>
/// Represents the physical circuit board of the robot.
/// Manages the connection between the visual programming blocks and the robot hardware components.
/// Attach to a GameObject representing the board model.
///
/// The board has four physical status LEDs (named LED_Green, LED_Red, LED_Yellow, LED_Blue
/// in the 3D model) that convey the current state:
/// <list type="table">
///   <item><term>Green</term><description>Powered on and ready.</description></item>
///   <item><term>Red</term><description>Error state (set via SetError).</description></item>
///   <item><term>Yellow</term><description>Briefly indicates command dispatch.</description></item>
///   <item><term>Blue</term><description>Connected to the programming interface.</description></item>
/// </list>
/// </summary>
public class BoardController : MonoBehaviour
{
    [Header("Robot")]
    [Tooltip("Reference to the robot controller that this board controls.")]
    [SerializeField] private RobotController robot;

    [Header("Board Status LEDs")]
    [Tooltip("Green LED — powered on / ready state.")]
    [SerializeField] private LEDController ledGreen;

    [Tooltip("Red LED — error state. Turn on via SetError(true).")]
    [SerializeField] private LEDController ledRed;

    [Tooltip("Yellow LED — briefly flashes when a command is dispatched.")]
    [SerializeField] private LEDController ledYellow;

    [Tooltip("Blue LED — connected to the programming interface.")]
    [SerializeField] private LEDController ledBlue;

    private bool isPowered;
    private bool isConnected;

    /// <summary>
    /// Returns whether the board is currently powered on.
    /// </summary>
    public bool IsPowered => isPowered;

    /// <summary>
    /// Returns whether the board is connected to the programming interface.
    /// </summary>
    public bool IsConnected => isConnected;

    /// <summary>
    /// Returns the robot controller attached to this board.
    /// </summary>
    public RobotController Robot => robot;

    /// <summary>
    /// Powers on the board.
    /// Green LED turns on; all other status LEDs turn off.
    /// </summary>
    public void PowerOn()
    {
        isPowered = true;
        SetAllLEDsOff();
        TurnOnLED(ledGreen);
    }

    /// <summary>
    /// Powers off the board and stops the robot.
    /// All status LEDs turn off.
    /// </summary>
    public void PowerOff()
    {
        isPowered = false;
        SetAllLEDsOff();

        if (robot != null)
            robot.Brake();
    }

    /// <summary>
    /// Marks the board as connected to the programming interface.
    /// Blue LED turns on; green LED turns off.
    /// </summary>
    public void Connect()
    {
        isConnected = true;
        if (!isPowered) return;

        TurnOffLED(ledGreen);
        TurnOnLED(ledBlue);
    }

    /// <summary>
    /// Marks the board as disconnected from the programming interface.
    /// Blue LED turns off; green LED turns on (board still powered).
    /// </summary>
    public void Disconnect()
    {
        isConnected = false;
        if (!isPowered) return;

        TurnOffLED(ledBlue);
        TurnOnLED(ledGreen);
    }

    /// <summary>
    /// Signals an error condition on the board.
    /// When <paramref name="error"/> is <c>true</c> the red LED turns on and the
    /// green LED turns off; when <c>false</c> the red LED turns off and — if the
    /// board is still powered — the green LED turns back on.
    /// </summary>
    public void SetError(bool error)
    {
        if (error)
        {
            TurnOffLED(ledGreen);
            TurnOnLED(ledRed);
        }
        else
        {
            TurnOffLED(ledRed);
            if (isPowered)
                TurnOnLED(ledGreen);
        }
    }

    /// <summary>
    /// Executes a command string on the robot.
    /// Interprets basic commands from the block programming system.
    /// The yellow LED briefly flashes during command dispatch; it turns off
    /// immediately after the command is forwarded to the robot controller.
    /// For commands that trigger longer actions (movement, etc.) the robot
    /// controller itself will manage any further visual feedback.
    /// </summary>
    public void ExecuteCommand(string command)
    {
        if (!isPowered || robot == null)
            return;

        // Yellow LED on to indicate command dispatch
        TurnOnLED(ledYellow);

        string trimmed = command.Trim().ToLowerInvariant();

        if (trimmed.StartsWith("thisway") || trimmed.StartsWith("forward"))
        {
            robot.MoveForward();
        }
        else if (trimmed.StartsWith("thatway") || trimmed.StartsWith("backward"))
        {
            robot.MoveBackward();
        }
        else if (trimmed.StartsWith("turnleft") || trimmed.StartsWith("left"))
        {
            robot.TurnLeft();
        }
        else if (trimmed.StartsWith("turnright") || trimmed.StartsWith("right"))
        {
            robot.TurnRight();
        }
        else if (trimmed.StartsWith("brake") || trimmed.StartsWith("stop"))
        {
            robot.Brake();
        }
        else if (trimmed.StartsWith("setpower") || trimmed.StartsWith("setspeed"))
        {
            string[] parts = trimmed.Split(' ');
            if (parts.Length > 1 && float.TryParse(parts[1], out float speed))
            {
                robot.SetSpeed(speed);
            }
        }
        else if (trimmed.StartsWith("ledon"))
        {
            string[] parts = trimmed.Split(' ');
            int index = 0;
            if (parts.Length > 1)
                int.TryParse(parts[1], out index);
            robot.TurnLEDOn(index);
        }
        else if (trimmed.StartsWith("ledoff"))
        {
            string[] parts = trimmed.Split(' ');
            int index = 0;
            if (parts.Length > 1)
                int.TryParse(parts[1], out index);
            robot.TurnLEDOff(index);
        }
        else if (trimmed.StartsWith("beep"))
        {
            robot.Beep();
        }

        // Yellow LED off after dispatch
        TurnOffLED(ledYellow);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void SetAllLEDsOff()
    {
        TurnOffLED(ledGreen);
        TurnOffLED(ledRed);
        TurnOffLED(ledYellow);
        TurnOffLED(ledBlue);
    }

    private static void TurnOnLED(LEDController led)
    {
        if (led != null) led.TurnOn();
    }

    private static void TurnOffLED(LEDController led)
    {
        if (led != null) led.TurnOff();
    }
}
