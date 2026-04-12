using UnityEngine;

/// <summary>
/// Main controller for the robot. Coordinates all robot components (wheels, sensors, LEDs, speaker).
/// Attach to the root GameObject of the robot model.
/// Provides a high-level API for controlling the robot from the block programming system.
/// </summary>
public class RobotController : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Robot configuration asset defining this robot's variant and capabilities.")]
    [SerializeField] private RobotConfiguration configuration;

    [Header("Components")]
    [Tooltip("Left wheel controller.")]
    [SerializeField] private WheelController leftWheel;

    [Tooltip("Right wheel controller.")]
    [SerializeField] private WheelController rightWheel;

    [Tooltip("LED controllers on the robot.")]
    [SerializeField] private LEDController[] leds;

    [Tooltip("Speaker controller on the robot.")]
    [SerializeField] private SpeakerController speaker;

    [Tooltip("Ultrasonic sensor controllers.")]
    [SerializeField] private UltrasonicSensorController[] ultrasonicSensors;

    [Tooltip("Color/light sensor controllers underneath the robot.")]
    [SerializeField] private ColorSensorController[] colorSensors;

    private float currentSpeed;
    private bool isMoving;

    /// <summary>
    /// Returns the current robot configuration.
    /// </summary>
    public RobotConfiguration Configuration => configuration;

    /// <summary>
    /// Returns whether the robot is currently in motion.
    /// </summary>
    public bool IsMoving => isMoving;

    /// <summary>
    /// Returns the current speed of the robot.
    /// </summary>
    public float CurrentSpeed => currentSpeed;

    /// <summary>
    /// Moves the robot forward at the current speed.
    /// Corresponds to the "thisway" / Forward block command.
    /// </summary>
    public void MoveForward()
    {
        isMoving = true;
        SetWheelSpeeds(currentSpeed, currentSpeed);
    }

    /// <summary>
    /// Moves the robot backward at the current speed.
    /// Corresponds to the "thatway" / Backward block command.
    /// </summary>
    public void MoveBackward()
    {
        isMoving = true;
        SetWheelSpeeds(-currentSpeed, -currentSpeed);
    }

    /// <summary>
    /// Turns the robot left by applying differential wheel speeds.
    /// Corresponds to the TurnLeft block command.
    /// </summary>
    public void TurnLeft()
    {
        isMoving = true;
        SetWheelSpeeds(-currentSpeed, currentSpeed);
    }

    /// <summary>
    /// Turns the robot right by applying differential wheel speeds.
    /// Corresponds to the TurnRight block command.
    /// </summary>
    public void TurnRight()
    {
        isMoving = true;
        SetWheelSpeeds(currentSpeed, -currentSpeed);
    }

    /// <summary>
    /// Stops the robot immediately.
    /// Corresponds to the Brake block command.
    /// </summary>
    public void Brake()
    {
        isMoving = false;
        SetWheelSpeeds(0f, 0f);
        if (leftWheel != null) leftWheel.Brake();
        if (rightWheel != null) rightWheel.Brake();
    }

    /// <summary>
    /// Sets the power/speed of the robot.
    /// Corresponds to the SetSpeed / "setpower" block command.
    /// </summary>
    public void SetSpeed(float speed)
    {
        float maxSpeed = configuration != null ? configuration.maxSpeed : 5f;
        currentSpeed = Mathf.Clamp(speed, 0f, maxSpeed);

        if (isMoving)
        {
            SetWheelSpeeds(currentSpeed, currentSpeed);
        }
    }

    /// <summary>
    /// Turns an LED on by index. Index is zero-based.
    /// </summary>
    public void TurnLEDOn(int index)
    {
        if (leds != null && index >= 0 && index < leds.Length && leds[index] != null)
        {
            leds[index].TurnOn();
        }
    }

    /// <summary>
    /// Turns an LED off by index. Index is zero-based.
    /// </summary>
    public void TurnLEDOff(int index)
    {
        if (leds != null && index >= 0 && index < leds.Length && leds[index] != null)
        {
            leds[index].TurnOff();
        }
    }

    /// <summary>
    /// Sets the color of an LED by index.
    /// </summary>
    public void SetLEDColor(int index, Color color)
    {
        if (leds != null && index >= 0 && index < leds.Length && leds[index] != null)
        {
            leds[index].SetColor(color);
        }
    }

    /// <summary>
    /// Plays a beep through the robot's speaker.
    /// </summary>
    public void Beep()
    {
        if (speaker != null)
        {
            speaker.Beep();
        }
    }

    /// <summary>
    /// Plays a tone at the specified frequency for a given duration through the speaker.
    /// </summary>
    public void PlayTone(float frequency, float duration)
    {
        if (speaker != null)
        {
            speaker.PlayTone(frequency, duration);
        }
    }

    /// <summary>
    /// Reads the distance value from an ultrasonic sensor by index.
    /// Returns the max range if the sensor index is invalid.
    /// </summary>
    public float ReadUltrasonicSensor(int index)
    {
        if (ultrasonicSensors != null && index >= 0 && index < ultrasonicSensors.Length
            && ultrasonicSensors[index] != null)
        {
            return ultrasonicSensors[index].Distance;
        }
        return float.MaxValue;
    }

    /// <summary>
    /// Reads the color value from a color sensor by index.
    /// Returns black if the sensor index is invalid.
    /// </summary>
    public Color ReadColorSensor(int index)
    {
        if (colorSensors != null && index >= 0 && index < colorSensors.Length
            && colorSensors[index] != null)
        {
            return colorSensors[index].DetectedColor;
        }
        return Color.black;
    }

    /// <summary>
    /// Reads the light level from a color sensor by index (0 = dark, 1 = bright).
    /// Returns 0 if the sensor index is invalid.
    /// </summary>
    public float ReadLightLevel(int index)
    {
        if (colorSensors != null && index >= 0 && index < colorSensors.Length
            && colorSensors[index] != null)
        {
            return colorSensors[index].LightLevel;
        }
        return 0f;
    }

    private void SetWheelSpeeds(float leftSpeed, float rightSpeed)
    {
        if (leftWheel != null) leftWheel.SetSpeed(leftSpeed);
        if (rightWheel != null) rightWheel.SetSpeed(rightSpeed);
    }
}
