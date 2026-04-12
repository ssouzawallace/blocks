using UnityEngine;

/// <summary>
/// Represents the physical circuit board of the robot.
/// Manages the connection between the visual programming blocks and the robot hardware components.
/// Attach to a GameObject representing the board model.
/// </summary>
public class BoardController : MonoBehaviour
{
    [Header("Robot")]
    [Tooltip("Reference to the robot controller that this board controls.")]
    [SerializeField] private RobotController robot;

    [Header("Board Status")]
    [Tooltip("LED indicator showing the board's power/connection status.")]
    [SerializeField] private LEDController statusLED;

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
    /// Powers on the board and activates the status LED.
    /// </summary>
    public void PowerOn()
    {
        isPowered = true;
        if (statusLED != null)
        {
            statusLED.SetColor(Color.green);
            statusLED.TurnOn();
        }
    }

    /// <summary>
    /// Powers off the board and deactivates the status LED.
    /// Also stops the robot if it is moving.
    /// </summary>
    public void PowerOff()
    {
        isPowered = false;
        if (statusLED != null)
        {
            statusLED.TurnOff();
        }

        if (robot != null)
        {
            robot.Brake();
        }
    }

    /// <summary>
    /// Marks the board as connected to the programming interface.
    /// Updates the status LED color.
    /// </summary>
    public void Connect()
    {
        isConnected = true;
        if (isPowered && statusLED != null)
        {
            statusLED.SetColor(Color.blue);
            statusLED.TurnOn();
        }
    }

    /// <summary>
    /// Marks the board as disconnected from the programming interface.
    /// Updates the status LED color.
    /// </summary>
    public void Disconnect()
    {
        isConnected = false;
        if (isPowered && statusLED != null)
        {
            statusLED.SetColor(Color.green);
            statusLED.TurnOn();
        }
    }

    /// <summary>
    /// Executes a command string on the robot.
    /// Interprets basic commands from the block programming system.
    /// </summary>
    public void ExecuteCommand(string command)
    {
        if (!isPowered || robot == null)
            return;

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
            {
                int.TryParse(parts[1], out index);
            }
            robot.TurnLEDOn(index);
        }
        else if (trimmed.StartsWith("ledoff"))
        {
            string[] parts = trimmed.Split(' ');
            int index = 0;
            if (parts.Length > 1)
            {
                int.TryParse(parts[1], out index);
            }
            robot.TurnLEDOff(index);
        }
        else if (trimmed.StartsWith("beep"))
        {
            robot.Beep();
        }
    }
}
