using UnityEngine;

public class GameProgressionManager : MonoBehaviour
{
    public static GameProgressionManager Instance { get; private set; }

    private const string BREAK_ROOM_SCENE = "Break Room";
    private const string BOSS_BREAK_ROOM_SCENE = "Break Room Boss";
    private const string FIRST_LEVEL_SCENE = "WareHouseLevel";

    private bool hasReachedBoss = false;
    private bool hasExitedInitialBreakRoom = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetReachedBoss(bool reached)
    {
        hasReachedBoss = reached;
        Debug.Log($"GameProgressionManager: Player has reached boss: {hasReachedBoss}");
    }

    public bool HasReachedBoss()
    {
        return hasReachedBoss;
    }

    public void SetExitedInitialBreakRoom(bool exited)
    {
        hasExitedInitialBreakRoom = exited;
        Debug.Log($"GameProgressionManager: Player has exited initial break room: {hasExitedInitialBreakRoom}");
    }

    public bool HasExitedInitialBreakRoom()
    {
        return hasExitedInitialBreakRoom;
    }

    public string GetRespawnScene()
    {
        if (hasReachedBoss)
        {
            return BOSS_BREAK_ROOM_SCENE;
        }
        else
        {
            return BREAK_ROOM_SCENE;
        }
    }

    public string GetBreakRoomExitScene()
    {
        return FIRST_LEVEL_SCENE;
    }

    public void ResetProgression()
    {
        hasReachedBoss = false;
        hasExitedInitialBreakRoom = false;
        Debug.Log("GameProgressionManager: Progression reset");
    }
}
