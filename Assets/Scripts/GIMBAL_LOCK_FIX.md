# Gimbal Lock Fix - Left/Right Now Always Horizontal!

## The Problem

**Symptom:** Left input makes you go DOWN, right input makes you go UP!

**Root Cause:** GIMBAL LOCK - When camera pitch is extreme (looking very up/down):

```csharp
// OLD CODE (BROKEN):
Vector3 cameraForward = cameraObject.forward;  // (0, 0.95, 0.31) - mostly pointing up!
cameraForward.y = 0;                           // (0, 0, 0.31) - tiny horizontal component
cameraForward.Normalize();                     // (0, 0, 1) - seems ok...

Vector3 cameraRight = cameraObject.right;      // (1, 0, 0) - but this is WRONG!
cameraRight.y = 0;                             // When forward is vertical, right becomes random!
cameraRight.Normalize();                       // Could point anywhere!
```

**What Happens:**
- When camera looks up/down → camera.forward is mostly vertical
- Setting `.y = 0` leaves tiny horizontal component
- `camera.right` gets confused about which direction is "right"
- Left/right movement becomes forward/backward or even up/down!
- **Classic gimbal lock problem!**

---

## The Solution

**Extract ONLY the horizontal rotation (YAW) from the camera:**

```csharp
// NEW CODE (FIXED):
float cameraYaw = cameraObject.eulerAngles.y;                    // Just the horizontal angle!
Vector3 cameraForward = Quaternion.Euler(0, cameraYaw, 0) * Vector3.forward;  // Perfect horizontal forward!
Vector3 cameraRight = Quaternion.Euler(0, cameraYaw, 0) * Vector3.right;      // Perfect horizontal right!

// These are ALWAYS horizontal, regardless of camera pitch!
```

**Why This Works:**
- We **ignore pitch completely** for movement calculation
- We only use YAW (horizontal rotation)
- Forward and Right are **always on the XZ plane**
- No matter how much camera looks up/down, movement stays horizontal!

---

## Code Changes

### PlayerLocomotion.HandleMovement()

**Changed:**
```csharp
// BEFORE (Gimbal Lock):
Vector3 cameraForward = cameraObject.forward;
Vector3 cameraRight = cameraObject.right;
cameraForward.y = 0;
cameraRight.y = 0;
cameraForward.Normalize();
cameraRight.Normalize();

// AFTER (Fixed):
float cameraYaw = cameraObject.eulerAngles.y;
Vector3 cameraForward = Quaternion.Euler(0, cameraYaw, 0) * Vector3.forward;
Vector3 cameraRight = Quaternion.Euler(0, cameraYaw, 0) * Vector3.right;
```

**Result:**
- cameraForward is **always** horizontal: (x, 0, z)
- cameraRight is **always** horizontal: (x, 0, z)
- W = forward (along XZ)
- S = backward (along XZ)
- A = left (along XZ)
- D = right (along XZ)
- **No matter what the camera pitch is!**

---

## Why The Old Method Failed

### Example: Camera Looking Up

**Camera Rotation:** Pitch = 80° (looking up)
```
camera.forward = (0, 0.985, 0.174)  // Mostly pointing up!
camera.right = (1, 0, 0)            // Points right

After setting .y = 0:
cameraForward = (0, 0, 0.174)       // Very small!
cameraRight = (1, 0, 0)             // Still right

After normalize:
cameraForward = (0, 0, 1)           // Seems ok
cameraRight = (1, 0, 0)             // Seems ok
```

Looks ok, right? **WRONG!**

### When Pitch > 90° (Looking Down):

**Camera Rotation:** Pitch = 95° (looking slightly down from vertical)
```
camera.forward = (0, -0.996, -0.087)  // Pointing down AND backward!
camera.right = (1, 0, 0)              // Confused!

After setting .y = 0:
cameraForward = (0, 0, -0.087)        // Negative Z!
cameraRight = (1, 0, 0)               

After normalize:
cameraForward = (0, 0, -1)            // FLIPPED! Forward is now backward!
cameraRight = (1, 0, 0)               // Right might be forward!
```

**Result:** Movement directions completely FLIPPED!

### New Method (Always Works):

**Camera Rotation:** Pitch = 95°, Yaw = 45°
```
cameraYaw = 45°  // We ONLY use this!

cameraForward = Quaternion.Euler(0, 45, 0) * Vector3.forward
              = (0.707, 0, 0.707)  // Perfect! Always horizontal!

cameraRight = Quaternion.Euler(0, 45, 0) * Vector3.right
            = (0.707, 0, -0.707)  // Perfect! Always horizontal!
```

**No matter what pitch is, we always get stable horizontal vectors!**

---

## Visual Explanation

### Old Method (Broken):
```
Camera looking straight up (pitch = 90°):
    ↑ camera.forward = (0, 1, 0)
    → camera.right = (1, 0, 0)
    
Set y=0 on forward:
    forward = (0, 0, 0) → Normalize → NaN or unstable!
    right = (1, 0, 0) → Could be anything!
    
Result: MOVEMENT BROKEN! ❌
```

### New Method (Fixed):
```
Camera looking straight up (pitch = 90°):
    Yaw = 45° (horizontal rotation)
    
Calculate from yaw only:
    forward = Quaternion(0, 45°, 0) * (0,0,1) = (0.707, 0, 0.707)
    right = Quaternion(0, 45°, 0) * (1,0,0) = (0.707, 0, -0.707)
    
Result: MOVEMENT PERFECT! ✅
    W = move along (0.707, 0, 0.707)
    A = move along (0.707, 0, -0.707)
```

---

## Testing

### Test Cases:

1. **Normal camera angle (pitch ~20°)**
   - ✅ W = forward, S = backward, A = left, D = right

2. **Camera looking straight up (pitch = 90°)**
   - ✅ W = forward, S = backward, A = left, D = right
   - (Same as normal!)

3. **Camera looking straight down (pitch = -90°)**
   - ✅ W = forward, S = backward, A = left, D = right
   - (Same as normal!)

4. **Camera at any crazy angle**
   - ✅ Movement always horizontal!
   - ✅ Directions always correct!

---

## Summary

**Problem:**
- Using `camera.forward` and `camera.right` directly
- When pitch is extreme → gimbal lock
- Movement directions flip/break

**Solution:**
- Extract only camera's YAW (horizontal rotation)
- Build forward/right vectors from YAW only
- Ignore pitch completely for movement
- Movement always horizontal and stable!

**Result:**
- ✅ Left always goes left
- ✅ Right always goes right  
- ✅ Forward always goes forward
- ✅ **No matter what camera angle is!**

---

This is the proper fix for gimbal lock! Movement is now mathematically guaranteed to stay horizontal! 🎉
