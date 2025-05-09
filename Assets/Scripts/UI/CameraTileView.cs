using UnityEngine;
using UnityEngine.UI;
using Klak.Ndi;
using System;

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
    
    /// <summary>
    /// Initializes the camera tile with the given camera info
    /// </summary>
    /// <param name="cameraInfo">The camera to display</param>
    /// <param name="settings">Layout settings</param>
    /// <param name="gearIcon">Gear icon texture</param>
    public void Initialize(CameraInfo cameraInfo, LayoutSettings settings, Texture2D gearIcon = null)
    {
        CameraInfo = cameraInfo;
        layoutSettings = settings;
        gearIconTexture = gearIcon;
        IsActive = cameraInfo.isActive;
        ApplySettings();
        EnsureNoSignalOverlay();
        SetNoSignalOverlay(!cameraInfo.isFeedAvailable);
        // Subscribe to feed status changes
        var registry = FindObjectOfType<NDIViewerApp>()?.GetCameraRegistry();
        if (registry != null)
        {
            registry.OnFeedStatusChanged += OnFeedStatusChanged;
        }
    }
    
    private void OnDestroy()
    {
        var registry = FindObjectOfType<NDIViewerApp>()?.GetCameraRegistry();
        if (registry != null)
        {
            registry.OnFeedStatusChanged -= OnFeedStatusChanged;
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
            borderImage.color = active 
                ? layoutSettings.ActiveTileBorderColor 
                : layoutSettings.TileBorderColor;
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
        
        // Add button component for click handling
        var button = gameObject.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
        button.colors = colors;
        
        // Set click handler
        button.onClick.AddListener(() => OnSelected?.Invoke(this));
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
} 