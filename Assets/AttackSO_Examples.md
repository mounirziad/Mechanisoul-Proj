# Attack System Setup Guide

## Creating Smooth Attack Data

With the updated AttackSO system, you can now create different types of attacks with unique movement properties:

### Light Attack Example
- **Move Distance**: 1.0f
- **Move Speed**: 6f  
- **Move Duration**: 0.25f
- **Rotate Towards Target**: true
- **Rotation Speed**: 900f

### Heavy Attack Example  
- **Move Distance**: 2.5f
- **Move Speed**: 12f
- **Move Duration**: 0.4f
- **Rotate Towards Target**: true
- **Rotation Speed**: 720f

### Combo Finisher Example
- **Move Distance**: 3.0f
- **Move Speed**: 15f
- **Move Duration**: 0.5f
- **Rotate Towards Target**: true  
- **Rotation Speed**: 1080f

## Animation Curve Tips

For the **Move Curve**, use these presets:
- **Fast Start, Slow End**: Creates a powerful lunge that slows down
- **Smooth**: Even movement throughout attack
- **Slow Start, Fast End**: Builds up momentum during attack

## Combat Feel Settings

In PlayerCombat:
- **Input Buffer Time**: 0.3f (responsive feel)
- **Early Combo Window**: 0.6f (60% through animation)  
- **Dodge Cancel Window**: 0.4f (40% through animation)

These settings create fluid, anime-style combat where attacks flow naturally into each other and can be canceled for defensive maneuvers.