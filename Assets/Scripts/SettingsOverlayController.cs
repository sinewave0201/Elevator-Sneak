
using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsOverlayController : MonoBehaviour
{
    public GameObject settingsOverlay;
    public RectTransform settingsPanel;

    void Update()
    {
        if (settingsOverlay == null ||
            !settingsOverlay.activeSelf)
            return;

        // Press ESC to close
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseSettings();
            return;
        }

        // Click outside to close
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (settingsPanel == null)
                return;

            Canvas canvas =
                settingsPanel.GetComponentInParent<Canvas>();

            Camera uiCamera = null;

            if (canvas != null &&
                canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = canvas.worldCamera;
            }

            Vector2 mousePosition =
                Mouse.current.position.ReadValue();

            bool isInside =
                RectTransformUtility.RectangleContainsScreenPoint(
                    settingsPanel,
                    mousePosition,
                    uiCamera
                );

            if (!isInside)
            {
                CloseSettings();
            }
        }
    }

    public void OpenSettings()
    {
        if (settingsOverlay != null)
            settingsOverlay.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);
    }
}
