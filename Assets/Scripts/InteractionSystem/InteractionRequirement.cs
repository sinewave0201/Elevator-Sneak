using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Interaction Requirement", menuName = "Interaction/Requirement")]
public class InteractionRequirement : ScriptableObject
{
    [SerializeField] private List<ItemDefinition> requiredItems = new List<ItemDefinition>();
    [SerializeField] private List<ProgressFlag> requiredProgress = new List<ProgressFlag>();
    [SerializeField] private DisguiseType requiredDisguise = DisguiseType.None;

    public bool IsMet(PlayerController player)
    {
        PlayerDataManager dataManager = PlayerDataManager.Instance;

        if (dataManager == null)
        {
            Debug.LogWarning("Cannot evaluate interaction requirements because no PlayerDataManager exists in the scene.");
            return false;
        }

        foreach (ItemDefinition item in requiredItems)
        {
            if (item == null || !dataManager.Inventory.HasItem(item))
                return false;
        }

        foreach (ProgressFlag flag in requiredProgress)
        {
            if (flag == ProgressFlag.None || !dataManager.Progress.HasFlag(flag))
                return false;
        }

        return requiredDisguise == DisguiseType.None ||
               dataManager.Disguise.IsWearing(requiredDisguise);
    }
}
