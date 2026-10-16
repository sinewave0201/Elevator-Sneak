using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [Header("Task Timer Scenes")]
    [SerializeField] private string staircaseSceneName = "StaircaseScene";
    [SerializeField] private string elevatorSceneName = "ElevatorScene";

    [SerializeField] private PlayerData data = new PlayerData();

    public PlayerData Data => data;
    public PlayerStats Stats => data.Stats;
    public PlayerInventory Inventory => data.Inventory;
    public PlayerProgress Progress => data.Progress;
    public PlayerDisguise Disguise => data.Disguise;
    public PlayerTaskTimer TaskTimer => data.TaskTimer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        data.Initialize();
        SceneManager.sceneLoaded += HandleSceneLoaded;
        ApplySceneTimeMultiplier(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        Instance = null;
    }

    private void Update()
    {
        data.TaskTimer.Tick(Time.deltaTime);
    }

    public void StartNewGame()
    {
        data.Reset();
        ApplySceneTimeMultiplier(SceneManager.GetActiveScene());
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySceneTimeMultiplier(scene);
    }

    private void ApplySceneTimeMultiplier(Scene scene)
    {
        if (scene.name == staircaseSceneName)
        {
            data.TaskTimer.SetTimeMultiplier(TravelMethod.Stairs);
            return;
        }

        if (scene.name == elevatorSceneName)
        {
            data.TaskTimer.SetTimeMultiplier(TravelMethod.Elevator);
            return;
        }

        data.TaskTimer.ResetTimeMultiplier();
    }
}
