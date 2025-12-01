using UnityEngine;
using TMPro;

public class AmmoDisplay : MonoBehaviour
{
    [SerializeField] private PlayerCombat playerCombatScript;
    [SerializeField] private TextMeshProUGUI ammoCountText;
    [SerializeField] private TextMeshProUGUI reloadingText;

    void Awake()
    {
        if (playerCombatScript == null)
        {
            playerCombatScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCombat>();
        }
    }

    void Start()
    {
        if (reloadingText != null)
        {
            reloadingText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        UpdateAmmoDisplay();
    }

    void UpdateAmmoDisplay()
    {
        if (playerCombatScript == null) return;

        if (playerCombatScript.IsReloading())
        {
            if (ammoCountText != null)
            {
                ammoCountText.gameObject.SetActive(false);
            }
            if (reloadingText != null)
            {
                reloadingText.gameObject.SetActive(true);
            }
        }
        else
        {
            if (ammoCountText != null)
            {
                ammoCountText.gameObject.SetActive(true);
                ammoCountText.text = $"{playerCombatScript.GetAmmoInClip()}/{playerCombatScript.GetClipSize()}";
            }
            if (reloadingText != null)
            {
                reloadingText.gameObject.SetActive(false);
            }
        }
    }
}
