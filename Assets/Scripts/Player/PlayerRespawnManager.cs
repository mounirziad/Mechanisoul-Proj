using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerRespawnManager : MonoBehaviour
{
    public static PlayerRespawnManager Instance { get; private set; }

    [Header("Respawn Settings")]
    [SerializeField] private float respawnHealth = 100f;
    [SerializeField] private bool resetVelocity = true;
    [SerializeField] private float sceneTransitionDelay = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float fadeInDuration = 1f;

    private GameObject player;
    private PlayerHealth playerHealth;
    private PlayerRagdoll playerRagdoll;
    private Rigidbody playerRigidbody;
    private PlayerLocomotion playerLocomotion;

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

    private void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerRagdoll = player.GetComponent<PlayerRagdoll>();
            playerRigidbody = player.GetComponent<Rigidbody>();
            playerLocomotion = player.GetComponent<PlayerLocomotion>();
        }
        else
        {
            Debug.LogWarning("PlayerRespawnManager: Player not found! Make sure the player has the 'Player' tag.");
        }
    }

    public void RespawnPlayer()
    {
        StartCoroutine(RespawnPlayerCoroutine());
    }

    private IEnumerator RespawnPlayerCoroutine()
    {
        if (player == null)
        {
            FindPlayer();
        }

        if (player == null)
        {
            Debug.LogError("PlayerRespawnManager: Cannot respawn - player not found!");
            yield break;
        }

        FadingScript fadingScript = FindFadingScript();
        
        if (fadingScript != null)
        {
            fadingScript.FadeOut();
            yield return new WaitForSeconds(fadeOutDuration);
        }
        else
        {
            yield return new WaitForSeconds(sceneTransitionDelay);
        }

        if (GameProgressionManager.Instance != null)
        {
            string respawnScene = GameProgressionManager.Instance.GetRespawnScene();
            Debug.Log($"PlayerRespawnManager: Loading respawn scene: {respawnScene}");
            
            ResetPlayerState();
            
            SceneManager.sceneLoaded += OnRespawnSceneLoaded;
            SceneManager.LoadScene(respawnScene);
        }
        else
        {
            Debug.LogError("PlayerRespawnManager: GameProgressionManager.Instance is null! Cannot determine respawn scene.");
        }
    }

    private void OnRespawnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnRespawnSceneLoaded;
        StartCoroutine(FullyResetPlayerAfterSceneLoad());
    }

    private IEnumerator FullyResetPlayerAfterSceneLoad()
    {
        yield return new WaitForEndOfFrame();
        
        FindPlayer();
        
        if (player != null)
        {
            ResetPlayerState();
            Debug.Log("PlayerRespawnManager: Player fully reset after scene load");
        }
        
        FadingScript fadingScript = FindFadingScript();
        
        if (fadingScript != null)
        {
            yield return new WaitForSeconds(0.1f);
            fadingScript.FadeIn();
        }
    }

    private void ResetPlayerState()
    {
        if (playerHealth != null)
        {
            playerHealth.Revive();
            playerHealth.Heal(respawnHealth);
        }

        if (playerRagdoll != null)
        {
            playerRagdoll.DeactivateRagdoll();
        }

        if (PersistentDeathUI.Instance != null)
        {
            PersistentDeathUI.Instance.gameObject.SetActive(false);
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        if (playerLocomotion != null)
        {
            playerLocomotion.enabled = true;
            playerLocomotion.inAirTimer = 0f;
            playerLocomotion.isGrounded = true;
            playerLocomotion.isJumping = false;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    private FadingScript FindFadingScript()
    {
        FadingScript fadingScript = FindFirstObjectByType<FadingScript>();
        
        if (fadingScript == null)
        {
            Debug.LogWarning("PlayerRespawnManager: FadingScript not found in the current scene. Make sure each scene has a FadingCanvas.");
        }
        
        return fadingScript;
    }
}
