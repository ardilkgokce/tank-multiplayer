# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A 2D multiplayer tank game built with Unity and Photon Unity Networking (PUN). Features team-based gameplay with color-coded destruction mechanics.

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
│   ├── Networking/
│   │   ├── NetworkManager.cs     # Photon connection and room management
│   │   ├── LobbyManager.cs       # Team selection UI, ready system
│   │   ├── PlayerInfo.cs         # Photon Custom Properties helper
│   │   ├── TankGameManager.cs    # Player spawning based on team/role
│   │   └── TeamManager.cs        # Layer assignment, camera culling
│   └── Player/
│       ├── TankController.cs     # Tank movement, shooting, network sync
│       ├── TankBullet.cs         # Bullet behavior, color-based destruction
│       ├── CameraFollow.cs       # Smooth camera following
│       └── SpectatorController.cs # Spectator camera with player switching
├── Prefabs/
│   ├── Resources/                # Network-instantiated prefabs
│   │   ├── Tank_Green/Grey/Orange/Purple/Yellow.prefab
│   │   ├── Bullet.prefab
│   │   └── SpectatorCamera.prefab
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
MenuScene → LobbyPanel (in MenuScene) → GameScene
```

1. **MenuScene**: Connect to Photon, auto-join/create room, team selection, ready system
2. **GameScene**: Team-based spawning, gameplay with shooting and destruction

### Namespaces
- `TankGame` - Color enum, Box script
- `TankGame.Networking` - NetworkManager, TankGameManager
- `TankGame.Player` - TankController, TankBullet, CameraFollow, SpectatorController
- Root namespace - PlayerInfo, LobbyManager, TeamManager

## Game Mechanics

### Movement
- **Controls:** WASD or Arrow keys
- **Physics:** Rigidbody2D velocity-based movement
- **Network sync:** Position and velocity via `OnPhotonSerializeView` with lag compensation

### Shooting
- **Control:** Space bar to fire
- **Fire rate:** 0.5 seconds between shots
- **Bullet behavior:** Travels +X direction, 3-second lifetime
- **Bullet spawn:** From FirePoint child transform

### Color-Based Destruction
- Tanks and boxes have colors: Green, Grey, Orange, Purple, Yellow
- **Matching colors:** Both bullet AND box are destroyed
- **Non-matching colors:** Only bullet is destroyed
- **Tank hits:** Bullet destroyed (no friendly fire damage yet)

### Team System
| Team | TEAM_ID | Spawn Area | Layer | Color Theme |
|------|---------|------------|-------|-------------|
| Team A | 0 | y = 0 | TeamA (8) | Green |
| Team B | 1 | y = -100 | TeamB (9) | Blue |

- Teams cannot see each other (camera culling mask)
- Teams cannot collide (Physics2D layer collision matrix)

### Spectator System
- **Tab:** Switch to next player
- **1-5:** Select specific player
- **Space:** Toggle follow/manual mode
- **R:** Refresh tank list
- **WASD:** Manual camera movement (when not following)

## Networking Architecture

### Photon Custom Properties
```csharp
// PlayerInfo.cs key constants
PLAYER_NAME      // string - Display name
TEAM_ID          // int - 0=TeamA, 1=TeamB
ROLE             // string - "Player" or "Spectator"
IS_READY         // bool - Ready status
TANK_COLOR_INDEX // int - 0-4 (determines tank color and spawn point)
```

### Network Flow
1. `NetworkManager` connects to Photon, joins/creates room
2. `LobbyManager` handles team selection and ready state
3. Master Client starts game when all players ready (min 2 players)
4. `PhotonNetwork.LoadLevel("GameScene")` syncs all clients
5. `TankGameManager` spawns players based on role and team
6. `TeamManager` assigns layers and configures camera culling

### Object Ownership
- **Player tanks:** Owned by spawning player
- **Bullets:** Owned by firing player
- **Scene boxes:** Owned by Master Client (destruction via RPC)
- **Spectator cameras:** Local only (not networked)

## Script Reference

### NetworkManager.cs (TankGame.Networking)
- Connects to Photon on Start
- Auto-joins random room or creates new one
- Room settings: 12 max players, AutomaticallySyncScene enabled

### LobbyManager.cs (Root)
- Manages team selection UI buttons
- Validates team capacity (5 per team, 2 spectators)
- Ready button locks in player settings
- Start button visible only to Master Client (requires 2+ players)

### TankGameManager.cs (TankGame.Networking)
- Spawns tanks at team spawn points based on TANK_COLOR_INDEX
- Spawns SpectatorCamera for spectator role
- Configures camera following and culling

### TeamManager.cs (Root)
- `AssignTeamLayer(GameObject, teamId)` - Recursive layer assignment
- `ConfigureCameraForTeam(Camera, teamId)` - Sets culling mask
- `GetTeamCameraPosition(teamId)` - Returns (0,0,-10) or (0,-100,-10)

### TankController.cs (TankGame.Player)
- Implements `IPunObservable` for network sync
- Only processes input when `photonView.IsMine`
- Smooth interpolation for remote tanks (lerp speed: 10)

### TankBullet.cs (TankGame.Player)
- Sets color via RPC for network sync
- `OnTriggerEnter2D` handles collision with boxes and tanks
- Requests box destruction via `Box.RequestDestroy()`

### Box.cs (TankGame)
- `GetColor()` returns box TankColor
- `RequestDestroy(bulletColor)` checks color match
- `DestroyBox()` RPC destroys via Master Client

## Unity Configuration

### Required Layers (must exist in TagManager)
- Layer 8: `TeamA`
- Layer 9: `TeamB`
- Layer 10: `Spectator`

### Physics2D Collision Matrix
- TeamA ↔ TeamB: **Disabled**
- Spectator ↔ TeamA/TeamB: **Disabled**
- All other collisions: **Enabled**

## Development

### Testing Multiplayer Locally
Use **ParrelSync** (included in project):
1. Window → ParrelSync → Clones Manager
2. Create a clone
3. Open clone in separate Unity Editor
4. Run both editors simultaneously

### Adding New Tank Features
1. Modify `TankController.cs` for behavior
2. Update prefabs in `Assets/Prefabs/Resources/`
3. Sync via `OnPhotonSerializeView` if network-relevant

### Adding New Player Properties
1. Add constant key in `PlayerInfo.cs`
2. Update `SetPlayerProperties()` method
3. Add getter method following existing pattern

### Adding New Destructible Objects
1. Create prefab with `PhotonView` and `Box` components
2. Assign `TankColor` in inspector
3. Place in scene or add to Resources/ for network instantiation

## Key Photon Settings
- **Location:** `Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset`
- **Game Version:** "1.0"
- **Max Players:** 12
- **AutomaticallySyncScene:** true
