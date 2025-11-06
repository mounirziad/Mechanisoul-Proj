# Z-Targeting System - Step-by-Step Setup

## Quick Start (5 Minutes)

Follow these exact steps to get the new system working:

---

## STEP 1: Backup Your Scene
**Time: 30 seconds**

1. Save your current scene (`Ctrl+S`)
2. Optionally duplicate the scene as backup
3. This allows easy rollback if needed

---

## STEP 2: Disable Old Lock-On System
**Time: 1 minute**

**On PlayerCharacter GameObject:**

1. Find `LockOnSystem` component
2. Uncheck the checkbox next to the component name (disables it)
3. Do NOT delete it yet - keep it for reference

**Result**: Old system is disabled but still visible for reference

---

## STEP 3: Add Core Targeting Component
**Time: 1 minute**

**On PlayerCharacter GameObject:**

1. Click "Add Component"
2. Type "ZTargetingSystem"
3. Click to add it

**Configure ZTargetingSystem:**
- Lock On Range: `20`
- Max Angle From Camera: `70`
- Target Layers: Click dropdown → Select `Enemy`
- Target Tag: `Enemy`
- Target Lost Distance: `25`
- Require Line Of Sight: ☐ (unchecked)
- Camera Transform: Drag `Lock-On-Camera` (or `MovementDefaultCamera`)
- Input Manager: Should auto-fill (PlayerCharacter's InputManager)

**Result**: Core targeting system is ready

---

## STEP 4: Add Player Rotation Handler
**Time: 1 minute**

**On PlayerCharacter GameObject:**

1. Click "Add Component"
2. Type "TargetedPlayerRotation"
3. Click to add it

**Configure TargetedPlayerRotation:**
- Z Targeting: Should auto-fill
- Camera Transform: Drag your main camera GameObject
- Input Manager: Should auto-fill
- Free Rotation Speed: `12`
- Free Rotation Smoothing: `0.15`
- Locked Rotation Speed: `15`
- Locked Rotation Smoothing: `0.1`
- Limit Rotation During Collision: ☑ (checked)
- Collision Rotation Multiplier: `0.4`

**Result**: Player rotation is now handled by new system

---

## STEP 5: Add Integration Adapter
**Time: 30 seconds**

**On PlayerCharacter GameObject:**

1. Click "Add Component"
2. Type "ZTargetingAdapter"
3. Click to add it

**Configure ZTargetingAdapter:**
- Z Targeting: Should auto-fill
- Player Combat: Should auto-fill
- Letterbox UI: Find and drag `LetterboxPrefab` from Hierarchy (if you have one)
- Update Combat Target: ☑ (checked)
- Control Letterbox: ☑ (checked)

**Result**: New system is connected to your existing combat code

---

## STEP 6: Setup Lock-On Camera
**Time: 1 minute**

### Option A: Use New Camera (Recommended)

**On Lock-On-Camera GameObject:**

1. Find `LockOnCamera` component
2. Uncheck it to disable (don't delete yet)
3. Click "Add Component"
4. Type "ZTargetingCamera"
5. Click to add it

**Configure ZTargetingCamera:**
- Player Target: Drag `PlayerCharacter`
- Z Targeting: Drag `PlayerCharacter` (component will auto-find ZTargetingSystem)
- Player Offset: `(0, 1.5, 0)`
- Shoulder Offset: `(0.6, 0.3, 0)`
- Default Distance: `5`
- Min Distance: `2`
- Max Distance: `10`
- Locked Distance: `6`
- Target Vertical Offset: `1`
- Target Framing Offset: `0.25`
- Adjust Distance By Target Distance: ☑ (checked)
- Distance Adjustment Factor: `0.4`
- Free Rotation Speed: `8`
- Locked Rotation Speed: `12`
- Manual Rotation Sensitivity: `2`
- Allow Manual Rotation: ☑ (checked)
- Position Smoothing: `10`
- Rotation Smoothing: `8`
- Lock Transition Smoothing: `6`
- Distance Smoothing: `5`
- Input Manager: Drag `PlayerCharacter`
- Use Mouse Input: ☑ (checked)
- Mouse Sensitivity: `0.15`
- Collision Handler: Should already be on camera
- Enable Collision Handling: ☑ (checked)

### Option B: Keep Old Camera (Bridge Mode)

**On Lock-On-Camera GameObject:**

1. Keep `LockOnCamera` enabled
2. Click "Add Component"
3. Type "LockOnCameraZTargetBridge"
4. Click to add it

**Configure Bridge:**
- Lock On Camera: Should auto-fill
- Z Targeting: Drag `PlayerCharacter`
- Lock Target Point: Drag `PlayerCharacter/LockTargetPoint`
- Target Height Offset: `1`
- Follow Speed: `10`

**In LockOnCamera component:**
- Change "Lock On System" field to point to `PlayerCharacter` (the ZTargetingAdapter will handle it)

**Result**: Camera is connected to new targeting system

---

## STEP 7: Test Basic Functionality
**Time: 1 minute**

1. Enter Play Mode
2. Press your Lock-On button (should be in Input System as "EnemyLockOn")
3. Verify:
   - ☑ Player locks onto enemy
   - ☑ Player rotates to face enemy
   - ☑ Camera frames player and enemy
   - ☑ Can move while locked
   - ☑ Press lock button again to unlock

**If it works**: Great! Continue to Step 8

**If it doesn't work**: Check troubleshooting section below

---

## STEP 8: Optional - Add Debug UI
**Time: 30 seconds**

For easier testing and debugging:

**On PlayerCharacter GameObject:**

1. Click "Add Component"
2. Type "ZTargetingDebugUI"
3. Click to add it

**Configure Debug UI:**
- Z Targeting: Should auto-fill
- Player Rotation: Should auto-fill
- Show Debug UI: ☑ (checked)
- Font Size: `16`
- Text Color: White

**Result**: On-screen debug info shows lock state and target info

---

## STEP 9: Fine-Tune Settings
**Time: 2-5 minutes**

### Make Locking Easier:
- ZTargetingSystem → Increase `Max Angle From Camera` to `90`
- ZTargetingSystem → Increase `Lock On Range` to `25`

### Make Rotation Snappier:
- TargetedPlayerRotation → Increase `Locked Rotation Speed` to `20`
- TargetedPlayerRotation → Decrease `Locked Rotation Smoothing` to `0.05`

### Adjust Camera Feel:
- ZTargetingCamera → Adjust `Locked Distance` (try 5-8)
- ZTargetingCamera → Adjust `Target Framing Offset` (try 0-0.5)
- ZTargetingCamera → Try toggling `Adjust Distance By Target Distance`

### Fix Camera Collision Issues:
- TargetedPlayerRotation → Adjust `Collision Rotation Multiplier` (try 0.3-0.6)
- Lower values = less rotation during collision (more stable)
- Higher values = more rotation during collision (more responsive)

---

## STEP 10: Clean Up (Optional)
**Time: 1 minute**

Once everything works perfectly:

1. Delete old `LockOnSystem` component from PlayerCharacter
2. Delete old `LockOnCamera` component from camera (if using new ZTargetingCamera)
3. Delete `LockOnCameraActivator` component if present
4. Remove old script files if desired:
   - `LockOnSystem.cs`
   - `LockOnCamera.cs`
   - `LockOnCameraActivator.cs`

**Result**: Clean project with only new system

---

## Troubleshooting

### ❌ "Can't lock onto any enemy"

**Check These:**
1. ZTargetingSystem → `Target Tag` is "Enemy"
2. Your enemies have tag "Enemy"
3. ZTargetingSystem → `Target Layers` includes Enemy layer
4. Your enemies are on Enemy layer
5. Enemy is within `Lock On Range` (check yellow gizmo in Scene view)
6. Enemy is within `Max Angle From Camera` degrees in front of you

**Quick Fix:**
- Increase `Lock On Range` to 50
- Increase `Max Angle From Camera` to 120
- Disable `Require Line Of Sight`

---

### ❌ "Player rotation is jerky/broken"

**Check These:**
1. TargetedPlayerRotation component is enabled
2. `Camera Transform` is assigned
3. `Z Targeting` reference is set

**Quick Fix:**
- Increase `Locked Rotation Smoothing` to 0.2
- Decrease `Locked Rotation Speed` to 10
- Check `Limit Rotation During Collision` is ON

---

### ❌ "Camera doesn't follow target"

**If using ZTargetingCamera:**
1. Check `Player Target` is set to PlayerCharacter
2. Check `Z Targeting` is set to PlayerCharacter
3. Check `Enable Collision Handling` is ON
4. Check `Collision Handler` is assigned

**If using old LockOnCamera:**
1. Make sure `LockOnCameraZTargetBridge` is added
2. Check bridge has correct references
3. Check `LockTargetPoint` exists in scene

---

### ❌ "Camera spins wildly during collision"

**Check These:**
1. TargetedPlayerRotation → `Limit Rotation During Collision` is ON
2. TargetedPlayerRotation → `Collision Rotation Multiplier` is low (0.3-0.5)

**Quick Fix:**
- Set `Collision Rotation Multiplier` to `0.3`
- Increase `Max Rotation Angle Per Frame` to `180`

---

### ❌ "Lock breaks immediately"

**Check These:**
1. `Target Lost Distance` > `Lock On Range`
2. `Target Validation Interval` isn't too low (0.2 is good)
3. Enemy isn't dying/disabled immediately

**Quick Fix:**
- Set `Target Lost Distance` to `30`
- Set `Target Validation Interval` to `0.3`

---

### ❌ "PlayerCombat doesn't get the target"

**Check These:**
1. ZTargetingAdapter is on PlayerCharacter
2. ZTargetingAdapter → `Update Combat Target` is ON
3. ZTargetingAdapter → `Player Combat` reference is set

**Quick Fix:**
- Remove and re-add ZTargetingAdapter
- Check PlayerCombat is on same GameObject

---

### ❌ "Input doesn't work"

**Check These:**
1. Input action "EnemyLockOn" exists in your Input System
2. Input action is in "PlayerActions" action map
3. ZTargetingSystem → `Input Manager` is set

**Quick Fix:**
- Open Input System asset
- Go to PlayerActions → Add Action "EnemyLockOn"
- Bind it to a button (e.g., right mouse, Q key, controller face button right)

---

## Advanced: Integration with PlayerLocomotion

If you want PlayerLocomotion to stop handling rotation when locked:

**Open PlayerLocomotion.cs**

**Find the `HandleRotation()` method**

**Add this at the very start:**

```csharp
private void HandleRotation()
{
    if (cameraObject == null) return;
    if (isJumping) { return; }
    
    // NEW CODE START
    ZTargetingSystem zTarget = GetComponent<ZTargetingSystem>();
    if (zTarget != null && zTarget.IsLocked)
    {
        // TargetedPlayerRotation handles rotation in LateUpdate when locked
        return;
    }
    // NEW CODE END
    
    // Rest of existing rotation code...
```

This prevents double-rotation handling.

---

## Verification Checklist

Use this to verify everything is working:

**Player Setup:**
- ☐ PlayerCharacter has `ZTargetingSystem`
- ☐ PlayerCharacter has `TargetedPlayerRotation`
- ☐ PlayerCharacter has `ZTargetingAdapter`
- ☐ Old `LockOnSystem` is disabled or deleted
- ☐ All references are filled (no "None" in Inspector)

**Camera Setup:**
- ☐ Lock-On-Camera has `ZTargetingCamera` OR bridge
- ☐ Camera references PlayerCharacter
- ☐ Camera has `CameraCollisionHandler`
- ☐ Old camera script is disabled (if using new one)

**Input Setup:**
- ☐ Input System has "EnemyLockOn" action
- ☐ Action is bound to a key/button
- ☐ Action is in "PlayerActions" map

**Enemy Setup:**
- ☐ Enemies have tag "Enemy"
- ☐ Enemies are on correct layer
- ☐ Enemies have colliders
- ☐ Enemies have `AiAgent` or `BasicEnemyHealth` component

**Testing:**
- ☐ Can press button to lock on
- ☐ Player rotates to face enemy
- ☐ Camera frames both player and enemy
- ☐ Can move while locked
- ☐ Lock releases when enemy dies
- ☐ Lock releases when enemy is too far
- ☐ Can toggle lock off with button
- ☐ Letterbox appears/disappears (if enabled)

---

## Next Steps

Once the basic system works:

1. **Tune the Feel**: Adjust speeds and smoothing to your preference
2. **Add Visual Feedback**: Create lock-on reticle, target indicators
3. **Add Audio**: Lock-on sound effects, target acquired cues
4. **Add Features**: Target switching with right stick
5. **Polish**: Add screen shake, zoom effects, particle effects

---

## Support

If you're still having issues:

1. Enable "Show Debug Info" on all components
2. Enable "Show Gizmos" in Scene view
3. Add `ZTargetingDebugUI` component
4. Check Console for error messages
5. Verify all enemy GameObjects in Scene view
