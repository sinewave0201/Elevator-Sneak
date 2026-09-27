using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Player Data/Item")]
public class ItemDefinition : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField, TextArea(2, 5)] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private bool stackable;
    [SerializeField, Min(1)] private int maximumStack = 1;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public bool Stackable => stackable;
    public int MaximumStack => stackable ? Mathf.Max(1, maximumStack) : 1;

    private void OnValidate()
    {
        maximumStack = Mathf.Max(1, maximumStack);
    }
}
