# FreeLook Camera Setup Guide

## Quick Setup Instructions

### 1. Setup Main Camera

1. Select your **Main Camera** in the scene
2. Remove the `CinemachineBrain` component (if you want to completely replace Cinemachine)
3. Add the following components:
   - `FreeLookCamera`
   - `CameraCollisionHandler`
   - `CameraInputHandler` (optional, if using PlayerInput)
   - `PlayerInput` (if not already present)

### 2. Configure FreeLookCamera

- **Target**: Drag your `PlayerCharacter` GameObject here
- **Target Offset**: Set to `(0, 1.5, 0)` to aim at the player's upper body/head
- **Orbit Sensitivity**: Start with `2.0` (adjust to preference)
- **Min/Max Vertical Angle**: Default `-20` to `80` works well
- **Default Distance**: `5.0` is a good starting point
- **Position/Rotation Smoothing**: `10.0` for responsive feel

### 3. Configure CameraCollisionHandler

- **Collision Layers**: Set to everything EXCEPT `Player` layer (uncheck Player)
- **Camera Radius**: `0.2` for normal spaces
- **Minimum Distance From Target**: `0.5` to prevent clipping into player
- **Use Adaptive Radius**: ✓ Enabled (helps in tight spaces)
- **Use Multiple Raycasts**: ✓ Enabled (better collision detection)

### 4. Setup Input Actions

You need to add these actions to your Input Actions asset:

```
Action Name: Look
  - Action Type: Value
  - Control Type: Vector2
  - Bindings:
    * Mouse: Delta [Mouse]
    * Gamepad: Right Stick [Gamepad]

Action Name: Zoom
  - Action Type: Value
  - Control Type: Axis
  - Bindings:
    * Mouse: Scroll/Y [Mouse]
    * Gamepad: Right Trigger - Left Trigger [Gamepad]
```

### 5. Layer Setup

Make sure your player is on the `Player` layer and exclude it from the collision detection to prevent the camera from colliding with the player itself.

## Features

### Standard Features
- Smooth orbit camera with horizontal and vertical rotation
- Configurable zoom with min/max distance
- Smooth position and rotation interpolation
- Target offset for precise aim point

### Advanced Collision Handling
- **Multiple Raycast System**: Uses several raycasts around the camera to detect obstacles
- **Adaptive Radius**: Automatically reduces camera collision radius in tight spaces
- **Sphere Cast Resolution**: Multiple sphere casts at different radii for accuracy
- **Collision Padding**: Prevents camera from getting too close to walls
- **Smooth Recovery**: Camera smoothly returns to desired position when space is available
- **Tight Space Detection**: Automatically detects when camera is in a confined area

### Why This Works Better Than Cinemachine Deoccluder

1. **Multiple Detection Methods**: Combines raycasts, sphere casts, and tight space detection
2. **Adaptive Behavior**: Changes collision radius based on environment
3. **No Jitter**: Smooth interpolation prevents camera shake in corners
4. **Predictable**: Clear, controllable behavior you can customize
5. **Performance**: Optimized for tight spaces without expensive operations

## Customization Tips

### For Tighter Spaces
- Reduce `Camera Radius` to `0.15` or lower
- Enable `Use Adaptive Radius`
- Increase `Raycast Count` to `7` or `9`
- Reduce `Min Adaptive Radius` to `0.03`

### For Faster Recovery
- Increase `Recovery Speed` to `8` or `10`
- Reduce `Position Smoothing` to `15`

### For Different Camera Feels
- **Action Game**: Lower smoothing (5-7), higher sensitivity (3-4)
- **Adventure Game**: Higher smoothing (10-15), medium sensitivity (2-3)
- **Slow/Strategic**: Very high smoothing (15-20), low sensitivity (1-2)

## Troubleshooting

**Camera clips through player:**
- Ensure player is on `Player` layer
- Make sure `Player` layer is excluded from `Collision Layers` in CameraCollisionHandler

**Camera too sensitive/not sensitive enough:**
- Adjust `Orbit Sensitivity` in FreeLookCamera
- For gamepad, adjust `Gamepad Sensitivity` in CameraInputHandler

**Camera jitters in corners:**
- Increase `Collision Padding`
- Increase `Recovery Speed`
- Enable `Use Adaptive Radius`

**Camera doesn't detect some obstacles:**
- Increase `Raycast Count`
- Increase `Sphere Cast Resolution`
- Check that obstacles have colliders and are on correct layers
