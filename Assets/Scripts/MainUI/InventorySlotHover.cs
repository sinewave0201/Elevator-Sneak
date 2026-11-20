using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UIImage = UnityEngine.UI.Image;

public class InventorySlotHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private mainUIManager owner;
    private ItemDefinition item;
    private int quantity;
    private int slotNumber;
    private UIImage background;
    private Outline outline;
    private Color normalColor;
    private Color highlightColor;

    public void Configure(mainUIManager manager, ItemDefinition definition, int count, int number, UIImage slotBackground, Outline slotOutline, Color normal, Color highlight)
    {
        owner = manager;
        item = definition;
        quantity = count;
        slotNumber = number;
        background = slotBackground;
        outline = slotOutline;
        normalColor = normal;
        highlightColor = highlight;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null) return;
        if (background != null) background.color = Color.Lerp(normalColor, highlightColor, 0.12f);
        if (outline != null) outline.effectColor = highlightColor;
        transform.localScale = new Vector3(1.035f, 1.035f, 1f);
        owner?.ShowItemDetails(item, quantity, slotNumber);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (background != null) background.color = normalColor;
        if (outline != null) outline.effectColor = new Color(0.34f, 0.37f, 0.35f, 1f);
        transform.localScale = Vector3.one;
        owner?.HideItemDetails();
    }

    private void OnDisable()
    {
        transform.localScale = Vector3.one;
        owner?.HideItemDetails();
    }
}
