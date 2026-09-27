using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [SerializeField] private PlayerData data = new PlayerData();

    public PlayerData Data => data;
    public PlayerStats Stats => data.Stats;
    public PlayerInventory Inventory => data.Inventory;
    public PlayerProgress Progress => data.Progress;
    public PlayerDisguise Disguise => data.Disguise;

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
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void StartNewGame()
    {
        data.Reset();
    }
}
