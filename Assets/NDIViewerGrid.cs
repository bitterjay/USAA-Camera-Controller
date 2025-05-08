using UnityEngine;
using UnityEngine.UI;
using Klak.Ndi;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class NDIViewerGrid : MonoBehaviour
{
    [System.Serializable]
    public class NDICamera
    {
        public string sourceName;
        public string title;
        [HideInInspector] public NdiReceiver receiver;
        [HideInInspector] public RawImage rawImage;
        [HideInInspector] public bool feedStarted;
    }

    // Runtime list (was serialized array)
    private readonly System.Collections.Generic.List<NDICamera> cameras = new System.Collections.Generic.List<NDICamera>();

    [Header("Layout Settings")]
    [Tooltip("Padding between cells in pixels.")]
    [SerializeField] private float padding = 10f;

    [Tooltip("Aspect ratio (width / height) of each cell.")]
    [SerializeField] private float aspectRatio = 16f / 9f;

    [Header("Internal (leave empty for auto)")]
    [Tooltip("NdiResources asset used by Klak NDI for GPU conversion. If left null the script will try to locate it automatically.")]
    [SerializeField] private NdiResources ndiResources;

    // ------------------------------------------------------------------
    [Header("Scaling")]
    [Tooltip("Global scale applied to each grid cell (1 = default).")]
    [Range(0.5f, 2f)]
    [SerializeField] private float scaleFactor = 1f;

    [Tooltip("Increment applied when pressing +/- keys.")]
    [SerializeField] private float scaleStep = 0.1f;

    private GridLayoutGroup gridGroup;
    private float baseCellWidth;
    private float baseCellHeight;
    private float _lastScaleFactor;

    // UI references
    private InputField titleInputField;
    private Transform sourceButtonsContainer;

    // Cached builtin font
    private static Font BuiltinFont => _builtinFont ?? (_builtinFont = LoadBuiltinFont());
    private static Font _builtinFont;

    private static Font LoadBuiltinFont()
    {
        Font font = null;
        try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
        catch { /* ignore */ }
        if (font == null)
        {
            try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
            catch { /* ignore */ }
        }
        return font;
    }

    private void Start()
    {
        // Log available NDI sources at start
        var sources = Klak.Ndi.NdiFinder.sourceNames;
        Debug.Log($"Available NDI sources: {string.Join(", ", sources)}");

        // --- Canvas --------------------------------------------------------
        var canvasObj = new GameObject("NDI Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Ensure EventSystem exists
        if (EventSystem.current == null)
        {
            var esObj = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(esObj);
        }

        // Create control panel UI (source + title + add button)
        CreateControlPanel(canvasObj.transform);

        // --- Grid Layout ---------------------------------------------------
        var gridObj = new GameObject("Grid", typeof(RectTransform));
        gridObj.transform.SetParent(canvasObj.transform, false);
        var gridRect = gridObj.GetComponent<RectTransform>();
        // Stretch horizontally across the entire player width while anchored to top-left.
        gridRect.anchorMin = new Vector2(0, 1);
        gridRect.anchorMax = new Vector2(1, 1); // stretch to right edge
        gridRect.pivot = new Vector2(0, 1);
        gridRect.offsetMin = new Vector2(padding, 0);   // left margin
        gridRect.offsetMax = new Vector2(-padding, 0);  // right margin
        gridRect.anchoredPosition = new Vector2(0, -padding);

        gridGroup = gridObj.AddComponent<GridLayoutGroup>();
        gridGroup.childAlignment = TextAnchor.UpperLeft;

        // GridGroup padding/spacing
        gridGroup.padding = new RectOffset(0, 0, (int)padding, (int)padding);
        gridGroup.spacing = new Vector2(padding, padding);

        // Initial layout configuration
        ConfigureGridLayout();

        _lastScaleFactor = scaleFactor;
        ApplyScale();

        // --- Create NDI Receivers & UI ------------------------------------
        for (int i = 0; i < cameras.Count; i++)
        {
            var cam = cameras[i];

            // Create NDI Receiver GameObject
            var ndiObj = new GameObject($"NDI_Receiver_{i}");
            cam.receiver = ndiObj.AddComponent<NdiReceiver>();
            cam.receiver.ndiName = cam.sourceName;

            Debug.Log($"Connecting to NDI source: {cam.sourceName}");

            // Inject resources if available/needed
            if (ndiResources == null)
            {
                // Try to find an existing instance (works in both editor & player).
                var found = Resources.FindObjectsOfTypeAll<NdiResources>();
                if (found != null && found.Length > 0) ndiResources = found[0];

#if UNITY_EDITOR
                // As a fallback in Editor, search asset database
                if (ndiResources == null)
                {
                    var guids = UnityEditor.AssetDatabase.FindAssets("t:Klak.Ndi.NdiResources");
                    if (guids != null && guids.Length > 0)
                    {
                        var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                        ndiResources = UnityEditor.AssetDatabase.LoadAssetAtPath<NdiResources>(path);
                    }
                }
#endif
            }
            if (ndiResources != null)
            {
                cam.receiver.SetResources(ndiResources);
            }

            // Create RawImage UI under the grid
            var imageObj = new GameObject($"NDI_Panel_{i}", typeof(RectTransform), typeof(RawImage));
            imageObj.transform.SetParent(gridObj.transform, false);
            var rawImage = imageObj.GetComponent<RawImage>();
            rawImage.texture = cam.receiver.texture;
            rawImage.raycastTarget = false; // not interactive for now

            cam.rawImage = rawImage;
        }
    }

    private void Update()
    {
        // --- Handle grid scaling --------------------------------------
        bool scaleChanged = false;
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus))
        {
            scaleFactor = Mathf.Min(scaleFactor + scaleStep, 2f);
            scaleChanged = true;
        }
        else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.Underscore))
        {
            scaleFactor = Mathf.Max(scaleFactor - scaleStep, 0.5f);
            scaleChanged = true;
        }

        // Also detect changes made through the Inspector during play mode
        if (!scaleChanged && !Mathf.Approximately(scaleFactor, _lastScaleFactor))
        {
            scaleChanged = true;
        }

        if (scaleChanged)
        {
            ApplyScale();
            _lastScaleFactor = scaleFactor;
        }

        if (cameras.Count == 0) return;
        foreach (var cam in cameras)
        {
            if (cam.rawImage == null || cam.receiver == null) continue;
            var tex = cam.receiver.texture;
            if (tex != null && cam.rawImage.texture != tex)
            {
                cam.rawImage.texture = tex;

                if (!cam.feedStarted)
                {
                    cam.feedStarted = true;
                    Debug.Log($"NDI feed detected: {cam.sourceName}");
                }
            }
        }
    }

    // Recomputes cell size based on current scale factor while respecting bounds.
    private void ApplyScale()
    {
        if (gridGroup == null) return;

        float cw = baseCellWidth * scaleFactor;
        float ch = baseCellHeight * scaleFactor;

        // Clamp so a single cell never exceeds half of the window width.
        float maxAllowedWidth = (Screen.width - 2 * padding) / 2f;
        if (cw > maxAllowedWidth)
        {
            float limit = maxAllowedWidth / baseCellWidth;
            cw = baseCellWidth * limit;
            ch = baseCellHeight * limit;
        }

        gridGroup.cellSize = new Vector2(cw, ch);
        _lastScaleFactor = scaleFactor;
    }

    // Coroutine to check if a receiver could create an internal Recv object
    private System.Collections.IEnumerator CheckReceiverReady(NDICamera cam, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (cam.receiver == null) yield break;

        var recv = cam.receiver.internalRecvObject;
        if (recv == null)
        {
            Debug.LogWarning($"[NDIViewerGrid] No NDI receiver created for '{cam.sourceName}'. Make sure the source name matches an active NDI stream on the network.");
        }
    }

    private void CreateControlPanel(Transform parent)
    {
        var panelObj = new GameObject("ControlPanel", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        var rt = panelObj.GetComponent<RectTransform>();
        panelObj.transform.SetParent(parent, false);

        // Anchor top-left similar to grid offset by padding
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(padding, -padding);

        // Visual
        var img = panelObj.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.6f);

        var hlg = panelObj.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 5f;
        hlg.padding = new RectOffset(5, 5, 5, 5);
        hlg.childAlignment = TextAnchor.MiddleLeft;

        var fitter = panelObj.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Title InputField
        titleInputField = CreateInputField(panelObj.transform, "Title", "Camera Title");

        // Container for source buttons
        var listObj = new GameObject("SourcesList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        listObj.transform.SetParent(panelObj.transform, false);
        var vlg = listObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 2f;
        vlg.childAlignment = TextAnchor.UpperLeft;
        sourceButtonsContainer = listObj.transform;

        // Initial build and refresh coroutine
        RefreshSourceButtons();
        StartCoroutine(RefreshSourcesLoop());
    }

    private InputField CreateInputField(Transform parent, string name, string placeholderText)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
        obj.transform.SetParent(parent, false);
        var image = obj.GetComponent<Image>();
        image.color = new Color(1, 1, 1, 0.1f);

        var input = obj.GetComponent<InputField>();

        // Text component
        var textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(obj.transform, false);
        var text = textObj.AddComponent<Text>();
        text.font = BuiltinFont;
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = Color.white;
        input.textComponent = text;

        // Placeholder
        var placeholderObj = new GameObject("Placeholder", typeof(RectTransform));
        placeholderObj.transform.SetParent(obj.transform, false);
        var placeholder = placeholderObj.AddComponent<Text>();
        placeholder.font = BuiltinFont;
        placeholder.fontSize = 14;
        placeholder.text = placeholderText;
        placeholder.fontStyle = FontStyle.Italic;
        placeholder.color = new Color(1,1,1,0.5f);
        input.placeholder = placeholder;

        // Size
        var rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(140, 28);

        var le = obj.AddComponent<LayoutElement>();
        le.preferredWidth = 140;
        le.preferredHeight = 28;

        text.fontSize = 14;

        return input;
    }

    private Button CreateButton(Transform parent, string label)
    {
        var obj = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        var image = obj.GetComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 0.2f, 1);

        var btn = obj.GetComponent<Button>();

        var txtObj = new GameObject("Text", typeof(RectTransform));
        txtObj.transform.SetParent(obj.transform, false);
        var txt = txtObj.AddComponent<Text>();
        txt.font = BuiltinFont;
        txt.text = label;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        var rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(60, 24);

        return btn;
    }

    private void AddCamera(string sourceName, string title)
    {
        // Check duplicates
        foreach (var c in cameras)
            if (c.sourceName == sourceName)
            {
                Debug.LogWarning($"Camera '{sourceName}' already added.");
                return;
            }

        var cam = new NDICamera { sourceName = sourceName, title = title };
        cameras.Add(cam);

        // Rebuild grid layout sizes based on new count
        ConfigureGridLayout();

        // Instantiate receiver & UI
        CreateCameraTile(cam);
    }

    private void ConfigureGridLayout()
    {
        int total = cameras.Count;
        int columns = Mathf.Min(3, total);
        if (columns == 0) columns = 1;
        int rows = Mathf.CeilToInt(total / (float)columns);
        rows = Mathf.Min(rows, 2);

        float gameWidth = Screen.width;
        float usableWidth = gameWidth - 2 * padding;
        float totalSpacing = (columns - 1) * padding;
        float cellWidth = (usableWidth - totalSpacing) / columns;
        float maxAllowed = (gameWidth - 2 * padding) / 2f;
        cellWidth = Mathf.Min(cellWidth, maxAllowed);
        float cellHeight = cellWidth / aspectRatio;

        gridGroup.cellSize = new Vector2(cellWidth, cellHeight);

        baseCellWidth = cellWidth;
        baseCellHeight = cellHeight;

        gridGroup.constraintCount = columns;
    }

    private void CreateCameraTile(NDICamera cam)
    {
        // Create NDI Receiver
        var ndiObj = new GameObject($"NDI_Receiver_{cam.sourceName}");
        cam.receiver = ndiObj.AddComponent<NdiReceiver>();
        cam.receiver.ndiName = cam.sourceName;

        if (ndiResources == null)
        {
            var found = Resources.FindObjectsOfTypeAll<NdiResources>();
            if (found != null && found.Length > 0) ndiResources = found[0];
#if UNITY_EDITOR
            if (ndiResources == null)
            {
                var guids = AssetDatabase.FindAssets("t:Klak.Ndi.NdiResources");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    ndiResources = AssetDatabase.LoadAssetAtPath<NdiResources>(path);
                }
            }
#endif
        }
        if (ndiResources != null) cam.receiver.SetResources(ndiResources);

        // RawImage UI
        var panelObj = new GameObject($"NDI_Panel_{cam.sourceName}", typeof(RectTransform), typeof(RawImage));
        panelObj.transform.SetParent(gridGroup.transform, false);
        var image = panelObj.GetComponent<RawImage>();
        image.texture = cam.receiver.texture;
        image.raycastTarget = false;
        cam.rawImage = image;

        // Title overlay
        if (!string.IsNullOrEmpty(cam.title))
        {
            var titleObj = new GameObject("Title", typeof(RectTransform));
            titleObj.transform.SetParent(panelObj.transform, false);
            var titleText = titleObj.AddComponent<Text>();
            titleText.font = BuiltinFont;
            titleText.text = cam.title;
            titleText.alignment = TextAnchor.UpperLeft;
            titleText.color = Color.white;
            var titlert = titleObj.GetComponent<RectTransform>();
            titlert.anchorMin = new Vector2(0, 1);
            titlert.anchorMax = new Vector2(1, 1);
            titlert.pivot = new Vector2(0, 1);
            titlert.offsetMin = new Vector2(4, -20);
            titlert.offsetMax = new Vector2(-4, 0);
        }

        Debug.Log($"Added NDI camera '{cam.sourceName}' with title '{cam.title}'");

        ConfigureGridLayout();
    }

    private IEnumerator RefreshSourcesLoop()
    {
        while (true)
        {
            RefreshSourceButtons();
            yield return new WaitForSeconds(5f);
        }
    }

    private void RefreshSourceButtons()
    {
        // Clear existing children
        foreach (Transform child in sourceButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var source in Klak.Ndi.NdiFinder.sourceNames)
        {
            var btn = CreateButton(sourceButtonsContainer, source);
            string captured = source; // capture local
            btn.onClick.AddListener(() =>
            {
                var title = titleInputField != null ? titleInputField.text.Trim() : string.Empty;
                AddCamera(captured, title);
            });
        }
    }
} 