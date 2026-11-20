using System;
using UnityEngine;

[Serializable]
public class PlayerStatValue
{
    [SerializeField] private PlayerStatType type;
    [SerializeField] private float startingValue;
    [SerializeField] private float minimumValue;
    [SerializeField] private float maximumValue = 100f;
    [SerializeField] private float currentValue;

    public PlayerStatType Type => type;
    public float StartingValue => startingValue;
    public float MinimumValue => minimumValue;
    public float MaximumValue => maximumValue;
    public float CurrentValue => currentValue;
    public float NormalizedValue => Mathf.Approximately(minimumValue, maximumValue)
        ? 0f
        : Mathf.InverseLerp(minimumValue, maximumValue, currentValue);

    public void Initialize()
    {
        NormalizeBounds();
        ResetValue();
    }

    public float SetValue(float value)
    {
        NormalizeBounds();
        currentValue = Mathf.Clamp(value, minimumValue, maximumValue);
        return currentValue;
    }

    public float ModifyValue(float amount)
    {
        return SetValue(currentValue + amount);
    }

    public void ResetValue()
    {
        currentValue = Mathf.Clamp(startingValue, minimumValue, maximumValue);
    }

    private void NormalizeBounds()
    {
        if (maximumValue < minimumValue)
            (minimumValue, maximumValue) = (maximumValue, minimumValue);
    }
}
