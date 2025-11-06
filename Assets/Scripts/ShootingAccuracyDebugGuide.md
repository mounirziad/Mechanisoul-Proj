# Shooting Accuracy Debug Guide 🎯

## Issue: Bullets not going where crosshair points

The shooting direction is now correctly calculated as:
```csharp
Vector3 shootDirection = (aimTarget - shootOrigin).normalized;
```

This calculates the direction **from the shoot point (player's hands) TO the aim target (crosshair position)**.

---

## ✅ How to Debug

### 1. Enable Debug Visualization
In Inspector, on `PlayerCombat` component:
- ☑️ Enable **Show Aim Debug**

### 2. Enter Play Mode and Shoot
You'll see debug output in Console and Scene view:

**Console Output:**
```
Using CameraAdapter aim target: (10.5, 2.3, 15.7)
Shoot Origin: (2.1, 1.5, 3.2), Aim Target: (10.5, 2.3, 15.7), Direction: (0.8, 0.1, 0.6)
```

**Scene View Lines:**
- **Yellow line**: From shoot point to aim target (where you're aiming)
- **Red line**: The actual raycast direction (should match yellow)

### 3. Check Which Aim System Is Being Used

Look for one of these messages in Console:

✅ **"Using CameraAdapter aim target"**  
→ Great! Using the custom AimCamera system (most accurate)

⚠️ **"Using Camera raycast aim target"**  
→ Fallback to manual raycast (still works, but CameraAdapter would be better)

❌ **"Camera.main is null! Using shootPoint forward fallback"**  
→ Problem! Camera not found

---

## 🔍 Common Issues & Solutions

### Issue 1: "CameraAdapter returns Vector3.zero"

**Symptoms:**
- Console shows: "Using Camera raycast aim target" instead of "Using CameraAdapter aim target"
- Bullets don't aim accurately

**Causes:**
1. `CameraManagerAdapter` not properly initialized
2. `AimCamera` not returning valid aim target
3. Not in aim mode when shooting

**Solutions:**

#### A. Verify CameraManagerAdapter exists:
```
Hierarchy → PlayerCharacter
Inspector → Check for "CameraManagerAdapter" component
```

#### B. Check AimCamera is active:
```
Hierarchy → MainCamera/AimCamera
Inspector → GameObject should be ACTIVE when aiming
```

#### C. Add this debug code temporarily:
```csharp
// In PlayerCombat.GetCameraCenterAimPoint(), add at the top:
Debug.Log($"CameraAdapter null? {cameraAdapter == null}");
if (cameraAdapter != null)
{
    Debug.Log($"IsAimActive? {cameraAdapter.IsAimCameraActive()}");
    Vector3 target = cameraAdapter.GetAimTarget();
    Debug.Log($"AimTarget: {target}");
}
```

---

### Issue 2: Bullets still shoot in wrong direction

**Symptoms:**
- Yellow line points at target correctly
- Red line points somewhere else
- Bullets follow the red line

**Cause:** The direction calculation is being overridden somewhere else.

**Solution:** Check that you're not using the OLD shooting code. Search for:
```csharp
// BAD - Old code that uses player rotation:
shootDirection = shootPoint.forward;

// GOOD - New code that aims at target:
shootDirection = (aimTarget - shootOrigin).normalized;
```

---

### Issue 3: Raycast doesn't hit anything

**Symptoms:**
- Yellow/red lines point correctly
- But nothing takes damage
- Tracers might be missing

**Causes:**
1. `hitscanLayerMask` excludes the target layer
2. `hitscanRange` is too short
3. Enemy colliders are on wrong layer

**Solutions:**

#### A. Check LayerMask includes enemies:
```
Inspector → PlayerCombat → Hitscan Layer Mask
Ensure "Enemy" layer is CHECKED ✓
```

#### B. Increase range:
```
Inspector → PlayerCombat → Hitscan Range
Try: 100 or 500
```

#### C. Verify enemy is on correct layer:
```
Hierarchy → Select Enemy
Inspector → Top-right: Layer = "Enemy"
```

---

### Issue 4: Crosshair and bullets aim at different points

**Symptoms:**
- Crosshair is at one location
- Bullets hit a different location

**Cause:** Crosshair and shooting are using different aim calculations.

**Solution:** Both should use `CameraManagerAdapter.GetAimTarget()`:

**CrosshairController:**
```csharp
Vector3 crosshairTarget = cameraAdapter.GetAimTarget();
```

**PlayerCombat:**
```csharp
Vector3 shootTarget = cameraAdapter.GetAimTarget();
```

They should return the **same value**!

---

## 🧪 Quick Test Procedure

### Test 1: Basic Aim Test
1. ✅ Play Mode
2. ✅ Enable `showAimDebug` in Inspector
3. ✅ Aim at a wall (right mouse button)
4. ✅ Check Scene view for **yellow line** from hands to crosshair
5. ✅ Shoot (left mouse button)
6. ✅ Check Console: Should say "Using CameraAdapter aim target"
7. ✅ Verify **red line** overlaps **yellow line**

**Expected Result:** Red and yellow lines should be identical!

### Test 2: Moving Target Test
1. ✅ Aim at enemy
2. ✅ Enemy moves
3. ✅ Crosshair should track enemy
4. ✅ Shoot
5. ✅ Bullet should hit where crosshair was pointing

**Expected Result:** Enemy takes damage!

### Test 3: Different Distances
1. ✅ Aim at close target (5 meters)
2. ✅ Shoot - should hit
3. ✅ Aim at far target (50 meters)
4. ✅ Shoot - should hit

**Expected Result:** Accurate at all distances!

---

## 📊 Understanding the Shooting System

### The Complete Flow:

```
Player Presses Shoot Button
    ↓
HandleShoot() called
    ↓
ShootHitscan() called
    ↓
GetCameraCenterAimPoint() gets target:
    ├─ Try: cameraAdapter.GetAimTarget()
    │      ↓
    │   AimCamera.GetAimTarget()
    │      ↓
    │   Returns: World position where crosshair points
    │
    ├─ Fallback: Camera.main raycast from screen center
    │      ↓
    │   Physics.Raycast from camera through screen center
    │
    └─ Final Fallback: shootPoint.forward * 10
    ↓
Calculate Direction:
shootDirection = (aimTarget - shootPoint.position).normalized
    ↓
Physics.Raycast(shootPoint, shootDirection, ...)
    ↓
Hit Enemy → Apply Damage!
```

---

## 🎯 What Should Happen

### With Custom Camera System (AimCamera):

1. **You hold right mouse button** to aim
2. **AimCamera activates**
3. **AimCamera performs raycast** from camera through screen center
4. **AimCamera.aimTarget** stores the hit point
5. **Crosshair appears** at that position (via CrosshairController)
6. **You press left mouse button** to shoot
7. **PlayerCombat.GetCameraCenterAimPoint()** calls `cameraAdapter.GetAimTarget()`
8. **Returns AimCamera.aimTarget** (the exact point where crosshair is!)
9. **Calculates direction** from shoot point to that target
10. **Shoots raycast** in that direction
11. **Bullet hits exactly where crosshair was pointing!** ✨

---

## 🔧 Advanced Debugging

### Visualize All Aim Data in Scene View

Add this to `PlayerCombat.OnDrawGizmos()`:

```csharp
if (isAiming && shootPoint != null)
{
    // Get all the aim data
    Vector3 aimTarget = GetCameraCenterAimPoint();
    Vector3 shootOrigin = shootPoint.position;
    Vector3 shootDirection = (aimTarget - shootOrigin).normalized;
    
    // Camera position
    Camera cam = Camera.main;
    if (cam != null)
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(cam.transform.position, 0.3f);
        Gizmos.DrawLine(cam.transform.position, aimTarget);
    }
    
    // Shoot point
    Gizmos.color = Color.blue;
    Gizmos.DrawWireSphere(shootOrigin, 0.15f);
    
    // Aim target (where crosshair points)
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(aimTarget, 0.25f);
    
    // The shot trajectory
    Gizmos.color = Color.yellow;
    Gizmos.DrawLine(shootOrigin, aimTarget);
    Gizmos.DrawLine(shootOrigin, shootOrigin + shootDirection * 100f);
}
```

This shows:
- **Cyan**: Camera position and camera-to-target line
- **Blue**: Shoot point (player's hands)
- **Red**: Aim target (crosshair)
- **Yellow**: Shot trajectory

**All yellow lines should point at the red sphere!**

---

## ✅ Final Checklist

Before testing, verify:

- [ ] `showAimDebug` is enabled in PlayerCombat Inspector
- [ ] `CameraManagerAdapter` component is on PlayerCharacter
- [ ] `CustomCameraController` component is on PlayerCharacter
- [ ] `CustomCameraController` has AimCamera reference set
- [ ] `AimCamera` GameObject exists in scene
- [ ] `shootPoint` is assigned in PlayerCombat Inspector
- [ ] `hitscanLayerMask` includes Enemy layer
- [ ] `CrosshairController` is on UI and has CameraAdapter reference

---

## 🎮 Test Now!

1. **Enter Play Mode**
2. **Press Right Mouse** to aim
3. **Check Console** for "Using CameraAdapter aim target"
4. **Check Scene view** for yellow/red debug lines
5. **Press Left Mouse** to shoot
6. **Verify bullets go where lines point**

If you see **"Using CameraAdapter aim target"** in console, and the bullets still don't aim correctly, **share the console output** and I'll help you fix it!

---

## 💡 Pro Tip

If everything looks correct but bullets still miss, it might be:
1. **Spawn point correction** - Try disabling `useSpawnCorrection`
2. **Trajectory correction** - The `GetCorrectedAimDirection()` might be interfering
3. **Animation** - Player animation might be moving the shootPoint

**Simple test:** Temporarily change `ShootHitscan()` to:
```csharp
Vector3 aimTarget = GetCameraCenterAimPoint();
Vector3 shootDirection = (aimTarget - shootPoint.position).normalized;
// Shoot directly at target, no corrections
Physics.Raycast(shootPoint.position, shootDirection, out hit, ...);
```

This bypasses all corrections and shoots **directly** at crosshair!
