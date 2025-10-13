using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(menuName = "Attacks/Normal Attack")]
public class AttackSO : ScriptableObject
{
    [Header("Animation")]
    public AnimatorOverrideController animatorOV;
    
    [Header("Damage")]
    public float damage;
    
    [Header("Movement")]
    public float moveDistance = 1.5f;      // How far forward this attack moves
    public float moveSpeed = 8f;           // How fast the movement is
    public float moveDuration = 0.3f;      // How long the movement lasts
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f); // Movement falloff curve
    
    [Header("Rotation")]
    public bool rotateTowardsTarget = true; // Whether to rotate towards target during attack
    public float rotationSpeed = 720f;     // Degrees per second rotation speed
    
    [Header("Attack Speed")]
    [Tooltip("Base animation speed multiplier for this specific attack (1.0 = normal speed)")]
    public float baseAnimationSpeed = 1f;
    
    [Tooltip("Whether this attack should be affected by Joy upgrade attack speed modifiers")]
    public bool affectedByAttackSpeedUpgrades = true;
    
    [Tooltip("Attack type tags for categorization (e.g., 'Light', 'Heavy', 'Special')")]
    public List<string> attackTypeTags = new List<string> { "Normal" };
}
