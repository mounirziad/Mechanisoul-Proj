# Attack Speed Modifier System with Joy Upgrade

## Overview
This system implements attack speed modification that works with your Animation Override Controller combo system. The Joy upgrade now affects the speed of all attack animations while preserving your existing combo flow.

## Key Components

### 1. AttackAnimationManager.cs
- **Purpose**: Manages animation speed for attacks only
- **Features**: 
  - Applies speed modifiers during attack states
  - Automatically resets speed when attacks complete
  - Works with individual AttackSO data
  - Supports min/max speed limits for balance

### 2. AttackSpeedUtility.cs
- **Purpose**: Static utility for centralized attack speed calculations
- **Features**:
  - Handles permanent upgrades (Joy) and temporary modifiers
  - Consistent calculation across all systems
  - Debug information generation
  - Extension methods for AttackSO

### 3. Enhanced AttackSO.cs
- **New Fields**:
  - `baseAnimationSpeed`: Per-attack speed multiplier
  - `affectedByAttackSpeedUpgrades`: Whether Joy upgrade affects this attack
  - `attackTypeTags`: For future categorization (Light/Heavy/Special)

### 4. Updated UpgradeHandler.cs
- **Joy Upgrade Progression**:
  - Level 1: +15% attack speed, 10% crit chance, 1.5x crit multiplier
  - Level 2: +25% attack speed, 20% crit chance, 1.75x crit multiplier  
  - Level 3: +40% attack speed, 30% crit chance, 2x crit multiplier
  - Level 4+: Scales further for future expansion

### 5. AttackSpeedDebugger.cs (Optional)
- **Purpose**: Testing and debugging tool
- **Features**: Live display of attack speed values, keyboard testing controls

## How It Works

1. **Joy Upgrade**: UpgradeHandler calculates speed bonus based on meleeJoyLvl
2. **Attack Execution**: PlayerCombat passes current AttackSO to AttackAnimationManager
3. **Speed Calculation**: AttackAnimationManager uses AttackSpeedUtility to calculate final speed
4. **Animation Application**: Animator.speed is modified during attack states only
5. **Auto Reset**: Speed returns to 1.0 when attack completes or is cancelled

## For Future Attacks

### Adding New Attacks
1. Create AttackSO asset in `/Assets/Scripts/Player/Combat/Attacks/`
2. Set up AnimatorOverrideController
3. Configure attack speed settings:
   - `baseAnimationSpeed`: 1.0 for normal, adjust as needed
   - `affectedByAttackSpeedUpgrades`: true for most attacks
   - `attackTypeTags`: Add relevant tags

### Attack Speed Settings Examples
```csharp
// Fast light attack
baseAnimationSpeed = 1.2f;
affectedByAttackSpeedUpgrades = true;
attackTypeTags = ["Light", "Fast"];

// Heavy attack (less affected by speed)
baseAnimationSpeed = 0.8f;
affectedByAttackSpeedUpgrades = true;
attackTypeTags = ["Heavy", "Slow"];

// Special attack (immune to speed changes)
baseAnimationSpeed = 1.0f;
affectedByAttackSpeedUpgrades = false;
attackTypeTags = ["Special", "Ultimate"];
```

## Testing

1. **Add AttackSpeedDebugger** to PlayerCharacter
2. **Use keyboard controls**:
   - `J` key: Increase Joy upgrade level
   - `K` key: Decrease Joy upgrade level
   - `T` key: Add temporary speed buff
3. **Watch console/UI** for speed values during attacks

## Integration Notes

- **No changes needed** to existing AttackSO assets (backward compatible)
- **Automatic component creation** if AttackAnimationManager missing
- **Safe fallbacks** if any component is missing
- **Only affects attack animations** - movement/idle unaffected
- **Works with all future attacks** automatically

## Performance Considerations

- Minimal overhead: Only active during attacks
- Coroutines automatically cleaned up
- No permanent memory allocations
- Safe for frequent use

This system ensures that the Joy upgrade meaningfully affects combat feel while maintaining the integrity of your animation override controller system and scaling to support all future attacks in your game.