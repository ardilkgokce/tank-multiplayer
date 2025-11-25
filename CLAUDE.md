# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A 2D multiplayer tank game built with Unity 2021.3.45f2 and Photon Unity Networking (PUN). Features team-based gameplay with 2 teams (5 players each) and spectator support (2 spectators max).

**Unity Version:** 2021.3.45f2
**Rendering:** Universal Render Pipeline (URP) 12.1.15 with 2D Renderer

## Game Architecture

### Scene Flow
```
MenuScene → LobbyPanel (in MenuScene) → GameScene
```

1. **MenuScene**: Connection to Photon, automatic room join/create
2. **LobbyPanel**: Team selection, player ready system, game start (Master Client only)
3. **GameScene**: Actual gameplay with team-based spawning

### Networking Architecture

The game uses Photon Custom Properties for player state synchronization:

```csharp
// PlayerInfo.cs - Key constants
PLAYER_NAME, TEAM_ID, ROLE, IS_READY, TANK_COLOR_INDEX
```

**Connection Flow:**
- `NetworkManager` (MenuScene): Connects to Photon, auto-joins/creates room
- `LobbyManager` (MenuScene): Manages team selection and ready state
- `TankGameManager` (GameScene): Spawns players based on their team/role

### Team System

- **Team A** (TEAM_ID = 0): Spawns at y=0 area, uses green layer color
- **Team B** (TEAM_ID = 1): Spawns at y=-100 area, uses blue layer color
- Teams cannot see each other (camera culling mask per team)
- Teams cannot collide (Physics2D layer collision matrix)

**Required Unity Layers** (must exist in Editor):
- `TeamA`
- `TeamB`
- `Spectator`

### Script Responsibilities

| Script | Location | Purpose |
|--------|----------|---------|
| `NetworkManager` | Networking/ | Photon connection and room management |
| `LobbyManager` | Networking/ | Team selection UI, ready system |
| `TankGameManager` | Networking/ | Player spawning based on team/role |
| `TeamManager` | Networking/ | Layer assignment, camera culling, collision config |
| `PlayerInfo` | Networking/ | Static helper for Photon Custom Properties |
| `TankController` | Player/ | Tank movement with network sync (IPunObservable) |
| `CameraFollow` | Player/ | Smooth camera following for players |
| `SpectatorController` | Player/ | Spectator camera with player switching |

### Namespaces
- `TankGame.Networking` - Network-related classes
- `TankGame.Player` - Player control classes
- Root namespace - `PlayerInfo`, `LobbyManager`, `TeamManager`

## Key Files

### Network Prefabs (must be in Resources/)
Tank prefabs are color-coded and located at `Assets/Prefabs/Resources/`:
- `Tank_Green`, `Tank_Grey`, `Tank_Orange`, `Tank_Purple`, `Tank_Yellow`
- `SpectatorCamera`

### Configuration
- `Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset` - Photon AppID and region

## Development

### Testing Multiplayer Locally
Use **ParrelSync** (included in project) to create project clones:
1. Window → ParrelSync → Clones Manager
2. Create a clone
3. Open clone in separate Unity Editor instance
4. Run both editors simultaneously

### Adding New Tank Features
1. Modify `TankController.cs` for behavior
2. Update prefabs in `Assets/Prefabs/Resources/`
3. Ensure changes sync via `OnPhotonSerializeView` if network-relevant

### Adding New Player Properties
1. Add constant key in `PlayerInfo.cs`
2. Update `SetPlayerProperties()` method
3. Add getter method following existing pattern
