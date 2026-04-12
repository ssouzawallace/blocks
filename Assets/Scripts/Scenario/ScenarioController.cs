using UnityEngine;

/// <summary>
/// Manages scenario/world environments for the robot simulation.
/// Handles loading and switching between different themed environments.
/// Attach to a manager GameObject in the scene.
/// </summary>
public class ScenarioController : MonoBehaviour
{
    /// <summary>
    /// Defines the available scenario/world types.
    /// </summary>
    public enum ScenarioType
    {
        Classroom,
        Outdoors,
        LavaVolcano,
        Underwater,
        Beach,
        StoreMall
    }

    [Header("Scenarios")]
    [Tooltip("Root GameObjects for each scenario. Index must match the ScenarioType enum order.")]
    [SerializeField] private GameObject[] scenarioRoots;

    [Tooltip("The scenario to load by default.")]
    [SerializeField] private ScenarioType defaultScenario = ScenarioType.Classroom;

    private ScenarioType activeScenario;

    /// <summary>
    /// Returns the currently active scenario type.
    /// </summary>
    public ScenarioType ActiveScenario => activeScenario;

    private void Start()
    {
        LoadScenario(defaultScenario);
    }

    /// <summary>
    /// Activates the specified scenario and deactivates all others.
    /// </summary>
    public void LoadScenario(ScenarioType scenario)
    {
        activeScenario = scenario;
        int targetIndex = (int)scenario;

        if (scenarioRoots == null)
            return;

        for (int i = 0; i < scenarioRoots.Length; i++)
        {
            if (scenarioRoots[i] != null)
            {
                scenarioRoots[i].SetActive(i == targetIndex);
            }
        }
    }

    /// <summary>
    /// Returns the display name of a scenario type.
    /// </summary>
    public static string GetScenarioDisplayName(ScenarioType scenario)
    {
        switch (scenario)
        {
            case ScenarioType.Classroom:    return "Classroom";
            case ScenarioType.Outdoors:     return "Outdoors";
            case ScenarioType.LavaVolcano:  return "Lava / Volcano";
            case ScenarioType.Underwater:   return "Underwater";
            case ScenarioType.Beach:        return "Beach";
            case ScenarioType.StoreMall:    return "Store / Mall";
            default:                        return scenario.ToString();
        }
    }
}
