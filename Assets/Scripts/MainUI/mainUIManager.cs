using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class mainUIManager : MonoBehaviour
{
    [Header("Object UI")]
    [SerializeField] private GameObject objectUI;
    [SerializeField] private ScrollRect objectScrollView;
    [SerializeField] private GameObject objectElementPrefab;
    [SerializeField] private Vector2 elementSpacing = new Vector2(12f, 12f);

    private readonly List<GameObject> spawnedObjectElements = new List<GameObject>();

    public void openObjectUI()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
            return;

        if (!TryGetObjectUIReferences(out RectTransform content))
            return;

        objectUI.SetActive(true);
        EnsureLayout(content);
        ClearObjectElements();

        PlayerInventory inventory = PlayerDataManager.Instance.Inventory;

        foreach (InventorySlot slot in inventory.Slots)
        {
            if (slot == null || slot.Item == null || slot.Quantity <= 0)
                continue;

            CreateObjectElement(content, slot);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    public void closeObjectUI()
    {
        ClearObjectElements();

        if (objectUI != null)
            objectUI.SetActive(false);
    }

    private bool TryGetObjectUIReferences(out RectTransform content)
    {
        content = null;

        if (objectUI == null)
        {
            Debug.LogError("Object UI is not assigned on Main UI Manager.", this);
            return false;
        }

        if (objectScrollView == null)
        {
            Debug.LogError("Object Scroll View is not assigned on Main UI Manager.", this);
            return false;
        }

        if (objectElementPrefab == null)
        {
            Debug.LogError("Object Element Prefab is not assigned on Main UI Manager.", this);
            return false;
        }

        if (objectScrollView.content == null)
        {
            Debug.LogError("The Object Scroll View does not have a Content RectTransform.", objectScrollView);
            return false;
        }

        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.Inventory == null)
        {
            Debug.LogError("Player inventory is unavailable.", this);
            return false;
        }

        content = objectScrollView.content;
        return true;
    }

    private void EnsureLayout(RectTransform content)
    {
        LayoutElement scrollViewLayout = objectScrollView.GetComponent<LayoutElement>();
        if (scrollViewLayout == null)
            scrollViewLayout = objectScrollView.gameObject.AddComponent<LayoutElement>();

        RectTransform scrollRectTransform = objectScrollView.transform as RectTransform;
        if (scrollRectTransform != null)
        {
            if (scrollViewLayout.preferredWidth < 0f)
                scrollViewLayout.preferredWidth = scrollRectTransform.rect.width;

            if (scrollViewLayout.preferredHeight < 0f)
                scrollViewLayout.preferredHeight = scrollRectTransform.rect.height;
        }

        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;
            grid.spacing = elementSpacing;

            RectTransform prefabRect = objectElementPrefab.transform as RectTransform;
            if (prefabRect != null && prefabRect.rect.width > 0f && prefabRect.rect.height > 0f)
                grid.cellSize = prefabRect.rect.size;
        }

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = content.gameObject.AddComponent<ContentSizeFitter>();

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void CreateObjectElement(RectTransform content, InventorySlot slot)
    {
        GameObject element = Instantiate(objectElementPrefab, content, false);
        ItemDefinition item = slot.Item;
        element.name = $"Object - {item.DisplayName}";

        LayoutElement layout = element.GetComponent<LayoutElement>();
        if (layout == null)
            layout = element.AddComponent<LayoutElement>();

        RectTransform elementRect = element.transform as RectTransform;
        if (elementRect != null)
        {
            layout.preferredWidth = elementRect.rect.width;
            layout.preferredHeight = elementRect.rect.height;
        }

        Image icon = element.GetComponentInChildren<Image>(true);
        if (icon != null)
        {
            icon.sprite = item.Icon;
            icon.preserveAspect = true;
            icon.enabled = item.Icon != null;
        }

        TMP_Text label = element.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            string displayName = string.IsNullOrWhiteSpace(item.DisplayName) ? item.name : item.DisplayName;
            label.text = slot.Quantity > 1 ? $"{displayName} x{slot.Quantity}" : displayName;
        }

        spawnedObjectElements.Add(element);
    }

    private void ClearObjectElements()
    {
        foreach (GameObject element in spawnedObjectElements)
        {
            if (element == null)
                continue;

            element.SetActive(false);
            Destroy(element);
        }

        spawnedObjectElements.Clear();
    }
}
