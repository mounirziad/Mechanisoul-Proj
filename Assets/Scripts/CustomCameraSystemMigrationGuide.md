# Custom Camera System Migration Guide

## Overview
This guide helps you migrate from Cinemachine to the custom camera system (FreeLookCamera + AimCamera + CustomCameraController).

---

## ✅ Automatic Compatibility

The following scripts have been updated to work with **both** systems automatically via `CameraManagerAdapter`:

### 1. **CrosshairController**
- ✓ Now uses `CameraManagerAdapter` instead of `ThirdPersonAimCameraManager`
- ✓ Works with both old Cinemachine and new custom system
- ✓ Auto-detects which system is active

### 2. **CameraPriorityFixer**  
- ✓ Detects if custom camera system is active
- ✓ Skips Cinemachine priority management when using custom cameras
- ✓ No manual changes needed

### 3. **CameraManagerAdapter**
- ✓ Provides unified API for both camera systems
- ✓ Automatically added to player GameObject when needed
- ✓ Forwards calls to appropriate camera system

---

## 🔄 API Compatibility Reference

All these methods work the same regardless of which camera system you use:

```csharp
// Get the adapter (works for both systems)
CameraManagerAdapter adapter = GetComponent<CameraManagerAdapter>();

// Or add it automatically if missing
if (adapter == null)
{
    adapter = gameObject.AddComponent<CameraManagerAdapter>();
}

// These methods work identically for both systems:
Vector3 aimTarget = adapter.GetAimTarget();
Vector3 aimDirection = adapter.GetAimDirection();
bool isAiming = adapter.IsAimCameraActive();
Transform aimTransform = adapter.GetAimTargetTransform(); // Custom system only
```

---

## 📝 Migration Checklist

### Step 1: Add CameraManagerAdapter to Player
The adapter is automatically added when needed, but you can manually add it:

1. Select your **PlayerCharacter** in hierarchy
2. Add Component → **CameraManagerAdapter**

### Step 2: Update Custom Scripts (If Any)

If you have custom scripts that reference `ThirdPersonAimCameraManager`, update them:

**Before (Cinemachine only):**
```csharp
public class MyShootingScript : MonoBehaviour
{
    private ThirdPersonAimCameraManager aimManager;
    
    void Start()
    {
        aimManager = GetComponent<ThirdPersonAimCameraManager>();
    }
    
    void Shoot()
    {
        Vector3 target = aimManager.GetAimTarget();
        Vector3 direction = (target - transform.position).normalized;
        // Fire projectile...
    }
}
```

**After (Works with both systems):**
```csharp
public class MyShootingScript : MonoBehaviour
{
    private CameraManagerAdapter cameraAdapter;
    
    void Start()
    {
        cameraAdapter = GetComponent<CameraManagerAdapter>();
        if (cameraAdapter == null)
        {
            cameraAdapter = gameObject.AddComponent<CameraManagerAdapter>();
        }
    }
    
    void Shoot()
    {
        Vector3 target = cameraAdapter.GetAimTarget();
        Vector3 direction = (target - transform.position).normalized;
        // Fire projectile...
    }
}
```

### Step 3: Verify PlayerCombat Compatibility

The `PlayerCombat` script should work automatically. It reads `isAiming` from `InputManager`, which is used by both systems.

If you have custom shooting logic in PlayerCombat, ensure it uses:
- `CameraManagerAdapter` instead of `ThirdPersonAimCameraManager`
- OR keep using `ThirdPersonAimCameraManager` if still on Cinemachine

### Step 4: Test All Camera-Dependent Features

Test these features to ensure they work correctly:

- ✅ Aiming (right mouse button)
- ✅ Crosshair visibility when aiming
- ✅ Shooting accuracy (bullets go where crosshair points)
- ✅ Lock-on camera switching
- ✅ Freelook camera when not aiming

---

## 🔍 Common Migration Scenarios

### Scenario 1: Shooting System
```csharp
// Works with both camera systems
CameraManagerAdapter camAdapter = GetComponent<CameraManagerAdapter>();

if (camAdapter.IsAimCameraActive())
{
    Vector3 aimPoint = camAdapter.GetAimTarget();
    Vector3 shootDirection = (aimPoint - weaponMuzzle.position).normalized;
    
    RaycastHit hit;
    if (Physics.Raycast(weaponMuzzle.position, shootDirection, out hit, 100f))
    {
        // Hit something
        DealDamage(hit.collider.gameObject);
    }
}
```

### Scenario 2: Grenade Throwing
```csharp
CameraManagerAdapter camAdapter = GetComponent<CameraManagerAdapter>();
Vector3 targetPosition = camAdapter.GetAimTarget();

// Calculate throw arc
Vector3 throwVelocity = CalculateThrowVelocity(
    hand.position, 
    targetPosition, 
    throwForce
);

grenade.GetComponent<Rigidbody>().AddForce(throwVelocity, ForceMode.Impulse);
```

### Scenario 3: Enemy AI Line of Sight
```csharp
CameraManagerAdapter camAdapter = player.GetComponent<CameraManagerAdapter>();

if (camAdapter.IsAimCameraActive())
{
    Vector3 playerAimDirection = camAdapter.GetAimDirection();
    
    // Check if player is aiming at this enemy
    Vector3 toEnemy = (transform.position - player.position).normalized;
    float dotProduct = Vector3.Dot(playerAimDirection, toEnemy);
    
    if (dotProduct > 0.9f) // Player is aiming at this enemy
    {
        TakeCover();
    }
}
```

---

## 🛠️ Advanced: Detecting Which System is Active

```csharp
CameraManagerAdapter adapter = GetComponent<CameraManagerAdapter>();

if (adapter.UsingCustomCameraSystem())
{
    Debug.Log("Using new custom camera system");
    // Custom system specific logic
}
else if (adapter.UsingCinemachineSystem())
{
    Debug.Log("Using Cinemachine camera system");
    // Cinemachine specific logic
}
```

---

## ⚠️ Breaking Changes

### Methods NOT Available in Custom System:
- `SetAimCameraDistance()` - Use AimCamera inspector settings instead
- `SetAimCameraOffset()` - Use AimCamera inspector settings instead  
- `ResetToOriginalSettings()` - Not needed in custom system

### New Methods (Custom System Only):
- `GetAimTargetTransform()` - Returns Transform of aim target for parenting effects

---

## 🎯 Best Practices

### 1. Always Use CameraManagerAdapter
```csharp
// ✅ GOOD - Works with both systems
CameraManagerAdapter adapter = GetComponent<CameraManagerAdapter>();
Vector3 target = adapter.GetAimTarget();

// ❌ BAD - Only works with one system
ThirdPersonAimCameraManager manager = GetComponent<ThirdPersonAimCameraManager>();
Vector3 target = manager.GetAimTarget();
```

### 2. Cache the Adapter Reference
```csharp
public class MyScript : MonoBehaviour
{
    private CameraManagerAdapter cameraAdapter;
    
    void Awake()
    {
        cameraAdapter = GetComponent<CameraManagerAdapter>();
    }
    
    void Update()
    {
        // Use cached reference
        if (cameraAdapter.IsAimCameraActive())
        {
            // Do something
        }
    }
}
```

### 3. Check for Null
```csharp
if (cameraAdapter != null)
{
    Vector3 target = cameraAdapter.GetAimTarget();
    // Use target...
}
```

---

## 📊 Feature Comparison

| Feature | Cinemachine | Custom System | Notes |
|---------|-------------|---------------|-------|
| GetAimTarget() | ✅ | ✅ | Identical API |
| GetAimDirection() | ✅ | ✅ | Identical API |
| IsAimCameraActive() | ✅ | ✅ | Identical API |
| GetAimTargetTransform() | ❌ | ✅ | Custom only |
| Wall Collision | ✅ | ✅ | Custom has better shoulder handling |
| Smoothing | ✅ | ✅ | Custom has more control |
| Lock-on Integration | ✅ | ✅ | Both work |

---

## 🐛 Troubleshooting

### Crosshair Not Showing
1. Check if `CrosshairController` has `CameraManagerAdapter` reference
2. Ensure `PlayerCombat.isAiming` is being set correctly
3. Verify crosshair GameObject is not disabled

### Shooting Inaccuracy  
1. Verify you're using `GetAimTarget()` not just camera forward
2. Check that bullets spawn from weapon muzzle, not camera
3. Ensure no LayerMask is blocking aim raycasts

### Camera Not Switching
1. Check `CustomCameraController` is on player
2. Verify FreeLookCamera and AimCamera references are set
3. Ensure `InputManager.aimInput` is being set correctly

---

## ✅ Verification Script

Add this to verify your setup:

```csharp
using UnityEngine;

public class CameraSystemVerifier : MonoBehaviour
{
    void Start()
    {
        CameraManagerAdapter adapter = GetComponent<CameraManagerAdapter>();
        
        if (adapter == null)
        {
            Debug.LogError("CameraManagerAdapter is missing!");
            return;
        }
        
        if (adapter.UsingCustomCameraSystem())
        {
            Debug.Log("✅ Using Custom Camera System");
            
            CustomCameraController controller = GetComponent<CustomCameraController>();
            if (controller == null)
            {
                Debug.LogError("❌ CustomCameraController is missing!");
            }
        }
        else if (adapter.UsingCinemachineSystem())
        {
            Debug.Log("✅ Using Cinemachine System");
            
            ThirdPersonAimCameraManager manager = GetComponent<ThirdPersonAimCameraManager>();
            if (manager == null)
            {
                Debug.LogError("❌ ThirdPersonAimCameraManager is missing!");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No camera system detected!");
        }
        
        Debug.Log($"GetAimTarget() returns: {adapter.GetAimTarget()}");
        Debug.Log($"IsAimCameraActive(): {adapter.IsAimCameraActive()}");
    }
}
```

Add this script to your player and check the console on play!

---

## 📞 Need Help?

If you encounter issues:
1. Check this migration guide
2. Verify all references are set in Inspector
3. Test with the verification script above
4. Check console for errors
