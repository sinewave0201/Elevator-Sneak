using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIImage = UnityEngine.UI.Image;

public class mainUIManager : MonoBehaviour
{
    [Header("Object UI")]
    [SerializeField] private GameObject objectUI;
    [SerializeField] private ScrollRect objectScrollView;
    [SerializeField] private GameObject objectElementPrefab;
    [SerializeField] private Vector2 elementSpacing = new Vector2(8f, 8f);

    private readonly List<GameObject> spawnedObjectElements = new List<GameObject>();
    private RectTransform slotsContent;
    private GameObject detailsPanel;
    private TMP_Text detailsName;
    private TMP_Text detailsDescription;
    private TMP_Text detailsMeta;
    private TMP_FontAsset terminalFont;

    private static readonly Color TerminalBlack = new Color(0.018f, 0.022f, 0.021f, 0.97f);
    private static readonly Color PanelBlack = new Color(0.035f, 0.043f, 0.041f, 1f);
    private static readonly Color SlotBlack = new Color(0.055f, 0.065f, 0.062f, 1f);
    private static readonly Color LineGrey = new Color(0.34f, 0.37f, 0.35f, 1f);
    private static readonly Color TextGrey = new Color(0.68f, 0.71f, 0.68f, 1f);
    private static readonly Color SignalYellow = new Color(0.84f, 0.82f, 0.05f, 1f);

    private void Awake()
    {
        ApplyBagButtonAppearance();
    }

    public void openObjectUI()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
            return;

        if (objectUI != null && objectUI.activeSelf)
        {
            closeObjectUI();
            return;
        }

        if (objectUI == null || PlayerDataManager.Instance == null || PlayerDataManager.Instance.Inventory == null)
        {
            Debug.LogError("Object UI or player inventory is unavailable.", this);
            return;
        }

        objectUI.SetActive(true);
        BuildInventoryBar();
        RefreshSlots();
    }

    public void closeObjectUI()
    {
        HideItemDetails();
        if (objectUI != null)
            objectUI.SetActive(false);
    }

    public void ShowItemDetails(ItemDefinition item, int quantity, int slotNumber)
    {
        if (detailsPanel == null || item == null)
            return;

        string displayName = string.IsNullOrWhiteSpace(item.DisplayName) ? item.name : item.DisplayName;
        detailsName.text = "> " + displayName.ToUpperInvariant();
        detailsDescription.text = string.IsNullOrWhiteSpace(item.Description)
            ? "NO ARCHIVED DESCRIPTION AVAILABLE."
            : item.Description.ToUpperInvariant();
        detailsMeta.text = "SLOT " + slotNumber.ToString("00") + "  //  QTY " + quantity.ToString("00");
        detailsPanel.SetActive(true);
    }

    public void HideItemDetails()
    {
        if (detailsPanel != null)
            detailsPanel.SetActive(false);
    }

    private void ApplyBagButtonAppearance()
    {
        Transform bag = transform.Find("object UI button");
        if (bag == null)
            return;

        RectTransform rect = bag as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(68f, 68f);
            rect.anchoredPosition = new Vector2(-24f, -24f);
        }

        UIImage image = bag.GetComponent<UIImage>();
        Sprite sprite = Resources.Load<Sprite>("Inventory/bag-icon");
        if (image != null && sprite != null)
        {
            image.sprite = sprite;
            image.preserveAspect = true;
        }
    }

    private void BuildInventoryBar()
    {
        if (objectScrollView != null)
            objectScrollView.gameObject.SetActive(false);
        Transform legacyClose = objectUI.transform.Find("close");
        if (legacyClose != null)
            legacyClose.gameObject.SetActive(false);

        ResolveTerminalFont();
        Transform existing = objectUI.transform.Find("Terminal Inventory Bar");
        if (existing != null)
        {
            slotsContent = existing.Find("Viewport/Content") as RectTransform;
            BindExistingDetails();
            return;
        }

        RectTransform bar = CreatePanel("Terminal Inventory Bar", objectUI.transform, TerminalBlack, LineGrey);
        SetRect(bar, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(606f, 132f), new Vector2(-30f, 30f));

        RectTransform signal = CreateImage("Signal Line", bar, SignalYellow);
        SetRect(signal, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 3f), Vector2.zero);

        TMP_Text header = CreateText("Header", bar, "OBJECT ARCHIVE // INVENTORY", 16f, SignalYellow, TextAlignmentOptions.Left);
        SetRect(header.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380f, 30f), new Vector2(18f, -11f));
        header.fontStyle = FontStyles.Bold;
        header.characterSpacing = 1.5f;

        TMP_Text hint = CreateText("Hint", bar, "HOVER TO INSPECT", 10f, TextGrey, TextAlignmentOptions.Right);
        SetRect(hint.rectTransform, Vector2.one, Vector2.one, Vector2.one, new Vector2(150f, 24f), new Vector2(-54f, -22f));

        RectTransform viewport = CreateImage("Viewport", bar, new Color(0f, 0f, 0f, 0.01f));
        SetRect(viewport, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(-58f, 78f), new Vector2(-12f, 12f));
        viewport.gameObject.AddComponent<RectMask2D>();

        slotsContent = CreateUIObject("Content", viewport);
        slotsContent.anchorMin = new Vector2(0f, 0f);
        slotsContent.anchorMax = new Vector2(0f, 1f);
        slotsContent.pivot = new Vector2(0f, 0.5f);
        slotsContent.anchoredPosition = Vector2.zero;
        slotsContent.sizeDelta = Vector2.zero;

        HorizontalLayoutGroup layout = slotsContent.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.padding = new RectOffset(0, 0, 2, 2);
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = slotsContent.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = slotsContent;
        scroll.viewport = viewport;
        scroll.horizontal = true;
        scroll.vertical = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;

        RectTransform close = CreateImage("Close", bar, PanelBlack);
        SetRect(close, Vector2.one, Vector2.one, Vector2.one, new Vector2(34f, 26f), new Vector2(-12f, -18f));
        AddOutline(close.gameObject, LineGrey);
        Button closeButton = close.gameObject.AddComponent<Button>();
        closeButton.targetGraphic = close.GetComponent<UIImage>();
        closeButton.onClick.AddListener(closeObjectUI);
        ColorBlock colors = closeButton.colors;
        colors.highlightedColor = new Color(1f, 0.95f, 0.35f, 1f);
        colors.pressedColor = SignalYellow;
        closeButton.colors = colors;
        TMP_Text closeLabel = CreateText("Label", close, "[X]", 12f, TextGrey, TextAlignmentOptions.Center);
        Stretch(closeLabel.rectTransform);

        BuildDetailsPanel();
    }

    private void BuildDetailsPanel()
    {
        RectTransform details = CreatePanel("Terminal Item Details", objectUI.transform, TerminalBlack, LineGrey);
        detailsPanel = details.gameObject;
        SetRect(details, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(430f, 118f), new Vector2(-30f, 174f));

        RectTransform signal = CreateImage("Signal Line", details, SignalYellow);
        signal.anchorMin = new Vector2(0f, 0f);
        signal.anchorMax = new Vector2(0f, 1f);
        signal.pivot = new Vector2(0f, 0.5f);
        signal.sizeDelta = new Vector2(5f, 0f);
        signal.anchoredPosition = new Vector2(2.5f, 0f);

        detailsName = CreateText("Name", details, "", 16f, SignalYellow, TextAlignmentOptions.Left);
        SetRect(detailsName.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(398f, 24f), new Vector2(16f, -14f));
        detailsName.fontStyle = FontStyles.Bold;

        detailsDescription = CreateText("Description", details, "", 11.5f, TextGrey, TextAlignmentOptions.TopLeft);
        SetRect(detailsDescription.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(398f, 48f), new Vector2(16f, -42f));
        detailsDescription.textWrappingMode = TextWrappingModes.Normal;
        detailsDescription.overflowMode = TextOverflowModes.Ellipsis;

        detailsMeta = CreateText("Meta", details, "", 10f, new Color(0.5f, 0.54f, 0.51f), TextAlignmentOptions.BottomRight);
        SetRect(detailsMeta.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(250f, 18f), new Vector2(-12f, 7f));
        detailsPanel.SetActive(false);
    }

    private void BindExistingDetails()
    {
        Transform details = objectUI.transform.Find("Terminal Item Details");
        detailsPanel = details != null ? details.gameObject : null;
        if (detailsPanel == null)
        {
            BuildDetailsPanel();
            return;
        }
        detailsName = details.Find("Name")?.GetComponent<TMP_Text>();
        detailsDescription = details.Find("Description")?.GetComponent<TMP_Text>();
        detailsMeta = details.Find("Meta")?.GetComponent<TMP_Text>();
        detailsPanel.SetActive(false);
    }

    private void RefreshSlots()
    {
        ClearObjectElements();
        PlayerInventory inventory = PlayerDataManager.Instance.Inventory;
        for (int i = 0; i < Mathf.Max(1, inventory.Capacity); i++)
        {
            InventorySlot data = i < inventory.Slots.Count ? inventory.Slots[i] : null;
            GameObject slot = CreateSlot(i + 1, data != null ? data.Item : null, data != null ? data.Quantity : 0);
            spawnedObjectElements.Add(slot);
        }
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(slotsContent);
    }

    private GameObject CreateSlot(int number, ItemDefinition item, int quantity)
    {
        RectTransform slot = CreateImage("Slot " + number.ToString("00"), slotsContent, SlotBlack);
        slot.sizeDelta = new Vector2(72f, 72f);
        LayoutElement layout = slot.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = layout.minWidth = 72f;
        layout.preferredHeight = layout.minHeight = 72f;
        Outline outline = AddOutline(slot.gameObject, LineGrey);

        TMP_Text index = CreateText("Index", slot, number.ToString("00"), 9f, new Color(0.46f, 0.49f, 0.47f), TextAlignmentOptions.TopLeft);
        SetRect(index.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(-10f, -8f), new Vector2(5f, -4f));

        RectTransform iconRect = CreateImage("Icon", slot, Color.white);
        SetRect(iconRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(50f, 50f), Vector2.zero);
        UIImage icon = iconRect.GetComponent<UIImage>();
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        if (item != null)
        {
            icon.sprite = item.Icon;
            icon.enabled = item.Icon != null;
            if (quantity > 1)
            {
                TMP_Text qty = CreateText("Quantity", slot, "x" + quantity, 10f, Color.white, TextAlignmentOptions.BottomRight);
                SetRect(qty.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(-8f, -6f), new Vector2(4f, 3f));
            }
        }
        else
        {
            icon.enabled = false;
            TMP_Text empty = CreateText("Empty", slot, "-", 14f, new Color(0.25f, 0.28f, 0.26f), TextAlignmentOptions.Center);
            Stretch(empty.rectTransform);
        }

        InventorySlotHover hover = slot.gameObject.AddComponent<InventorySlotHover>();
        hover.Configure(this, item, quantity, number, slot.GetComponent<UIImage>(), outline, SlotBlack, SignalYellow);
        return slot.gameObject;
    }

    private void ClearObjectElements()
    {
        if (slotsContent != null)
            for (int i = slotsContent.childCount - 1; i >= 0; i--)
            {
                GameObject child = slotsContent.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        spawnedObjectElements.Clear();
    }

    private void ResolveTerminalFont()
    {
        if (terminalFont != null) return;
        TMP_Text dialogueText = Resources.FindObjectsOfTypeAll<TMP_Text>().FirstOrDefault(text => text != null && text.name == "DialogueText");
        terminalFont = dialogueText != null ? dialogueText.font : TMP_Settings.defaultFontAsset;
    }

    private RectTransform CreatePanel(string name, Transform parent, Color color, Color outlineColor)
    {
        RectTransform panel = CreateImage(name, parent, color);
        AddOutline(panel.gameObject, outlineColor);
        return panel;
    }

    private Outline AddOutline(GameObject target, Color color)
    {
        Outline outline = target.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(1f, -1f);
        return outline;
    }

    private RectTransform CreateImage(string name, Transform parent, Color color)
    {
        RectTransform rect = CreateUIObject(name, parent);
        rect.gameObject.AddComponent<UIImage>().color = color;
        return rect;
    }

    private RectTransform CreateUIObject(string name, Transform parent)
    {
        GameObject target = new GameObject(name, typeof(RectTransform));
        RectTransform rect = target.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private TMP_Text CreateText(string name, Transform parent, string value, float size, Color color, TextAlignmentOptions alignment)
    {
        TextMeshProUGUI text = CreateUIObject(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = terminalFont;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 position)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
