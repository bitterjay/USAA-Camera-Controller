using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;

public class ViscaControlPanelController : MonoBehaviour
{
    private ViscaOverIpSender viscaSender;
    private CameraInfo activeCamera;
    private CameraRegistry cameraRegistry;
    
    // Default values (for fallback only)
    [SerializeField] private string defaultViscaIp = "192.168.1.104";
    [SerializeField] private int defaultViscaPort = 52381;

    // Add methods to update IP and port from CameraTileView
    public void SetIPAddress(string ipAddress)
    {
        if (!string.IsNullOrEmpty(ipAddress) && activeCamera != null)
        {
            activeCamera.viscaIp = ipAddress;
            UpdateViscaSender();
            
            // Update registry if available
            if (cameraRegistry != null)
            {
                cameraRegistry.UpdateCameraConnection(activeCamera, ipAddress, activeCamera.viscaPort);
            }
        }
    }
    
    public void SetPort(int port)
    {
        if (port > 0 && activeCamera != null)
        {
            activeCamera.viscaPort = port;
            UpdateViscaSender();
            
            // Update registry if available
            if (cameraRegistry != null)
            {
                cameraRegistry.UpdateCameraConnection(activeCamera, activeCamera.viscaIp, port);
            }
        }
    }
    
    private void UpdateViscaSender()
    {
        if (activeCamera == null)
        {
            Debug.LogWarning("Cannot update sender - no active camera");
            return;
        }
        
        if (viscaSender == null)
        {
            // Create a new sender if it doesn't exist
            viscaSender = new ViscaOverIpSender(activeCamera.viscaIp, activeCamera.viscaPort);
        }
        else
        {
            // Update existing sender's connection
            viscaSender.UpdateConnection(activeCamera.viscaIp, activeCamera.viscaPort);
        }
    }

    private void OnEnable()
    {
        // Find the NDIViewerApp and get the CameraRegistry
        var ndiApp = FindObjectOfType<NDIViewerApp>();
        if (ndiApp != null)
        {
            cameraRegistry = ndiApp.GetCameraRegistry();
            
            if (cameraRegistry != null)
            {
                // Subscribe to active camera changes
                cameraRegistry.OnCameraAdded += OnCameraListChanged;
                cameraRegistry.OnCameraRemoved += OnCameraListChanged;
                // Subscribe to camera selection events
                cameraRegistry.OnCameraSelected += OnCameraSelected;
                
                // Set initial active camera if one exists
                if (cameraRegistry.ActiveCamera != null)
                {
                    SetActiveCamera(cameraRegistry.ActiveCamera);
                }
            }
        }
        else
        {
            Debug.LogError("NDIViewerApp not found!");
        }

        SetupUI();
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
        
        // Create a validation helper method for all UI callbacks
        ViscaOverIpSender GetCurrentSender()
        {
            // This ensures we always get the latest reference, not a cached one
            if (viscaSender == null)
            {
                Debug.LogError("ViscaSender is null when trying to use it - recreating");
                
                // If we have an active camera, use its IP
                if (activeCamera != null)
                {
                    // Create new sender
                    viscaSender = new ViscaOverIpSender(activeCamera);
                }
                else
                {
                    // Fallback to default values
                    viscaSender = new ViscaOverIpSender(defaultViscaIp, defaultViscaPort);
                }
            }
            
            return viscaSender;
        }

        // Pan Controls (custom VisualElements from UXML)
        var panLeftVE = root.Q<VisualElement>("panLeftVE");
        if (panLeftVE != null)
        {
            panLeftVE.RegisterCallback<PointerDownEvent>(async evt => {
                var sender = GetCurrentSender();
                await (sender?.PanLeft() ?? Task.CompletedTask);
            });
            
            panLeftVE.RegisterCallback<PointerUpEvent>(async evt => {
                var sender = GetCurrentSender();
                await (sender?.Stop() ?? Task.CompletedTask);
            });
            
            panLeftVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                var sender = GetCurrentSender();
                await (sender?.Stop() ?? Task.CompletedTask);
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
                var sender = GetCurrentSender();
                await (sender?.PanRight() ?? Task.CompletedTask);
            });
            
            panRightVE.RegisterCallback<PointerUpEvent>(async evt => {
                var sender = GetCurrentSender();
                await (sender?.Stop() ?? Task.CompletedTask);
            });
            
            panRightVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                var sender = GetCurrentSender();
                await (sender?.Stop() ?? Task.CompletedTask);
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
                var sender = GetCurrentSender();
                await (sender?.TiltUp() ?? Task.CompletedTask);
            });
            
            tiltUpVE.RegisterCallback<PointerUpEvent>(async evt => {
                var sender = GetCurrentSender();
                await (sender?.Stop() ?? Task.CompletedTask);
            });
            
            tiltUpVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                var sender = GetCurrentSender();
                await (sender?.Stop() ?? Task.CompletedTask);
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
                var sender = GetCurrentSender();
                await (sender?.TiltDown() ?? Task.CompletedTask);
            });
            
            tiltDownVE.RegisterCallback<PointerUpEvent>(async evt => {
                var sender = GetCurrentSender();
                await (sender?.Stop() ?? Task.CompletedTask);
            });
            
            tiltDownVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                var sender = GetCurrentSender();
                await (sender?.Stop() ?? Task.CompletedTask);
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
                var sender = GetCurrentSender();
                await (sender?.ZoomIn() ?? Task.CompletedTask);
            });
            
            zoomInVE.RegisterCallback<PointerUpEvent>(async evt => {
                var sender = GetCurrentSender();
                await (sender?.ZoomStop() ?? Task.CompletedTask);
            });
            
            zoomInVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                var sender = GetCurrentSender();
                await (sender?.ZoomStop() ?? Task.CompletedTask);
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
                var sender = GetCurrentSender();
                await (sender?.ZoomOut() ?? Task.CompletedTask);
            });
            
            zoomOutVE.RegisterCallback<PointerUpEvent>(async evt => {
                var sender = GetCurrentSender();
                await (sender?.ZoomStop() ?? Task.CompletedTask);
            });
            
            zoomOutVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                var sender = GetCurrentSender();
                await (sender?.ZoomStop() ?? Task.CompletedTask);
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
                var sender = GetCurrentSender();
                await (sender?.Stop() ?? Task.CompletedTask);
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
                var sender = GetCurrentSender();
                await (sender?.Home() ?? Task.CompletedTask);
            };
        }
        else
        {
            Debug.LogError("resetButton not found in UI!");
        }
    }

    private void OnCameraListChanged(CameraInfo camera)
    {
        // Always update to the current active camera
        SetActiveCamera(cameraRegistry?.ActiveCamera);
    }

    private void OnCameraSelected(CameraInfo camera)
    {
        // Update the active camera
        SetActiveCamera(camera);
    }

    private void SetActiveCamera(CameraInfo camera)
    {
        if (camera == null)
        {
            Debug.LogWarning("SetActiveCamera called with null camera");
            return;
        }
        
        // Check if we're setting the same camera
        bool isSameCamera = activeCamera == camera;
        if (isSameCamera) return;

        // Set the selected camera as active
        activeCamera = camera;
        
        // Update the VISCA sender with the new camera info
        if (viscaSender != null)
        {
            viscaSender.UpdateConnection(camera.viscaIp, camera.viscaPort);
        }
        else
        {
            // Create a new sender with the camera's IP and port
            viscaSender = new ViscaOverIpSender(camera);
        }
    }

    private void OnDisable()
    {
        if (viscaSender != null)
        {
            viscaSender.Dispose();
            viscaSender = null;
        }
        
        if (cameraRegistry != null)
        {
            cameraRegistry.OnCameraAdded -= OnCameraListChanged;
            cameraRegistry.OnCameraRemoved -= OnCameraListChanged;
            // Unsubscribe from camera selection events
            cameraRegistry.OnCameraSelected -= OnCameraSelected;
        }
    }

    private void Update()
    {
        // Ensure we have a valid VISCA sender
        if (viscaSender == null && activeCamera != null)
        {
            Debug.LogWarning("viscaSender is null in Update() - recreating");
            viscaSender = new ViscaOverIpSender(activeCamera);
        }
        
        // Only validate sender's IP if we have an active camera
        if (viscaSender != null && activeCamera != null)
        {
            // Check if the current viscaSender's IP doesn't match what we expect
            bool ipMismatch = viscaSender.cameraIp != activeCamera.viscaIp;
            bool portMismatch = viscaSender.cameraPort != activeCamera.viscaPort;
            
            if (ipMismatch || portMismatch)
            {
                Debug.LogError($"VISCA SENDER IP/PORT MISMATCH: Sender has {viscaSender.cameraIp}:{viscaSender.cameraPort} but camera has {activeCamera.viscaIp}:{activeCamera.viscaPort}");
                
                // Update the connection instead of recreating
                try 
                {
                    viscaSender.UpdateConnection(activeCamera.viscaIp, activeCamera.viscaPort);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to update VISCA sender in Update, recreating: {ex.Message}");
                    // Fall back to recreation if update fails
                    viscaSender.Dispose();
                    viscaSender = null;
                    viscaSender = new ViscaOverIpSender(activeCamera);
                }
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