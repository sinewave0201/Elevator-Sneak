using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class audioManager : MonoBehaviour
{
    [System.Serializable]
    public struct AudioList
    {
        public string flag;
        public AudioClip clip;
    }

    public static audioManager Instance { get; private set; }

    public AudioSource audioSource;
    public AudioList[] audioList;

    [Header("Scene Music")]
    [SerializeField] private string titleSceneName = "beginning";
    [SerializeField] private string titleFlag = "title";   // Elevator Sneak-Final
    [SerializeField] private string gameFlag = "game";     // task1-loop

    [Header("Crossfade")]
    [SerializeField] private float crossfadeDuration = 1.5f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;

    // Two sources: one plays the current song, the other fades in the next one.
    private AudioSource[] sources;
    private int activeIndex;
    private Coroutine fadeRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    private void Awake()
    {
        // Only one audioManager survives across scenes, so the music never restarts.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        AudioSource second = gameObject.AddComponent<AudioSource>();
        second.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
        second.priority = audioSource.priority;
        second.spatialBlend = audioSource.spatialBlend;

        sources = new[] { audioSource, second };
        foreach (AudioSource source in sources)
        {
            // An AudioSource with Play On Awake may already be playing its own clip here.
            // Stop it and clear it, so this script fully controls what plays and at what volume.
            source.Stop();
            source.clip = null;
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0f;
        }
        activeIndex = 0;

        SceneManager.sceneLoaded += HandleSceneLoaded;
        PlayForScene(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        Instance = null;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayForScene(scene);
    }

    private void PlayForScene(Scene scene)
    {
        string flag = scene.name == titleSceneName ? titleFlag : gameFlag;
        Play(flag);
    }

    // Crossfades to the clip with this flag. If it is already playing, keeps going without restarting.
    public void Play(string flag)
    {
        AudioClip clip = GetClip(flag);
        if (clip == null)
        {
            Debug.LogWarning($"audioManager: no clip with flag \"{flag}\". Audio List has: {DescribeAudioList()}");
            return;
        }

        AudioSource current = sources[activeIndex];
        if (current.clip == clip && current.isPlaying)
            return;

        int nextIndex = 1 - activeIndex;
        AudioSource next = sources[nextIndex];

        // If the next source is still fading out this same clip, fade it back in instead of restarting it.
        if (next.clip != clip || !next.isPlaying)
        {
            // The music files don't preload, so load the data before playing.
            if (clip.loadState != AudioDataLoadState.Loaded)
                clip.LoadAudioData();

            next.clip = clip;
            next.volume = 0f;
            next.Play();
        }

        Debug.Log($"audioManager: crossfading to \"{clip.name}\" (load state {clip.loadState}, isPlaying {next.isPlaying})");

        activeIndex = nextIndex;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Crossfade(current, next));
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to)
    {
        float fromStart = from.volume;
        float toStart = to.volume;
        float t = 0f;

        while (t < crossfadeDuration)
        {
            // Unscaled time so pausing or slowing the game doesn't freeze the fade.
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / crossfadeDuration);
            from.volume = Mathf.Lerp(fromStart, 0f, k);
            to.volume = Mathf.Lerp(toStart, musicVolume, k);
            yield return null;
        }

        from.volume = 0f;
        from.Stop();
        to.volume = musicVolume;
        fadeRoutine = null;

        Debug.Log($"audioManager: fade done, \"{(to.clip != null ? to.clip.name : "none")}\" volume {to.volume}, isPlaying {to.isPlaying}, AudioListener.volume {AudioListener.volume}, paused {AudioListener.pause}");
    }

    private AudioClip GetClip(string flag)
    {
        if (audioList == null)
            return null;

        foreach (AudioList entry in audioList)
        {
            // Ignore case and stray spaces, so "Title " still matches "title".
            if (entry.flag != null &&
                string.Equals(entry.flag.Trim(), flag.Trim(), System.StringComparison.OrdinalIgnoreCase))
                return entry.clip;
        }
        return null;
    }

    private string DescribeAudioList()
    {
        if (audioList == null || audioList.Length == 0)
            return "(empty)";

        var parts = new System.Collections.Generic.List<string>();
        foreach (AudioList entry in audioList)
        {
            string clipName = entry.clip != null ? entry.clip.name : "no clip";
            parts.Add($"\"{entry.flag}\" -> {clipName}");
        }
        return string.Join(", ", parts);
    }
}
