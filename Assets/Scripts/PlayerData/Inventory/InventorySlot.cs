using System;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    [SerializeField] private ItemDefinition item;
    [SerializeField, Min(1)] private int quantity = 1;

    public ItemDefinition Item => item;
    public int Quantity => quantity;
    public int RemainingCapacity => item == null ? 0 : item.MaximumStack - quantity;

    public InventorySlot(ItemDefinition item, int quantity)
    {
        this.item = item;
        this.quantity = Mathf.Clamp(quantity, 1, item != null ? item.MaximumStack : 1);
    }

    public int Add(int amount)
    {
        if (item == null || amount <= 0)
            return amount;

        int accepted = Mathf.Min(amount, RemainingCapacity);
        quantity += accepted;
        return amount - accepted;
    }

    public int Remove(int amount)
    {
        int removed = Mathf.Min(Mathf.Max(0, amount), quantity);
        quantity -= removed;
        return removed;
    }
}
