# Z-Targeting System - Quick Reference

## Component Overview

### ZTargetingSystem (Player)
**Purpose**: Core targeting logic - finds, validates, and manages lock-on targets

**Key Settings**:
- `Lock On Range`: How far you can lock onto enemies (default: 20m)
- `Max Angle From Camera`: Field of view for targeting (default: 70°)
- `Target Layers`: Which layers contain enemies
- `Target Tag`: Tag to identify enemies (default: "Enemy")

**Key Methods**:
```csharp
zTargeting.IsLocked              // Is currently locked onto a target?
zTargeting.CurrentTarget         // The current locked target (or null)
zTargeting.ToggleLock()          // Toggle lock on/off
zTargeting.AcquireTarget()       // Find and lock best target
zTargeting.ClearTarget()         // Release current target
zTargeting.GetTargetDirection()  // Direction to target
```

---

### TargetedPlayerRotation (Player)
**Purpose**: Handles player rotation - faces target when locked, follows camera when free

**Key Settings**:
- `Free Rotation Speed`: Rotation speed in normal movement (default: 12)
- `Locked Rotation Speed`: Rotation speed when locked on (default: 15)
- `Collision Rotation Multiplier`: Slows rotation during camera collision (default: 0.4)

**How It Works**:
- Locked Mode: Automatically rotates player to face the target
- Free Mode: Rotates player based on movement input
- Collision Mode: Reduces rotation speed when camera is colliding

---

### ZTargetingCamera (Camera)
**Purpose**: Camera controller optimized for lock-on gameplay

**Key Settings**:
- `Default Distance`: Camera distance in free mode (default: 5m)
- `Locked Distance`: Camera distance when locked (default: 6m)
- `Target Framing Offset`: Horizontal offset to frame target (default: 0.25)
- `Adjust Distance By Target Distance`: Pulls camera back when target is far

**Camera Behavior**:
- Free Mode: Follows player, manual rotation with mouse/stick
- Locked Mode: Frames both player and target, limited manual rotation
- Collision Mode: Automatically pulls camera forward to avoid geometry

---

### ZTargetingAdapter (Player)
**Purpose**: Connects new system to existing PlayerCombat and UI

**What It Does**:
- Updates `PlayerCombat.currentTarget` when you lock/unlock
- Shows/hides letterbox UI when locked
- Provides compatibility layer for existing systems

---

## API Comparison

### Old System → New System

| Old LockOnSystem | New ZTargetingSystem |
|-----------------|---------------------|
| `IsLocked()` | `IsLocked` (property) |
| `currentLockTarget` | `CurrentTarget` (property) |
| `SetLockTarget(t)` | `SetTarget(t)` |
| `ClearLock()` | `ClearTarget()` |
| `ToggleLock()` | `ToggleLock()` |

### Accessing the System

```csharp
// Get the targeting system
ZTargetingSystem zTarget = player.GetComponent<ZTargetingSystem>();

// Check if locked
if (zTarget.IsLocked)
{
    Transform target = zTarget.CurrentTarget;
    Vector3 dirToTarget = zTarget.GetTargetDirection();
    // Use target...
}

// Force lock onto specific target
zTarget.SetTarget(enemyTransform);

// Clear lock
zTarget.ClearTarget();
```

---

## Events

Subscribe to events for custom behavior:

```csharp
void Start()
{
    ZTargetingSystem zTarget = GetComponent<ZTargetingSystem>();
    
    // Called when target changes (includes lock/unlock)
    zTarget.OnTargetChanged += (newTarget, oldTarget) => {
        if (newTarget != null)
            Debug.Log($"Locked onto {newTarget.name}");
        else
            Debug.Log("Lock released");
    };
    
    // Called when lock state changes
    zTarget.OnLockStateChanged += (isLocked) => {
        if (isLocked)
            Debug.Log("LOCKED ON");
        else
            Debug.Log("LOCK OFF");
    };
}
```

---

## Common Tasks

### Change Lock-On Range
```csharp
// In Inspector: ZTargetingSystem > Lock On Range
// Or in code:
zTargeting.lockRange = 30f; // Allow locking from 30m away
```

### Make Targeting More Forgiving
```csharp
// In Inspector: ZTargetingSystem
// - Increase "Max Angle From Camera" to 90 or higher
// - Increase "Lock On Range"
// - Disable "Require Line Of Sight"
```

### Adjust Rotation Feel When Locked
```csharp
// In Inspector: TargetedPlayerRotation
// - Increase "Locked Rotation Speed" for snappier rotation
// - Decrease "Locked Rotation Smoothing" for more responsive feel
```

### Fix Camera During Lock
```csharp
// In Inspector: ZTargetingCamera
// - Adjust "Locked Distance" for camera pull-back
// - Adjust "Target Framing Offset" to shift target left/right
// - Enable "Adjust Distance By Target Distance" for dynamic camera
```

### Handle Camera Collision Better
```csharp
// In Inspector: TargetedPlayerRotation
// - Enable "Limit Rotation During Collision"
// - Adjust "Collision Rotation Multiplier" (0.3-0.6)
// Lower = more restrictive during collision
```

---

## Debugging

### Enable Debug Info

1. **ZTargetingSystem**: Check "Show Debug Info" to log targeting events
2. **TargetedPlayerRotation**: Check "Show Debug Info" to log rotation state
3. **ZTargetingCamera**: Check "Show Debug Info" to log camera state
4. **Add ZTargetingDebugUI**: Shows real-time info on screen

### Common Issues

**Can't lock onto enemies**:
- Check enemy has correct tag (`Enemy`)
- Check enemy is on correct layer (set in ZTargetingSystem)
- Check enemy is within range and angle
- Look at Scene view - yellow gizmo shows lock range

**Player spins wildly**:
- Reduce "Locked Rotation Speed"
- Increase "Locked Rotation Smoothing"
- Check "Limit Rotation During Collision" is enabled

**Camera feels wrong**:
- Adjust smoothing values in ZTargetingCamera
- Check collision handler is assigned
- Try different locked distance values

**Lock breaks too easily**:
- Increase "Target Lost Distance" (should be > Lock On Range)
- Increase "Target Validation Interval" to check less frequently

---

## Integration Notes

### With PlayerCombat
The `ZTargetingAdapter` automatically updates `PlayerCombat.currentTarget`:
```csharp
// PlayerCombat can just use:
if (currentTarget != null)
{
    // Attack target
}
```

### With PlayerLocomotion
You have two options:

**Option 1**: Let `TargetedPlayerRotation` handle everything
- Just return early from `HandleRotation()` when locked
- TargetedPlayerRotation runs in LateUpdate and handles rotation

**Option 2**: Integrate into PlayerLocomotion
- Replace `lockOnSystem` references with `zTargeting`
- Use `zTargeting.IsLocked` and `zTargeting.CurrentTarget`

### With Attack System
```csharp
// In PlayerCombat or similar:
ZTargetingSystem zTarget = GetComponent<ZTargetingSystem>();
if (zTarget.IsLocked && zTarget.CurrentTarget != null)
{
    // Face target during attack
    Vector3 attackDir = zTarget.GetTargetDirection();
    // Apply attack toward target
}
```

---

## Performance Notes

- Target validation runs at `targetValidationInterval` (default: 0.2s)
- Dead targets are cached to avoid repeated checks
- Uses HashSet for efficient duplicate filtering
- Minimal GC allocations (no per-frame List allocations)

---

## Extension Ideas

### Add Target Switching (Right Stick)
```csharp
// In InputManager or PlayerLocomotion:
Vector2 rightStick = // ... get right stick input
if (rightStick.magnitude > 0.8f)
{
    zTargeting.SwitchToNextTarget(rightStick);
}
```

### Add Target Priority System
Extend `ZTargetingSystem.FindBestTarget()`:
```csharp
// Prefer bosses over normal enemies
AiAgent agent = target.GetComponent<AiAgent>();
if (agent != null && agent.isBoss)
{
    combinedScore *= 0.5f; // Make bosses more "attractive"
}
```

### Add Lock-On Indicator
Subscribe to events and spawn UI:
```csharp
zTargeting.OnTargetChanged += (newTarget, oldTarget) => {
    if (newTarget != null)
    {
        // Spawn lock-on reticle above target
        Instantiate(lockOnReticlePrefab, newTarget.position + Vector3.up * 2f, Quaternion.identity);
    }
};
```
