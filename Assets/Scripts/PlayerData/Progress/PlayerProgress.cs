using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProgress
{
    [SerializeField] private List<ProgressFlag> completedFlags = new List<ProgressFlag>();

    [NonSerialized] private HashSet<ProgressFlag> completedFlagSet;

    [field: NonSerialized]
    public event Action<ProgressFlag, bool> ProgressChanged;

    public void Initialize()
    {
        completedFlags.RemoveAll(flag => flag == ProgressFlag.None);
        completedFlagSet = new HashSet<ProgressFlag>(completedFlags);
        SyncSerializedFlags();
    }

    public bool HasFlag(ProgressFlag flag)
    {
        EnsureInitialized();
        return flag != ProgressFlag.None && completedFlagSet.Contains(flag);
    }

    public bool SetFlag(ProgressFlag flag, bool completed = true)
    {
        if (flag == ProgressFlag.None)
            return false;

        EnsureInitialized();
        bool changed = completed
            ? completedFlagSet.Add(flag)
            : completedFlagSet.Remove(flag);

        if (!changed)
            return false;

        SyncSerializedFlags();
        ProgressChanged?.Invoke(flag, completed);
        return true;
    }

    public void Reset()
    {
        EnsureInitialized();

        if (completedFlagSet.Count == 0)
            return;

        ProgressFlag[] flagsToClear = new ProgressFlag[completedFlagSet.Count];
        completedFlagSet.CopyTo(flagsToClear);
        completedFlagSet.Clear();
        SyncSerializedFlags();

        foreach (ProgressFlag flag in flagsToClear)
            ProgressChanged?.Invoke(flag, false);
    }

    private void EnsureInitialized()
    {
        if (completedFlagSet == null)
            Initialize();
    }

    private void SyncSerializedFlags()
    {
        completedFlags.Clear();
        completedFlags.AddRange(completedFlagSet);
        completedFlags.Sort();
    }
}
