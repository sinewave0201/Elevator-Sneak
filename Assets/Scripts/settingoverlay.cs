using UnityEngine;

using UnityEngine.EventSystems;

public class SettingsOverlayController : MonoBehaviour
{
    public GameObject settingsOverlay;
    public RectTransform settingsPanel;

    void Update()
    {
        if (settingsOverlay == null ||
            !settingsOverlay.activeSelf)
            return;

        // 按 Esc 关闭
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseSettings();
            return;
        }

        // 点击图片外面关闭
        if (Input.GetMouseButtonDown(0))
        {
            if (settingsPanel == null)
                return;

            bool inside = RectTransformUtility
                .RectangleContainsScreenPoint(
                    settingsPanel,
                    Input.mousePosition,
                    GetUICamera(settingsPanel)
                );

            if (!inside)
            {
                CloseSettings();
            }
        }
    }

    Camera GetUICamera(RectTransform rect)
    {
        Canvas canvas = rect.GetComponentInParent<Canvas>();

        if (canvas == null ||
            canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return canvas.worldCamera;
    }

    public void OpenSettings()
    {
        settingsOverlay.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsOverlay.SetActive(false);
    }
}

