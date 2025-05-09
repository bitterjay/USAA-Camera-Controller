# NDI Viewer Refactoring Plan

## Current Issues

The current implementation places most functionality in a single monolithic class (NDIViewerManager), making it difficult to maintain, extend, and test. The code has:

1. Too many responsibilities in one class
2. Tightly coupled components
3. Limited separation of concerns
4. Minimal use of interfaces for flexibility

## Proposed Architecture

We'll refactor the application using a component-based architecture with clear separation of concerns:

### 1. Core Structure

```
Assets/
├── Scripts/
│   ├── Core/               # Core application logic
│   ├── UI/                 # UI components
│   ├── Cameras/            # Camera handling
│   ├── Settings/           # Configuration and settings
│   └── Utilities/          # Helpers and extensions
├── Prefabs/                # Reusable UI elements
├── Resources/              # Shared resources
└── Scenes/                 # Unity scenes
```

### 2. Class Breakdown

#### Core Layer

- **`NDIViewerApp`**: Main application controller (replaces NDIViewerManager)
- **`CameraRegistry`**: Manages camera discovery and registration
- **`GridLayoutController`**: Handles grid layout calculations

#### Models

- **`CameraInfo`**: Data model for camera information (already exists)
- **`LayoutSettings`**: Configuration for layout appearance
- **`UserPreferences`**: User settings persistence

#### UI Components

- **`CameraTileView`**: Visual representation of a camera tile
- **`StatusBarView`**: Bottom status bar component
- **`SettingsPanelView`**: Settings panel UI
- **`CameraListItemView`**: Item in the camera list
- **`InsertionPointView`**: The "+" insertion point for reordering

#### Services

- **`NDISourceService`**: Handles NDI source discovery
- **`UIFactory`**: Creates UI elements programmatically

## Migration Plan

### Phase 1: Core Framework

1. Create folder structure
2. Extract models (CameraInfo, settings classes)
3. Create interfaces for major components

### Phase 2: Component Extraction

1. Extract CameraTileView from NDIViewerManager
2. Extract StatusBarView from NDIViewerManager
3. Extract SettingsPanelView from NDIViewerManager
4. Create UIFactory for element creation

### Phase 3: Core Logic Extraction

1. Create NDIViewerApp as the new controller
2. Create CameraRegistry for camera management
3. Move grid layout logic to GridLayoutController

### Phase 4: Wiring and Integration

1. Connect components through events and interfaces
2. Update Unity references
3. Add dependency injection where appropriate

## Detailed Class Specifications

### NDIViewerApp
```csharp
public class NDIViewerApp : MonoBehaviour
{
    [SerializeField] private LayoutSettings layoutSettings;
    
    private CameraRegistry cameraRegistry;
    private GridLayoutController gridController;
    private StatusBarView statusBar;
    private SettingsPanelView settingsPanel;
    
    // Core application lifecycle
    private void Start() { /* Initialize components */ }
    
    // Event handlers
    public void OnCameraDiscovered(CameraInfo camera) { /* ... */ }
    public void OnCameraSelected(CameraInfo camera) { /* ... */ }
}
```

### CameraTileView
```csharp
public class CameraTileView : MonoBehaviour
{
    public CameraInfo Camera { get; private set; }
    public bool IsActive { get; private set; }
    
    // Events
    public event System.Action<CameraTileView> OnSelected;
    
    // UI References
    private RawImage videoDisplay;
    private Text displayNameText;
    private Text sourceNameText;
    
    public void Initialize(CameraInfo camera) { /* ... */ }
    public void SetActive(bool active) { /* ... */ }
}
```

### Additional Benefits

1. **Testability**: Smaller components are easier to unit test
2. **Reusability**: Components can be reused in other projects
3. **Extensibility**: New features can be added without modifying existing code
4. **Maintainability**: Easier to understand and maintain with clearly defined responsibilities

## Implementation Order

1. Create folder structure
2. Setup interfaces
3. Extract models
4. Create initial UI components
5. Implement core services
6. Wire everything together
7. Add unit tests 