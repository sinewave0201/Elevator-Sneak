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

        // Music only (ignored for sound effects). Every music track loops by crossfading its end into a fresh copy of itself.
        [Tooltip("Music: how many seconds the loop crossfade takes, i.e. it crossfades over the LAST this-many seconds of the track. 0 = use Crossfade Duration.")]
        public float loopFadeDuration;
        [Tooltip("Music, advanced: counted from the START of the track, in seconds. This pass starts fading out here while the next pass fades in. 0 = automatic: track length minus the loop fade duration.")]
        public float loopFadeOutAt;
        [Tooltip("Music: seconds into the track where the next pass starts playing. Usually 0 (the beginning).")]
        public float loopRestartAt;
        [Tooltip("Music: play the track through once and stop, no loop (e.g. the credits song).")]
        public bool playOnce;
    }

    [System.Serializable]
    public struct SceneMusic
    {
        [Tooltip("Scene name, e.g. \"end\".")]
        public string sceneName;
        [Tooltip("Audio List flag to play when that scene loads, e.g. \"credits\".")]
        public string flag;
    }

    public static audioManager Instance { get; private set; }

    public AudioSource audioSource;
    public AudioList[] audioList;

    [Header("Scene Music")]
    [Tooltip("Scenes that play the title theme. Every other scene plays the game music (unless StopMusic() is holding it).")]
    [SerializeField] private string[] titleSceneNames = { "Title", "beginning" };
    [Tooltip("Scenes that always play a specific track when they load, e.g. end -> credits. Checked before the title/game rules.")]
    [SerializeField] private SceneMusic[] sceneMusic = { new SceneMusic { sceneName = "end", flag = "credits" } };
    [Tooltip("Scenes with no music at all. Music fades out when one of these loads.")]
    [SerializeField] private string[] silentSceneNames = { };
    [SerializeField] private string titleFlag = "title";   // Elevator Sneak-Final
    [SerializeField] private string gameFlag = "game";     // task1prerelease2

    [Header("Crossfade")]
    [Tooltip("Used for music fades: switching tracks, StopMusic(), and looping a track that has no Loop Fade Duration of its own.")]
    [SerializeField, Min(0.05f)] private float crossfadeDuration = 1.5f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;

    [Header("Sound Effects")]
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [Tooltip("How long a sound effect takes to fade out when it is cut off by a scene finishing loading.")]
    [SerializeField, Min(0f)] private float sfxCutFadeDuration = 0.15f;

    // Two sources: one plays the current song, the other fades in the next one.
    private AudioSource[] sources;
    // Separate source for one-shot sounds (footsteps etc.); lives on this object, so it keeps playing across scene loads.
    private AudioSource sfxSource;
    private int activeIndex;
    private Coroutine fadeRoutine;
    private bool waitingForCue;
    private bool stopSfxOnSceneLoad;
    private Coroutine sfxFadeRoutine;

    // The Audio List entry that is currently playing as music (for its loop crossfade settings).
    private AudioList currentEntry;
    private bool hasCurrentEntry;

    // The task timer we listen to, so the task music stops when the task is completed (clocking in).
    private PlayerTaskTimer subscribedTaskTimer;

    // The prefab at Assets/Resources/audioManager.prefab. Edit the Audio List and settings there.
    private const string PrefabResourcePath = "audioManager";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    // Spawns the audioManager when the game starts, whichever scene you press Play in (Title, beginning, Level 1...).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
            return;

        GameObject prefab = Resources.Load<GameObject>(PrefabResourcePath);
        if (prefab == null)
        {
            Debug.LogError($"audioManager: no prefab at Resources/{PrefabResourcePath}. Music will not play.");
            return;
        }

        Instantiate(prefab).name = prefab.name;
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
            // Never use the hard AudioSource loop: Update() starts the next pass and crossfades into it.
            source.loop = false;
            source.playOnAwake = false;
            source.volume = 0f;
        }
        activeIndex = 0;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
        sfxSource.spatialBlend = audioSource.spatialBlend;
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        SceneManager.sceneLoaded += HandleSceneLoaded;
        SubscribeToTaskTimer();
        PlayForScene(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        if (subscribedTaskTimer != null)
            subscribedTaskTimer.TaskCompleted -= HandleTaskCompleted;
        Instance = null;
    }

    // PlayerDataManager lives in the gameplay scenes, not Title, so try again whenever a scene loads.
    private void SubscribeToTaskTimer()
    {
        PlayerTaskTimer timer = PlayerDataManager.Instance != null ? PlayerDataManager.Instance.TaskTimer : null;
        if (timer == null || timer == subscribedTaskTimer)
            return;

        if (subscribedTaskTimer != null)
            subscribedTaskTimer.TaskCompleted -= HandleTaskCompleted;
        subscribedTaskTimer = timer;
        subscribedTaskTimer.TaskCompleted += HandleTaskCompleted;
    }

    // Task 1 done (the player clocked in): fade the task music out and keep it silent.
    private void HandleTaskCompleted()
    {
        StopMusic();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // e.g. the running footsteps: they cover the loading time and stop once the new scene is ready.
        if (stopSfxOnSceneLoad)
        {
            stopSfxOnSceneLoad = false;
            StopSFX();
        }

        SubscribeToTaskTimer();
        PlayForScene(scene);
    }

    private void PlayForScene(Scene scene)
    {
        if (System.Array.IndexOf(silentSceneNames, scene.name) >= 0)
        {
            StopMusic();
            return;
        }

        if (sceneMusic != null)
        {
            foreach (SceneMusic entry in sceneMusic)
            {
                if (entry.sceneName == scene.name && !string.IsNullOrWhiteSpace(entry.flag))
                {
                    Play(entry.flag);
                    return;
                }
            }
        }

        if (System.Array.IndexOf(titleSceneNames, scene.name) >= 0)
        {
            Play(titleFlag);
            return;
        }

        // After StopMusic(), stay silent across scene loads until something calls Play() again
        // (e.g. the receptionist conversation switching to the game music).
        if (waitingForCue)
            return;

        Play(gameFlag);
    }

    // Fades the music out and keeps it silent, even across scene loads, until Play() is called.
    public void StopMusic()
    {
        waitingForCue = true;
        hasCurrentEntry = false;
        FadeTo(null);
    }

    // Crossfades to the clip with this flag. If it is already playing, keeps going without restarting.
    public void Play(string flag)
    {
        if (!TryGetEntry(flag, out AudioList entry) || entry.clip == null)
        {
            Debug.LogWarning($"audioManager: no clip with flag \"{flag}\". Audio List has: {DescribeAudioList()}");
            return;
        }

        AudioClip clip = entry.clip;
        waitingForCue = false;
        currentEntry = entry;
        hasCurrentEntry = true;
        WarnAboutLoopPoints(entry);

        AudioSource current = sources[activeIndex];
        if (current.clip == clip && current.isPlaying)
        {
            // Already the current song: just make sure it is fading in, not out (e.g. right after StopMusic()).
            if (current.volume < musicVolume || fadeRoutine != null)
                FadeTo(current);
            return;
        }

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
            next.time = 0f;
        }
        activeIndex = nextIndex;
        FadeTo(next);
    }

    private void Update()
    {
        // Loop logic for every music track: each pass plays once; at its fade point the next pass starts and they crossfade.
        // Play-once tracks (credits) just end; no next pass.
        if (!hasCurrentEntry || waitingForCue || currentEntry.playOnce)
            return;

        AudioSource current = sources[activeIndex];
        if (current.clip != currentEntry.clip || !current.isPlaying)
            return;

        float fadeOutAt = GetLoopFadeOutAt(currentEntry);

        if (current.time < fadeOutAt)
            return;

        // Reached the fade point: start a fresh copy on the other source and crossfade into it.
        int nextIndex = 1 - activeIndex;
        AudioSource next = sources[nextIndex];
        next.clip = current.clip;
        next.loop = false;
        next.volume = 0f;
        // A restart point at or after the fade point would trigger again immediately, so fall back to the start.
        float restartAt = currentEntry.loopRestartAt < fadeOutAt ? Mathf.Max(0f, currentEntry.loopRestartAt) : 0f;
        next.Play();
        next.time = restartAt;

        activeIndex = nextIndex;
        FadeTo(next, GetLoopFadeDuration(currentEntry));
    }

    private float GetLoopFadeDuration(AudioList entry)
    {
        return entry.loopFadeDuration > 0f ? entry.loopFadeDuration : crossfadeDuration;
    }

    // Where this track's loop crossfade starts, in seconds from the start of the track.
    private float GetLoopFadeOutAt(AudioList entry)
    {
        float length = entry.clip.length;
        float auto = length - GetLoopFadeDuration(entry);
        // 0, or a time the track doesn't reach, means automatic: the crossfade fills the last Crossfade Duration seconds.
        float fadeOutAt = entry.loopFadeOutAt > 0f && entry.loopFadeOutAt < length ? entry.loopFadeOutAt : auto;
        return Mathf.Clamp(fadeOutAt, 0.1f, Mathf.Max(0.1f, length - 0.05f));
    }

    private void WarnAboutLoopPoints(AudioList entry)
    {
        float length = entry.clip.length;
        float duration = GetLoopFadeDuration(entry);
        if (entry.loopFadeOutAt >= length)
            Debug.LogWarning($"audioManager: \"{entry.flag}\" Loop Fade Out At is {entry.loopFadeOutAt}s, but the track is only {length:0.##}s long. Using automatic ({length - duration:0.##}s). It counts from the START of the track.");
        else if (entry.loopFadeOutAt > 0f && entry.loopFadeOutAt < length * 0.5f)
            Debug.LogWarning($"audioManager: \"{entry.flag}\" Loop Fade Out At is {entry.loopFadeOutAt}s, so it will loop every ~{entry.loopFadeOutAt:0.##}s. It counts from the START of the track ({length:0.##}s long). To crossfade over the last N seconds, set Loop Fade Duration to N and Loop Fade Out At to 0.");

        if (entry.loopRestartAt > 0f && entry.loopRestartAt >= GetLoopFadeOutAt(entry))
            Debug.LogWarning($"audioManager: \"{entry.flag}\" Loop Restart At ({entry.loopRestartAt}s) is at or after the fade point, so the next pass starts from 0 instead.");
    }

    // Plays the clip with this flag once (no loop), on top of the music. It stops by itself when the clip ends.
    // stopOnSceneLoad: cut it off (with a short fade) as soon as the next scene finishes loading.
    public void PlaySFX(string flag, bool stopOnSceneLoad = false)
    {
        AudioClip clip = GetClip(flag);
        if (clip == null)
        {
            Debug.LogWarning($"audioManager: no clip with flag \"{flag}\". Audio List has: {DescribeAudioList()}");
            return;
        }

        // Cancel a cut-off fade that may still be running from an earlier sound.
        if (sfxFadeRoutine != null)
        {
            StopCoroutine(sfxFadeRoutine);
            sfxFadeRoutine = null;
        }
        sfxSource.volume = 1f;

        stopSfxOnSceneLoad = stopOnSceneLoad;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // Quickly fades out and stops every sound effect that is playing.
    public void StopSFX()
    {
        if (sfxFadeRoutine != null)
            StopCoroutine(sfxFadeRoutine);
        sfxFadeRoutine = StartCoroutine(SfxFadeOut());
    }

    private IEnumerator SfxFadeOut()
    {
        float start = sfxSource.volume;
        float t = 0f;
        while (t < sfxCutFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            sfxSource.volume = Mathf.Lerp(start, 0f, Mathf.Clamp01(t / sfxCutFadeDuration));
            yield return null;
        }

        sfxSource.Stop();
        sfxSource.volume = 1f;
        sfxFadeRoutine = null;
    }

    // Fades `target` up to musicVolume and every other source down to silence. Null fades everything out.
    // The one fade used for all music: switching tracks, looping a track, and StopMusic().
    private void FadeTo(AudioSource target, float duration = -1f)
    {
        if (duration <= 0f)
            duration = crossfadeDuration;
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(target, duration));
    }

    private IEnumerator FadeRoutine(AudioSource target, float duration)
    {
        float[] startVolumes = new float[sources.Length];
        for (int i = 0; i < sources.Length; i++)
            startVolumes[i] = sources[i].volume;

        float t = 0f;
        while (t < duration)
        {
            // Unscaled time so pausing or slowing the game doesn't freeze the fade.
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            for (int i = 0; i < sources.Length; i++)
            {
                float end = sources[i] == target ? musicVolume : 0f;
                sources[i].volume = Mathf.Lerp(startVolumes[i], end, k);
            }
            yield return null;
        }

        foreach (AudioSource source in sources)
        {
            if (source == target)
            {
                source.volume = musicVolume;
            }
            else
            {
                source.volume = 0f;
                source.Stop();
            }
        }
        fadeRoutine = null;
    }

    private AudioClip GetClip(string flag)
    {
        return TryGetEntry(flag, out AudioList entry) ? entry.clip : null;
    }

    private bool TryGetEntry(string flag, out AudioList result)
    {
        result = default;
        if (audioList == null || flag == null)
            return false;

        foreach (AudioList entry in audioList)
        {
            // Ignore case and stray spaces, so "Title " still matches "title".
            if (entry.flag != null &&
                string.Equals(entry.flag.Trim(), flag.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                result = entry;
                return true;
            }
        }
        return false;
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
