using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class ElevatorTitleIntro : MonoBehaviour
{
    [Header("3D title shot")]
    [SerializeField] private Transform shotCamera;
    [SerializeField] private Transform orbitCenter;
    [SerializeField] private Transform frontPose;
    [SerializeField] private Transform lensTarget;
    [SerializeField] private Transform closePose;

    [SerializeField] private RectTransform securityCamera;
    [SerializeField] private RectTransform lens;
    [SerializeField] private CanvasGroup hallway;
    [SerializeField] private CanvasGroup feed;
    [SerializeField] private Image transition;
    [SerializeField] private Button viewFeed;
    [SerializeField] private Button startGame;
    [SerializeField] private string openingScene = "Assets/Scenes/beginning.unity";

    [Header("Monitor signal")]
    [SerializeField] private Image monitorTint;
    [SerializeField] private RectTransform scanlineLayer;
    [SerializeField] private Image recordingDot;

    private TMPro.TMP_Text viewFeedLabel;
    private float viewFeedLabelAlpha;

    private void LateUpdate()
    {
        if (viewFeedLabel != null && viewFeedLabel.isActiveAndEnabled)
        {
            const float pulseDuration = 1.65f;
            float pulsePhase = Mathf.Repeat(Time.unscaledTime, pulseDuration) / pulseDuration;
            float brightness = Mathf.Lerp(0.48f, 1f, 0.5f + 0.5f * Mathf.Cos(pulsePhase * Mathf.PI * 2f));
            viewFeedLabel.alpha = viewFeedLabelAlpha * brightness;
        }

        if (monitorTint == null || !feed.gameObject.activeInHierarchy || feed.alpha <= 0f)
            return;

        float time = Time.unscaledTime;
        float noise = Mathf.PerlinNoise(0.27f, time * 6.6f);
        float phase = Mathf.Repeat(time, 3.8f);
        float dip = Mathf.Max(0f, 1f - Mathf.Abs(phase - 3.2f) / 0.14f);
        Color tint = monitorTint.color;
        tint.a = 0.695f + 0.095f * noise + 0.06f * dip;

        if (recordingDot != null)
        {
            Color dot = recordingDot.color;
            dot.a = Mathf.Repeat(time, 1.2f) < 0.79f ? 1f : 0.18f;
            recordingDot.color = dot;
        }
        monitorTint.color = tint;

        scanlineLayer.anchoredPosition = new Vector2(0f, Mathf.Repeat(time * 10f, 8f));
    }

    private void Awake()
    {
        viewFeedLabel = viewFeed.GetComponentInChildren<TMPro.TMP_Text>(true);
        if (viewFeedLabel != null)
            viewFeedLabelAlpha = viewFeedLabel.alpha;

        viewFeed.onClick.AddListener(OpenFeed);
        startGame.onClick.AddListener(StartGame);
    }

    private void OpenFeed()
    {
        viewFeed.interactable = false;
        viewFeed.gameObject.SetActive(false);
        StartCoroutine(RevealFeed());
    }

    [Header("Opening elevator door")]
    [SerializeField] private Transform elevatorDoorLeft;
    [SerializeField] private Transform elevatorDoorRight;
    [SerializeField] private Transform elevatorDoorFrame;

    private IEnumerator OpenElevatorDoor()
    {
        Vector3 leftClosed = elevatorDoorLeft.localPosition;
        Vector3 rightClosed = elevatorDoorRight.localPosition;
        Vector3 travel = elevatorDoorFrame.TransformVector(Vector3.right * 0.34f);
        Vector3 leftOpen = leftClosed - elevatorDoorLeft.parent.InverseTransformVector(travel);
        Vector3 rightOpen = rightClosed + elevatorDoorRight.parent.InverseTransformVector(travel);

        const float duration = 1.05f;
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            float eased = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            elevatorDoorLeft.localPosition = Vector3.Lerp(leftClosed, leftOpen, eased);
            elevatorDoorRight.localPosition = Vector3.Lerp(rightClosed, rightOpen, eased);
            yield return null;
        }

        elevatorDoorLeft.localPosition = leftOpen;
        elevatorDoorRight.localPosition = rightOpen;
    }

    private IEnumerator RevealFeed()
    {
        if (elevatorDoorLeft != null && elevatorDoorRight != null && elevatorDoorFrame != null)
            yield return OpenElevatorDoor();

        if (shotCamera != null)
            yield return PlayCctvShot();
        else
            yield return PlayFlatShot();

        SetTransitionAlpha(1f);
        hallway.gameObject.SetActive(false);
        feed.gameObject.SetActive(true);

        for (float elapsed = 0f; elapsed < 0.3f; elapsed += Time.unscaledDeltaTime)
        {
            float progress = elapsed / 0.3f;
            SetTransitionAlpha(1f - progress);
            feed.alpha = progress;
            yield return null;
        }

        SetTransitionAlpha(0f);
        feed.alpha = 1f;
        feed.interactable = true;
        feed.blocksRaycasts = true;
        startGame.Select();
    }

    private IEnumerator PlayCctvShot()
    {
        Vector3 center = orbitCenter.position;
        Vector3 initialOffset = shotCamera.position - center;
        Vector3 finalOffset = frontPose.position - center;
        Vector3 initialFraming = shotCamera.position + shotCamera.forward * initialOffset.magnitude;

        const float orbitDuration = 1.35f;
        for (float elapsed = 0f; elapsed < orbitDuration; elapsed += Time.unscaledDeltaTime)
        {
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / orbitDuration);
            shotCamera.position = center + Vector3.Slerp(initialOffset, finalOffset, progress);
            shotCamera.LookAt(Vector3.Lerp(initialFraming, lensTarget.position, progress));
            yield return null;
        }

        const float pushDuration = 0.78f;
        for (float elapsed = 0f; elapsed < pushDuration; elapsed += Time.unscaledDeltaTime)
        {
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / pushDuration);
            shotCamera.position = Vector3.Lerp(frontPose.position, closePose.position, progress);
            shotCamera.LookAt(lensTarget.position);
            SetTransitionAlpha(Mathf.InverseLerp(0.87f, 1f, progress));
            yield return null;
        }

        shotCamera.position = closePose.position;
        shotCamera.LookAt(lensTarget.position);
    }

    private IEnumerator PlayFlatShot()
    {
        Vector2 initialPosition = securityCamera.anchoredPosition;
        Vector2 lensPosition = lens.anchoredPosition;
        Quaternion initialRotation = securityCamera.localRotation;
        Vector2 endPosition = -lensPosition * 12f;

        for (float elapsed = 0f; elapsed < 1.15f; elapsed += Time.unscaledDeltaTime)
        {
            float progress = elapsed / 1.15f;
            float turn = Mathf.SmoothStep(0f, 1f, progress / 0.55f);
            float zoom = Mathf.SmoothStep(0f, 1f, (progress - 0.2f) / 0.8f);
            securityCamera.localRotation = Quaternion.Slerp(initialRotation, Quaternion.identity, turn);
            securityCamera.localScale = Vector3.one * Mathf.Lerp(1f, 12f, zoom);
            securityCamera.anchoredPosition = Vector2.Lerp(initialPosition, endPosition, zoom);
            SetTransitionAlpha(Mathf.InverseLerp(0.82f, 0.98f, progress));
            yield return null;
        }

    }

    private void SetTransitionAlpha(float alpha)
    {
        Color color = transition.color;
        color.a = alpha;
        transition.color = color;
    }

    private void StartGame()
    {
        if (!Application.CanStreamedLevelBeLoaded(openingScene))
        {
            Debug.LogError($"The opening scene must be in the active build profile: {openingScene}", this);
            return;
        }

        startGame.interactable = false;
        SceneManager.LoadScene(openingScene);
    }
}
