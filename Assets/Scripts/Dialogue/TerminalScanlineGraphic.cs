using UnityEngine;
using UnityEngine.UI;

public class TerminalScanlineGraphic : MaskableGraphic
{
    [SerializeField, Min(2f)] private float lineSpacing = 4f;
    [SerializeField, Range(.25f, 2f)] private float lineThickness = .75f;
    [SerializeField, Min(.02f)] private float scrollSpeed = .18f;

    private float lastRefresh;

    private void Update()
    {
        if (Time.unscaledTime - lastRefresh < .08f)
            return;

        lastRefresh = Time.unscaledTime;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = rectTransform.rect;
        float offset = Mathf.Repeat(Time.unscaledTime * scrollSpeed * lineSpacing, lineSpacing);
        Color32 scanlineColor = color;

        for (float y = rect.yMin - lineSpacing + offset; y < rect.yMax; y += lineSpacing)
        {
            int index = vh.currentVertCount;
            vh.AddVert(new Vector3(rect.xMin, y), scanlineColor, Vector2.zero);
            vh.AddVert(new Vector3(rect.xMax, y), scanlineColor, Vector2.right);
            vh.AddVert(new Vector3(rect.xMax, y + lineThickness), scanlineColor, Vector2.one);
            vh.AddVert(new Vector3(rect.xMin, y + lineThickness), scanlineColor, Vector2.up);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }
    }
}
