using System;
using UnityEngine;

[Serializable]
public class PlayerTaskTimer
{
    [SerializeField, Min(0f)] private float elevatorTimeMultiplier = 1f;
    [SerializeField, Min(0f)] private float stairsTimeMultiplier = 2f;

    private TimedTaskDefinition currentTask;
    private float remainingSeconds;
    private bool isRunning;

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
        TimeChanged?.Invoke(remainingSeconds);
    }

    public void CompleteCurrentTask()
    {
        currentTask = null;
        remainingSeconds = 0f;
        isRunning = false;
        TimeChanged?.Invoke(remainingSeconds);
    }

    public void SpendTime(float seconds)
    {
        if (!isRunning || seconds <= 0f)
            return;

        remainingSeconds = Mathf.Max(0f, remainingSeconds - seconds);
        TimeChanged?.Invoke(remainingSeconds);

        if (remainingSeconds > 0f)
            return;

        isRunning = false;
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
