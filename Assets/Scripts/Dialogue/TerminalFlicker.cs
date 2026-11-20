using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TerminalFlicker : MonoBehaviour
{
    [SerializeField, Range(0f, .15f)] private float maximumDip = .035f;
    [SerializeField, Min(.2f)] private float minimumInterval = 2.5f;
    [SerializeField, Min(.02f)] private float flickerDuration = .08f;

    private CanvasGroup canvasGroup;
    private float nextFlickerTime;
    private float flickerEndTime;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        canvasGroup.alpha = 1f;
        ScheduleNext();
    }

    private void Update()
    {
        if (Time.unscaledTime >= nextFlickerTime)
        {
            flickerEndTime = Time.unscaledTime + flickerDuration;
            ScheduleNext();
        }

        canvasGroup.alpha = Time.unscaledTime < flickerEndTime
            ? 1f - maximumDip
            : 1f;
    }

    private void ScheduleNext()
    {
        nextFlickerTime = Time.unscaledTime + minimumInterval + Random.Range(0.5f, 2.2f);
    }
}
