using System;
using UnityEngine;

[Serializable]
public class PlayerData
{
    [SerializeField] private PlayerStats stats = new PlayerStats();
    [SerializeField] private PlayerInventory inventory = new PlayerInventory();
    [SerializeField] private PlayerProgress progress = new PlayerProgress();
    [SerializeField] private PlayerDisguise disguise = new PlayerDisguise();

    public PlayerStats Stats => stats;
    public PlayerInventory Inventory => inventory;
    public PlayerProgress Progress => progress;
    public PlayerDisguise Disguise => disguise;

    public void Initialize()
    {
        stats.Initialize();
        inventory.Initialize();
        progress.Initialize();
        disguise.Initialize();
    }

    public void Reset()
    {
        stats.Reset();
        inventory.Reset();
        progress.Reset();
        disguise.Reset();
    }
}
