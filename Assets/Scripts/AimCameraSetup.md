# Aim Camera Setup Guide

## Overview

The `AimCamera` script provides an over-the-shoulder third-person aiming camera with robust collision handling, aim target detection, and smooth camera movement. It's designed to replace Cinemachine's Third Person Aim camera with a fully script-based approach.

## Quick Setup Instructions

### Option 1: Using CameraManager (Recommended)

This approach manages switching between FreeLook and Aim cameras automatically.

#### 1. Setup Camera Hierarchy

Create this structure in your scene:
```
Main Camera (Active)
  └─ Camera
  └─ AudioListener
  
FreeLookCameraVirtual (Separate GameObject)
  └─ FreeLookCamera
  └─ CameraCollisionHandler
  └─ CameraClippingFix (optional)
  
AimCameraVirtual (Separate GameObject)
  └─ AimCamera
  └─ CameraCollisionHandler  
  └─ CameraClippingFix (optional)
```

#### 2. Configure PlayerCharacter

Add `CameraManager` component to your PlayerCharacter:
- **Main Camera**: Drag your Main Camera here
- **FreeLook Camera**: Assign the FreeLookCamera script
- **Aim Camera**: Assign the AimCamera script
- **FreeLook Camera Object**: The GameObject with FreeLookCamera
- **Aim Camera Object**: The GameObject with AimCamera
- **Transition Speed**: `5-10` for smooth switching
- **Smooth Transition**: ✓ Checked

#### 3. Configure FreeLookCameraVirtual

**FreeLookCamera Component:**
- Target: PlayerCharacter
- Target Offset: `(0, 1.5, 0)`
- Player Input Manager: Auto-finds or assign manually
- Use Mouse Delta: ✓ Checked
- Mouse Sensitivity: `0.1`
- Default Distance: `5`
- Position Smoothing: `10-15`

**CameraCollisionHandler Component:**
- Collision Layers: Everything except Player
- Camera Radius: `0.15`
- Collision Padding: `0.2`
- Near Camera Extra Padding: `0.15`
- Use Multiple Raycasts: ✓ Checked

#### 4. Configure AimCameraVirtual

**AimCamera Component:**
- Target: PlayerCharacter
- Target Offset: `(0, 1.5, 0)`
- Player Input Manager: Auto-finds or assign manually
- Use Mouse Delta: ✓ Checked
- Mouse Sensitivity: `0.1`
- Shoulder Offset: `(0.6, 0, 0)` for right shoulder or `(-0.6, 0, 0)` for left
- Camera Distance: `3`
- Min Distance: `1`
- Aim Layers: Everything except Player
- Aim Distance: `100`
- Ignore Player Layer: ✓ Checked
- Position Smoothing: `15-20` (higher for snappier aim)
- Aim Target Smoothing: `15`

**CameraCollisionHandler Component:**
- Same settings as FreeLook camera
- You may want slightly more aggressive padding: `0.25`

---

### Option 2: Manual Switching (Advanced)

If you want to manage camera switching yourself:

1. Create the aim camera GameObject
2. Add `AimCamera` and `CameraCollisionHandler` components
3. Configure as described above
4. Enable/disable the camera GameObject or script when needed
5. Access aim target via `aimCamera.GetAimTarget()`

---

## Key Features

### Shoulder Offset
- **Positive X**: Camera over right shoulder
- **Negative X**: Camera over left shoulder
- **Y**: Vertical offset from target
- **Z**: Forward/backward offset

### Aim Target Detection
- Raycasts from player position in camera look direction
- Returns hit point if obstacle found
- Returns distant point if no hit
- Smoothly interpolates for stable aiming
- Automatically creates an AimTarget transform for reference

### Collision Handling
- Uses the same robust collision system as FreeLookCamera
- Pulls camera closer when obstacles detected
- Smoothly recovers when space available
- Adaptive radius in tight spaces

---

## API Reference

### Public Methods

```csharp
// Set the target to follow
void SetTarget(Transform newTarget)

// Get the current aim point in world space
Vector3 GetAimTarget()

// Get the camera's forward direction
Vector3 GetAimDirection()

// Get the aim target transform (useful for debug visualization)
Transform GetAimTargetTransform()
```

---

## Integration with Existing Code

### Replacing ThirdPersonAimCameraManager

If you're using `ThirdPersonAimCameraManager`, update your code:

**Old (Cinemachine):**
```csharp
ThirdPersonAimCameraManager aimManager = GetComponent<ThirdPersonAimCameraManager>();
Vector3 aimPoint = aimManager.GetAimTarget();
bool isAiming = aimManager.IsAimCameraActive();
```

**New (Script-based):**
```csharp
CameraManager cameraManager = GetComponent<CameraManager>();
Vector3 aimPoint = cameraManager.GetAimTarget();
bool isAiming = cameraManager.IsAimCameraActive();
Vector3 aimDirection = cameraManager.GetAimDirection();
```

### For PlayerCombat Scripts

Update any references to the old aim camera system:

```csharp
// Get aim direction for shooting
CameraManager camManager = GetComponent<CameraManager>();
if (camManager.IsAimCameraActive())
{
    Vector3 shootDirection = camManager.GetAimDirection();
    Vector3 targetPoint = camManager.GetAimTarget();
    
    // Use these for your shooting logic
}
```

---

## Configuration Tips

### For Tight Combat Spaces
- Reduce Camera Distance to `2.5`
- Increase Collision Padding to `0.25`
- Reduce Camera Radius to `0.1`
- Increase Position Smoothing to `20`

### For Open World/Long Range
- Increase Camera Distance to `4-5`
- Reduce Position Smoothing to `10`
- Increase Aim Distance to `200`

### For Fast-Paced Action
- Increase Position Smoothing to `25-30`
- Increase Rotation Smoothing to `20`
- Reduce Aim Target Smoothing to `10`

### For Tactical/Slow Gameplay
- Reduce Smoothing values to `8-10`
- Increase Aim Target Smoothing to `20`
- Consider adding slight shoulder sway (custom script)

---

## Troubleshooting

**Camera clips through walls:**
- Increase Collision Padding
- Add/configure CameraClippingFix component
- Reduce Camera Near Plane

**Aim target jumps around:**
- Increase Aim Target Smoothing
- Check Aim Layers mask (ensure it includes environment)

**Camera too slow to respond:**
- Increase Position Smoothing (higher = faster)
- Increase Rotation Smoothing

**Camera feels floaty:**
- Reduce smoothing values
- Consider removing smoothing entirely for instant response

**Wrong shoulder view:**
- Adjust Shoulder Offset X value (positive = right, negative = left)

**Aim point not detecting enemies:**
- Check Aim Layers includes enemy layer
- Verify enemies have colliders

---

## Advanced: Creating a Shoulder Swap Feature

Add this to your CameraManager or create a separate script:

```csharp
[Header("Shoulder Swap")]
[SerializeField] private KeyCode shoulderSwapKey = KeyCode.V;
private bool rightShoulder = true;

void Update()
{
    if (Input.GetKeyDown(shoulderSwapKey))
    {
        SwapShoulder();
    }
}

void SwapShoulder()
{
    rightShoulder = !rightShoulder;
    if (aimCamera != null)
    {
        Vector3 offset = aimCamera.shoulderOffset;
        offset.x = rightShoulder ? 0.6f : -0.6f;
        // You'll need to expose shoulderOffset as public or create a setter
    }
}
```

---

## Performance Notes

- Aim target raycast runs every frame (optimized with QueryTriggerInteraction.Ignore)
- Collision detection uses multiple raycasts (5-9 per frame)
- All calculations in LateUpdate for smooth camera movement
- Consider reducing raycast count in CameraCollisionHandler if targeting low-end devices

---

## Next Steps

1. Setup your cameras following the Quick Setup guide
2. Test in your scenes and adjust smoothing values
3. Configure shoulder offset to your preference
4. Update any existing combat/aiming scripts to use CameraManager
5. Add CameraClippingFix if experiencing near-plane clipping
6. Fine-tune collision settings for your game's spaces
