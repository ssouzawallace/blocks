using UnityEngine;

/// <summary>
/// Simulates an ultrasonic distance sensor on the robot.
/// Casts a ray in the sensor's forward direction and reports the distance to the nearest obstacle.
/// Attach to a child GameObject positioned and oriented to represent the sensor's location and direction.
/// </summary>
public class UltrasonicSensorController : MonoBehaviour
{
    [Tooltip("Maximum detection range in world units (meters).")]
    [SerializeField] private float maxRange = 4f;

    [Tooltip("LayerMask to filter which objects the sensor can detect.")]
    [SerializeField] private LayerMask detectionLayers = ~0;

    [Tooltip("Enable to draw a debug ray in the Scene view.")]
    [SerializeField] private bool showDebugRay = true;

    private float lastDistance;

    /// <summary>
    /// Returns the last measured distance. Returns maxRange if no obstacle is detected.
    /// </summary>
    public float Distance => lastDistance;

    /// <summary>
    /// Returns the maximum detection range of the sensor.
    /// </summary>
    public float MaxRange => maxRange;

    /// <summary>
    /// Returns true if the sensor is currently detecting an obstacle within range.
    /// </summary>
    public bool IsDetecting => lastDistance < maxRange;

    private void Awake()
    {
        lastDistance = maxRange;
    }

    private void FixedUpdate()
    {
        lastDistance = Measure();
    }

    /// <summary>
    /// Performs a single distance measurement and returns the result.
    /// </summary>
    public float Measure()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxRange, detectionLayers))
        {
            return hit.distance;
        }
        return maxRange;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugRay)
            return;

        Gizmos.color = IsDetecting ? Color.red : Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * lastDistance);

        if (IsDetecting)
        {
            Gizmos.DrawWireSphere(transform.position + transform.forward * lastDistance, 0.05f);
        }
    }
}
