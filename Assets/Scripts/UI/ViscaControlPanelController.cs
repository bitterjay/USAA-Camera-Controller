using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;

public class ViscaControlPanelController : MonoBehaviour
{
    private ViscaOverIpSender viscaSender;
    private CameraInfo activeCamera;
    private CameraRegistry cameraRegistry;
    private bool isInitialized = false;
    
    // Current camera IP address - now using NDIViewerApp.ActiveCameraIP instead
    
    [SerializeField] private string fallbackIp = "192.168.1.100";
    [SerializeField] private int fallbackPort = 52381;

    private string ipAddress;

    private void Awake()
    {
        Debug.Log("ViscaControlPanelController Awake called");
        ipAddress = FindObjectOfType<NDIViewerApp>().currentIP;
        // No initialization here - wait for camera selection
    }
    
    private void OnEnable()
    {
        Debug.Log("ViscaControlPanelController OnEnable called - waiting for camera selection");
        
        // Just set up the UI but don't do any camera initialization yet
        SetupUI();
        
        // Get reference to NDIViewerApp and set up camera selection callback
        var currentIP = FindObjectOfType<NDIViewerApp>().currentIP;
        if (currentIP != null)
        {
            // Get the camera registry to subscribe to camera selection events
            cameraRegistry = FindObjectOfType<NDIViewerApp>().GetCameraRegistry();
            if (cameraRegistry != null)
            {
                // Subscribe to camera selection events
                cameraRegistry.OnCameraSelected += OnCameraSelected;
                Debug.Log("Subscribed to camera selection events - waiting for user to click a camera");
            }
            else
            {
                Debug.LogWarning("Camera registry is null");
            }
        }
        else
        {
            Debug.LogError("NDIViewerApp not found!");
        }
    }
    
    // This method is called when a camera is clicked in the UI
    public void OnCameraSelected(CameraInfo camera)
    {
        Debug.Log($"▶▶▶ CAMERA CLICKED: {camera.niceName} ◀◀◀");
        
        // Get the current IP address - try from NDIViewerApp.currentIP first, then static property
        var ndiApp = FindObjectOfType<NDIViewerApp>();
        string currentIP = ndiApp.currentIP;
        
        if (ndiApp != null && !string.IsNullOrEmpty(currentIP))
        {
            Debug.Log($"Using currentIP from NDIViewerApp instance: {currentIP}");
        }
        else
        {
            // Fall back to static property
            currentIP = NDIViewerApp.ActiveCameraIP;
            
            if (currentIP == "Not Set")
            {
                Debug.LogWarning("No IP address available, using fallback IP");
                currentIP = fallbackIp;
            }
            else
            {
                Debug.Log($"Using ActiveCameraIP static property: {currentIP}");
            }
        }
        
        // Make sure the selected camera has the current IP
        if (camera != null && string.IsNullOrEmpty(camera.viscaIp))
        {
            camera.viscaIp = currentIP;
            Debug.Log($"Set camera viscaIp to: {currentIP}");
        }
        
        // Initialize if this is the first camera selection
        if (!isInitialized)
        {
            Debug.Log("First camera selection - initializing controller");
            InitializePTZControls(camera);
        }
        else
        {
            // Just update the active camera
            SetActiveCamera(camera);
        }
    }
    
    // Initialize the PTZ controls with the selected camera
    public void InitializePTZControls(CameraInfo camera)
    {
        Debug.Log("Initializing PTZ Controls");
        
        // Clean up any existing sender
        if (viscaSender != null)
        {
            viscaSender.Dispose();
            viscaSender = null;
        }
        
        if (camera != null)
        {
            activeCamera = camera;
            viscaSender = new ViscaOverIpSender(camera);
            Debug.Log($"Created VISCA sender for camera: {camera.niceName}, IP: {camera.viscaIp}, Port: {camera.viscaPort}");
        }
        else
        {
            Debug.LogWarning("No camera provided, using fallback values");
            viscaSender = new ViscaOverIpSender(fallbackIp, fallbackPort);
            Debug.Log($"Created fallback VISCA sender with IP: {fallbackIp}:{fallbackPort}");
        }
        
        isInitialized = true;
        Debug.Log("PTZ Controller initialized successfully");
    }
    
    private void SetActiveCamera(CameraInfo camera)
    {
        Debug.Log($"Setting active camera: {(camera != null ? camera.niceName : "null")}");
        
        if (camera == null)
        {
            Debug.LogWarning("SetActiveCamera called with null camera");
            return;
        }
        
        // Check if we're setting the same camera
        bool isSameCamera = activeCamera == camera;
        if (isSameCamera)
        {
            Debug.Log($"Camera {camera.niceName} is already active, no change needed");
            return;
        }

        // Set the selected camera as active
        activeCamera = camera;
        
        // Debug.Log($"======================================");
        Debug.Log($"CAMERA SELECTED: {camera.niceName}, IP: {camera.viscaIp}");
        // Debug.Log($"======================================");
        
        // Create a new sender with the camera's info
        if (viscaSender != null)
        {
            viscaSender.Dispose();
        }
        
        // Create a new sender with the camera's IP and port
        viscaSender = new ViscaOverIpSender(camera);
        Debug.Log($"Updated VISCA sender for camera: {camera.niceName}, IP: {camera.viscaIp}");
    }
    
    private bool EnsureViscaSender()
    {
        // If we haven't been initialized yet, don't create a sender
        if (!isInitialized)
        {
            Debug.Log("EnsureViscaSender called before initialization - not creating sender yet");
            return false;
        }
        
        if (viscaSender != null)
            return true;
            
        Debug.LogWarning("viscaSender is null, attempting to recreate");
        
        if (activeCamera != null)
        {
            Debug.Log($"Active camera: {(activeCamera != null ? $"{activeCamera.niceName} ({activeCamera.viscaIp})" : "null")}");
            
            // Create a new sender with the camera's IP and port
            viscaSender = new ViscaOverIpSender(activeCamera);
            Debug.Log($"Recreated VISCA sender for camera: {activeCamera.niceName}, IP: {activeCamera.viscaIp}");
            return true;
        }
        else
        {
            // Create with fallback values
            viscaSender = new ViscaOverIpSender(fallbackIp, fallbackPort);
            Debug.Log($"Created fallback VISCA sender with IP: {fallbackIp}:{fallbackPort}");
            return true;
        }
    }

    private void SetupUI()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("UIDocument component not found!");
            return;
        }

        var root = uiDoc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("rootVisualElement is null!");
            return;
        }

        Debug.Log("Setting up UI control callbacks");
        
        // // Create an overlay to indicate inactive state
        // var inactiveOverlay = new VisualElement();
        // inactiveOverlay.name = "inactiveOverlay";
        // inactiveOverlay.style.backgroundColor = new Color(0, 0, 0, 0.7f);
        // inactiveOverlay.style.position = Position.Absolute;
        // inactiveOverlay.style.left = 0;
        // inactiveOverlay.style.top = 0;
        // inactiveOverlay.style.right = 0;
        // inactiveOverlay.style.bottom = 0;
        
        // var messageLabel = new Label("Select a camera to activate controls");
        // messageLabel.style.color = Color.white;
        // messageLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        // messageLabel.style.fontSize = 16;
        // messageLabel.style.position = Position.Absolute;
        // messageLabel.style.left = 0;
        // messageLabel.style.top = 0;
        // messageLabel.style.right = 0;
        // messageLabel.style.bottom = 0;
        
        // inactiveOverlay.Add(messageLabel);
        // root.Add(inactiveOverlay);
        
        // Pan Controls (custom VisualElements from UXML)
        var panLeftVE = root.Q<VisualElement>("panLeftVE");
        if (panLeftVE != null)
        {
            panLeftVE.RegisterCallback<PointerDownEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Pan Left button pressed, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.PanLeft() ?? Task.CompletedTask);
            });
            
            panLeftVE.RegisterCallback<PointerUpEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Pan Left button released, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Stop() ?? Task.CompletedTask);
            });
            
            panLeftVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Pan Left pointer left, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Stop() ?? Task.CompletedTask);
            });
        }
        else
        {
            Debug.LogError("panLeftVE element not found in UI!");
        }

        var panRightVE = root.Q<VisualElement>("panRightVE");
        if (panRightVE != null)
        {
            panRightVE.RegisterCallback<PointerDownEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Pan Right button pressed, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.PanRight() ?? Task.CompletedTask);
            });
            
            panRightVE.RegisterCallback<PointerUpEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Pan Right button released, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Stop() ?? Task.CompletedTask);
            });
            
            panRightVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Pan Right pointer left, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Stop() ?? Task.CompletedTask);
            });
        }
        else
        {
            Debug.LogError("panRightVE element not found in UI!");
        }

        // Tilt Controls (custom VisualElements from UXML)
        var tiltUpVE = root.Q<VisualElement>("tiltUpVE");
        if (tiltUpVE != null)
        {
            tiltUpVE.RegisterCallback<PointerDownEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Tilt Up button pressed, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.TiltUp() ?? Task.CompletedTask);
            });
            
            tiltUpVE.RegisterCallback<PointerUpEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Tilt Up button released, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Stop() ?? Task.CompletedTask);
            });
            
            tiltUpVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Tilt Up pointer left, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Stop() ?? Task.CompletedTask);
            });
        }
        else
        {
            Debug.LogError("tiltUpVE element not found in UI!");
        }

        var tiltDownVE = root.Q<VisualElement>("tiltDownVE");
        if (tiltDownVE != null)
        {
            tiltDownVE.RegisterCallback<PointerDownEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Tilt Down button pressed, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.TiltDown() ?? Task.CompletedTask);
            });
            
            tiltDownVE.RegisterCallback<PointerUpEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Tilt Down button released, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                await (viscaSender?.Stop() ?? Task.CompletedTask);
            });
            
            tiltDownVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Tilt Down pointer left, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Stop() ?? Task.CompletedTask);
            });
        }
        else
        {
            Debug.LogError("tiltDownVE element not found in UI!");
        }

        // Zoom Controls (custom VisualElements from UXML)
        var zoomInVE = root.Q<VisualElement>("zoomInVE");
        if (zoomInVE != null)
        {
            zoomInVE.RegisterCallback<PointerDownEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Zoom In button pressed, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.ZoomIn() ?? Task.CompletedTask);
            });
            
            zoomInVE.RegisterCallback<PointerUpEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Zoom In button released, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.ZoomStop() ?? Task.CompletedTask);
            });
            
            zoomInVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Zoom In pointer left, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.ZoomStop() ?? Task.CompletedTask);
            });
        }
        else
        {
            Debug.LogError("zoomInVE element not found in UI!");
        }

        var zoomOutVE = root.Q<VisualElement>("zoomOutVE");
        if (zoomOutVE != null)
        {
            zoomOutVE.RegisterCallback<PointerDownEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Zoom Out button pressed, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.ZoomOut() ?? Task.CompletedTask);
            });
            
            zoomOutVE.RegisterCallback<PointerUpEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Zoom Out button released, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.ZoomStop() ?? Task.CompletedTask);
            });
            
            zoomOutVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Zoom Out pointer left, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.ZoomStop() ?? Task.CompletedTask);
            });
        }
        else
        {
            Debug.LogError("zoomOutVE element not found in UI!");
        }

        // Stop and Reset Controls
        var stop = root.Q<Button>("stopButton");
        if (stop != null)
        {
            stop.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Stop button clicked, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Stop() ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("stopButton not found in UI!");
        }

        var reset = root.Q<Button>("resetButton");
        if (reset != null)
        {
            reset.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Reset button clicked, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Home() ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("resetButton not found in UI!");
        }
        
        // Focus Controls
        var focusNearVE = root.Q<VisualElement>("focusNearVE");
        if (focusNearVE != null)
        {
            focusNearVE.RegisterCallback<PointerDownEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Focus Near button pressed");
                if (EnsureViscaSender())
                    await (viscaSender?.FocusNear() ?? Task.CompletedTask);
            });
            
            focusNearVE.RegisterCallback<PointerUpEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Focus Near button released");
                if (EnsureViscaSender())
                    await (viscaSender?.FocusStop() ?? Task.CompletedTask);
            });
            
            focusNearVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Focus Near pointer left");
                if (EnsureViscaSender())
                    await (viscaSender?.FocusStop() ?? Task.CompletedTask);
            });
        }
        else
        {
            Debug.LogError("focusNearVE element not found in UI!");
        }

        var focusFarVE = root.Q<VisualElement>("focusFarVE");
        if (focusFarVE != null)
        {
            focusFarVE.RegisterCallback<PointerDownEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Focus Far button pressed");
                if (EnsureViscaSender())
                    await (viscaSender?.FocusFar() ?? Task.CompletedTask);
            });
            
            focusFarVE.RegisterCallback<PointerUpEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Focus Far button released");
                if (EnsureViscaSender())
                    await (viscaSender?.FocusStop() ?? Task.CompletedTask);
            });
            
            focusFarVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                if (!isInitialized) return;
                
                Debug.Log($"Focus Far pointer left");
                if (EnsureViscaSender())
                    await (viscaSender?.FocusStop() ?? Task.CompletedTask);
            });
        }
        else
        {
            Debug.LogError("focusFarVE element not found in UI!");
        }

        var focusAutoButton = root.Q<Button>("focusAutoButton");
        if (focusAutoButton != null)
        {
            focusAutoButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Auto Focus button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.FocusAuto() ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("focusAutoButton not found in UI!");
        }

        var focusManualButton = root.Q<Button>("focusManualButton");
        if (focusManualButton != null)
        {
            focusManualButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Manual Focus button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.FocusManual() ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("focusManualButton not found in UI!");
        }

        var focusOnePushButton = root.Q<Button>("focusOnePushButton");
        if (focusOnePushButton != null)
        {
            focusOnePushButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"One Push Auto Focus button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.FocusOnePush() ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("focusOnePushButton not found in UI!");
        }
        
        // White Balance Controls
        var wbAutoButton = root.Q<Button>("wbAutoButton");
        if (wbAutoButton != null)
        {
            wbAutoButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Auto White Balance button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.WhiteBalanceAuto() ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("wbAutoButton not found in UI!");
        }

        var wbIndoorButton = root.Q<Button>("wbIndoorButton");
        if (wbIndoorButton != null)
        {
            wbIndoorButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Indoor White Balance button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.WhiteBalanceIndoor() ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("wbIndoorButton not found in UI!");
        }

        var wbOutdoorButton = root.Q<Button>("wbOutdoorButton");
        if (wbOutdoorButton != null)
        {
            wbOutdoorButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Outdoor White Balance button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.WhiteBalanceOutdoor() ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("wbOutdoorButton not found in UI!");
        }

        var wbOnePushButton = root.Q<Button>("wbOnePushButton");
        if (wbOnePushButton != null)
        {
            wbOnePushButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"One Push White Balance button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.WhiteBalanceOnePush() ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("wbOnePushButton not found in UI!");
        }

        var wbATWButton = root.Q<Button>("wbATWButton");
        if (wbATWButton != null)
        {
            wbATWButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"ATW White Balance button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.WhiteBalanceATW() ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("wbATWButton not found in UI!");
        }

        var wbOnePushTriggerButton = root.Q<Button>("wbOnePushTriggerButton");
        if (wbOnePushTriggerButton != null)
        {
            wbOnePushTriggerButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"One Push Trigger White Balance button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.WhiteBalanceOnePushTrigger() ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("wbOnePushTriggerButton not found in UI!");
        }
        
        // Exposure Controls
        var expFullAutoButton = root.Q<Button>("expFullAutoButton");
        if (expFullAutoButton != null)
        {
            expFullAutoButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Full Auto Exposure button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.ExposureFullAuto() ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("expFullAutoButton not found in UI!");
        }

        var expManualButton = root.Q<Button>("expManualButton");
        if (expManualButton != null)
        {
            expManualButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Manual Exposure button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.ExposureManual() ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("expManualButton not found in UI!");
        }

        var expShutterPriorityButton = root.Q<Button>("expShutterPriorityButton");
        if (expShutterPriorityButton != null)
        {
            expShutterPriorityButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Shutter Priority Exposure button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.ExposureShutterPriority() ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("expShutterPriorityButton not found in UI!");
        }

        var expIrisPriorityButton = root.Q<Button>("expIrisPriorityButton");
        if (expIrisPriorityButton != null)
        {
            expIrisPriorityButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Iris Priority Exposure button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.ExposureIrisPriority() ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("expIrisPriorityButton not found in UI!");
        }
        
        // Camera Memory Preset Controls
        
        // Preset 1
        var preset1RecallButton = root.Q<Button>("preset1RecallButton");
        if (preset1RecallButton != null)
        {
            preset1RecallButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Recall Preset 1 button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.PresetRecall(0) ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("preset1RecallButton not found in UI!");
        }
        
        var preset1SetButton = root.Q<Button>("preset1SetButton");
        if (preset1SetButton != null)
        {
            preset1SetButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Set Preset 1 button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.PresetSet(0) ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("preset1SetButton not found in UI!");
        }
        
        var preset1ResetButton = root.Q<Button>("preset1ResetButton");
        if (preset1ResetButton != null)
        {
            preset1ResetButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Reset Preset 1 button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.PresetReset(0) ?? Task.CompletedTask);
            };
        }
        else
        {
            // Debug.LogError("preset1ResetButton not found in UI!");
        }
        
        // Preset 2
        var preset2RecallButton = root.Q<Button>("preset2RecallButton");
        if (preset2RecallButton != null)
        {
            preset2RecallButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Recall Preset 2 button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.PresetRecall(1) ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("preset2RecallButton not found in UI!");
        }
        
        var preset2SetButton = root.Q<Button>("preset2SetButton");
        if (preset2SetButton != null)
        {
            preset2SetButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Set Preset 2 button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.PresetSet(1) ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("preset2SetButton not found in UI!");
        }
        
        var preset2ResetButton = root.Q<Button>("preset2ResetButton");
        if (preset2ResetButton != null)
        {
            preset2ResetButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Reset Preset 2 button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.PresetReset(1) ?? Task.CompletedTask);
            };
        }
        else
        {
            //Debug.LogError("preset2ResetButton not found in UI!");
        }
        
        // Preset 3
        var preset3RecallButton = root.Q<Button>("preset3RecallButton");
        if (preset3RecallButton != null)
        {
            preset3RecallButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Recall Preset 3 button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.PresetRecall(2) ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("preset3RecallButton not found in UI!");
        }
        
        var preset3SetButton = root.Q<Button>("preset3SetButton");
        if (preset3SetButton != null)
        {
            preset3SetButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Set Preset 3 button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.PresetSet(2) ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("preset3SetButton not found in UI!");
        }
        
        var preset3ResetButton = root.Q<Button>("preset3ResetButton");
        if (preset3ResetButton != null)
        {
            preset3ResetButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Reset Preset 3 button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.PresetReset(2) ?? Task.CompletedTask);
            };
        }
        else
        {
            //Debug.LogError("preset3ResetButton not found in UI!");
        }
        
        // Preset 4
        var preset4RecallButton = root.Q<Button>("preset4RecallButton");
        if (preset4RecallButton != null)
        {
            preset4RecallButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Recall Preset 4 button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.PresetRecall(3) ?? Task.CompletedTask);
            };
        }
        else
        {
            //Debug.LogError("preset4RecallButton not found in UI!");
        }
        
        var preset4SetButton = root.Q<Button>("preset4SetButton");
        if (preset4SetButton != null)
        {
            preset4SetButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Set Preset 4 button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.PresetSet(3) ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("preset4SetButton not found in UI!");
        }
        
        var preset4ResetButton = root.Q<Button>("preset4ResetButton");
        if (preset4ResetButton != null)
        {
            preset4ResetButton.clicked += async () => {
                if (!isInitialized) return;
                
                Debug.Log($"Reset Preset 4 button clicked");
                if (EnsureViscaSender())
                    await (viscaSender?.PresetReset(3) ?? Task.CompletedTask);
            };
        }
        else
        {
            //Debug.LogError("preset4ResetButton not found in UI!");
        }
        
        // Diagonal Pan/Tilt Controls
        var panTiltUpLeftVE = root.Q<VisualElement>("panTiltUpLeftVE");
        if (panTiltUpLeftVE != null)
        {
            panTiltUpLeftVE.RegisterCallback<PointerDownEvent>(async evt => {
                if (!isInitialized) return;
                Debug.Log($"Pan Tilt Up-Left button pressed, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.PanTiltUpLeft() ?? Task.CompletedTask);
            });
            panTiltUpLeftVE.RegisterCallback<PointerUpEvent>(async evt => {
                if (!isInitialized) return;
                Debug.Log($"Pan Tilt Up-Left button released, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Stop() ?? Task.CompletedTask);
            });
            panTiltUpLeftVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                if (!isInitialized) return;
                Debug.Log($"Pan Tilt Up-Left pointer left, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Stop() ?? Task.CompletedTask);
            });
        }
        else
        {
            Debug.LogError("panTiltUpLeftVE element not found in UI!");
        }

        var panTiltUpRightVE = root.Q<VisualElement>("panTiltUpRightVE");
        if (panTiltUpRightVE != null)
        {
            panTiltUpRightVE.RegisterCallback<PointerDownEvent>(async evt => {
                if (!isInitialized) return;
                Debug.Log($"Pan Tilt Up-Right button pressed, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.PanTiltUpRight() ?? Task.CompletedTask);
            });
            panTiltUpRightVE.RegisterCallback<PointerUpEvent>(async evt => {
                if (!isInitialized) return;
                Debug.Log($"Pan Tilt Up-Right button released, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Stop() ?? Task.CompletedTask);
            });
            panTiltUpRightVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                if (!isInitialized) return;
                Debug.Log($"Pan Tilt Up-Right pointer left, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Stop() ?? Task.CompletedTask);
            });
        }
        else
        {
            Debug.LogError("panTiltUpRightVE element not found in UI!");
        }

        var panTiltDownLeftVE = root.Q<VisualElement>("panTiltDownLeftVE");
        if (panTiltDownLeftVE != null)
        {
            panTiltDownLeftVE.RegisterCallback<PointerDownEvent>(async evt => {
                if (!isInitialized) return;
                Debug.Log($"Pan Tilt Down-Left button pressed, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.PanTiltDownLeft() ?? Task.CompletedTask);
            });
            panTiltDownLeftVE.RegisterCallback<PointerUpEvent>(async evt => {
                if (!isInitialized) return;
                Debug.Log($"Pan Tilt Down-Left button released, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Stop() ?? Task.CompletedTask);
            });
            panTiltDownLeftVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                if (!isInitialized) return;
                Debug.Log($"Pan Tilt Down-Left pointer left, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Stop() ?? Task.CompletedTask);
            });
        }
        else
        {
            Debug.LogError("panTiltDownLeftVE element not found in UI!");
        }

        var panTiltDownRightVE = root.Q<VisualElement>("panTiltDownRightVE");
        if (panTiltDownRightVE != null)
        {
            panTiltDownRightVE.RegisterCallback<PointerDownEvent>(async evt => {
                if (!isInitialized) return;
                Debug.Log($"Pan Tilt Down-Right button pressed, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.PanTiltDownRight() ?? Task.CompletedTask);
            });
            panTiltDownRightVE.RegisterCallback<PointerUpEvent>(async evt => {
                if (!isInitialized) return;
                Debug.Log($"Pan Tilt Down-Right button released, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Stop() ?? Task.CompletedTask);
            });
            panTiltDownRightVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                if (!isInitialized) return;
                Debug.Log($"Pan Tilt Down-Right pointer left, viscaSender is {(viscaSender != null ? "valid" : "null")}");
                if (EnsureViscaSender())
                    await (viscaSender?.Stop() ?? Task.CompletedTask);
            });
        }
        else
        {
            Debug.LogError("panTiltDownRightVE element not found in UI!");
        }
        
        Debug.Log("ViscaControlPanelController UI setup complete");
    }

    

    private void OnCameraListChanged(CameraInfo camera)
    {
        Debug.Log($"Camera list changed: {(camera != null ? camera.niceName : "null")}");
        // Don't automatically change active camera - wait for user click
    }

    private void OnDisable()
    {
        Debug.Log("ViscaControlPanelController OnDisable called");
        
        // Cleanup sender if it was created
        if (viscaSender != null)
        {
            Debug.Log("Disposing viscaSender");
            viscaSender.Dispose();
            viscaSender = null;
        }
        
        // Unsubscribe from events we subscribed to
        if (cameraRegistry != null)
        {
            Debug.Log("Unsubscribing from camera events");
            // Only unsubscribe from the event we subscribed to in OnEnable
            cameraRegistry.OnCameraSelected -= OnCameraSelected;
        }
        
        // Reset initialization state
        isInitialized = false;
    }

    private void Update()
    {
        // Only check for sender if we're initialized
        if (!isInitialized)
            return;
            
        // Check periodically if we need to recreate the sender
        if (viscaSender == null && activeCamera != null)
        {
            Debug.LogWarning("viscaSender is null in Update, recreating");
            
            // Only create a new sender if we have a valid IP
            if (!string.IsNullOrEmpty(activeCamera.viscaIp))
            {
                viscaSender = new ViscaOverIpSender(activeCamera);
                Debug.Log($"Recreated VISCA sender for camera: {activeCamera.niceName}, IP: {activeCamera.viscaIp}");
            }
            else
            {
                // Try to get IP from NDIViewerApp
                var ndiApp = FindObjectOfType<NDIViewerApp>();
                string currentIp = null;
                
                if (ndiApp != null && !string.IsNullOrEmpty(ndiApp.currentIP))
                {
                    currentIp = ndiApp.currentIP;
                }
                else
                {
                    // Fall back to static property
                    currentIp = NDIViewerApp.ActiveCameraIP;
                    
                    if (currentIp == "Not Set")
                    {
                        currentIp = fallbackIp;
                    }
                }
                
                // Set the IP on the camera
                activeCamera.viscaIp = currentIp;
                
                // Create the sender
                viscaSender = new ViscaOverIpSender(activeCamera);
                Debug.Log($"Created VISCA sender with recovered IP: {currentIp}");
            }
        }
    }
}

public static class TaskExtensions
{
    public static System.Collections.IEnumerator AsCoroutine(this Task task)
    {
        yield return new WaitUntil(() => task.IsCompleted || task.IsFaulted || task.IsCanceled);
        
        if (task.IsFaulted)
        {
            Debug.LogError($"Task failed with exception: {task.Exception}");
        }
    }
}