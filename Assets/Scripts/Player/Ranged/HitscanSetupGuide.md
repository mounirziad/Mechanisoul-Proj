# Hitscan Shooting System Setup Guide

## Overview
Your player now supports both **Projectile** and **Hitscan** shooting modes. You can switch between them using the `Shooting Mode` dropdown in the PlayerCombat component.

## What Changed

### PlayerCombat.cs
- Added `ShootingMode` enum (Projectile or Hitscan)
- Reorganized ranged settings into sections
- Added new hitscan-specific fields
- Created `ShootHitscan()` method for instant hit detection
- Refactored `HandleShoot()` to support both modes

### New Scripts
- **HitscanTracer.cs**: Optional visual tracer line effect

## Hitscan vs Projectile

### Projectile Mode
- Spawns a physical projectile GameObject
- Has travel time
- Can be dodged
- Better for slower weapons (rockets, grenades, magic)

### Hitscan Mode
- Instant hit detection using raycasts
- No travel time
- Cannot be dodged
- Better for guns, lasers, fast weapons

## Setup Instructions

### 1. Configure PlayerCombat Component

Select your Player GameObject and locate the PlayerCombat component:

#### Shooting Mode
- Set `Shooting Mode` to **Hitscan**

#### Shared Ranged Settings
- `Shoot Point`: Transform where shots originate (already configured)
- `Weapon Damage`: Base damage per shot (default: 10)
- `Base Fire Rate`: Shots per second (default: 2)
- `Ranged Enabled`: Toggle shooting on/off

#### Hitscan Settings (New)
- `Hitscan Range`: Maximum shooting distance (default: 100)
- `Hitscan Layer Mask`: What layers the raycast can hit (default: Everything)
- `Impact Effect Prefab`: Visual effect when hitting surfaces
- `Muzzle Flash Prefab`: Visual effect at shoot point when firing
- `Tracer Line Prefab`: LineRenderer prefab for bullet trail
- `Tracer Duration`: How long the tracer line stays visible (default: 0.1s)

### 2. Create Visual Effects (Optional but Recommended)

#### Muzzle Flash
1. Create a new Particle System
2. Configure it for a quick flash effect
3. Save as a prefab in `/Assets/Prefabs`
4. Assign to `Muzzle Flash Prefab` field

#### Impact Effect
1. Create a Particle System for bullet impacts
2. Add sparks, dust, or blood effects
3. Save as a prefab
4. Assign to `Impact Effect Prefab` field

#### Tracer Line
1. Create an empty GameObject
2. Add LineRenderer component
3. Configure LineRenderer:
   - Width: 0.05 - 0.1
   - Material: Bright/emissive material
   - Color: Yellow/white with gradient
   - Position Count: 2
4. Add the HitscanTracer script component
5. Save as a prefab
6. Assign to `Tracer Line Prefab` field

### 3. Configure Layer Mask

To prevent shooting yourself:
1. In PlayerCombat, find `Hitscan Layer Mask`
2. Uncheck the "Player" layer
3. Make sure "Enemy" layer is checked

### 4. Test Your Hitscan Weapon

1. Enter Play Mode
2. Aim at an enemy (Right Mouse Button / Left Trigger)
3. Shoot (Left Mouse Button / Right Trigger)
4. Verify instant hit detection
5. Check Console for any warnings

## Switching Between Modes

You can easily switch between Projectile and Hitscan:
1. Select Player GameObject
2. Find PlayerCombat component
3. Change `Shooting Mode` dropdown
4. Configure mode-specific settings as needed

## Synergies & Upgrades

Both modes support your existing upgrade system:

### Joy Upgrades
- Fire rate multiplier (both modes)
- Damage multiplier (both modes)
- Explosion on hit VFX (both modes)

### Anger Upgrades
- AoE/DoT explosion on impact (both modes)
- Damage over time zones (both modes)

## Advanced: Customizing Hitscan Behavior

### Penetration
To add bullet penetration, modify the `ShootHitscan()` method:

```csharp
RaycastHit[] hits = Physics.RaycastAll(shootOrigin, shootDirection, hitscanRange, hitscanLayerMask);
foreach (var hit in hits)
{
    // Process each hit
}
```

### Spread Pattern
Add random spread for less accurate weapons:

```csharp
Vector3 spread = new Vector3(
    Random.Range(-spreadAmount, spreadAmount),
    Random.Range(-spreadAmount, spreadAmount),
    0
);
Vector3 shootDirection = (GetAccurateAimDirection() + spread).normalized;
```

### Damage Falloff
Reduce damage at longer ranges:

```csharp
float distance = Vector3.Distance(shootOrigin, hit.point);
float falloffMultiplier = Mathf.Clamp01(1f - (distance / hitscanRange));
float finalDamage = weaponDamage * falloffMultiplier * rangedMods.joyDamageMultiplier;
```

## Troubleshooting

### Hitscan not hitting enemies
- Check `Hitscan Layer Mask` includes "Enemy" layer
- Verify enemies have colliders
- Ensure `Hitscan Range` is high enough

### Tracer line not appearing
- Verify `Tracer Line Prefab` is assigned
- Check LineRenderer material is visible
- Increase `Tracer Duration` for testing

### VFX not spawning
- Ensure `Anger Explosion Prefab` and `Joy Explosion Prefab` are assigned
- Check upgrade system has enabled these features
- Verify VFX prefabs are valid

### Performance issues
- Reduce particle count in VFX prefabs
- Lower `Tracer Duration`
- Use object pooling for frequently spawned effects

## Performance Considerations

Hitscan is generally more performant than projectiles because:
- No physics simulation for projectiles
- No GameObject instantiation every shot (except VFX)
- Instant collision detection
- No need to track multiple projectiles

However, complex visual effects can impact performance. Use object pooling for:
- Tracer lines
- Muzzle flashes
- Impact effects

## Next Steps

Consider implementing:
1. Different weapon types with different hitscan properties
2. Weapon switching system
3. Recoil and accuracy mechanics
4. Headshot detection (check hit.collider for head bones)
5. Critical hit system with Joy synergy
