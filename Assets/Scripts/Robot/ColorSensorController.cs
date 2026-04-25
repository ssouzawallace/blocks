using UnityEngine;

/// <summary>
/// Simulates a color/light sensor mounted underneath the robot.
/// Casts a ray downward to detect the surface color beneath the sensor.
/// Useful for line-following and surface-detection scenarios.
/// </summary>
public class ColorSensorController : MonoBehaviour
{
    [Tooltip("Maximum detection distance downward in world units.")]
    [SerializeField] private float maxRange = 0.5f;

    [Tooltip("LayerMask to filter which surfaces the sensor can detect.")]
    [SerializeField] private LayerMask detectionLayers = ~0;

    [Tooltip("Enable to draw a debug ray in the Scene view.")]
    [SerializeField] private bool showDebugRay = true;

    private Color detectedColor = Color.black;
    private float detectedLightLevel;
    private bool isDetecting;
    private float lastHitDistance;

    /// <summary>
    /// Returns the last detected surface color. Returns black if nothing is detected.
    /// </summary>
    public Color DetectedColor => detectedColor;

    /// <summary>
    /// Returns a light level value (0 = dark, 1 = bright) derived from the detected color.
    /// </summary>
    public float LightLevel => detectedLightLevel;

    /// <summary>
    /// Returns true if the sensor is currently detecting a surface within range.
    /// </summary>
    public bool IsDetecting => isDetecting;

    private void FixedUpdate()
    {
        Scan();
    }

    /// <summary>
    /// Performs a single color scan of the surface below.
    /// </summary>
    public void Scan()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, maxRange, detectionLayers))
        {
            isDetecting = true;
            lastHitDistance = hit.distance;

            Renderer hitRenderer = hit.collider.GetComponent<Renderer>();
            if (hitRenderer != null && hitRenderer.sharedMaterial != null)
            {
                detectedColor = hitRenderer.sharedMaterial.color;
            }
            else
            {
                detectedColor = Color.white;
            }

            detectedLightLevel = (detectedColor.r + detectedColor.g + detectedColor.b) / 3f;
        }
        else
        {
            isDetecting = false;
            lastHitDistance = maxRange;
            detectedColor = Color.black;
            detectedLightLevel = 0f;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugRay)
            return;

        Gizmos.color = isDetecting ? detectedColor : Color.gray;

        Gizmos.DrawRay(transform.position, -transform.up * lastHitDistance);
        if (isDetecting)
        {
            Gizmos.DrawWireSphere(transform.position - transform.up * lastHitDistance, 0.02f);
        }
    }
}
