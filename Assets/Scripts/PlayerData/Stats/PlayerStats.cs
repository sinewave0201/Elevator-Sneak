using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerStats
{
    [SerializeField] private List<PlayerStatValue> values = new List<PlayerStatValue>();

    [NonSerialized] private Dictionary<PlayerStatType, PlayerStatValue> valuesByType;

    [field: NonSerialized]
    public event Action<PlayerStatType, float, float> StatChanged;

    [field: NonSerialized]
    public event Action<float, float> SuspicionChanged;

    [field: NonSerialized]
    public event Action PlayerExposed;

    public void Initialize()
    {
        valuesByType = new Dictionary<PlayerStatType, PlayerStatValue>();

        foreach (PlayerStatValue stat in values)
        {
            if (stat == null)
                continue;

            if (valuesByType.ContainsKey(stat.Type))
            {
                Debug.LogError($"Player stat '{stat.Type}' is configured more than once.");
                continue;
            }

            stat.Initialize();
            valuesByType.Add(stat.Type, stat);
        }
    }

    public bool HasStat(PlayerStatType type)
    {
        EnsureInitialized();
        return valuesByType.ContainsKey(type);
    }

    public float Get(PlayerStatType type)
    {
        return TryGetValue(type, out PlayerStatValue stat) ? stat.CurrentValue : 0f;
    }

    public float GetMinimum(PlayerStatType type)
    {
        return TryGetValue(type, out PlayerStatValue stat) ? stat.MinimumValue : 0f;
    }

    public float GetMaximum(PlayerStatType type)
    {
        return TryGetValue(type, out PlayerStatValue stat) ? stat.MaximumValue : 0f;
    }

    public float GetNormalized(PlayerStatType type)
    {
        return TryGetValue(type, out PlayerStatValue stat) ? stat.NormalizedValue : 0f;
    }

    public bool TryGet(PlayerStatType type, out float value)
    {
        if (TryGetValue(type, out PlayerStatValue stat))
        {
            value = stat.CurrentValue;
            return true;
        }

        value = 0f;
        return false;
    }

    public bool Set(PlayerStatType type, float value)
    {
        if (!TryGetValue(type, out PlayerStatValue stat))
            return false;

        float previousValue = stat.CurrentValue;
        bool wasExposed = IsSuspicionExposed();
        float newValue = stat.SetValue(value);

        NotifyChanged(type, previousValue, newValue, wasExposed);
        return true;
    }

    public bool Modify(PlayerStatType type, float amount)
    {
        if (!TryGetValue(type, out PlayerStatValue stat))
            return false;

        return Set(type, stat.CurrentValue + amount);
    }

    public void AddSuspicion(float amount)
    {
        if (amount <= 0f)
            return;

        float multiplier = HasStat(PlayerStatType.SuspicionGainMultiplier)
            ? Get(PlayerStatType.SuspicionGainMultiplier)
            : 1f;

        Modify(PlayerStatType.Suspicion, amount * Mathf.Max(0f, multiplier));
    }

    public void ReduceSuspicion(float amount)
    {
        if (amount <= 0f)
            return;

        Modify(PlayerStatType.Suspicion, -amount);
    }

    public void Reset()
    {
        EnsureInitialized();

        foreach (KeyValuePair<PlayerStatType, PlayerStatValue> pair in valuesByType)
        {
            float previousValue = pair.Value.CurrentValue;
            pair.Value.ResetValue();
            NotifyChanged(pair.Key, previousValue, pair.Value.CurrentValue, false);
        }
    }

    private bool TryGetValue(PlayerStatType type, out PlayerStatValue stat)
    {
        EnsureInitialized();

        if (valuesByType.TryGetValue(type, out stat))
            return true;

        Debug.LogError($"Player stat '{type}' is not configured in PlayerDataManager.");
        return false;
    }

    private void EnsureInitialized()
    {
        if (valuesByType == null)
            Initialize();
    }

    private bool IsSuspicionExposed()
    {
        return valuesByType != null &&
               valuesByType.TryGetValue(PlayerStatType.Suspicion, out PlayerStatValue suspicion) &&
               suspicion.CurrentValue >= suspicion.MaximumValue;
    }

    private void NotifyChanged(
        PlayerStatType type,
        float previousValue,
        float newValue,
        bool wasExposed)
    {
        if (Mathf.Approximately(previousValue, newValue))
            return;

        StatChanged?.Invoke(type, previousValue, newValue);

        if (type != PlayerStatType.Suspicion)
            return;

        float maximum = GetMaximum(PlayerStatType.Suspicion);
        SuspicionChanged?.Invoke(newValue, maximum);

        if (!wasExposed && newValue >= maximum)
            PlayerExposed?.Invoke();
    }
}
