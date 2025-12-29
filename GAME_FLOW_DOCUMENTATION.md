# Game Flow & Backend System Documentation

## Overview
This document explains the complete game flow, heart system, and persistent backend for your Unity game.

---

## 🎯 Core Systems

### 1. **Heart System (3 Lives Backend)**

**Implementation**: `GameModeManager.cs` - `lives` variable

**Key Features**:
- ✅ Player starts with **3 hearts**
- ✅ **Persistent across ALL scenes** (DontDestroyOnLoad)
- ✅ Losing a minigame reduces hearts by 1
- ✅ Hearts displayed via `HeartUIHandler` component
- ✅ Game Over triggers when hearts reach 0

**Code Reference**:
```csharp
// In GameModeManager.cs
public int lives = 3;  // Persistent heart count

// When player fails a minigame
if (!success)
{
    lives--;
    heartUIHandler.UpdateHearts(lives);
}
```

---

### 2. **Persistence System**

**Implementation**: `GameModeManager.cs` - Singleton Pattern

**What Persists**:
- ❤️ **Hearts (lives)** - Maintains across all scenes
- 🎯 **Score** - Total accumulated points
- 📊 **Current Mode** (Easy/Medium/Hard/God)
- 🔢 **Minigames Completed** in current mode

**How It Works**:
```csharp
// Singleton ensures one instance across all scenes
private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);  // Never destroyed!
    }
}
```

---

### 3. **Game Loop Logic**

#### **Easy / Medium / Hard Modes**
- Each mode requires **exactly 6 minigames**
- After 6th game: `TempTransition` → Advance to next mode
- Counter resets to 0 when entering new mode

**Flow**:
```
Game 1-5: GameEnd → GameStart (same mode)
Game 6:   GameEnd → TempTransition → GameStart (next mode)
```

**Code Logic**:
```csharp
minigamesCompletedInMode++;

if (minigamesCompletedInMode >= gamesPerMode)  // 6 games
{
    if (currentMode != GameMode.God)
    {
        minigamesCompletedInMode = 0;  // Reset
        AdvanceDifficulty();           // Easy→Medium→Hard→God
        ShowModeTransition();          // Display transition
    }
}
```

#### **God Mode**
- ♾️ **No game limit** - loops infinitely
- Continues until hearts reach 0
- 2x score multiplier for successful games

**Flow**:
```
God Mode: GameEnd → GameStart (infinite loop until lives = 0)
```

---

## 📋 Complete Scene Flow

### **Normal Game Progression**
```
Landing Page
    ↓ (Press Start)
Opening Scene (animation)
    ↓
Mode Display Scene (shows "Easy")
    ↓
Game Start (intro animation)
    ↓
Minigame 1 (randomized)
    ↓
Game End (shows +score or failed)
    ↓
Game Start
    ↓
Minigame 2 (randomized)
    ↓
Game End
    ↓
... (repeat until 6 games in mode)
    ↓
TempTransition (shows "Medium" in yellow)
    ↓
Game Start
    ↓
Minigame 1 (Medium difficulty)
    ↓
... (6 more games)
    ↓
TempTransition (shows "Hard" in red)
    ↓
... (6 more games)
    ↓
TempTransition (shows "God" in white)
    ↓
... (infinite games until game over)
```

### **Game Over Flow (When Hearts = 0)**
```
Game End (shows failure)
    ↓
Score Scene (final total score) ← Only shown here!
    ↓
Closing Scene (game over animation + score display)
    ↓
Name Input (embedded in Closing Scene)
    ↓
Landing Page (game resets)
```

---

## 🎮 Key Implementation Details

### **When Minigame Ends**
Called by each minigame when player wins/loses:
```csharp
GameModeManager.Instance.ResolveMinigame(success: true/false, timeBonus: 50);
```

**What Happens**:
1. ❤️ If failed: `lives--` (heart system)
2. 🎯 Calculate score (mode-based + time bonus)
3. 📺 Load `GameEnd` scene
4. ⚖️ Check lives:
   - **Lives = 0**: Score Scene → Closing Scene → End
   - **Lives > 0**: Continue to next game

### **Mode Advancement**
Automatically triggered after 6 games:
```csharp
if (minigamesCompletedInMode >= 6)
{
    // Reset counter
    minigamesCompletedInMode = 0;
    
    // Advance difficulty
    currentMode = nextMode;  // Easy→Medium→Hard→God
    
    // Show colored transition
    ShowModeTransition(currentMode);
}
```

### **Score Scene Behavior**
⚠️ **Important**: Score Scene **ONLY** appears at final game over!

**Before Fix**: Score Scene appeared after every minigame
**After Fix**: Score Scene only appears when lives = 0

---

## 🔧 Components Explained

### **GameModeManager.cs**
- **Role**: Central backend controller
- **Manages**: Hearts, score, mode, game loop
- **Persistence**: Yes (DontDestroyOnLoad)

### **HeartUIHandler.cs**
- **Role**: Visual display of hearts
- **Updates**: Called by GameModeManager when lives change
- **Shows/Hides**: During gameplay vs. menus

### **MinigameRandomizer.cs**
- **Role**: Randomizes minigame order
- **Shuffles**: Prevents repetitive sequences
- **Persistence**: Yes (DontDestroyOnLoad)

### **TempTransitionController.cs**
- **Role**: Shows colored transition between modes
- **Colors**: Green (Easy), Yellow (Medium), Red (Hard), White (God)

---

## ✅ Solved Issues

### **Issue 1: Game Stops After One Minigame**
**Problem**: Game wasn't looping after first minigame
**Solution**: Fixed `PostGameSequence()` logic to properly continue to next game

### **Issue 2: Score Scene Appearing Everywhere**
**Problem**: Score Scene loaded after every game
**Solution**: Moved Score Scene to only appear when `lives <= 0`

### **Issue 3: Mode Not Advancing After 6 Games**
**Problem**: Counter wasn't resetting, mode didn't advance
**Solution**: Reset `minigamesCompletedInMode = 0` before advancing mode

---

## 🧪 Testing Checklist

- [ ] Hearts display correctly in all minigames
- [ ] Hearts persist when moving between scenes
- [ ] Losing a minigame reduces hearts by 1
- [ ] Game continues after winning/losing (hearts > 0)
- [ ] After 6 games in Easy: Transition to Medium
- [ ] After 6 games in Medium: Transition to Hard
- [ ] After 6 games in Hard: Transition to God
- [ ] God Mode continues infinitely until hearts = 0
- [ ] When hearts = 0: Score Scene → Closing Scene
- [ ] Score Scene ONLY appears at final game over
- [ ] Score persists across all scenes
- [ ] Final score shown in Closing Scene
- [ ] Name input works after Closing Scene
- [ ] Landing Page resets all values

---

## 📝 Code Architecture

```
GameModeManager (Persistent Singleton)
    ├── Heart System (lives variable)
    ├── Score System (score variable)
    ├── Mode Progression (currentMode, minigamesCompletedInMode)
    └── Scene Flow Control (PostGameSequence)

HeartUIHandler (Visual Display)
    └── Updates based on GameModeManager.lives

MinigameRandomizer (Persistent)
    └── Provides random minigame selection

TempTransitionController
    └── Shows mode transitions with colors
```

---

## 🎨 Mode Colors Reference

| Mode   | Color  | Games Required | Score Multiplier |
|--------|--------|----------------|------------------|
| Easy   | Green  | 6              | 1x               |
| Medium | Yellow | 6              | 1x               |
| Hard   | Red    | 6              | 1x               |
| God    | White  | ∞ (Unlimited)  | 2x               |

---

## 🚀 Quick Reference

**To end a minigame** (call from any minigame script):
```csharp
GameModeManager.Instance.ResolveMinigame(success: true, timeBonus: 50);
```

**To check current hearts**:
```csharp
int remainingLives = GameModeManager.Instance.lives;
```

**To get current mode**:
```csharp
GameModeManager.GameMode mode = GameModeManager.Instance.currentMode;
```

**To get total score**:
```csharp
int totalScore = GameModeManager.Instance.score;
```

---

**System Status**: ✅ Fully Implemented
**Last Updated**: December 29, 2025
