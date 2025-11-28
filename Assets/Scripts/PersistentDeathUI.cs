using UnityEngine;

public class PersistentDeathUI : MonoBehaviour
{
    public static PersistentDeathUI Instance { get; private set; }

    [Header("Canvas Settings")]
    [SerializeField] private int canvasSortingOrder = 200;

    private Canvas canvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = canvasSortingOrder;
        }
        
        gameObject.SetActive(false);
    }
}
