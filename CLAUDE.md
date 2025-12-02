# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity VR project for PICO headsets (PCVR/MR) that captures and transmits VR tracking data (head and controller positions, rotations, velocities, button states) to a remote server via HTTP POST requests.

**Unity Version**: 2022.3.62f2c1

**Target Platform**: Android (PICO VR Headset)

## Key Dependencies

- **PICO Unity Integration SDK** 3.3.2-20251111 (local package in `Packages/`)
- **Unity XR Interaction Toolkit** 3.3.0
- **Unity XR OpenXR** 1.14.3
- **Unity Input System** (new input system, not legacy)
- **TextMeshPro** 3.0.7

## Project Structure

```
Assets/
├── Scenes/              # Unity scenes (PicoXr.unity is main scene)
├── Scripts/             # Core C# scripts
│   ├── DataTracking.cs  # Main VR tracking and data transmission
│   ├── SendVRData.cs    # Data structures for VR tracking data
│   └── UIController.cs  # Basic UI interaction logic
├── XR/                  # XR configuration (loaders, settings)
├── XRI/                 # XR Interaction Toolkit settings
├── Samples/             # XR Interaction Toolkit sample assets
└── Resources/           # Unity resources

Packages/
├── PICO Unity Integration SDK-3.3.2-20251111/  # PICO SDK (local package)
└── manifest.json        # Unity package dependencies

ProjectSettings/         # Unity project configuration
```

## Architecture

### VR Data Tracking System

The core functionality centers around the `DataTracking` namespace with two main components:

1. **DataTracking.cs** (`Assets/Scripts/DataTracking.cs`)
   - MonoBehaviour that runs on the XR Origin GameObject
   - Uses Unity's new Input System with `InputActionReference` for all input
   - Tracks head (HMD) and both controllers (left/right) pose data
   - Captures: position, rotation, linear velocity, angular velocity, button states, axes
   - Sends JSON data to a configurable HTTPS server endpoint via UnityWebRequest
   - Implements `CustomCertificateHandler` to bypass SSL validation (development only)
   - Currently sends data every frame (see `Update()` method)

2. **SendVRData.cs** (`Assets/Scripts/SendVRData.cs`)
   - Defines serializable data structures for VR tracking data
   - Key classes:
     - `SendVRData`: Root data container with head, left, right, timestamp, state, battery
     - `HeadInfo`: Head position, rotation, velocities
     - `ControllerInfo`: Controller pose, velocities, button states[7], axes[4]
     - `Vector3Data`, `QuaternionData`, `Vector4Data`: Serialization wrappers
     - `ButtonState`: Button value, pressed, touched states

### Input System Integration

- Uses Unity's Input System (not legacy Input Manager)
- All inputs are mapped through `InputActionReference` fields exposed in Inspector
- Actions must be enabled in `Awake()` and subscribed in `OnEnable()`
- Button states are tracked for:
  - Right controller: A button (index 4), B button (index 5), Grip (index 1)
  - Left controller: Grip (index 1)
- Button array indices are fixed (length 7) per PICO controller specification

### PICO SDK Integration

- PICO Unity Integration SDK is installed as a local package
- Video see-through (passthrough) is enabled via `PXR_Manager.EnableVideoSeeThrough = true` in `DataTracking.Awake()`
- Uses PICO's implementation of OpenXR for XR management

### UI System and Ray Interaction

3. **UIController.cs** (`Assets/Scripts/UIController.cs`)
   - **Programmatically generates entire UI** - no manual scene setup required
   - Creates Canvas, modal window, buttons, and all UI elements through code
   - Supports XR ray interaction through Unity's XR Interaction Toolkit
   - Automatically adds `TrackedDeviceGraphicRaycaster` for XR compatibility

   **Key Features:**
   - Fully code-generated UI (no Inspector dragging needed)
   - Dynamic button creation via `AddButton(text, callback, color)` API
   - Built-in button management (Confirm, Cancel, Apply)
   - Configurable window size, position, and appearance via Inspector
   - Vertical layout for automatic button arrangement

   **Usage:**
   1. Create empty GameObject in scene
   2. Add UIController component
   3. Configure XR Ray Interactor on controllers (one-time setup)
   4. UI generates automatically at runtime

   **Customization:**
   - Modify `AddDefaultButtons()` method to define your buttons
   - Use `AddButton()` API for runtime button creation
   - Adjust colors, sizes, and layout in Inspector
   - Detailed guide: `Assets/Scripts/README_UICONTROLLER.md`

   **Ray Interaction Requirements:**
   - Left/Right Hand Controllers need:
     - `XR Ray Interactor` with "Enable Interaction with UI GameObjects" enabled
     - `XR Interactor Line Visual` for visual feedback
     - `Line Renderer` for ray rendering
   - These components are typically included in XR Interaction Toolkit samples

## Common Development Commands

### Opening the Project

Open in Unity Hub or directly with Unity 2022.3.62f2c1:

```bash
# Windows
"C:\Program Files\Unity\Hub\Editor\2022.3.62f2c1\Editor\Unity.exe" -projectPath "C:\work_project\pico_project\02_pico_pcvr"
```

### Building for PICO

1. Ensure Android build support is installed
2. In Unity: File → Build Settings → Android → Switch Platform
3. Configure Player Settings:
   - XR Plug-in Management: Enable OpenXR + PICO
   - Set minimum API level to Android 10.0 (API 29) or higher
4. Build Settings → Build or Build And Run

### Version Control

Standard Unity `.gitignore` is configured:
- Library/, Temp/, Logs/, UserSettings/ are excluded
- `.csproj` and `.sln` files are excluded (auto-generated)

### Testing in Editor

- XR Device Simulator samples are available in `Assets/Samples/`
- Configure Input Actions in Edit → Project Settings → XR Plug-in Management

## Important Notes

### Data Transmission

- Server URL is configured in the Inspector on the DataTracking component
- Default: `https://localhost:5000/poseData`
- SSL certificate validation is disabled (only for development!)
- Data is sent every frame by default (line 371 in DataTracking.cs has commented interval logic)
- JSON serialization uses Unity's `JsonUtility`

### Input Action Setup

All `InputActionReference` fields in `DataTracking.cs` must be assigned in the Inspector, referencing the XR Interaction Toolkit's default input actions:
- Head: Position, Rotation, Velocity, Angular Velocity
- Controllers: Position, Rotation, Velocity, Angular Velocity, Buttons

### Button Mapping

Controller button array structure (length 7):
- Index 0: (unused)
- Index 1: Grip button
- Index 2-3: (unused)
- Index 4: A button (right controller only)
- Index 5: B button (right controller only)
- Index 6: (unused)

### Performance Considerations

- Data is currently sent every frame (60-90 fps) which generates significant network traffic
- Consider uncommenting the interval logic (lines 369-373 in DataTracking.cs) for production
- `sendInterval` field controls transmission rate (default 0.1s = 10Hz)

## Language and Comments

The codebase contains Chinese comments (简体中文). Key terms:
- 新增 = newly added
- 默认 = default
- 改为 = changed to
- 保留 = retain/keep
