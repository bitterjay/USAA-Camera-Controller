using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Klak.Ndi;
using System.Linq;
using UnityEngine.EventSystems;

/// <summary>
/// Main application controller for the NDI Viewer
/// </summary>
public class NDIViewerApp : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayoutSettings layoutSettings;
    
    [Header("Resources")]
    [SerializeField] private NdiResources ndiResources;
    
    [Header("Icons")]
    [Tooltip("PNG/Texture for the global settings gear (top right)")]
    [SerializeField] private Texture2D globalGearIconTexture;
    
    [Tooltip("PNG/Texture for each camera's gear button (bottom left)")]
    [SerializeField] private Texture2D cameraGearIconTexture;
    
    [Tooltip("PNG/Texture for close (X) button on panels")]
    [SerializeField] private Texture2D closeIconTexture;
    
    // Core components
    private CameraRegistry cameraRegistry;
    private GridLayoutController gridController;
    
    // UI components
    private Canvas mainCanvas;
    private Transform gridContainer;
    private GridLayoutGroup gridLayout;
    private StatusBarView statusBar;
    private Transform settingsPanel;
    private Transform settingsListContainer;
    
    // Runtime state
    private Dictionary<CameraInfo, CameraTileView> cameraTiles = new Dictionary<CameraInfo, CameraTileView>();
    private bool isInEditMode = false;
    private CameraInfo selectedCamera = null;
    
    private void Start()
    {
        // Create canvas and ensure event system exists
        SetupCanvas();
        
        // Initialize the camera registry
        cameraRegistry = new CameraRegistry(ndiResources);
        cameraRegistry.OnCameraAdded += OnCameraAdded;
        cameraRegistry.OnCameraRemoved += OnCameraRemoved;
        cameraRegistry.OnCamerasReordered += OnCamerasReordered;
        
        // Create the status bar at the bottom
        CreateStatusBar();
        
        // Create grid container and layout controller
        CreateGridContainer();
        gridController = new GridLayoutController(gridLayout, layoutSettings);
        
        // Create settings button
        CreateGlobalSettingsButton();
        
        // Auto-discover cameras
        cameraRegistry.AutoDiscoverCameras();
        
        // Start the refresh loop
        StartCoroutine(RefreshSourcesLoop());
    }
    
    private void Update()
    {
        // Update textures for all camera tiles
        foreach (var tile in cameraTiles.Values)
        {
            tile.UpdateTexture();
        }
        // Check and attempt to reconnect lost NDI feeds
        cameraRegistry.CheckFeeds();
    }
    
    #region Setup Methods
    
    private void SetupCanvas()
    {
        // Create canvas
        var canvasObj = new GameObject("NDICanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        mainCanvas = canvasObj.GetComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Configure canvas scaler
        var scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Ensure event system exists
        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(es);
        }
        
        // Create dark background
        var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(mainCanvas.transform, false);
        var bgRT = bgObj.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        bgObj.GetComponent<Image>().color = Color.black;
    }
    
    private void CreateStatusBar()
    {
        var statusBarObj = new GameObject("StatusBar", typeof(RectTransform));
        statusBarObj.transform.SetParent(mainCanvas.transform, false);
        
        var rt = statusBarObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.sizeDelta = new Vector2(0, 30); // Height of 30 pixels
        rt.anchoredPosition = new Vector2(0, 0);
        
        statusBar = statusBarObj.AddComponent<StatusBarView>();
        statusBar.Initialize();
    }
    
    private void CreateGridContainer()
    {
        // Create grid container
        var gridObj = new GameObject("Grid", typeof(RectTransform));
        gridObj.transform.SetParent(mainCanvas.transform, false);
        gridContainer = gridObj.transform;
        
        // Configure grid rect transform
        var rt = gridObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0, 1);
        rt.offsetMin = new Vector2(layoutSettings.Padding, -Screen.height);
        rt.offsetMax = new Vector2(-layoutSettings.Padding, 0);
        rt.anchoredPosition = new Vector2(0, -layoutSettings.Padding);
        
        // Add grid layout component
        gridLayout = gridObj.AddComponent<GridLayoutGroup>();
        gridLayout.padding = new RectOffset(0, 0, (int)layoutSettings.Padding, (int)layoutSettings.Padding);
        gridLayout.spacing = new Vector2(layoutSettings.Padding, layoutSettings.Padding);
        gridLayout.childAlignment = TextAnchor.UpperLeft;
    }
    
    private void CreateGlobalSettingsButton()
    {
        var btn = UIFactory.CreateIconButton(
            mainCanvas.transform,
            globalGearIconTexture,
            "⚙",
            new Vector2(1, 1),
            TextAnchor.MiddleCenter,
            new Vector2(24, 24)
        );
        
        var rt = btn.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(-layoutSettings.Padding, -layoutSettings.Padding);
        
        btn.onClick.AddListener(ToggleSettingsPanel);
    }
    
    #endregion
    
    #region Camera Management
    
    private void OnCameraAdded(CameraInfo camera)
    {
        // Create tile and add to dictionary
        CreateCameraTile(camera);
        
        // Update the grid layout
        gridController.ConfigureLayout(cameraRegistry.Cameras.Count);
        
        // Update settings panel if open
        RefreshSettingsList();
        
        // If this is the only camera and none active, make it active
        if (cameraRegistry.Cameras.Count == 1 && cameraRegistry.ActiveCamera == null)
        {
            SetActiveCamera(camera);
        }
    }
    
    private void OnCameraRemoved(CameraInfo camera)
    {
        // Remove tile
        if (cameraTiles.TryGetValue(camera, out var tile))
        {
            Destroy(tile.gameObject);
            cameraTiles.Remove(camera);
        }
        
        // Update the grid layout
        gridController.ConfigureLayout(cameraRegistry.Cameras.Count);
        
        // Update settings panel if open
        RefreshSettingsList();
    }
    
    private void OnCamerasReordered()
    {
        // Reorder tiles to match camera order
        for (int i = 0; i < cameraRegistry.Cameras.Count; i++)
        {
            var camera = cameraRegistry.Cameras[i];
            if (cameraTiles.TryGetValue(camera, out var tile))
            {
                tile.transform.SetSiblingIndex(i);
            }
        }
        
        // Update settings panel if open
        RefreshSettingsList();
    }
    
    private void CreateCameraTile(CameraInfo camera)
    {
        // Create tile object
        var tileObj = new GameObject($"Tile_{camera.sourceName}", typeof(RectTransform), typeof(Image));
        tileObj.transform.SetParent(gridContainer, false);
        
        // Add CameraTileView component and initialize
        var tileView = tileObj.AddComponent<CameraTileView>();
        tileView.Initialize(camera, layoutSettings, cameraGearIconTexture);
        tileView.OnSelected += OnCameraTileSelected;
        tileView.OnDisplayNameChanged += OnCameraDisplayNameChanged;
        
        // Update active state
        tileView.SetActive(camera.isActive);
        
        // Store reference to tile object in camera
        camera.tileObject = tileObj;
        
        // Add to dictionary
        cameraTiles[camera] = tileView;
    }
    
    private void OnCameraTileSelected(CameraTileView tile)
    {
        SetActiveCamera(tile.CameraInfo);
    }
    
    private void OnCameraDisplayNameChanged(CameraInfo camera, string newName)
    {
        // Update the name in the registry
        cameraRegistry.UpdateCameraName(camera, newName);
        
        // Update status bar if this is the active camera
        if (camera.isActive)
        {
            statusBar.SetActiveCamera(camera);
        }
        
        // Update settings panel if open
        RefreshSettingsList();
    }
    
    private void SetActiveCamera(CameraInfo camera)
    {
        // Update registry
        cameraRegistry.SetActiveCamera(camera);
        
        // Update tile visuals
        foreach (var kvp in cameraTiles)
        {
            kvp.Value.SetActive(kvp.Key == camera);
        }
        
        // Update status bar
        statusBar.SetActiveCamera(camera);
    }
    
    private IEnumerator RefreshSourcesLoop()
    {
        while (true)
        {
            // Wait a few seconds
            yield return new WaitForSeconds(3f);
            
            // Check for new sources
            bool added = cameraRegistry.RefreshSources();
            
            // Update layout if needed
            if (added)
            {
                gridController.ConfigureLayout(cameraRegistry.Cameras.Count);
            }
        }
    }
    
    #endregion
    
    #region Settings Panel
    
    private void ToggleSettingsPanel()
    {
        if (settingsPanel == null)
        {
            BuildSettingsPanel();
        }
        else
        {
            settingsPanel.gameObject.SetActive(!settingsPanel.gameObject.activeSelf);
        }
    }
    
    private void BuildSettingsPanel()
    {
        // Create panel
        var panelObj = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image));
        panelObj.transform.SetParent(mainCanvas.transform, false);
        var img = panelObj.GetComponent<Image>();
        img.color = layoutSettings.SettingsPanelBackground;
        
        // Configure rect transform
        var rt = panelObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(layoutSettings.SettingsPanelWidth, layoutSettings.SettingsPanelHeight);
        rt.anchoredPosition = new Vector2(-layoutSettings.SettingsPanelWidth/2 - layoutSettings.Padding, 
                                         -layoutSettings.SettingsPanelHeight/2 - layoutSettings.Padding);
        
        // Create scroll container
        var scroll = new GameObject("Scroll", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Mask), typeof(Image));
        scroll.transform.SetParent(panelObj.transform, false);
        var scrt = scroll.GetComponent<RectTransform>();
        scrt.anchorMin = new Vector2(0.5f, 0f);
        scrt.anchorMax = new Vector2(0.5f, 1f);
        scrt.pivot = new Vector2(0.5f, 0.5f);
        
        float scrollWidth = layoutSettings.SettingsItemWidth + 2 * 10f;
        scrt.sizeDelta = new Vector2(scrollWidth, -layoutSettings.SettingsScrollVerticalPadding);
        scrt.anchoredPosition = Vector2.zero;
        
        var vlg = scroll.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 4f;
        vlg.padding = new RectOffset(4, 4, 4, 4);
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;
        
        var scrollImg = scroll.GetComponent<Image>();
        scrollImg.color = layoutSettings.SettingsScrollBackground;
        
        // Store reference to list container
        settingsListContainer = scroll.transform;
        
        // Add close button
        var closeBtn = UIFactory.CreateIconButton(
            panelObj.transform,
            closeIconTexture,
            "X",
            new Vector2(1, 1),
            TextAnchor.MiddleCenter,
            new Vector2(24, 24)
        );
        var cRT = closeBtn.GetComponent<RectTransform>();
        cRT.anchoredPosition = new Vector2(-10, -10);
        closeBtn.onClick.AddListener(ToggleSettingsPanel);
        
        // Store reference to panel
        settingsPanel = panelObj.transform;
        
        // Populate with items
        RefreshSettingsList();
    }
    
    private void RefreshSettingsList()
    {
        if (settingsListContainer == null) return;
        
        // Clear existing items
        foreach (Transform child in settingsListContainer)
            Destroy(child.gameObject);
            
        // If in edit mode, show reordering UI
        if (isInEditMode && selectedCamera != null)
        {
            // Add insertion point at the beginning
            CreateInsertionPoint(0);
            
            // Add items and insertion points
            for (int i = 0; i < cameraRegistry.Cameras.Count; i++)
            {
                var camera = cameraRegistry.Cameras[i];
                
                // Create the item
                var item = CreateSettingsItem(camera);
                
                // Highlight selected camera
                if (camera == selectedCamera)
                {
                    var img = item.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = new Color(0.7f, 0.9f, 0.7f);
                    }
                }
                
                // Add insertion point after this item
                if (i < cameraRegistry.Cameras.Count - 1)
                {
                    CreateInsertionPoint(i + 1);
                }
            }
            
            // Add final insertion point
            CreateInsertionPoint(cameraRegistry.Cameras.Count);
        }
        else
        {
            // Normal mode - just show items
            foreach (var camera in cameraRegistry.Cameras)
            {
                CreateSettingsItem(camera);
            }
        }
    }
    
    private GameObject CreateSettingsItem(CameraInfo camera)
    {
        var item = new GameObject("Item", typeof(RectTransform), typeof(Image));
        item.transform.SetParent(settingsListContainer, false);
        
        var img = item.GetComponent<Image>();
        img.color = layoutSettings.SettingsItemBackground;
        
        var rt = item.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(layoutSettings.SettingsItemWidth, layoutSettings.SettingsItemHeight);
        
        // Add camera name text
        var txt = UIFactory.CreateLabel(
            item.transform,
            camera.niceName,
            TextAnchor.MiddleCenter,
            layoutSettings.SettingsItemTextColor
        );
        
        // Add layout element
        var le = item.AddComponent<LayoutElement>();
        le.minHeight = layoutSettings.SettingsItemHeight;
        le.minWidth = layoutSettings.SettingsItemWidth;
        le.preferredHeight = layoutSettings.SettingsItemHeight;
        le.preferredWidth = layoutSettings.SettingsItemWidth;
        le.flexibleHeight = 0;
        le.flexibleWidth = 0;
        
        // Store camera reference
        var cli = item.AddComponent<CameraListItem>();
        cli.cameraInfo = camera;
        
        // Add button for selection
        var btn = item.AddComponent<Button>();
        btn.onClick.AddListener(() => {
            // Toggle edit mode
            if (isInEditMode && selectedCamera == camera)
            {
                // Exit edit mode
                isInEditMode = false;
                selectedCamera = null;
            }
            else
            {
                // Enter edit mode
                isInEditMode = true;
                selectedCamera = camera;
            }
            
            RefreshSettingsList();
        });
        
        return item;
    }
    
    private void CreateInsertionPoint(int insertIndex)
    {
        var container = new GameObject("InsertPoint", typeof(RectTransform), typeof(LayoutElement));
        container.transform.SetParent(settingsListContainer, false);
        
        var le = container.GetComponent<LayoutElement>();
        le.minHeight = 24;
        le.minWidth = layoutSettings.SettingsItemWidth;
        le.preferredHeight = 24;
        le.preferredWidth = layoutSettings.SettingsItemWidth;
        
        // Create plus button
        var btn = UIFactory.CreateButton(
            container.transform,
            "+",
            new Vector2(24, 24),
            () => {
                // Move the selected camera to this position
                if (selectedCamera != null)
                {
                    cameraRegistry.ReorderCamera(selectedCamera, insertIndex);
                    
                    // Exit edit mode
                    isInEditMode = false;
                    selectedCamera = null;
                    
                    RefreshSettingsList();
                }
            }
        );
        
        // Set color and position
        var img = btn.GetComponent<Image>();
        img.color = new Color(0.2f, 0.7f, 0.2f);
        
        var btnRT = btn.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0.5f);
        btnRT.anchorMax = new Vector2(0.5f, 0.5f);
    }
    
    #endregion
    
    public CameraRegistry GetCameraRegistry()
    {
        return cameraRegistry;
    }
} 