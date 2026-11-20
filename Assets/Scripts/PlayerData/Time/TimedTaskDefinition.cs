using UnityEngine;

[CreateAssetMenu(fileName = "New Timed Task", menuName = "Player Data/Timed Task")]
public class TimedTaskDefinition : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField, Min(1f)] private float timeLimitSeconds = 60f; //default value

    public string DisplayName => displayName;
    public float TimeLimitSeconds => Mathf.Max(1f, timeLimitSeconds);

    private void OnValidate()
    {
        timeLimitSeconds = Mathf.Max(1f, timeLimitSeconds);
    }
}
