# Z-Targeting System - Component Layout Reference

## Visual Guide: Where Everything Goes

---

## PlayerCharacter GameObject

```
PlayerCharacter
│
├─ Transform
├─ Animator
├─ Rigidbody
├─ CapsuleCollider
│
├─ InputManager ✓ (existing)
├─ PlayerManager ✓ (existing)
├─ PlayerLocomotion ✓ (existing)
├─ PlayerCombat ✓ (existing)
├─ PlayerHealth ✓ (existing)
│
├─ ❌ LockOnSystem (DISABLE THIS - old)
│
├─ ✨ ZTargetingSystem (NEW - ADD THIS)
│   ├─ Lock On Range: 20
│   ├─ Max Angle From Camera: 70
│   ├─ Target Layers: Enemy
│   ├─ Target Tag: "Enemy"
│   ├─ Camera Transform → Lock-On-Camera
│   └─ Input Manager → auto-filled
│
├─ ✨ TargetedPlayerRotation (NEW - ADD THIS)
│   ├─ Z Targeting → auto-filled
│   ├─ Camera Transform → Lock-On-Camera
│   ├─ Input Manager → auto-filled
│   ├─ Free Rotation Speed: 12
│   ├─ Locked Rotation Speed: 15
│   └─ Collision Rotation Multiplier: 0.4
│
├─ ✨ ZTargetingAdapter (NEW - ADD THIS)
│   ├─ Z Targeting → auto-filled
│   ├─ Player Combat → auto-filled
│   ├─ Letterbox UI → LetterboxPrefab
│   ├─ Update Combat Target: ☑
│   └─ Control Letterbox: ☑
│
└─ ✨ ZTargetingDebugUI (OPTIONAL - for testing)
    ├─ Z Targeting → auto-filled
    ├─ Player Rotation → auto-filled
    └─ Show Debug UI: ☑
```

---

## Lock-On-Camera GameObject (Option A - New Camera)

```
Lock-On-Camera
│
├─ Transform
├─ Camera ✓ (existing)
├─ AudioListener ✓ (existing)
├─ UniversalAdditionalCameraData ✓ (existing)
│
├─ ❌ LockOnCamera (DISABLE THIS - old)
│
├─ ✨ ZTargetingCamera (NEW - ADD THIS)
│   ├─ Player Target → PlayerCharacter
│   ├─ Z Targeting → PlayerCharacter
│   ├─ Player Offset: (0, 1.5, 0)
│   ├─ Shoulder Offset: (0.6, 0.3, 0)
│   ├─ Default Distance: 5
│   ├─ Locked Distance: 6
│   ├─ Target Vertical Offset: 1
│   ├─ Target Framing Offset: 0.25
│   ├─ Adjust Distance By Target Distance: ☑
│   ├─ Input Manager → PlayerCharacter
│   ├─ Collision Handler → existing on camera
│   └─ Enable Collision Handling: ☑
│
├─ CameraCollisionHandler ✓ (keep existing)
└─ CameraClippingFix ✓ (keep existing)
```

---

## Lock-On-Camera GameObject (Option B - Keep Old Camera)

```
Lock-On-Camera
│
├─ Transform
├─ Camera ✓ (existing)
├─ AudioListener ✓ (existing)
├─ UniversalAdditionalCameraData ✓ (existing)
│
├─ ✓ LockOnCamera (KEEP ENABLED)
│   └─ Lock On System → PlayerCharacter (updated reference)
│
├─ ✨ LockOnCameraZTargetBridge (NEW - ADD THIS)
│   ├─ Lock On Camera → auto-filled
│   ├─ Z Targeting → PlayerCharacter
│   ├─ Lock Target Point → PlayerCharacter/LockTargetPoint
│   ├─ Target Height Offset: 1
│   └─ Follow Speed: 10
│
├─ CameraCollisionHandler ✓ (keep existing)
└─ CameraClippingFix ✓ (keep existing)
```

---

## Scene Hierarchy View

```
CameraTesting (Scene)
├─ MovementDefaultCamera
│  └─ (your free-look camera components)
│
├─ Lock-On-Camera ← Configure this
│  ├─ Camera
│  ├─ ✨ ZTargetingCamera (or bridge)
│  └─ CameraCollisionHandler
│
├─ PlayerCharacter ← Configure this
│  ├─ ✨ ZTargetingSystem
│  ├─ ✨ TargetedPlayerRotation
│  ├─ ✨ ZTargetingAdapter
│  ├─ PlayerLocomotion (existing)
│  ├─ PlayerCombat (existing)
│  └─ ... (other existing components)
│
├─ FinalEnemyPrefab ← Must have:
│  ├─ Tag: "Enemy"
│  ├─ Layer: Enemy
│  ├─ AiAgent or BasicEnemyHealth
│  └─ Collider
│
└─ LetterboxPrefab ← Optional UI
   └─ (letterbox bars)
```

---

## Inspector Quick Reference

### ZTargetingSystem Inspector

```
┌─────────────────────────────────────┐
│ ZTargetingSystem                    │
├─────────────────────────────────────┤
│ Target Detection                    │
│ • Lock On Range          [20      ] │
│ • Max Angle From Camera  [70      ] │
│ • Target Layers          [Enemy  ▼] │
│ • Target Tag             [Enemy   ] │
│                                     │
│ Target Validation                   │
│ • Target Lost Distance   [25      ] │
│ • Target Validation Int  [0.2     ] │
│ • Require Line Of Sight  [ ]        │
│ • Obstacle Layers        [      ▼] │
│                                     │
│ Target Switching                    │
│ • Allow Target Switching [✓]        │
│ • Switch Target Cooldown [0.3     ] │
│                                     │
│ References                          │
│ • Camera Transform       [Lock-On▼] │
│ • Input Manager          [Player ▼] │
│                                     │
│ Debug                               │
│ • Show Debug Info        [ ]        │
│ • Show Gizmos            [✓]        │
└─────────────────────────────────────┘
```

### TargetedPlayerRotation Inspector

```
┌─────────────────────────────────────┐
│ TargetedPlayerRotation              │
├─────────────────────────────────────┤
│ References                          │
│ • Z Targeting            [Player ▼] │
│ • Camera Transform       [Lock-On▼] │
│ • Input Manager          [Player ▼] │
│                                     │
│ Free Rotation Settings              │
│ • Free Rotation Speed    [12      ] │
│ • Free Rotation Smooth   [0.15    ] │
│                                     │
│ Locked Rotation Settings            │
│ • Locked Rotation Speed  [15      ] │
│ • Locked Rotation Smooth [0.1     ] │
│ • Min Distance For Rot   [0.5     ] │
│                                     │
│ Camera Collision Handling           │
│ • Limit Rotation         [✓]        │
│ • Collision Rotation Mul [0.4     ] │
│ • Max Rotation Angle     [120     ] │
│                                     │
│ Debug                               │
│ • Show Debug Info        [ ]        │
└─────────────────────────────────────┘
```

### ZTargetingAdapter Inspector

```
┌─────────────────────────────────────┐
│ ZTargetingAdapter                   │
├─────────────────────────────────────┤
│ References                          │
│ • Z Targeting            [Player ▼] │
│ • Player Combat          [Player ▼] │
│ • Letterbox UI           [Letter ▼] │
│                                     │
│ Settings                            │
│ • Update Combat Target   [✓]        │
│ • Control Letterbox      [✓]        │
└─────────────────────────────────────┘
```

### ZTargetingCamera Inspector

```
┌─────────────────────────────────────┐
│ ZTargetingCamera                    │
├─────────────────────────────────────┤
│ Target References                   │
│ • Player Target          [Player ▼] │
│ • Z Targeting            [Player ▼] │
│ • Player Offset          [0,1.5,0 ] │
│                                     │
│ Camera Position                     │
│ • Shoulder Offset        [0.6,0.3,0]│
│ • Default Distance       [5       ] │
│ • Min Distance           [2       ] │
│ • Max Distance           [10      ] │
│                                     │
│ Locked Mode Settings                │
│ • Locked Distance        [6       ] │
│ • Target Vertical Offset [1       ] │
│ • Target Framing Offset  [0.25    ] │
│ • Adjust Distance        [✓]        │
│ • Distance Adj Factor    [0.4     ] │
│                                     │
│ Rotation Settings                   │
│ • Free Rotation Speed    [8       ] │
│ • Locked Rotation Speed  [12      ] │
│ • Manual Rotation Sens   [2       ] │
│ • Allow Manual Rotation  [✓]        │
│                                     │
│ Smoothing                           │
│ • Position Smoothing     [10      ] │
│ • Rotation Smoothing     [8       ] │
│ • Lock Trans Smoothing   [6       ] │
│ • Distance Smoothing     [5       ] │
│                                     │
│ Input                               │
│ • Input Manager          [Player ▼] │
│ • Use Mouse Input        [✓]        │
│ • Mouse Sensitivity      [0.15    ] │
│                                     │
│ Collision                           │
│ • Collision Handler      [Lock-On▼] │
│ • Enable Collision Hand  [✓]        │
│                                     │
│ Debug                               │
│ • Show Debug Info        [ ]        │
│ • Show Gizmos            [✓]        │
└─────────────────────────────────────┘
```

---

## Color-Coded Setup Checklist

### ✅ KEEP (Don't Touch)
- PlayerLocomotion
- PlayerCombat  
- InputManager
- PlayerManager
- CameraCollisionHandler
- All other existing components

### ❌ DISABLE (Uncheck)
- Old LockOnSystem component
- Old LockOnCamera component (if using new one)

### ✨ ADD NEW
- ZTargetingSystem (on PlayerCharacter)
- TargetedPlayerRotation (on PlayerCharacter)
- ZTargetingAdapter (on PlayerCharacter)
- ZTargetingCamera (on Lock-On-Camera)

### 🔧 CONFIGURE
- Assign references (most auto-fill)
- Set targeting parameters
- Tune rotation speeds
- Adjust camera distances

### 🎮 TEST
- Lock onto enemy
- Move around
- Attack target
- Check camera
- Verify rotation

---

## Reference Connections Map

```
Flow of Data:

Input System
    ↓
InputManager (PlayerCharacter)
    ↓
ZTargetingSystem (PlayerCharacter)
    ↓ CurrentTarget
    ├→ TargetedPlayerRotation (PlayerCharacter)
    │      ↓ transform.rotation
    │  PlayerCharacter rotates
    │
    ├→ ZTargetingAdapter (PlayerCharacter)
    │      ↓ currentTarget
    │  PlayerCombat (PlayerCharacter)
    │
    └→ ZTargetingCamera (Lock-On-Camera)
           ↓ camera transform
       Lock-On-Camera frames target
```

---

## Scene Gizmos (When "Show Gizmos" enabled)

**In Scene View, you'll see:**

🟡 **Yellow Sphere** around player
   - Lock-on range visualization
   - Size = `Lock On Range`

🔴 **Red Sphere** around player  
   - Target lost distance
   - Size = `Target Lost Distance`

🟢 **Green Line** from player to target
   - Only when locked
   - Shows lock-on connection

🔵 **Blue Ray** from camera
   - Camera forward direction
   - Helps debug camera rotation

---

## Quick Setup Checklist

**Player Setup:**
- [ ] Add ZTargetingSystem
- [ ] Add TargetedPlayerRotation
- [ ] Add ZTargetingAdapter
- [ ] Disable old LockOnSystem
- [ ] Verify all references filled

**Camera Setup:**
- [ ] Add ZTargetingCamera (or bridge)
- [ ] Disable old LockOnCamera (if using new)
- [ ] Verify player reference
- [ ] Verify collision handler

**Enemy Setup:**
- [ ] Enemies tagged "Enemy"
- [ ] Enemies on Enemy layer
- [ ] Enemies have colliders
- [ ] Enemies have health component

**Input Setup:**
- [ ] EnemyLockOn action exists
- [ ] Action is bound to button
- [ ] InputManager on player

**Testing:**
- [ ] Press button to lock
- [ ] Player faces enemy
- [ ] Camera frames both
- [ ] No errors in console

---

## File Locations

```
Assets/
└─ Scripts/
   ├─ Player/
   │  └─ Combat/
   │     ├─ ZTargetingSystem.cs ← Core targeting
   │     ├─ TargetedPlayerRotation.cs ← Rotation
   │     └─ ZTargetingAdapter.cs ← Integration
   │
   ├─ ZTargetingCamera.cs ← New camera
   ├─ LockOnCameraZTargetBridge.cs ← Old camera bridge
   ├─ ZTargetingDebugUI.cs ← Debug overlay
   │
   └─ Documentation:
      ├─ ZTargeting_Summary.md
      ├─ ZTargeting_Setup_Guide.md
      ├─ ZTargeting_Quick_Reference.md
      ├─ ZTargeting_Step_By_Step.md
      └─ ZTargeting_Component_Layout.md ← You are here
```

---

## Inspector Tips

### Auto-Fill References
Most references auto-fill using `GetComponent<T>()`:
- Z Targeting → searches same GameObject
- Player Combat → searches same GameObject
- Input Manager → searches same GameObject

### Manual References
These need manual assignment:
- Camera Transform → Drag Lock-On-Camera
- Letterbox UI → Drag from Hierarchy
- Player Target → Drag PlayerCharacter

### Layer Setup
Make sure you have an "Enemy" layer:
1. Inspector → Layers → Edit Layers
2. Add "Enemy" to available layers
3. Set enemy GameObjects to Enemy layer

### Tag Setup
Make sure you have "Enemy" tag:
1. Inspector → Tags → Add Tag
2. Add "Enemy"
3. Set enemy GameObjects to Enemy tag

---

This component layout guide shows exactly where everything goes and how it's connected. Use this as a reference during setup!
