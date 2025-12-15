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
│   ├── Box.cs                    # Destructible box behavior
│   ├── TankColor.cs              # Color enum (Green, Grey, Orange, Purple, Yellow)
│   ├── Block/
│   │   ├── BlockSpawner.cs       # Spawns blocks and finish line
│   │   ├── BlockMover.cs         # Moves blocks left
│   │   └── FinishLine.cs         # Finish line detection
│   ├── Game/
│   │   ├── GameController.cs     # Game flow control (F1 start, F5 reload)
│   │   ├── GameSessionManager.cs # Game state, win/lose logic
│   │   ├── GameReadyPanel.cs     # Team name input and ready UI
│   │   └── GameEndUI.cs          # End game UI (win/lose screen)
│   ├── Input/
│   │   └── MobileInputManager.cs # Mobile joystick and fire button
│   ├── Networking/
│   │   ├── NetworkManager.cs     # Photon connection and room management
│   │   ├── LobbyManager.cs       # Team selection UI
│   │   ├── PlayerInfo.cs         # Photon Custom Properties helper
│   │   ├── TankGameManager.cs    # Player spawning based on team/role
│   │   └── TeamManager.cs        # Layer assignment, camera culling
│   ├── Player/
│   │   ├── TankController.cs     # Tank movement, shooting, joystick support
│   │   ├── TankBullet.cs         # Bullet behavior, color-based destruction
│   │   ├── CameraFollow.cs       # Smooth camera following
│   │   └── SpectatorController.cs # Spectator camera
│   └── Score/
│       ├── ScoreManager.cs       # Team score management (Photon synced)
│       └── ScoreDisplay.cs       # 2D World Space score display with team names
├── Prefabs/
│   ├── Resources/                # Network-instantiated prefabs
│   │   ├── Tank_Green/Grey/Orange/Purple/Yellow.prefab
│   │   ├── Bullet.prefab
│   │   ├── SpectatorCamera.prefab
│   │   ├── FinishLine.prefab     # Finish line prefab
│   │   └── Block_*.prefab        # Block prefabs
│   ├── Box_Green/Grey/Orange/Purple/Yellow.prefab
│   └── Blocks/                   # Level design block variants
│       └── BoxGreen_1 through BoxGreen_8.prefab
├── Scenes/
│   ├── MenuScene.unity           # Connection and lobby
│   └── GameScene.unity           # Gameplay arena
└── Imports/Sprites/              # Tank and block sprites
```

## Game Architecture

### Scene Flow
```
MenuScene → LobbyPanel (Team Selection) → GameScene → GameReadyPanel → F1 Start → Gameplay
```

1. **MenuScene**: Connect to Photon, auto-join/create room, team selection (4 buttons)
2. **GameScene**: Tank spawning, ready panel for team names, F1 to start game

### Game Flow
```
GameScene loads
    ↓
TankGameManager spawns tanks/spectators
    ↓
GameReadyPanel shows (players enter team names)
    ↓
Master Client presses F1 (GameController.StartGame)
    ↓
BlockSpawner.StartSpawning() begins
    ↓
Blocks spawn and move left towards tanks
    ↓
Players shoot matching-color boxes to destroy them
    ↓
Score increases per destroyed box (+10 points)
    ↓
FinishLine spawns after all blocks
    ↓
FinishLine passes through all tanks = Team Finished
    ↓
GameSessionManager determines winner (higher score wins)
    ↓
GameEndUI shows result (KAZANDINIZ/KAYBETTİNİZ) with team names
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
- Root namespace - PlayerInfo, LobbyManager, TeamManager

## Game Controls

### Keyboard Controls
| Key | Function | Who |
|-----|----------|-----|
| F1 | Start Game | Master Client only |
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
- **Network sync:** Position and velocity via `OnPhotonSerializeView` with lag compensation

### Shooting
- **Control:** Space bar or Mobile Fire Button
- **Fire rate:** 0.5 seconds between shots
- **Bullet behavior:** Travels +X direction, 3-second lifetime
- **Bullet spawn:** From FirePoint child transform
- **Test Mode:** When enabled, bullets destroy ALL color boxes (for testing)

### Color-Based Destruction
- Tanks and boxes have colors: Green, Grey, Orange, Purple, Yellow
- **Matching colors:** Both bullet AND box are destroyed, +10 score
- **Non-matching colors:** Only bullet is destroyed
- **Tank hits:** Bullet destroyed (no friendly fire damage yet)

### Block System
- **BlockSpawner:** Waits for GameController.StartSpawning(), spawns blocks at intervals
- **BlockMover:** Moves blocks left at configurable speed
- **FinishLine:** Spawns after all blocks, waits for GameController.OnGameStarted to move
- **InstantiationData:** Speed and settings synced via PhotonNetwork.Instantiate data

### Team Name System
- **GameReadyPanel:** Players enter custom team name before game starts
- **One player ready = entire team ready** (first player sets team name)
- **Spectators:** Auto-marked as ready
- **Team names displayed:** In ScoreDisplay and GameEndUI

### Score System
- **ScoreManager:** Singleton, synced via Photon Room Properties
- **Points per box:** 10 (configurable)
- **ScoreDisplay:** 2D TextMeshPro showing "{TeamName}: {Score}"

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
- Custom team names stored in Room Properties

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

### Network Flow
1. `NetworkManager` connects to Photon, joins/creates room
2. `LobbyManager` handles team selection (4 buttons)
3. Master Client loads GameScene when ready
4. `TankGameManager` spawns tanks/spectators
5. `GameReadyPanel` shows for team name input
6. Master Client presses F1 → `GameController.StartGame()`
7. Checks `AreBothTeamsReady()` then fires `RPC_StartGame`
8. `BlockSpawner.StartSpawning()` begins
9. `FinishLine` starts moving after `OnGameStarted` event
10. `GameSessionManager` handles finish detection and winner

### Object Ownership
- **Player tanks:** Owned by spawning player
- **Bullets:** Owned by firing player
- **Blocks:** Owned by Master Client (PhotonNetwork.Instantiate)
- **Scene boxes (in blocks):** Use RPC for destruction (RpcTarget.All)
- **Spectator cameras:** Local only (not networked)

### InstantiationData Usage
- **Bullet:** `[0] = tankColor (int)`
- **Block:** `[0] = moveSpeed (float), [1] = destroyXPosition (float)`
- **FinishLine:** `[0] = moveSpeed, [1] = destroyXPosition, [2] = requiredTankCount (int), [3] = teamId (int)`

## Script Reference

### GameController.cs (TankGame.Game) - NEW
- Central game flow control
- **F1:** Start game (requires both teams ready)
- **F5:** Reload scene (RPC to all clients)
- Events: `OnGameStarted`, `OnGamePaused`, `OnSceneReloading`
- Methods: `StartGame()`, `ReloadScene()`, `IsGameStarted()`
- Triggers `BlockSpawner.StartSpawning()` on game start

### GameReadyPanel.cs (TankGame.Game) - NEW
- Team name input UI at game start
- One player ready = entire team ready
- Shows readiness status for both teams
- Auto-hides when game starts
- Events: `OnAllPlayersReady`, `OnLocalPlayerReady(teamName)`

### BlockSpawner.cs (TankGame.Block)
- **Does NOT auto-start** - waits for `StartSpawning()` call
- Spawns blocks at intervals for both teams
- Spawns FinishLine after all blocks (with spawnInterval delay)
- Master Client only
- `StartSpawning()` resets index and starts spawning

### FinishLine.cs (TankGame.Block)
- **Does NOT move until game starts** - listens to `GameController.OnGameStarted`
- Counts tanks passing through (OnTriggerEnter2D)
- Fires `OnTeamFinished` event when requiredTankCount reached

### GameSessionManager.cs (TankGame.Game)
- Singleton managing game state
- Listens to FinishLine.OnTeamFinished
- Determines winner based on score (higher wins, tie = first finisher)
- States: Playing → WaitingFinish → Ended
- Events: `OnGameStateChanged(GameState)`, `OnGameEnded(winnerTeamId, teamAScore, teamBScore)`

### GameEndUI.cs (TankGame.Game)
- Shows end game panel with team names
- Displays KAZANDINIZ/KAYBETTİNİZ based on local player's team
- Shows winner team name and both scores
- Hint text: "F5 ile yeni oyun" (Master Client) / "Oda sahibi yeni oyun başlatabilir"

### ScoreDisplay.cs (TankGame.Score)
- 2D World Space TextMeshPro display
- Shows "{TeamName}: {Score}" format
- Listens to Room Properties for team name updates
- Inherits from MonoBehaviourPunCallbacks

### TankController.cs (TankGame.Tank)
- Movement with WASD/Arrows or Joystick
- Shooting with Space or Fire Button
- `SetJoystick(Joystick)` for mobile input
- `OnFireButtonDown()` / `OnFireButtonUp()` for mobile fire

### TankBullet.cs (TankGame.Tank)
- **Namespace changed:** TankGame.Tank (was TankGame.Player)
- Color from InstantiationData
- Adds score via ScoreManager.Instance.AddScore()
- `isDestroyed` flag prevents double-destroy

### MobileInputManager.cs (TankGame.MobileInput) - NEW
- Manages mobile joystick and fire button
- Only active for players (not spectators)
- Activates when game starts, disables when game ends
- Integrates with TankController

### PlayerInfo.cs (Root) - ENHANCED
New methods for team names:
- `GetCustomTeamName(int teamId)` - Get team name from Room Properties
- `SetCustomTeamName(int teamId, string name)` - Set team name
- `IsTeamReady(int teamId)` - Check team ready status
- `SetTeamReady(int teamId, bool ready)` - Set team ready
- `AreBothTeamsReady()` - Check if both teams ready
- `ResetTeamReadyStates()` - Reset for new game
- `IsGameStarted()` / `SetGameStarted(bool)` - Game state

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

### Required Layers (must exist in TagManager)
- Layer 8: `TeamA`
- Layer 9: `TeamB`
- Layer 10: `Spectator`

### Physics2D Collision Matrix
- TeamA ↔ TeamB: **Disabled**
- Spectator ↔ TeamA/TeamB: **Disabled**
- All other collisions: **Enabled**

### Required Prefabs in Resources/
- Tank_Green, Tank_Grey, Tank_Orange, Tank_Purple, Tank_Yellow
- Bullet
- SpectatorCamera
- FinishLine (with PhotonView, FinishLine script, BoxCollider2D trigger)
- Block prefabs (with PhotonView, BlockMover script)

## Development

### Testing Multiplayer Locally
Use **ParrelSync** (included in project):
1. Window → ParrelSync → Clones Manager
2. Create a clone
3. Open clone in separate Unity Editor
4. Run both editors simultaneously

### Test Mode
Enable `testMode` in TankController Inspector to allow bullets to destroy ALL color boxes.

### GameScene Setup Checklist
- [ ] GameController GameObject with script
- [ ] BlockSpawner GameObject with script
- [ ] SpawnPointTeamA and SpawnPointTeamB transforms assigned
- [ ] Block prefab names added to BlockSpawner list
- [ ] FinishLine prefab in Resources/ folder
- [ ] ScoreManager GameObject with script
- [ ] ScoreDisplay for Team A (teamId=0) with TextMeshPro
- [ ] ScoreDisplay for Team B (teamId=1) with TextMeshPro
- [ ] GameSessionManager GameObject with script
- [ ] Canvas with GameReadyPanel script and UI elements
- [ ] Canvas with GameEndUI script and EndGamePanel

## Current Development Status

### Completed Features
- ✅ Photon connection and room management
- ✅ Team selection (4 buttons in lobby)
- ✅ Tank spawning and movement
- ✅ Shooting system with network sync
- ✅ Color-based box destruction
- ✅ Block spawner and movement
- ✅ Finish line detection
- ✅ Score system (per team, Photon synced)
- ✅ Score display (2D World Space with team names)
- ✅ Game session management (win/lose logic)
- ✅ End game UI (KAZANDINIZ/KAYBETTİNİZ with team names)
- ✅ Custom team names (GameReadyPanel)
- ✅ Game control (F1 start, F5 reload)
- ✅ Mobile input support (joystick, fire button)

### TODO / Next Steps
- [ ] Top 10 leaderboard after game end
- [ ] Tank death/respawn system
- [ ] Health system
- [ ] Sound effects
- [ ] Visual polish

## Key Photon Settings
- **Location:** `Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset`
- **Game Version:** "1.0"
- **Max Players:** 12
- **AutomaticallySyncScene:** true
