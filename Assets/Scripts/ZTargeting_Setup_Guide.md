# Z-Targeting System Setup Guide

## Overview

This new z-targeting system is a complete replacement for your existing lock-on system. It's designed to work cleanly with your lock-on camera and resolve the rotation issues you were experiencing.

## Key Components

1. **ZTargetingSystem** - Core targeting logic (target acquisition, validation, switching)
2. **TargetedPlayerRotation** - Handles player rotation in both free and locked modes
3. **ZTargetingCamera** - Camera controller for lock-on mode
4. **ZTargetingAdapter** - Bridges the new system with your existing PlayerCombat

## Setup Instructions

### Step 1: Prepare Your Player Character

1. Select the `PlayerCharacter` GameObject in the scene
2. **DISABLE or REMOVE** the following components (don't delete yet, just disable):
   - `LockOnSystem` (old system)
   - Keep everything else as is

### Step 2: Add New Z-Targeting Components to Player

On the `PlayerCharacter` GameObject, add these new components in this order:

1. **Add ZTargetingSystem**
   - Lock On Range: `20`
   - Max Angle From Camera: `70`
   - Target Layers: `Enemy` layer
   - Target Tag: `Enemy`
   - Target Lost Distance: `25`
   - Require Line Of Sight: `false` (can enable later if needed)
   - Camera Transform: Drag your `Lock-On-Camera` or `MovementDefaultCamera`
   - Input Manager: Should auto-assign from PlayerCharacter

2. **Add TargetedPlayerRotation**
   - Z Targeting: Should auto-assign (the component you just added)
   - Camera Transform: Drag your camera
   - Input Manager: Should auto-assign
   - Free Rotation Speed: `12`
   - Locked Rotation Speed: `15`
   - Limit Rotation During Collision: `true`
   - Collision Rotation Multiplier: `0.4`

3. **Add ZTargetingAdapter**
   - Z Targeting: Should auto-assign
   - Player Combat: Should auto-assign
   - Letterbox UI: Drag your letterbox UI from the scene (if you have one)
   - Update Combat Target: `true`
   - Control Letterbox: `true`

### Step 3: Setup the Lock-On Camera

#### Option A: Using New ZTargetingCamera (Recommended)

1. Select your `Lock-On-Camera` GameObject
2. **DISABLE** the old `LockOnCamera` component
3. **Add** the `ZTargetingCamera` component:
   - Player Target: Drag `PlayerCharacter`
   - Z Targeting: Drag `PlayerCharacter` (it will find the component)
   - Player Offset: `(0, 1.5, 0)`
   - Shoulder Offset: `(0.6, 0.3, 0)`
   - Default Distance: `5`
   - Locked Distance: `6`
   - Target Vertical Offset: `1`
   - Adjust Distance By Target Distance: `true`
   - Input Manager: Drag `PlayerCharacter` InputManager
   - Collision Handler: Should already be on the camera
   - Enable Collision Handling: `true`

#### Option B: Modify Existing LockOnCamera

If you want to keep using your existing `LockOnCamera`, you'll need to update its references:

1. Keep `LockOnCamera` enabled
2. Change `Lock On System` field from old `LockOnSystem` to reference your new `ZTargetingAdapter`
3. You may need to modify the script slightly (see below)

### Step 4: Update PlayerLocomotion (Optional)

Your `PlayerLocomotion` currently handles rotation. You have two options:

#### Option A: Use TargetedPlayerRotation (Recommended)

1. In `PlayerLocomotion.HandleRotation()`, at the very start, add:
```csharp
private void HandleRotation()
{
    // Let TargetedPlayerRotation handle rotation when locked
    TargetedPlayerRotation targetedRotation = GetComponent<TargetedPlayerRotation>();
    if (targetedRotation != null)
    {
        // TargetedPlayerRotation handles all rotation in LateUpdate
        // Only handle special cases here (aiming, etc) if NOT locked
        ZTargetingSystem zTarget = GetComponent<ZTargetingSystem>();
        if (zTarget != null && zTarget.IsLocked)
        {
            return; // Let TargetedPlayerRotation handle it
        }
    }
    
    // Rest of your existing rotation code for free movement and aiming...
}
```

#### Option B: Integrate into PlayerLocomotion

Replace the lock-on rotation section in `HandleRotation()`:

```csharp
// Replace this block:
// if (lockOnSystem != null && lockOnSystem.IsLocked() && lockOnSystem.currentLockTarget != null)

// With this:
ZTargetingSystem zTargeting = GetComponent<ZTargetingSystem>();
if (zTargeting != null && zTargeting.IsLocked && zTargeting.CurrentTarget != null)
{
    targetDirection = zTargeting.CurrentTarget.position - transform.position;
    targetDirection.y = 0;
    
    if (targetDirection.magnitude < 1f)
    {
        targetDirection = transform.forward;
    }
    else
    {
        targetDirection.Normalize();
    }
    
    // Camera collision handling
    if (cameraInCollision)
    {
        currentRotationSpeed = rotationSpeed * 0.5f;
        
        float angleToTarget = Vector3.Angle(transform.forward, targetDirection);
        if (angleToTarget > maxPlayerRotationDuringCollision)
        {
            targetDirection = Vector3.RotateTowards(transform.forward, targetDirection,
                maxPlayerRotationDuringCollision * Mathf.Deg2Rad, 0f);
        }
    }
    else
    {
        currentRotationSpeed = rotationSpeed * 2f;
    }
    
    if (targetDirection != Vector3.zero)
    {
        Quaternion lockOnRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lockOnRotation, currentRotationSpeed * Time.deltaTime);
    }
    return;
}
```

### Step 5: Test the System

1. Enter Play Mode
2. Press your lock-on button (should be bound to `EnemyLockOn` action)
3. Verify:
   - Player locks onto nearest enemy in front
   - Player rotates to face the enemy
   - Camera frames both player and enemy
   - Letterbox appears when locked
   - Target switches to new enemy when current one dies
   - Lock releases when enemy is too far or out of range

### Step 6: Fine-Tuning

#### Targeting Feel
- Increase `Max Angle From Camera` for easier targeting
- Decrease for more precise targeting
- Adjust `Lock On Range` to control how far you can lock

#### Rotation Feel
- Increase `Locked Rotation Speed` for snappier rotation
- Increase `Collision Rotation Multiplier` if rotation feels too slow during collision
- Adjust `Max Rotation Angle Per Frame` to prevent rotation issues

#### Camera Feel
- Adjust `Locked Distance` for how far the camera pulls back
- Modify `Target Framing Offset` to offset the target horizontally
- Enable `Adjust Distance By Target Distance` for dynamic camera distance
- Tweak `Distance Adjustment Factor` to control how much distance changes

## Comparison with Old System

### Old System Issues:
- Camera collision caused player rotation to go "out of wack"
- Complex interdependencies between LockOnSystem and LockOnCamera
- Rotation handled in multiple places

### New System Benefits:
- Clean separation of concerns (targeting, rotation, camera)
- Robust camera collision handling
- Smooth transitions between locked and free modes
- Event-driven architecture (easy to extend)
- Better target validation and retargeting

## Advanced Features

### Target Switching

To enable manual target switching (like Zelda), you can call:
```csharp
zTargeting.SwitchToNextTarget(new Vector2(1, 0)); // Switch right
zTargeting.SwitchToNextTarget(new Vector2(-1, 0)); // Switch left
```

You'll need to bind this to right stick input in your InputManager.

### Custom Events

The ZTargetingSystem exposes events you can subscribe to:

```csharp
zTargeting.OnTargetChanged += (newTarget, oldTarget) => {
    Debug.Log($"Target changed from {oldTarget?.name} to {newTarget?.name}");
};

zTargeting.OnLockStateChanged += (isLocked) => {
    Debug.Log($"Lock state: {isLocked}");
};
```

### Integration with Other Systems

The `ZTargetingAdapter` automatically updates `PlayerCombat.currentTarget`. For other systems, you can:

```csharp
ZTargetingSystem zTarget = player.GetComponent<ZTargetingSystem>();
if (zTarget.IsLocked)
{
    Transform target = zTarget.CurrentTarget;
    // Do something with target
}
```

## Troubleshooting

### Camera doesn't follow target smoothly
- Increase `Lock Transition Smoothing` on ZTargetingCamera
- Check that `Collision Handler` is assigned

### Player rotation feels jerky
- Increase `Locked Rotation Smoothing` on TargetedPlayerRotation
- Decrease `Locked Rotation Speed`

### Can't lock onto enemies
- Check `Target Layers` includes the Enemy layer
- Verify `Target Tag` matches your enemy tag
- Increase `Max Angle From Camera` or `Lock On Range`

### Lock breaks too easily
- Increase `Target Lost Distance`
- Decrease `Target Validation Interval`

### Rotation issues during camera collision
- Adjust `Collision Rotation Multiplier` (0.3-0.6 recommended)
- Check `Limit Rotation During Collision` is enabled
- Make sure LockOnCamera has `IsInCollision()` method

## Migration from Old System

Once you've confirmed the new system works:

1. Delete the old `LockOnSystem` component
2. Delete the old `LockOnCamera` script file (if you're using ZTargetingCamera)
3. Delete `LockOnCameraActivator` (no longer needed)
4. Clean up any references to the old system in your code

## Notes

- The new system uses property `CurrentTarget` instead of field `currentTarget`
- The new system uses method `IsLocked` (property) instead of `IsLocked()` method
- All rotation is handled automatically - you don't need to manually rotate player in PlayerCombat
- Camera collision is handled by the camera scripts, not the targeting system
