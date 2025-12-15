# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A 2D multiplayer tank game built with Unity and Photon Unity Networking (PUN). Features team-based gameplay with color-coded destruction mechanics. Tanks stay stationary while blocks move towards them - players must destroy matching-color blocks to survive.

**Unity Version:** 2021.3.45f2
**Rendering:** Universal Render Pipeline (URP) 12.1.15 with 2D Renderer
**Networking:** Photon PUN 2

### Player Limits
- **Teams:** 2 teams (Team A and Team B)
- **Players per team:** 5 max
- **Spectators:** 2 max
- **Total room capacity:** 12

## Project Structure

```
Assets/
├── Scripts/
│   ├── Box.cs                    # Destructible box behavior + tank collision penalty
│   ├── TankColor.cs              # Color enum (Green, Grey, Orange, Purple, Yellow)
│   ├── Block/
│   │   ├── BlockSpawner.cs       # Spawns blocks and finish line
│   │   ├── BlockMover.cs         # Moves blocks left
│   │   └── FinishLine.cs         # Finish line detection
│   ├── Game/
│   │   ├── GameController.cs     # Game flow control (F1 countdown start, F5 reload)
│   │   ├── GameSessionManager.cs # Game state, win/lose logic, CSV kayıt
│   │   ├── GameReadyPanel.cs     # Team name input (max 17 chars) and ready UI
│   │   └── GameEndUI.cs          # End game UI + Leaderboard (RPC synced)
│   ├── Input/
│   │   └── MobileInputManager.cs # Mobile joystick and fire button
│   ├── Networking/
│   │   ├── NetworkManager.cs     # Photon connection and room management
│   │   ├── LobbyManager.cs       # Team selection UI
│   │   ├── PlayerInfo.cs         # Photon Custom Properties helper
│   │   ├── TankGameManager.cs    # Player spawning based on team/role
│   │   └── TeamManager.cs        # Layer assignment, camera culling
│   ├── Player/
│   │   ├── TankController.cs     # Tank movement, shooting, damage flash, network interpolation
│   │   ├── TankBullet.cs         # Bullet behavior, stick mechanic, continuous collision
│   │   ├── CameraFollow.cs       # Smooth camera following
│   │   └── SpectatorController.cs # Spectator camera
│   ├── Score/
│   │   ├── ScoreManager.cs       # Team score management (AddScore, SubtractScore)
│   │   └── ScoreDisplay.cs       # 2D World Space score display with team names
│   └── UI/
│       ├── FloatingText.cs       # Object pooled floating +/- points display
│       └── FloatingTextManager.cs # RPC synced floating text spawner
├── Prefabs/
│   ├── Resources/                # Network-instantiated prefabs
│   │   ├── Tank_Green/Grey/Orange/Purple/Yellow.prefab
│   │   ├── Bullet.prefab
│   │   ├── SpectatorCamera.prefab
│   │   ├── FinishLine.prefab
│   │   ├── Block_*.prefab
│   │   └── FloatingText.prefab   # Floating score text
│   ├── Box_Green/Grey/Orange/Purple/Yellow.prefab
│   └── Blocks/
│       └── BoxGreen_1 through BoxGreen_8.prefab
├── Scenes/
│   ├── MenuScene.unity           # Connection and lobby
│   └── GameScene.unity           # Gameplay arena
└── kayitlar/                     # Game results CSV folder (auto-created)
    └── oyun_sonuclari.csv        # Game history for leaderboard
```

## Game Architecture

### Scene Flow
```
MenuScene → LobbyPanel (Team Selection) → GameScene → GameReadyPanel → F1 Countdown (10s) → Gameplay
```

1. **MenuScene**: Connect to Photon, auto-join/create room, team selection (4 buttons)
2. **GameScene**: Tank spawning, ready panel for team names, F1 starts 10-second countdown

### Game Flow
```
GameScene loads
    ↓
TankGameManager spawns tanks/spectators
    ↓
GameReadyPanel shows (players enter team names, max 17 chars)
    ↓
Master Client presses F1
    ↓
10-second countdown (synced via RPC)
    ↓
"BAŞLA!" shown, game starts
    ↓
BlockSpawner.StartSpawning() begins
    ↓
Blocks spawn and move left towards tanks
    ↓
Players shoot matching-color boxes (+10 points, bullet sticks 1s before destroy)
    ↓
Tank hits box = -10 points, red flash, box destroyed
    ↓
FinishLine spawns after all blocks
    ↓
FinishLine passes through all tanks = Team Finished
    ↓
GameSessionManager determines winner, saves to CSV
    ↓
GameEndUI shows result (5s), then Leaderboard (top 10 from CSV)
    ↓
Master Client presses F5 to reload (new game)
```

### Namespaces
- `TankGame` - Color enum, Box script
- `TankGame.Block` - BlockSpawner, BlockMover, FinishLine
- `TankGame.Game` - GameController, GameSessionManager, GameReadyPanel, GameEndUI
- `TankGame.Networking` - NetworkManager, TankGameManager
- `TankGame.Tank` - TankController, TankBullet, CameraFollow, SpectatorController
- `TankGame.Score` - ScoreManager, ScoreDisplay
- `TankGame.MobileInput` - MobileInputManager
- `TankGame.UI` - FloatingText, FloatingTextManager
- Root namespace - PlayerInfo, LobbyManager, TeamManager

## Game Controls

### Keyboard Controls
| Key | Function | Who |
|-----|----------|-----|
| F1 | Start Countdown (10s) | Master Client only |
| F5 | Reload Scene | Master Client only |
| WASD/Arrows | Tank movement | Players |
| Space | Shoot | Players |

### Mobile Controls
- **Joystick**: Tank movement (MobileInputManager)
- **Fire Button**: Shoot (PointerDown/PointerUp events)

## Game Mechanics

### Movement
- **Controls:** WASD, Arrow keys, or Mobile Joystick
- **Physics:** Rigidbody2D velocity-based movement
- **Network sync:** SmoothDamp + Velocity prediction for smooth remote player movement

### Network Interpolation (TankController)
- `smoothTime = 0.08f` - SmoothDamp time
- `teleportThreshold = 3f` - Teleport if too far
- `velocityPredictionFactor = 0.5f` - Predict movement with velocity
- Lag compensation in OnPhotonSerializeView

### Shooting
- **Control:** Space bar or Mobile Fire Button
- **Fire rate:** 0.5 seconds between shots
- **Bullet behavior:**
  - Travels +X direction, 3-second lifetime
  - **Stick mechanic:** Bullet sticks to matching box for 1s, then both destroy
  - Continuous collision detection for fast bullets
- **Test Mode:** When enabled, bullets destroy ALL color boxes

### Color-Based Destruction
- Tanks and boxes have colors: Green, Grey, Orange, Purple, Yellow
- **Matching colors:** Bullet sticks to box (1s), then both destroyed, +10 score
- **Non-matching colors:** Only bullet is destroyed
- **Tank collision with box:** Box destroyed, -10 score, tank red flash

### Visual Feedback
- **FloatingText:** +10 green (up), -10 red (down) - object pooled, RPC synced
- **Damage Flash:** Tank flashes red for 1 second when losing points

### Countdown System
- F1 triggers 10-second countdown (configurable)
- All clients see countdown via RPC
- "BAŞLA!" shown for 1 second before game starts
- Countdown text deactivates after start

### Score System
- **ScoreManager:** AddScore() and SubtractScore() methods
- **Points per box:** 10 (configurable)
- **ScoreDisplay:** Shows "SKOR: {value}" format

### Game Recording (CSV)
- **Location:** `kayitlar/oyun_sonuclari.csv` (next to Assets folder)
- **Format:** Tarih;Saat;Takim A;Takim A Puan;Takim B;Takim B Puan;Kazanan;Oyun Suresi
- **Saved by:** Master Client only, on game end
- **Used for:** Leaderboard top 10

### Leaderboard
- Shows after EndGame panel (5 second delay)
- Top 10 teams by highest score ever
- Same team = best score kept
- Synced via RPC from Master Client to all players
- Inspector: 10x TeamName texts + 10x TeamScore texts

### Team Name Input
- **Max characters:** 17 (configurable in GameReadyPanel)
- One player ready = entire team ready
- Stored in Room Properties

### Win Condition
- FinishLine passes through all team tanks = Team Finished
- Both teams finish (or 10 second timeout) → Higher score wins
- Tie → First team to finish wins

### Team System
| Team | TEAM_ID | Spawn Area | Layer | Default Name |
|------|---------|------------|-------|--------------|
| Team A | 0 | y = 0 | TeamA (8) | "Takım A" |
| Team B | 1 | y = -100 | TeamB (9) | "Takım B" |

- Teams cannot see each other (camera culling mask)
- Teams cannot collide (Physics2D layer collision matrix)

## Networking Architecture

### Photon Custom Properties

```csharp
// Player Properties (PlayerInfo.cs)
PLAYER_NAME      // string - Display name
TEAM_ID          // int - 0=TeamA, 1=TeamB
ROLE             // string - "Player" or "Spectator"
IS_READY         // bool - Ready status
TANK_COLOR_INDEX // int - 0-4 (determines tank color and spawn point)

// Room Properties - Scores
ScoreTeamA       // int - Team A score
ScoreTeamB       // int - Team B score

// Room Properties - Game State (GameSessionManager)
GameState        // int - Playing(0)/WaitingFinish(1)/Ended(2)
WinnerTeam       // int - 0, 1, or -1 (not determined)
TeamAFinished    // bool
TeamBFinished    // bool

// Room Properties - Team Names & Ready (PlayerInfo)
TEAM_A_NAME      // string - Custom team name
TEAM_B_NAME      // string - Custom team name
TEAM_A_READY     // bool - Team A ready for game
TEAM_B_READY     // bool - Team B ready for game
GAME_STARTED     // bool - Game has started

// Room Properties - Game Control (GameController)
GamePaused       // bool - Game paused state
```

### Key RPC Methods
- `GameController.RPC_StartCountdown(int seconds)` - Start countdown on all clients
- `GameController.RPC_StartGame()` - Start game on all clients
- `TankController.RPC_DamageFlash()` - Flash tank red on all clients
- `TankBullet.RPC_StickToBox(int boxViewID)` - Sync bullet stick effect
- `FloatingTextManager.RPC_ShowFloatingText(int points, Vector3 pos)` - Show floating text
- `GameEndUI.RPC_ReceiveLeaderboard(string[] names, int[] scores)` - Sync leaderboard

### Object Ownership
- **Player tanks:** Owned by spawning player
- **Bullets:** Owned by firing player
- **Blocks:** Owned by Master Client
- **Scene boxes:** Use RPC for destruction
- **Spectator cameras:** Local only

## Script Reference

### GameController.cs (TankGame.Game)
- Central game flow control with countdown
- **F1:** Start countdown (10s default), requires both teams ready
- **F5:** Reload scene
- `countdownText` - TMP_Text for countdown display
- `countdownSeconds = 10` - Configurable countdown
- `countdownStartMessage = "BAŞLA!"` - Message at 0

### Box.cs (TankGame)
- OnTriggerEnter2D detects tank collision
- Subtracts score, shows floating text, triggers damage flash
- Only tank owner processes collision (prevents duplicates)

### TankController.cs (TankGame.Tank)
- `TriggerDamageFlash()` - RPC to all clients for red flash
- `SyncRemotePlayer()` - Smooth network interpolation with SmoothDamp

### TankBullet.cs (TankGame.Tank)
- `StickToBox(Box box)` - Bullet becomes child of box
- `stickDestroyDelay = 1f` - Wait before destroying both
- `rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous`

### GameSessionManager.cs (TankGame.Game)
- `SaveGameResult()` - Saves to CSV on game end
- Records: date, time, team names, scores, winner, duration

### GameEndUI.cs (TankGame.Game)
- Requires PhotonView component
- `leaderboardDelay = 5f` - Seconds before showing leaderboard
- `leaderboardTeamNames` - List of 10 TMP texts
- `leaderboardTeamScores` - List of 10 TMP texts
- `SendLeaderboardToAll()` - Master reads CSV, sends via RPC

### FloatingTextManager.cs (TankGame.UI)
- Singleton with PhotonView
- `ShowFloatingText(int points, Vector3 position)` - RPC to all

### FloatingText.cs (TankGame.UI)
- Object pooled (INITIAL_POOL_SIZE = 20)
- Green +points float up, Red -points float down

### ScoreManager.cs (TankGame.Score)
- `AddScore(int teamId, int points)` - Add points
- `SubtractScore(int teamId, int points)` - Subtract points

### GameReadyPanel.cs (TankGame.Game)
- `maxTeamNameLength = 17` - Character limit for team names
- `nameInput.characterLimit` set in Start()

## Events System

```csharp
// GameController
static event Action OnGameStarted;
static event Action OnGamePaused;
static event Action OnSceneReloading;

// GameSessionManager
static event Action<GameState> OnGameStateChanged;
static event Action<int, int, int> OnGameEnded; // (winnerTeamId, teamAScore, teamBScore)

// GameReadyPanel
static event Action OnAllPlayersReady;
static event Action<string> OnLocalPlayerReady; // (teamName)

// FinishLine
static event Action<int> OnTeamFinished; // (teamId)

// ScoreManager
static event Action<int, int> OnScoreChanged; // (teamAScore, teamBScore)
```

## Unity Configuration

### Required Layers
- Layer 8: `TeamA`
- Layer 9: `TeamB`
- Layer 10: `Spectator`

### Required Components
- **GameEndUI:** Needs PhotonView component
- **FloatingTextManager:** Needs PhotonView component
- **GameController:** Has PhotonView (RequireComponent)

### GameScene Setup Checklist
- [ ] GameController with countdownText reference
- [ ] BlockSpawner with spawn settings
- [ ] ScoreManager singleton
- [ ] ScoreDisplay for each team
- [ ] GameSessionManager singleton
- [ ] GameReadyPanel with UI elements
- [ ] GameEndUI with PhotonView and leaderboard texts (10 each)
- [ ] FloatingTextManager with PhotonView
- [ ] FloatingText prefab in Resources

## Current Development Status

### Completed Features
- ✅ Photon connection and room management
- ✅ Team selection (4 buttons in lobby)
- ✅ Tank spawning and movement
- ✅ Shooting system with network sync
- ✅ Color-based box destruction with stick mechanic
- ✅ Block spawner and movement
- ✅ Finish line detection
- ✅ Score system (AddScore, SubtractScore)
- ✅ Score display with team names
- ✅ Tank collision penalty (-points, red flash)
- ✅ Floating text feedback (+/- points)
- ✅ Game session management (win/lose logic)
- ✅ Game result CSV recording
- ✅ End game UI with team names
- ✅ Leaderboard (top 10, RPC synced)
- ✅ Custom team names (max 17 chars)
- ✅ 10-second countdown before game start
- ✅ Smooth network interpolation (SmoothDamp)
- ✅ Mobile input support

### TODO / Next Steps
- [ ] Tank death/respawn system
- [ ] Health system
- [ ] Sound effects
- [ ] Visual polish

## Key Photon Settings
- **Location:** `Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset`
- **Game Version:** "1.0"
- **Max Players:** 12
- **AutomaticallySyncScene:** true
