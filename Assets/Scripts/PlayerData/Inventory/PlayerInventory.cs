using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerInventory
{
    [SerializeField, Min(1)] private int capacity = 12;
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();

    public int Capacity => capacity;
    public IReadOnlyList<InventorySlot> Slots => slots;

    [field: NonSerialized]
    public event Action InventoryChanged;

    [field: NonSerialized]
    public event Action<ItemDefinition, int> ItemAdded;

    [field: NonSerialized]
    public event Action<ItemDefinition, int> ItemRemoved;

    public void Initialize()
    {
        capacity = Mathf.Max(1, capacity);
        slots.RemoveAll(slot => slot == null || slot.Item == null || slot.Quantity <= 0);
    }

    public bool CanAddItem(ItemDefinition item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        int availableSpace = 0;

        if (item.Stackable)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.Item == item)
                    availableSpace += Mathf.Max(0, slot.RemainingCapacity);
            }
        }

        int freeSlots = Mathf.Max(0, capacity - slots.Count);
        availableSpace += freeSlots * item.MaximumStack;
        return availableSpace >= amount;
    }

    public bool AddItem(ItemDefinition item, int amount = 1)
    {
        if (!CanAddItem(item, amount))
            return false;

        int remaining = amount;

        if (item.Stackable)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.Item != item || slot.RemainingCapacity <= 0)
                    continue;

                remaining = slot.Add(remaining);

                if (remaining == 0)
                    break;
            }
        }

        while (remaining > 0)
        {
            int quantity = Mathf.Min(remaining, item.MaximumStack);
            slots.Add(new InventorySlot(item, quantity));
            remaining -= quantity;
        }

        ItemAdded?.Invoke(item, amount);
        InventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(ItemDefinition item, int amount = 1)
    {
        if (item == null || amount <= 0 || GetItemCount(item) < amount)
            return false;

        int remaining = amount;

        for (int i = slots.Count - 1; i >= 0 && remaining > 0; i--)
        {
            InventorySlot slot = slots[i];

            if (slot.Item != item)
                continue;

            remaining -= slot.Remove(remaining);

            if (slot.Quantity == 0)
                slots.RemoveAt(i);
        }

        ItemRemoved?.Invoke(item, amount);
        InventoryChanged?.Invoke();
        return true;
    }

    public bool HasItem(ItemDefinition item, int amount = 1)
    {
        return item != null && amount > 0 && GetItemCount(item) >= amount;
    }

    public int GetItemCount(ItemDefinition item)
    {
        if (item == null)
            return 0;

        int count = 0;

        foreach (InventorySlot slot in slots)
        {
            if (slot.Item == item)
                count += slot.Quantity;
        }

        return count;
    }

    public void Reset()
    {
        if (slots.Count == 0)
            return;

        slots.Clear();
        InventoryChanged?.Invoke();
    }
}
