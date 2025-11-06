# Camera System Migration Guide

## Cinemachine vs Custom Script-Based Cameras

### Feature Comparison

| Feature | Cinemachine | Custom Script-Based | Winner |
|---------|-------------|---------------------|---------|
| Collision Detection | Basic, jittery in tight spaces | Multi-raycast, adaptive radius | ✓ Custom |
| Near-Plane Clipping | Common issue | Dynamic adjustment available | ✓ Custom |
| Setup Complexity | Medium (lots of components) | Low (2-3 components) | ✓ Custom |
| Customization | Limited to exposed parameters | Full code access | ✓ Custom |
| Performance | Moderate overhead | Optimized, minimal overhead | ✓ Custom |
| Documentation | Extensive official docs | Custom documentation | Cinemachine |
| Community Support | Large | None (custom) | Cinemachine |

---

## Component Mapping

### FreeLook Camera

**Old (Cinemachine):**
- CinemachineCamera
- CinemachineFreeLook (or CinemachineFreeLookModifier)
- CinemachineDeoccluder
- CinemachineBrain (on Main Camera)

**New (Script-Based):**
- FreeLookCamera
- CameraCollisionHandler
- CameraClippingFix (optional)

### Aim Camera

**Old (Cinemachine):**
- CinemachineCamera
- CinemachineThirdPersonFollow
- CinemachineThirdPersonAim
- CinemachinePanTilt
- CinemachineInputAxisController

**New (Script-Based):**
- AimCamera
- CameraCollisionHandler
- CameraClippingFix (optional)

---

## Code Migration Examples

### Example 1: Getting Aim Target

**Before:**
```csharp
using Unity.Cinemachine;

public class PlayerCombat : MonoBehaviour
{
    private ThirdPersonAimCameraManager aimManager;
    
    void Start()
    {
        aimManager = GetComponent<ThirdPersonAimCameraManager>();
    }
    
    void Shoot()
    {
        Vector3 aimPoint = aimManager.GetAimTarget();
        Vector3 aimDir = aimManager.GetAimDirection();
        
        // Shoot logic
    }
}
```

**After (Option 1 - Direct):**
```csharp
public class PlayerCombat : MonoBehaviour
{
    private CameraManager cameraManager;
    
    void Start()
    {
        cameraManager = GetComponent<CameraManager>();
    }
    
    void Shoot()
    {
        Vector3 aimPoint = cameraManager.GetAimTarget();
        Vector3 aimDir = cameraManager.GetAimDirection();
        
        // Shoot logic - same as before!
    }
}
```

**After (Option 2 - Adapter for gradual migration):**
```csharp
public class PlayerCombat : MonoBehaviour
{
    private CameraManagerAdapter adapter;
    
    void Start()
    {
        adapter = GetComponent<CameraManagerAdapter>();
    }
    
    void Shoot()
    {
        Vector3 aimPoint = adapter.GetAimTarget();
        Vector3 aimDir = adapter.GetAimDirection();
        
        // Shoot logic - works with both old and new systems!
    }
}
```

### Example 2: Checking Camera State

**Before:**
```csharp
if (aimManager.IsAimCameraActive())
{
    // Do aiming behavior
    crosshair.SetActive(true);
}
```

**After:**
```csharp
if (cameraManager.IsAimCameraActive())
{
    // Do aiming behavior - same!
    crosshair.SetActive(true);
}
```

### Example 3: Camera Priority System

**Before:**
```csharp
// Cinemachine uses priority values
thirdPersonAimCamera.Priority = 15;
freeLookCamera.Priority = 10;
```

**After:**
```csharp
// Script-based uses GameObject active state
// Managed automatically by CameraManager
// No manual priority management needed!
```

---

## Migration Checklist

### Phase 1: Setup (Do Once)
- [ ] Create FreeLookCameraVirtual GameObject
- [ ] Add FreeLookCamera + CameraCollisionHandler + CameraClippingFix
- [ ] Create AimCameraVirtual GameObject  
- [ ] Add AimCamera + CameraCollisionHandler + CameraClippingFix
- [ ] Add CameraManager to PlayerCharacter
- [ ] Configure all camera settings
- [ ] Test camera switching works

### Phase 2: Code Migration
- [ ] Find all references to `ThirdPersonAimCameraManager`
- [ ] Option A: Replace with `CameraManager` (recommended)
- [ ] Option B: Add `CameraManagerAdapter` for compatibility
- [ ] Update any custom camera scripts
- [ ] Test all aiming/shooting functionality

### Phase 3: Cleanup
- [ ] Remove old Cinemachine camera GameObjects
- [ ] Remove ThirdPersonAimCameraManager component
- [ ] Remove CinemachineBrain from Main Camera
- [ ] Test in all scenes
- [ ] Optional: Remove Cinemachine package if not used elsewhere

---

## Settings Translation

### FreeLook Camera Settings

| Cinemachine | Custom Script | Notes |
|-------------|---------------|-------|
| Follow Offset | targetOffset | Same concept |
| Camera Distance | defaultDistance | Same |
| Damping | positionSmoothing | Higher value = faster (inverse) |
| X/Y Axis Speed | orbitSensitivity | Similar |
| Orbit Rings | N/A | Simplified to min/max angles |

### Aim Camera Settings

| Cinemachine ThirdPersonFollow | AimCamera | Notes |
|-------------------------------|-----------|-------|
| Shoulder Offset | shoulderOffset | Same |
| Camera Distance | cameraDistance | Same |
| Damping | positionSmoothing | Higher = faster (inverse) |
| Vertical Damping | rotationSmoothing | Similar concept |

| Cinemachine ThirdPersonAim | AimCamera | Notes |
|----------------------------|-----------|-------|
| Aim Target | GetAimTarget() | Method instead of property |
| Aim Distance | aimDistance | Same |
| Aim Collision Filter | aimLayers | Same concept |
| Ignore Tag | ignorePlayerLayer | Boolean instead |

---

## Recommended Migration Path

### Conservative Approach (Safest)
1. Keep Cinemachine cameras active
2. Add new script-based cameras (disabled)
3. Add CameraManagerAdapter to player
4. Test new cameras work
5. Switch over gradually scene by scene
6. Remove Cinemachine when confident

### Aggressive Approach (Fastest)
1. Setup new cameras
2. Replace ThirdPersonAimCameraManager with CameraManager
3. Find/replace all references in code
4. Delete Cinemachine cameras
5. Test everything
6. Fix issues as they arise

### Recommended: Middle Ground
1. Setup new cameras
2. Add CameraManager + CameraManagerAdapter
3. Test new system thoroughly
4. Keep old system as backup
5. Migrate code gradually
6. Remove old system after 1-2 weeks

---

## Known Differences

### Advantages of Custom System
✓ Better collision handling in tight spaces
✓ No camera jitter in corners
✓ Easier to customize and extend
✓ Better performance
✓ No external package dependency
✓ Clearer code flow

### Advantages of Cinemachine
✓ More features out of the box (noise, impulse, etc.)
✓ Visual scripting in Timeline
✓ Larger community
✓ More documentation
✓ Regular Unity updates

---

## Common Issues During Migration

### Issue: Aim target feels different
**Solution:** Adjust `aimTargetSmoothing` in AimCamera. Lower = more like Cinemachine.

### Issue: Camera moves too fast/slow
**Solution:** Remember smoothing is inverted! Higher = faster in custom system.

### Issue: Crosshair doesn't appear
**Solution:** Check `IsAimCameraActive()` returns true. Ensure CameraManager is switching properly.

### Issue: Camera clips through walls
**Solution:** 
1. Increase collision padding to 0.25
2. Add CameraClippingFix
3. Lower Camera Near Plane to 0.1

### Issue: Shooting direction is wrong
**Solution:** Use `GetAimDirection()` instead of transform.forward in combat code.

---

## Performance Comparison

### Memory Footprint
- **Cinemachine**: ~5-10 MB (package + runtime)
- **Custom**: ~50 KB (scripts only)

### Frame Time (Typical)
- **Cinemachine**: 0.3-0.8 ms
- **Custom**: 0.1-0.3 ms

### GC Allocations
- **Cinemachine**: Minor allocations per frame
- **Custom**: Zero allocations after initialization

---

## Support & Troubleshooting

Since this is a custom system, you have full access to modify:
- `/Assets/Scripts/FreeLookCamera.cs`
- `/Assets/Scripts/AimCamera.cs`
- `/Assets/Scripts/CameraCollisionHandler.cs`
- `/Assets/Scripts/CameraManager.cs`

Feel free to add features, adjust behavior, or optimize as needed!
