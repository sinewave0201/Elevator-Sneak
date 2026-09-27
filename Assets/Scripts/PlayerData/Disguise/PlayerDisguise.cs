using System;
using UnityEngine;

[Serializable]
public class PlayerDisguise
{
    [SerializeField] private DisguiseType startingDisguise = DisguiseType.None;
    [SerializeField] private DisguiseType currentDisguise = DisguiseType.None;

    public DisguiseType Current => currentDisguise;

    [field: NonSerialized]
    public event Action<DisguiseType, DisguiseType> DisguiseChanged;

    public void Initialize()
    {
        currentDisguise = startingDisguise;
    }

    public bool Equip(DisguiseType disguise)
    {
        if (currentDisguise == disguise)
            return false;

        DisguiseType previous = currentDisguise;
        currentDisguise = disguise;
        DisguiseChanged?.Invoke(previous, currentDisguise);
        return true;
    }

    public bool Remove()
    {
        return Equip(DisguiseType.None);
    }

    public bool IsWearing(DisguiseType disguise)
    {
        return currentDisguise == disguise;
    }

    public void Reset()
    {
        Equip(startingDisguise);
    }
}
