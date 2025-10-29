CUTSCENE SCENE TRANSITION GUIDE
================================

The cutscene system now supports automatic scene transitions when cutscenes complete.

HOW IT WORKS:
-------------

1. IMMEDIATE TRANSITION (Default Behavior)
   - When you press SPACE on the LAST shot's dialogue, the scene loads IMMEDIATELY
   - NO camera restoration, NO going back to default camera
   - The current cutscene camera stays active until the new scene loads
   - This is controlled by "Transition On Last Dialogue Input" (enabled by default)
   - Perfect for seamless cutscene-to-gameplay transitions

2. DELAYED TRANSITION (Alternative)
   - Disable "Transition On Last Dialogue Input" to use this mode
   - Cutscene completes normally, cameras restore, then scene loads after delay
   - Use this if you want to show something after the cutscene ends

SETUP:
------

On CutsceneManager:
1. Set "Scene To Load On Complete" to your target scene name (e.g., "GameplayScene")
2. Choose transition mode:
   - Enable "Transition On Last Dialogue Input" = press space on last dialogue to load scene
   - Disable it = cutscene ends normally, then loads scene after delay
3. Adjust "Delay Before Scene Transition" (0.5s default, can be 0 for instant)

On CutsceneExample:
1. Set "Scene To Load After Cutscene" to your target scene name
2. This automatically sets the scene on the CutsceneManager when you start the cutscene

IMPORTANT:
----------
- Scenes must be added to Build Settings (File > Build Settings)
- Scene names are case-sensitive
- If no scene name is set, cutscene will just end normally
- With immediate mode, the delay is very brief (just enough to feel smooth)

FLOW EXAMPLES:
--------------

Example 1: Opening Cutscene → Main Menu (IMMEDIATE)
- Scene To Load On Complete: "MainMenu"
- Transition On Last Dialogue Input: true
- Delay: 0.5

Flow: Shot 1 → [Space] → Shot 2 → [Space] → (last shot dialogue) → [Space] → 0.5s pause → MainMenu loads
      ^^^ Camera stays on Shot 2 until scene loads ^^^

Example 2: Level Intro → Gameplay (INSTANT)
- Scene To Load On Complete: "Level1"
- Transition On Last Dialogue Input: true
- Delay: 0

Flow: Shot 1 → [Space] → (last shot) → [Space] → Level1 loads instantly

Example 3: Ending Cutscene → Credits (DELAYED)
- Scene To Load On Complete: "Credits"
- Transition On Last Dialogue Input: false
- Delay: 2.0

Flow: Shot 1 → [Space] → Shot 2 → [Space] → cutscene ends → camera restores → wait 2s → credits load
