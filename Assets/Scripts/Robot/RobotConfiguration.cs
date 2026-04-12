using UnityEngine;

/// <summary>
/// ScriptableObject that defines a robot variant configuration.
/// Create instances via Assets > Create > Blocks > Robot Configuration.
/// Each variant specifies which sensors and components the robot has.
/// </summary>
[CreateAssetMenu(fileName = "NewRobotConfiguration", menuName = "Blocks/Robot Configuration")]
public class RobotConfiguration : ScriptableObject
{
    /// <summary>
    /// Defines the available robot variant types.
    /// </summary>
    public enum RobotVariant
    {
        /// <summary>Basic two-wheeled robot with no sensors.</summary>
        TwoWheeler,
        /// <summary>Two-wheeled robot with color/light sensors underneath.</summary>
        TwoWheelerWithColorSensor,
        /// <summary>Two-wheeled robot with color/light sensors underneath and an ultrasonic sensor in front.</summary>
        TwoWheelerWithColorAndFrontUltrasonic,
        /// <summary>Two-wheeled robot with color/light sensors and an array of 6-8 ultrasonic sensors around it.</summary>
        TwoWheelerWithColorAndUltrasonicArray
    }

    [Header("Identity")]
    [Tooltip("Display name for this robot variant.")]
    public string displayName = "Two-Wheeler Robot";

    [Tooltip("The variant type of this robot.")]
    public RobotVariant variant = RobotVariant.TwoWheeler;

    [Header("Wheels")]
    [Tooltip("Number of wheels on the robot.")]
    public int wheelCount = 2;

    [Tooltip("Maximum wheel speed in units per second.")]
    public float maxSpeed = 5f;

    [Header("Color/Light Sensors")]
    [Tooltip("Whether the robot has color/light sensors underneath.")]
    public bool hasColorSensors;

    [Tooltip("Number of color/light sensors underneath the robot.")]
    public int colorSensorCount;

    [Header("Ultrasonic Sensors")]
    [Tooltip("Whether the robot has ultrasonic distance sensors.")]
    public bool hasUltrasonicSensors;

    [Tooltip("Number of ultrasonic sensors on the robot.")]
    public int ultrasonicSensorCount;

    [Tooltip("Maximum detection range of ultrasonic sensors in world units.")]
    public float ultrasonicMaxRange = 4f;

    [Header("LEDs")]
    [Tooltip("Whether the robot has LED indicators.")]
    public bool hasLEDs = true;

    [Tooltip("Number of LED indicators on the robot.")]
    public int ledCount = 1;

    [Header("Speaker")]
    [Tooltip("Whether the robot has a speaker for sound output.")]
    public bool hasSpeaker = true;

    [Header("Model")]
    [Tooltip("Prefab reference for the robot's 3D model.")]
    public GameObject modelPrefab;

    /// <summary>
    /// Creates a default configuration for the basic two-wheeler variant.
    /// </summary>
    public static RobotConfiguration CreateTwoWheeler()
    {
        var config = CreateInstance<RobotConfiguration>();
        config.displayName = "Two-Wheeler";
        config.variant = RobotVariant.TwoWheeler;
        config.wheelCount = 2;
        config.hasColorSensors = false;
        config.hasUltrasonicSensors = false;
        return config;
    }

    /// <summary>
    /// Creates a default configuration for the two-wheeler with color sensors.
    /// </summary>
    public static RobotConfiguration CreateTwoWheelerWithColorSensor()
    {
        var config = CreateInstance<RobotConfiguration>();
        config.displayName = "Two-Wheeler with Color Sensors";
        config.variant = RobotVariant.TwoWheelerWithColorSensor;
        config.wheelCount = 2;
        config.hasColorSensors = true;
        config.colorSensorCount = 2;
        config.hasUltrasonicSensors = false;
        return config;
    }

    /// <summary>
    /// Creates a default configuration for the two-wheeler with color sensors and front ultrasonic.
    /// </summary>
    public static RobotConfiguration CreateTwoWheelerWithColorAndFrontUltrasonic()
    {
        var config = CreateInstance<RobotConfiguration>();
        config.displayName = "Two-Wheeler with Color + Front Ultrasonic";
        config.variant = RobotVariant.TwoWheelerWithColorAndFrontUltrasonic;
        config.wheelCount = 2;
        config.hasColorSensors = true;
        config.colorSensorCount = 2;
        config.hasUltrasonicSensors = true;
        config.ultrasonicSensorCount = 1;
        return config;
    }

    /// <summary>
    /// Creates a default configuration for the two-wheeler with color sensors and ultrasonic array.
    /// </summary>
    public static RobotConfiguration CreateTwoWheelerWithColorAndUltrasonicArray()
    {
        var config = CreateInstance<RobotConfiguration>();
        config.displayName = "Two-Wheeler with Color + Ultrasonic Array";
        config.variant = RobotVariant.TwoWheelerWithColorAndUltrasonicArray;
        config.wheelCount = 2;
        config.hasColorSensors = true;
        config.colorSensorCount = 2;
        config.hasUltrasonicSensors = true;
        config.ultrasonicSensorCount = 8;
        return config;
    }
}
