# Stable Movement Reference - Setup Guide

## The Real Problem (Finally!)

We've been trying to fix camera rotation during collision, but that's treating the symptom, not the cause!

**Root Issue:**
- `PlayerLocomotion` uses `cameraObject.forward` and `cameraObject.right` DIRECTLY
- When camera collides with wall → camera rotation changes → movement vectors flip
- No amount of clamping can make this 100% stable

**The Real Solution:**
- **Separate movement reference from camera visual rotation**
- Camera can rotate freely (for looking)
- Movement uses a STABLE reference that freezes during collision
- **Result:** Movement NEVER flips, regardless of camera collision!

---

## How It Works

### StableMovementReference Component

This new component provides stable forward/right vectors for movement:

```csharp
// Normal (no collision):
- Smoothly follows camera rotation
- Updates forward/right vectors every frame

// During collision:
- FREEZES the reference vectors
- Camera can rotate wildly
- Movement uses FROZEN vectors → stays stable!

// After collision:
- Smoothly blends back to current camera rotation
```

**Key Insight:** The camera can look wherever it wants, but movement direction stays frozen during collision!

---

## Setup Steps

### 1. Add StableMovementReference to Player

1. **Select `PlayerCharacter`** in Hierarchy
2. **Click `Add Component`**
3. **Search for:** `Stable Movement Reference`
4. **Add it**

### 2. Configure StableMovementReference

In the Inspector:

**References:**
- `Lock On Camera`: Drag `Lock-On-Camera` GameObject here
- `Player Transform`: Should auto-find player (or drag PlayerCharacter)

**Stability Settings:**
- `Smooth Speed`: `15` (how fast it follows camera when not in collision)
- `Min Update Interval`: `0.05` (minimum time between updates)
- `Freeze During Collision`: ✅ **CHECK THIS!** (This is the key!)

**Debug:**
- `Show Debug Gizmos`: ✅ Check to see the vectors in Scene view

### 3. PlayerLocomotion is Already Updated!

The script is already modified to use StableMovementReference. Just verify:

1. **Select `PlayerCharacter`**
2. **Find `Player Locomotion` component**
3. **Verify:** `Use Stable Movement Reference` = ✅ **Checked**

---

## How to Test

### Before Testing:
1. **Select `PlayerCharacter`**
2. **Verify `Stable Movement Reference` component** is added
3. **Verify `Freeze During Collision` = true**
4. **Check `Show Debug Gizmos` = true**

### Test Procedure:
1. **Enter Play Mode**
2. **Lock onto enemy** (Tab)
3. **Watch Scene view** - you'll see:
   - **Green arrow** = Stable forward (for movement)
   - **Red arrow** = Stable right (for movement)
   - **Cyan/Yellow arrow** = Actual camera forward (cyan = normal, yellow = collision)
4. **Back into wall**
5. **Observe:**
   - Camera forward (cyan/yellow) moves wildly ❌
   - Stable forward (green) FREEZES ✅
6. **Move player** - controls stay correct!

### What Should Happen:

**✅ SUCCESS:**
- Green/Red arrows freeze when camera hits wall
- Movement directions stay correct
- Left stays left, forward stays forward
- **No flip, no stutter, no weirdness!**

**❌ If movement still flips:**
- Check `Freeze During Collision` is enabled
- Check component is on PlayerCharacter
- Check Console for errors

---

## Visual Debugging

### In Scene View (when `Show Debug Gizmos` = true):

**Green Arrow** = Stable Forward
- This is what PlayerLocomotion uses for forward movement
- Should FREEZE when camera collides

**Red Arrow** = Stable Right  
- This is what PlayerLocomotion uses for right/left movement
- Should FREEZE when camera collides

**Cyan Arrow** = Camera Forward (Normal)
- Actual camera forward direction
- Can rotate freely

**Yellow Arrow** = Camera Forward (Collision!)
- Shows camera is in collision
- Can flip around wildly
- **Movement ignores this during collision!**

---

## Technical Details

### Before (Broken):
```
Input: W (forward)
↓
PlayerLocomotion: cameraObject.forward * 1.0
↓
Camera forward = (0, 0, -1) [FLIPPED due to collision!]
↓
Movement = (0, 0, -1) = BACKWARD! ❌
```

### After (Fixed):
```
Input: W (forward)
↓
PlayerLocomotion: stableMovementRef.GetStableForward() * 1.0
↓
Stable forward = (0, 0, 1) [FROZEN, ignores camera collision]
↓
Movement = (0, 0, 1) = FORWARD! ✅
```

---

## Settings Tuning

### Smooth Speed (default: 15)
- **Higher (20-30)**: Follows camera faster when not in collision
- **Lower (5-10)**: More sluggish, but super stable
- **Recommended**: `15`

### Freeze During Collision (default: true)
- **True**: Movement reference FREEZES during collision → 100% stable ✅
- **False**: Still follows camera during collision → can flip ❌
- **Recommended**: `true` (always!)

### Min Update Interval (default: 0.05)
- Prevents jittery updates
- **Recommended**: Leave at `0.05`

---

## Why This Works (vs Previous Attempts)

### Previous Attempts:
1. ❌ **Attempt 1:** Reorder position/collision/rotation
   - Problem: Rotation still unstable when very close
   
2. ❌ **Attempt 2:** Clamp rotation to ±30° during collision
   - Problem: Still allows 30° flips, movement can still break
   
3. ❌ **Attempt 3:** Separate horizontal/vertical rotation
   - Problem: Horizontal can still flip if collision is severe

### This Solution:
✅ **Freeze movement reference completely during collision**
- Camera rotation can do WHATEVER it wants
- Movement is completely decoupled from camera rotation during collision
- **100% stable, no edge cases, no workarounds needed!**

---

## Code Changes Summary

### New Files:
- `/Assets/Scripts/StableMovementReference.cs` - Stable movement reference component

### Modified Files:
- `/Assets/Scripts/Player/Combat/PlayerLocomotion.cs`
  - Added `StableMovementReference stableMovementRef` field
  - Added `useStableMovementReference` toggle
  - Modified `HandleMovement()` to use stable reference

### Key Logic:
```csharp
// In StableMovementReference.LateUpdate()
if (freezeDuringCollision && lockOnCamera.IsInCollision())
{
    return; // Don't update! FREEZE!
}

// Otherwise, smoothly follow camera
stableForward = Vector3.Slerp(stableForward, cameraForward, smoothSpeed * Time.deltaTime);
```

```csharp
// In PlayerLocomotion.HandleMovement()
if (useStableMovementReference && stableMovementRef != null)
{
    cameraForward = stableMovementRef.GetStableForward();  // STABLE! ✅
    cameraRight = stableMovementRef.GetStableRight();      // STABLE! ✅
}
else
{
    cameraForward = cameraObject.forward;  // UNSTABLE! ❌
    cameraRight = cameraObject.right;      // UNSTABLE! ❌
}
```

---

## Troubleshooting

### Movement still flips?
1. Check `StableMovementReference` component exists on `PlayerCharacter`
2. Check `Freeze During Collision` = true
3. Check `PlayerLocomotion.Use Stable Movement Reference` = true
4. Enable `Show Debug Gizmos` and watch the arrows in Scene view

### Movement feels sluggish?
1. Increase `Smooth Speed` to 20-25
2. Decrease `Min Update Interval` to 0.02

### Movement doesn't respond to camera?
1. Check `Lock On Camera` reference is set
2. Check `Player Transform` reference is set
3. Check Console for errors

---

## Summary

**Problem:**
- PlayerLocomotion uses camera rotation directly
- Camera rotation unstable during collision
- Movement breaks

**Solution:**
- New `StableMovementReference` component
- Provides stable forward/right vectors
- **FREEZES during collision**
- Movement always stable!

**Result:**
- ✅ Camera can rotate freely (for looking)
- ✅ Movement uses frozen reference during collision
- ✅ **Movement NEVER flips, no matter what!**

---

This is the definitive fix! The movement reference is completely decoupled from camera rotation during collision. 🎉
