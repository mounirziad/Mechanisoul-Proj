# 🎮 MOVEMENT FIX - QUICK START

## The Problem
Player movement directions flip when lock-on camera hits walls.

## The Solution
**Stable Movement Reference** - freezes movement direction during camera collision.

---

## 🚀 Setup (2 Minutes)

### 1. Add Component to Player
1. Select `PlayerCharacter` in Hierarchy
2. Click `Add Component`
3. Type: `Stable Movement Reference`
4. Add it

### 2. Configure Component
In the `Stable Movement Reference` component:

| Setting | Value | Why |
|---------|-------|-----|
| Lock On Camera | Drag `Lock-On-Camera` | Needs to know when camera collides |
| Player Transform | Auto-fills | Optional reference |
| Smooth Speed | `15` | Good balance |
| Min Update Interval | `0.05` | Prevents jitter |
| **Freeze During Collision** | ✅ **TRUE** | **This fixes the issue!** |
| Show Debug Gizmos | ✅ TRUE (for testing) | See it working in Scene view |

### 3. Verify PlayerLocomotion
Select `PlayerCharacter` → Find `Player Locomotion` component:
- `Use Stable Movement Reference` = ✅ **Checked** (should already be)

---

## ✅ Test It

### Enter Play Mode
1. Lock onto enemy (Tab)
2. **Watch Scene view** (important!)
3. Back into wall
4. Move player

### What You Should See

**In Scene View:**
- 🟢 **Green arrow** = Stable forward (FREEZES during collision)
- 🔴 **Red arrow** = Stable right (FREEZES during collision)  
- 🟡 **Yellow arrow** = Camera forward (moves wildly during collision)

**In Game:**
- ✅ Movement stays correct
- ✅ Forward stays forward
- ✅ Left stays left
- ✅ **NO FLIPPING!**

---

## 🔧 If It Doesn't Work

### Check These:
1. ✅ `StableMovementReference` component on `PlayerCharacter`?
2. ✅ `Freeze During Collision` = true?
3. ✅ `Lock On Camera` reference is set?
4. ✅ `PlayerLocomotion.Use Stable Movement Reference` = true?
5. ✅ No errors in Console?

### Still Not Working?
- Enable `Show Debug Gizmos`
- Watch Scene view when hitting wall
- Green arrow should FREEZE when yellow arrow appears
- If green arrow still moves → component not working

---

## 🎯 How It Works (Simple)

**Before:**
```
Camera hits wall → Camera rotates → 
Movement uses camera rotation → FLIP! ❌
```

**After:**
```
Camera hits wall → Movement reference FREEZES →
Movement ignores camera rotation → STABLE! ✅
```

**The Magic:**
- Camera can rotate freely (you still look around normally)
- Movement uses a FROZEN reference during collision
- **Camera visual ≠ Movement direction** (decoupled!)

---

## 📊 Settings Guide

### Freeze During Collision
- **TRUE** = Movement freezes during collision (100% stable) ✅  
- **FALSE** = Still follows camera (can flip) ❌
- **Use:** Always TRUE!

### Smooth Speed
- **Higher (20)** = Follows camera faster when not in collision
- **Lower (10)** = More sluggish, but very stable
- **Default:** 15 (good balance)

### Show Debug Gizmos
- **TRUE** = See arrows in Scene view (great for debugging)
- **FALSE** = No visual debug
- **Use:** TRUE for testing, FALSE for release

---

## 📝 Summary

1. ✅ Add `StableMovementReference` to `PlayerCharacter`
2. ✅ Set `Freeze During Collision` = true
3. ✅ Set `Lock On Camera` reference
4. ✅ Test with `Show Debug Gizmos` = true
5. ✅ **Movement should NEVER flip now!**

---

## 🎉 Done!

Your lock-on camera movement should now be 100% stable, even when colliding with walls!

**Files Changed:**
- `/Assets/Scripts/StableMovementReference.cs` (NEW)
- `/Assets/Scripts/Player/Combat/PlayerLocomotion.cs` (UPDATED)

**Documentation:**
- `/Assets/Scripts/StableMovementReferenceSetup.md` (Full details)
- `/Assets/Scripts/MOVEMENT_FIX_QUICK_START.md` (This file)
