using UnityEngine;

public class Overload : MonoBehaviour, IDamage
{
    [Header("Stats")]
    [SerializeField] public float maxHealth = 75f;
    [SerializeField] public float currentHealth;
    [SerializeField] private float damageProvider = 8f;

    public float rotationSpeed;
    public GameObject blade;
    private bool attacking;

    private bool isDead = false;

    public float GetDamage()
    {
        return damageProvider;
    }
}
