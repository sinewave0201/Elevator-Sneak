using System;
using UnityEngine;

[Serializable]
public class PlayerTaskTimer
{
    [Header("Current Task")]
    [SerializeField] private TimedTaskDefinition Task;

    [Header("Travel Time")]
    [SerializeField, Min(0f)] private float elevatorTimeMultiplier = 1f;
    [SerializeField, Min(0f)] private float stairsTimeMultiplier = 2f;

    private TimedTaskDefinition currentTask;
    private float remainingSeconds;
    private bool isRunning;
    private int lastLoggedWholeSecond = -1;

    public TimedTaskDefinition CurrentTask => currentTask;
    public float RemainingSeconds => remainingSeconds;
    public bool IsRunning => isRunning;

    [field: NonSerialized]
    public event Action<float> TimeChanged;

    [field: NonSerialized]
    public event Action TimeExpired;

    public void Initialize()
    {
        Reset();

        if (Task != null)
            StartTask(Task);
    }

    public void StartTask(TimedTaskDefinition task)
    {
        if (task == null)
        {
            Debug.LogError("Cannot start a timed task without a task definition.");
            return;
        }

        currentTask = task;
        remainingSeconds = task.TimeLimitSeconds;
        isRunning = true;
        lastLoggedWholeSecond = Mathf.CeilToInt(remainingSeconds);
        Debug.Log($"[TaskTimer] Started '{task.DisplayName}' with {remainingSeconds:F2}s remaining.");
        TimeChanged?.Invoke(remainingSeconds);
    }

    public void CompleteCurrentTask()
    {
        string completedTaskName = currentTask != null ? currentTask.DisplayName : "<none>";
        currentTask = null;
        remainingSeconds = 0f;
        isRunning = false;
        lastLoggedWholeSecond = -1;
        Debug.Log($"[TaskTimer] Completed '{completedTaskName}'.");
        TimeChanged?.Invoke(remainingSeconds);
    }

    public void SpendTime(float seconds)
    {
        if (!isRunning || seconds <= 0f)
            return;

        float previousSeconds = remainingSeconds;
        remainingSeconds = Mathf.Max(0f, remainingSeconds - seconds);
        TimeChanged?.Invoke(remainingSeconds);

        int currentWholeSecond = Mathf.CeilToInt(remainingSeconds);
        if (currentWholeSecond != lastLoggedWholeSecond)
        {
            lastLoggedWholeSecond = currentWholeSecond;
            Debug.Log(
                $"[TaskTimer] '{currentTask.DisplayName}': {remainingSeconds:F2}s remaining " +
                $"(spent {previousSeconds - remainingSeconds:F3}s)."
            );
        }

        if (remainingSeconds > 0f)
            return;

        isRunning = false;
        Debug.Log($"[TaskTimer] '{currentTask.DisplayName}' expired.");
        TimeExpired?.Invoke();
    }

    public void SpendTravelTime(float baseSeconds, TravelMethod method)
    {
        SpendTime(baseSeconds * GetTravelMultiplier(method));
    }

    public void Tick(float deltaTime)
    {
        SpendTime(deltaTime);
    }

    public void Reset()
    {
        currentTask = null;
        remainingSeconds = 0f;
        isRunning = false;
        lastLoggedWholeSecond = -1;
        TimeChanged?.Invoke(remainingSeconds);
    }

    private float GetTravelMultiplier(TravelMethod method)
    {
        switch (method)
        {
            case TravelMethod.Stairs:
                return Mathf.Max(0f, stairsTimeMultiplier);
            case TravelMethod.Elevator:
            default:
                return Mathf.Max(0f, elevatorTimeMultiplier);
        }
    }
}
