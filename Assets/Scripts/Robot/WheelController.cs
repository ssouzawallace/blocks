using UnityEngine;

/// <summary>
/// Controls wheel rotation for a two-wheeled robot.
/// Attach to each wheel GameObject on the robot model.
/// The wheel rotates around its local X-axis based on the current speed.
/// </summary>
public class WheelController : MonoBehaviour
{
    [Tooltip("Rotation speed multiplier in degrees per second per unit of robot speed.")]
    [SerializeField] private float rotationMultiplier = 360f;

    private float currentSpeed;

    /// <summary>
    /// Sets the wheel speed. Positive values rotate forward, negative values rotate backward.
    /// </summary>
    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
    }

    /// <summary>
    /// Immediately stops the wheel rotation.
    /// </summary>
    public void Brake()
    {
        currentSpeed = 0f;
    }

    private void Update()
    {
        if (Mathf.Abs(currentSpeed) > Mathf.Epsilon)
        {
            float rotationAngle = currentSpeed * rotationMultiplier * Time.deltaTime;
            transform.Rotate(Vector3.right, rotationAngle, Space.Self);
        }
    }
}
