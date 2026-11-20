using UnityEngine;
using UnityEngine.UI;

public class SuspicionGraphGraphic : MaskableGraphic
{
    [SerializeField, Range(0, 100)] private float suspicion = 18f;
    [SerializeField] private Color gridColor = new Color(0.32f, 0.38f, 0.33f, 0.25f);

    public void SetSuspicion(int value)
    {
        suspicion = Mathf.Clamp(value, 0, 100);
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = rectTransform.rect;
        AddGrid(vh, r);
        Color32 line = color;
        float baseY = Mathf.Lerp(r.yMin + r.height * .70f, r.yMin + r.height * .30f, suspicion / 100f);
        float[] bumps = { 0f, 0f, -2f, 1f, 0f, -6f, 2f, 0f, 0f, -3f, 1f, 0f, 0f, -4f, 2f, 0f, 0f };
        float step = r.width / (bumps.Length - 1);
        for (int i = 0; i < bumps.Length - 1; i++)
        {
            Vector2 a = new(r.xMin + i * step, baseY + bumps[i]);
            Vector2 b = new(r.xMin + (i + 1) * step, baseY + bumps[i + 1]);
            AddLine(vh, a, b, 1.6f, line);
        }
    }

    void AddGrid(VertexHelper vh, Rect r)
    {
        for (int i = 0; i <= 8; i++) AddLine(vh, new Vector2(Mathf.Lerp(r.xMin, r.xMax, i / 8f), r.yMin), new Vector2(Mathf.Lerp(r.xMin, r.xMax, i / 8f), r.yMax), .55f, gridColor);
        for (int i = 0; i <= 4; i++) AddLine(vh, new Vector2(r.xMin, Mathf.Lerp(r.yMin, r.yMax, i / 4f)), new Vector2(r.xMax, Mathf.Lerp(r.yMin, r.yMax, i / 4f)), .55f, gridColor);
    }

    static void AddLine(VertexHelper vh, Vector2 a, Vector2 b, float width, Color color)
    {
        Vector2 n = Vector2.Perpendicular((b - a).normalized) * width * .5f;
        int v = vh.currentVertCount;
        vh.AddVert(a - n, color, Vector2.zero); vh.AddVert(a + n, color, Vector2.up);
        vh.AddVert(b + n, color, Vector2.one); vh.AddVert(b - n, color, Vector2.right);
        vh.AddTriangle(v, v + 1, v + 2); vh.AddTriangle(v, v + 2, v + 3);
    }
}
