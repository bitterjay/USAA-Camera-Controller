using UnityEngine;
using UnityEngine.UI;
using Klak.Ndi;
using System;
using System.Linq; // Add LINQ namespace for IndexOf extension method

namespace PTZ.UI
{
    /// <summary>
    /// Visual representation of a camera tile in the grid
    /// </summary>
    public class CameraTileView : MonoBehaviour
    {
        // Camera data
        public CameraInfo CameraInfo { get; private set; }
        
        // State
        public bool IsActive { get; private set; }
        
        // UI Components
        private RawImage videoDisplay;
        private Image borderImage;
        private Text displayNameText;
        private Text sourceNameText;
        private Button settingsButton; 
        
        // Settings reference
        private LayoutSettings layoutSettings;
        
        // Icon textures
        private Texture2D gearIconTexture;
        
        // Events
        public event Action<CameraTileView> OnSelected;
        public event Action<CameraInfo, string> OnDisplayNameChanged;
        
        private GameObject noSignalOverlay;
        private bool overlayInitialized = false;
        
        // Reference to the camera registry
        private CameraRegistry cameraRegistry;
        
        private NDIViewerApp ndiApp;
        
        // Remove the field initializer that uses FindObjectOfType
        public string ndiAppIp = "";
        
        /// <summary>
        /// Initializes the camera tile with the given camera info
        /// </summary>
        /// <param name="cameraInfo">Camera information</param>
        /// <param name="settings">Layout settings</param>
        /// <param name="gearIcon">Optional gear icon texture</param>
        public void Initialize(CameraInfo cameraInfo, LayoutSettings settings, Texture2D gearIcon = null)
        {
            CameraInfo = cameraInfo;
            layoutSettings = settings;
            gearIconTexture = gearIcon;
            IsActive = cameraInfo.isActive;
            
            // Create visual elements
            CreateVisualElements();
            
            // Initialize overlay
            EnsureNoSignalOverlay();
            SetNoSignalOverlay(!cameraInfo.isFeedAvailable);
            
            // Subscribe to feed status changes
            if (CameraInfo != null)
            {
                var app = FindObjectOfType<NDIViewerApp>();
                if (app != null)
                {
                    ndiApp = app;
                    ndiAppIp = app.currentIP;
                    
                    // Get the camera registry
                    cameraRegistry = app.GetCameraRegistry();
                    if (cameraRegistry != null)
                    {
                        cameraRegistry.OnFeedStatusChanged += OnFeedStatusChanged;
                    }
                }
            }
            
            // Apply settings after everything is initialized
            ApplySettings();
        }
        
        private void OnDestroy()
        {
            if (cameraRegistry != null)
            {
                cameraRegistry.OnFeedStatusChanged -= OnFeedStatusChanged;
            }
        }
        
        private void OnFeedStatusChanged(CameraInfo cam, bool available)
        {
            if (cam == CameraInfo)
            {
                SetNoSignalOverlay(!available);
            }
        }
        
        private void EnsureNoSignalOverlay()
        {
            if (overlayInitialized) return;
            overlayInitialized = true;
            noSignalOverlay = new GameObject("NoSignalOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            noSignalOverlay.transform.SetParent(transform, false);
            var img = noSignalOverlay.GetComponent<UnityEngine.UI.Image>();
            img.color = new Color(0, 0, 0, 0.7f);
            var rt = noSignalOverlay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            // Add label
            var labelObj = new GameObject("NoSignalLabel", typeof(RectTransform));
            labelObj.transform.SetParent(noSignalOverlay.transform, false);
            var label = labelObj.AddComponent<UnityEngine.UI.Text>();
            label.text = "NO SIGNAL";
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = 32;
            label.color = Color.white;
            label.font = UIFactory.BuiltinFont;
            var labelRT = labelObj.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;
            noSignalOverlay.SetActive(false);
        }
        
        private void SetNoSignalOverlay(bool show)
        {
            if (noSignalOverlay != null)
                noSignalOverlay.SetActive(show);
        }
        
        /// <summary>
        /// Sets this camera as active/inactive
        /// </summary>
        /// <param name="active">Whether this tile should be active</param>
        public void SetActive(bool active)
        {
            if (IsActive == active) return;
            
            IsActive = active;
            CameraInfo.isActive = active;

            // Update border color based on active state
            if (borderImage != null)
            {
                // Set border color
                borderImage.color = active 
                    ? layoutSettings.ActiveTileBorderColor 
                    : layoutSettings.TileBorderColor;
                // Set background color (use a separate image or overlay if needed)
                borderImage.color = active 
                    ? layoutSettings.ActiveTileBackgroundColor 
                    : layoutSettings.TileBackgroundColor;
            }
        }
        
        /// <summary>
        /// Updates the textures from the NDI receiver
        /// </summary>
        public void UpdateTexture()
        {
            if (CameraInfo == null)
            {
                Debug.LogWarning("UpdateTexture: CameraInfo is null");
                return;
            }
            
            if (CameraInfo.receiver == null)
            {
                Debug.LogWarning($"UpdateTexture: Receiver is null for camera {CameraInfo.niceName}");
                return;
            }
            
            if (videoDisplay == null)
            {
                Debug.LogWarning($"UpdateTexture: VideoDisplay is null for camera {CameraInfo.niceName}");
                return;
            }
            
            var tex = CameraInfo.receiver.texture;
            if (tex == null)
            {
                Debug.LogWarning($"UpdateTexture: Texture is null for camera {CameraInfo.niceName}");
                return;
            }
            
            if (videoDisplay.texture != tex)
            {
                videoDisplay.texture = tex;
                Debug.Log($"Updated texture for camera {CameraInfo.niceName}");
                
            }
        }
        
        /// <summary>
        /// Creates the visual elements of the tile
        /// </summary>
        private void CreateVisualElements()
        {
            // This is the border
            borderImage = GetComponent<Image>();
            if (borderImage == null)
                borderImage = gameObject.AddComponent<Image>();
            
            // Inner container for video
            var innerContainer = new GameObject("VideoContainer", typeof(RectTransform), typeof(RawImage));
            innerContainer.transform.SetParent(transform, false);
            var innerRT = innerContainer.GetComponent<RectTransform>();
            innerRT.anchorMin = Vector2.zero;
            innerRT.anchorMax = Vector2.one;
            innerRT.offsetMin = new Vector2(layoutSettings.TileBorderWidth, layoutSettings.TileBorderWidth);
            innerRT.offsetMax = new Vector2(-layoutSettings.TileBorderWidth, -layoutSettings.TileBorderWidth);
            
            // Video display
            videoDisplay = innerContainer.GetComponent<RawImage>();
            videoDisplay.raycastTarget = false;
            
            // Display name text (nice name)
            displayNameText = UIFactory.CreateLabel(
                transform, 
                CameraInfo.niceName,
                TextAnchor.UpperLeft,
                Color.white
            );
            var displayNameRT = displayNameText.GetComponent<RectTransform>();
            displayNameRT.offsetMin = new Vector2(4, -20);
            displayNameRT.offsetMax = new Vector2(-4, 0);
            
            // NDI source name text (smaller)
            sourceNameText = UIFactory.CreateLabel(
                transform, 
                "NDI: " + CameraInfo.sourceName,
                TextAnchor.UpperLeft,
                new Color(0.8f, 0.8f, 0.8f)
            );
            sourceNameText.fontSize = 10;
            var sourceNameRT = sourceNameText.GetComponent<RectTransform>();
            sourceNameRT.offsetMin = new Vector2(4, -40);
            sourceNameRT.offsetMax = new Vector2(-4, -20);
            
            // Add settings button (gear icon) in bottom-left corner
            AddSettingsButton();
            
            // Initialize our click handler
            InitializeClickHandler();
        }
        
        private void AddSettingsButton()
        {
            // Create a button in the bottom-left corner
            var btnObj = new GameObject("SettingsButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(transform, false);
            
            // Position it
            var btnRT = btnObj.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0, 0);
            btnRT.anchorMax = new Vector2(0, 0);
            btnRT.pivot = new Vector2(0.5f, 0.5f);
            btnRT.sizeDelta = new Vector2(24, 24);
            btnRT.anchoredPosition = new Vector2(15, 15);
            
            // Set appearance
            var btnImg = btnObj.GetComponent<Image>();
            btnImg.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
            
            if (gearIconTexture != null)
            {
                // Use the provided gear icon texture
                btnImg.sprite = Sprite.Create(
                    gearIconTexture, 
                    new Rect(0, 0, gearIconTexture.width, gearIconTexture.height), 
                    new Vector2(0.5f, 0.5f)
                );
            }
            else
            {
                // Fallback to text if no texture
                var txtObj = new GameObject("Icon", typeof(RectTransform));
                txtObj.transform.SetParent(btnObj.transform, false);
                var txt = txtObj.AddComponent<Text>();
                txt.font = UIFactory.BuiltinFont;
                txt.text = "⚙";
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = Color.white;
                txt.fontSize = 16;
                txt.raycastTarget = false;
                
                // Set up text rect transform
                var textRT = txtObj.GetComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.offsetMin = Vector2.zero;
                textRT.offsetMax = Vector2.zero;
            }
            
            // Add button component
            settingsButton = btnObj.GetComponent<Button>();
            settingsButton.onClick.AddListener(ShowSettingsPopup);
        }
        
        private void ShowSettingsPopup()
        {
            // Create popup
            var popup = CameraSettingsPopup.Create(transform, CameraInfo, layoutSettings);
            
            // Listen for name changes
            popup.OnSaveChanges += OnSettingsPopupSaved;
        }
        
        private void OnSettingsPopupSaved(string newName)
        {
            // Update name
            CameraInfo.niceName = newName;
            
            // Update UI
            if (displayNameText != null)
            {
                displayNameText.text = newName;
            }
            
            // Notify listeners
            OnDisplayNameChanged?.Invoke(CameraInfo, newName);
        }
        
        /// <summary>
        /// Updates the visual elements based on the current settings
        /// </summary>
        private void ApplySettings()
        {
            CreateVisualElements();
            
            // Set border color based on active state
            borderImage.color = IsActive 
                ? layoutSettings.ActiveTileBorderColor 
                : layoutSettings.TileBorderColor;
            
            // Update the video texture
            UpdateTexture();
        }
        
        /// <summary>
        /// Updates the display name text
        /// </summary>
        /// <param name="newName">The new display name</param>
        public void UpdateDisplayName(string newName)
        {
            if (displayNameText != null)
            {
                displayNameText.text = newName;
            }
        }


        private void InitializeClickHandler()
        {
            // Add button component for click handling, but only if not already present
            var button = gameObject.GetComponent<Button>();
            if (button == null)
                button = gameObject.AddComponent<Button>();

            if (button != null)
            {
                // Clear any existing listeners to avoid duplicates
                button.onClick.RemoveAllListeners();
                
                // Add our click handler
                button.onClick.AddListener(() => {
                    Debug.Log($"⭐⭐⭐ CAMERA TILE CLICKED: {CameraInfo.niceName} ⭐⭐⭐");
                    
                    // Extract IP address from camera name
                    string extractedIp = null;
                    if (!string.IsNullOrEmpty(CameraInfo.niceName))
                    {
                        // Look for an IP address pattern in the name
                        var ipPattern = @"\b(?:\d{1,3}\.){3}\d{1,3}\b";
                        var match = System.Text.RegularExpressions.Regex.Match(CameraInfo.niceName, ipPattern);
                        if (match.Success)
                        {
                            extractedIp = match.Value;
                            Debug.Log($"Extracted IP {extractedIp} from camera name: {CameraInfo.niceName}");
                        }
                    }

                    // Update NDIViewerApp with the extracted IP
                    if (ndiApp != null && !string.IsNullOrEmpty(extractedIp))
                    {
                        ndiApp.currentIP = extractedIp;
                        Debug.Log($"Updated NDIViewerApp.currentIP to: {extractedIp}");
                    }
                    
                    // Set this camera as active in the registry
                    if (cameraRegistry != null)
                    {
                        cameraRegistry.SetActiveCamera(CameraInfo);
                    }
                    
                    // Always call the VISCA controller after setting active
                    var viscaController = FindObjectOfType<ViscaControlPanelController>();
                    if (viscaController != null)
                    {
                        viscaController.OnCameraSelected(CameraInfo);
                    }
                    
                    // Call any additional listeners
                    OnSelected?.Invoke(this);
                });
            }
        }

        private void CycleCamera(int direction)
        {
            var ndiApp = FindObjectOfType<NDIViewerApp>();
            if (ndiApp != null)
                cameraRegistry = ndiApp.GetCameraRegistry();

            if (cameraRegistry == null || cameraRegistry.Cameras.Count == 0)
                return;

            var cameras = cameraRegistry.Cameras.ToList();
            int count = cameras.Count;
            int currentIndex = cameras.IndexOf(cameraRegistry.ActiveCamera);

            // If no camera is active, select the first one
            CameraInfo selectedCamera;
            if (currentIndex < 0)
            {
                selectedCamera = cameras[0];
                ndiApp?.SetActiveCamera(selectedCamera);
                Debug.Log($"No camera was active. Selected first camera: {selectedCamera.niceName}");
            }
            else
            {
                int newIndex = (currentIndex + direction + count) % count;
                selectedCamera = cameras[newIndex];
                ndiApp?.SetActiveCamera(selectedCamera);
                Debug.Log($"Switched to camera: {selectedCamera.niceName}");
            }

                ndiApp.currentIP = selectedCamera.viscaIp;
                Debug.Log($"NDIViewerApp currentIP set to: {ndiApp.currentIP}");

            // Notify the VISCA controller so the sender is created
            var viscaController = FindObjectOfType<ViscaControlPanelController>();
           
                viscaController.OnCameraSelected(selectedCamera);
                Debug.Log("ViscaControlPanelController.OnCameraSelected invoked from gamepad selection.");
        }

        // Add a method to get the current preset snapshots for this camera
        public Texture2D[] GetPresetSnapshots()
        {
            var arr = new Texture2D[4];
            for (int i = 0; i < 4; i++)
            {
                arr[i] = PresetSnapshotManager.GetSnapshot(CameraInfo, i);
            }
            return arr;
        }
    }
} 