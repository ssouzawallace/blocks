using UnityEngine;

/// <summary>
/// Controls an LED indicator on the robot.
/// Attach to a GameObject that has a Renderer component.
/// Toggles the emission color to simulate LED on/off behavior.
/// </summary>
public class LEDController : MonoBehaviour
{
    [Tooltip("The color the LED emits when turned on.")]
    [SerializeField] private Color ledColor = Color.green;

    [Tooltip("Emission intensity when the LED is on.")]
    [SerializeField] private float emissionIntensity = 2f;

    private Renderer ledRenderer;
    private MaterialPropertyBlock propertyBlock;
    private bool isOn;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    /// <summary>
    /// Returns whether the LED is currently on.
    /// </summary>
    public bool IsOn => isOn;

    private void Awake()
    {
        ledRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        TurnOff();
    }

    /// <summary>
    /// Turns the LED on with the configured color and intensity.
    /// </summary>
    public void TurnOn()
    {
        isOn = true;
        ApplyEmission(ledColor * emissionIntensity);
    }

    /// <summary>
    /// Turns the LED off (no emission).
    /// </summary>
    public void TurnOff()
    {
        isOn = false;
        ApplyEmission(Color.black);
    }

    /// <summary>
    /// Toggles the LED between on and off states.
    /// </summary>
    public void Toggle()
    {
        if (isOn)
            TurnOff();
        else
            TurnOn();
    }

    /// <summary>
    /// Sets the LED color. If the LED is currently on, the display updates immediately.
    /// </summary>
    public void SetColor(Color color)
    {
        ledColor = color;
        if (isOn)
        {
            ApplyEmission(ledColor * emissionIntensity);
        }
    }

    private void ApplyEmission(Color emissionColor)
    {
        if (ledRenderer == null)
            return;

        ledRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(EmissionColorId, emissionColor);
        propertyBlock.SetColor(BaseColorId, isOn ? ledColor : Color.gray);
        ledRenderer.SetPropertyBlock(propertyBlock);
    }
}
