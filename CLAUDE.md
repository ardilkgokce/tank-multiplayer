# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity multiplayer game project using Photon Unity Networking (PUN). The project is built with Unity 2021.3.45f2 and uses the Universal Render Pipeline (URP) with 2D features enabled.

**Project Name:** tank-multiplayer
**Unity Version:** 2021.3.45f2
**Rendering Pipeline:** Universal Render Pipeline (URP) 12.1.15
**Primary Framework:** Photon Unity Networking (PUN)

## Architecture

### Networking Architecture
The project uses Photon Unity Networking (PUN) for multiplayer functionality:
- **Photon Realtime** - Core networking layer for real-time multiplayer communication
- **PhotonUnityNetworking** - Unity-specific PUN implementation with MonoBehaviour integration
- **PhotonChat** - Separate chat functionality layer
- **PhotonView** - Component for synchronizing GameObjects across the network

### Key Photon Components
- `PhotonServerSettings.asset` - Network configuration (AppIDs, regions, protocol settings) located at `Assets/Photon/PhotonUnityNetworking/Resources/`
- `MonoBehaviourPunCallbacks` - Base class for scripts that need to respond to Photon callbacks
- `PhotonNetwork` - Static class providing access to all networking functionality
- `PhotonView` - Component that enables network synchronization for GameObjects

### Project Structure
```
Assets/
├── Photon/                          # Photon networking SDK
│   ├── PhotonUnityNetworking/       # PUN main implementation
│   │   ├── Code/                    # Core PUN scripts
│   │   ├── Demos/                   # Example implementations
│   │   │   ├── PunBasics-Tutorial/  # Basic multiplayer tutorial
│   │   │   ├── DemoAsteroids/       # Asteroids multiplayer demo
│   │   │   ├── DemoSlotRacer/       # Slot car racing demo
│   │   │   ├── DemoProcedural/      # Procedural generation demo
│   │   │   └── PunCockpit/          # Debug/testing tool
│   │   └── Resources/               # PhotonServerSettings.asset
│   ├── PhotonRealtime/              # Core realtime networking
│   ├── PhotonChat/                  # Chat functionality
│   └── PhotonLibs/                  # Native libraries
├── Scenes/                          # Game scenes
│   └── SampleScene.unity
└── Settings/                        # URP settings and templates
```

## Development Commands

### Opening the Project
- Open the project in Unity Hub and launch with Unity 2021.3.45f2
- The project uses Visual Studio or Rider as the IDE (configured in preferences)

### Building
- **Build for development:** File → Build Settings → Build (Unity Editor)
- **Build for production:** File → Build Settings → Build (ensure "Development Build" is unchecked)

### Running the Game
- Press the Play button in Unity Editor to test in Play Mode
- For multiplayer testing, build a standalone executable and run alongside the Unity Editor

### Testing Multiplayer Locally
1. Build a standalone player (File → Build Settings → Build)
2. Run the built executable
3. Press Play in Unity Editor
4. Both instances should connect to the same Photon room

## Photon Integration Patterns

### Creating Network Objects
Use `PhotonNetwork.Instantiate()` instead of Unity's `Instantiate()`:
```csharp
PhotonNetwork.Instantiate(prefabName, position, rotation, 0);
```
Prefabs must be in a `Resources/` folder to be instantiated over the network.

### Network Synchronization
- Add `PhotonView` component to GameObjects that need network sync
- Implement `IPunObservable` interface for custom serialization
- Use `PhotonTransformView` for position/rotation sync
- Use `PhotonRigidbodyView` or `PhotonRigidbody2DView` for physics sync

### RPC Calls
```csharp
[PunRPC]
void MyRpcMethod(params)
{
    // Method implementation
}

// Call the RPC
photonView.RPC("MyRpcMethod", RpcTarget.All, params);
```

### Connection Flow
1. Connect to Photon: `PhotonNetwork.ConnectUsingSettings()`
2. Join/Create Room: `PhotonNetwork.JoinRandomRoom()` or `PhotonNetwork.CreateRoom()`
3. Spawn player: `PhotonNetwork.Instantiate()` in room
4. Leave room: `PhotonNetwork.LeaveRoom()`

### Callbacks
Inherit from `MonoBehaviourPunCallbacks` and override relevant methods:
- `OnConnectedToMaster()` - Connected to Photon Cloud
- `OnJoinedRoom()` - Successfully joined a room
- `OnPlayerEnteredRoom()` - Another player joined
- `OnPlayerLeftRoom()` - Another player left

## Important Unity-Specific Notes

### Script Compilation
- Unity automatically compiles C# scripts when they're saved
- Compilation errors prevent entering Play Mode
- Check the Console window for compilation errors

### Scene Management
- Changes to scenes must be saved (Ctrl+S or File → Save)
- Scene changes in Play Mode are lost when exiting Play Mode

### Prefab Workflow
- Apply changes to prefab instances using the "Apply" button in the Inspector
- Breaking prefab connections should be done intentionally

### Universal Render Pipeline
- Graphics settings are in `Assets/Settings/UniversalRP.asset`
- 2D Renderer configuration is in `Assets/Settings/Renderer2D.asset`
- Shaders and materials must be URP-compatible

## Key Files

- `Packages/manifest.json` - Unity package dependencies
- `ProjectSettings/ProjectSettings.asset` - Project configuration
- `Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset` - Photon network configuration
- `Assets/Settings/UniversalRP.asset` - URP graphics settings

## Dependencies

The project uses these Unity packages:
- com.unity.render-pipelines.universal (12.1.15) - Universal Render Pipeline
- com.unity.textmeshpro (3.0.6) - Text rendering
- com.unity.timeline (1.6.5) - Animation sequencing
- com.unity.feature.2d (2.0.1) - 2D game features
- com.unity.test-framework (1.1.33) - Unit testing

External dependencies:
- Photon Unity Networking (included in Assets/Photon/)
