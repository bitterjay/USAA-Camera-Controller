using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Klak.Ndi;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NDIViewerManager : MonoBehaviour
{
    [System.Serializable]
    public class CameraInfo
    {
        public string sourceName;
        public string niceName;
        public NdiReceiver receiver;
        public RawImage rawImage;
        public GameObject tileObject;
    }

    [Header("Layout")]
    [SerializeField] private float padding = 10f;
    [SerializeField] private float aspectRatio = 16f / 9f;

    [Header("Resources (optional)")]
    [SerializeField] private NdiResources ndiResources;

    [Header("Icons")]
    [Tooltip("PNG/Texture for the global settings gear (top right). Optional.")]
    [SerializeField] private Texture2D globalGearIconTexture;

    [Tooltip("PNG/Texture for each camera\'s gear button (bottom left). Optional.")]
    [SerializeField] private Texture2D cameraGearIconTexture;

    [Tooltip("PNG/Texture for close (X) button on panels. Optional.")]
    [SerializeField] private Texture2D closeIconTexture;

    [Header("Tile Appearance")]
    [SerializeField] private Color tileBorderColor = Color.white;
    [SerializeField] private float tileBorderWidth = 1f;

    private readonly List<CameraInfo> cameras = new List<CameraInfo>();

    private Canvas _canvas;
    private GridLayoutGroup _gridGroup;
    private float baseCellWidth;
    private float baseCellHeight;

    // UI refs
    private Transform settingsPanel; // global reorder panel
    private Transform settingsListContainer;

    private static Font BuiltinFont;

    private void Awake()
    {
        if (BuiltinFont == null)
        {
            try { BuiltinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch { BuiltinFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
        }
    }

    private void Start()
    {
        SetupCanvas();
        AutoLoadCameras();
        // Start polling for new sources every few seconds
        StartCoroutine(SourceRefreshLoop());
        CreateGlobalSettingsButton();
        ConfigureGridLayout();
    }

    private void SetupCanvas()
    {
        var canvasObj = new GameObject("NDI Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        _canvas = canvasObj.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // EventSystem guarantee
        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(es);
        }

        // Background
        var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(canvasObj.transform, false);
        var bgRT = bgObj.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        bgObj.GetComponent<Image>().color = Color.black;

        // Grid
        var gridObj = new GameObject("Grid", typeof(RectTransform));
        gridObj.transform.SetParent(canvasObj.transform, false);
        var rt = gridObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0, 1);
        rt.offsetMin = new Vector2(padding, -Screen.height);
        rt.offsetMax = new Vector2(-padding, 0);
        rt.anchoredPosition = new Vector2(0, -padding);

        _gridGroup = gridObj.AddComponent<GridLayoutGroup>();
        _gridGroup.padding = new RectOffset(0, 0, (int)padding, (int)padding);
        _gridGroup.spacing = new Vector2(padding, padding);
        _gridGroup.childAlignment = TextAnchor.UpperLeft;
    }

    private void AutoLoadCameras()
    {
        cameras.Clear();
        foreach (var src in NdiFinder.sourceNames)
        {
            AddCameraInternal(src, src); // niceName defaults to src
        }
    }

    private void AddCameraInternal(string sourceName, string niceName)
    {
        var info = new CameraInfo { sourceName = sourceName, niceName = niceName };
        cameras.Add(info);
        CreateTile(info);
        RefreshGlobalSettingsList();
    }

    private void CreateTile(CameraInfo cam)
    {
        // Receiver
        var recvObj = new GameObject("Receiver_" + cam.sourceName);
        cam.receiver = recvObj.AddComponent<NdiReceiver>();
        cam.receiver.ndiName = cam.sourceName;
        if (ndiResources == null)
        {
            var found = Resources.FindObjectsOfTypeAll<NdiResources>();
            if (found.Length > 0) ndiResources = found[0];
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

        // Container acting as border background
        var container = new GameObject("TileBorder_" + cam.sourceName, typeof(RectTransform), typeof(Image));
        container.transform.SetParent(_gridGroup.transform, false);
        var borderImage = container.GetComponent<Image>();
        borderImage.color = tileBorderColor;

        // Inner RawImage for NDI texture
        var panelObj = new GameObject("Tile_" + cam.sourceName, typeof(RectTransform), typeof(RawImage));
        panelObj.transform.SetParent(container.transform, false);

        var innerRT = panelObj.GetComponent<RectTransform>();
        innerRT.anchorMin = Vector2.zero;
        innerRT.anchorMax = Vector2.one;
        innerRT.offsetMin = new Vector2(tileBorderWidth, tileBorderWidth);
        innerRT.offsetMax = new Vector2(-tileBorderWidth, -tileBorderWidth);

        var rawImage = panelObj.GetComponent<RawImage>();
        rawImage.texture = cam.receiver.texture;
        rawImage.raycastTarget = false;
        rawImage.color = Color.white;

        cam.rawImage = rawImage;
        cam.tileObject = container;

        // Title text (niceName)
        var titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(container.transform, false);
        var titleText = titleObj.AddComponent<Text>();
        titleText.font = BuiltinFont;
        titleText.text = cam.niceName;
        titleText.alignment = TextAnchor.UpperLeft;
        titleText.color = Color.white;
        var titleRT = titleObj.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0, 1);
        titleRT.offsetMin = new Vector2(4, -20);
        titleRT.offsetMax = new Vector2(-4, 0);

        // Gear button bottom-left
        var gearBtn = CreateIconButton(container.transform, cameraGearIconTexture, "⚙", new Vector2(0, 0), TextAnchor.MiddleCenter);
        gearBtn.onClick.AddListener(() => { ShowTileSettings(cam, titleText); });

        // Position inside tile with padding
        var gRT = gearBtn.GetComponent<RectTransform>();
        gRT.anchoredPosition = new Vector2(12, 12);

        ConfigureGridLayout();
    }

    private Button CreateIconButton(Transform parent, Texture2D iconTexture, string label, Vector2 anchorPivot, TextAnchor textAnchor)
    {
        var obj = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorPivot;
        rt.anchorMax = anchorPivot;
        rt.pivot = anchorPivot;
        rt.sizeDelta = new Vector2(24, 24);
        var img = obj.GetComponent<Image>();
        img.color = Color.white;

        if (iconTexture != null)
        {
            var sprite = Sprite.Create(iconTexture, new Rect(0,0,iconTexture.width, iconTexture.height), new Vector2(0.5f,0.5f));
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
        }
        var btn = obj.GetComponent<Button>();

        if (iconTexture == null)
        {
            var txtObj = new GameObject("T", typeof(RectTransform));
            txtObj.transform.SetParent(obj.transform, false);
            var txt = txtObj.AddComponent<Text>();
            txt.font = BuiltinFont;
            txt.text = label;
            txt.alignment = textAnchor;
            txt.color = Color.black;
            txt.fontSize = 16;
            txt.raycastTarget = false;
        }
        return btn;
    }

    // --- Tile settings popup ------------------------------------------------
    private void ShowTileSettings(CameraInfo cam, Text titleText)
    {
        var popup = new GameObject("TileSettings", typeof(RectTransform), typeof(Image));
        popup.transform.SetParent(cam.tileObject.transform, false);
        var img = popup.GetComponent<Image>();
        img.color = new Color(0,0,0,0.8f);
        var rt = popup.GetComponent<RectTransform>();
        // Stretch over entire tile with 40 px internal padding (increased from 30)
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(40, 40);
        rt.offsetMax = new Vector2(-40, -40);

        // Create a container for content with padding
        var contentContainer = new GameObject("ContentContainer", typeof(RectTransform));
        contentContainer.transform.SetParent(popup.transform, false);
        var containerRT = contentContainer.GetComponent<RectTransform>();
        containerRT.anchorMin = Vector2.zero;
        containerRT.anchorMax = Vector2.one;
        containerRT.pivot = new Vector2(0.5f, 0.5f);
        containerRT.offsetMin = new Vector2(20, 20); // 20px padding from edges
        containerRT.offsetMax = new Vector2(-20, -20); // 20px padding from edges

        // NDI name label
        var ndiLabel = CreateLabel(contentContainer.transform, "NDI: " + cam.sourceName);
        var input = CreateInputField(contentContainer.transform, cam.niceName);
        var saveBtn = CreateButton(contentContainer.transform, "Save");

        // Position input field below label with more padding
        var inRT = input.GetComponent<RectTransform>();
        inRT.anchorMin = new Vector2(0,1);
        inRT.anchorMax = new Vector2(1,1);
        inRT.pivot = new Vector2(0.5f,1);
        inRT.anchoredPosition = new Vector2(0, -60);  // Increased from -50

        // Position save button bottom center inside popup with more padding
        var sbRT = saveBtn.GetComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(0.5f, 0);
        sbRT.anchorMax = new Vector2(0.5f, 0);
        sbRT.pivot = new Vector2(0.5f, 0);
        sbRT.anchoredPosition = new Vector2(0, 40);  // Increased from 30
        sbRT.sizeDelta = new Vector2(100, 36);  // Maintained larger touch target

        saveBtn.onClick.AddListener(() => {
            cam.niceName = input.text.Trim();
            titleText.text = cam.niceName;
            Destroy(popup);
            RefreshGlobalSettingsList();
        });
    }

    private Text CreateLabel(Transform parent, string text)
    {
        var obj = new GameObject("Label", typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var t = obj.AddComponent<Text>();
        t.font = BuiltinFont;
        t.text = text;
        t.alignment = TextAnchor.UpperLeft;
        t.color = Color.white;
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0,1);
        rt.anchorMax = new Vector2(1,1);
        rt.pivot = new Vector2(0,1);
        rt.offsetMin = new Vector2(4,-20);
        rt.offsetMax = new Vector2(-4,0);
        return t;
    }

    private InputField CreateInputField(Transform parent, string value)
    {
        var obj = new GameObject("Input", typeof(RectTransform), typeof(Image), typeof(InputField));
        obj.transform.SetParent(parent, false);
        var img = obj.GetComponent<Image>();
        img.color = new Color(1,1,1,0.1f);
        var input = obj.GetComponent<InputField>();

        var textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(obj.transform, false);
        var text = textObj.AddComponent<Text>();
        text.font = BuiltinFont;
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = Color.white;
        input.textComponent = text;

        var placeholderObj = new GameObject("Placeholder", typeof(RectTransform));
        placeholderObj.transform.SetParent(obj.transform, false);
        var ph = placeholderObj.AddComponent<Text>();
        ph.font = BuiltinFont;
        ph.fontSize = 14;
        ph.text = "Nice Name";
        ph.fontStyle = FontStyle.Italic;
        ph.color = new Color(1,1,1,0.4f);
        input.placeholder = ph;

        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0,0.5f);
        rt.anchorMax = new Vector2(1,0.5f);
        rt.pivot = new Vector2(0.5f,0.5f);
        rt.sizeDelta = new Vector2(0,24); // Changed from -20 to 0 to make it full width

        // Set up text and placeholder RectTransforms to fill the input field
        var textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0, 0);
        textRT.anchorMax = new Vector2(1, 1);
        textRT.offsetMin = new Vector2(5, 0); // Small padding on left
        textRT.offsetMax = new Vector2(-5, 0); // Small padding on right

        var phRT = placeholderObj.GetComponent<RectTransform>();
        phRT.anchorMin = new Vector2(0, 0);
        phRT.anchorMax = new Vector2(1, 1);
        phRT.offsetMin = new Vector2(5, 0);
        phRT.offsetMax = new Vector2(-5, 0);

        input.text = value;
        return input;
    }

    private Button CreateButton(Transform parent, string label)
    {
        var obj = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        var img = obj.GetComponent<Image>();
        img.color = new Color(0.2f,0.6f,0.2f,1);
        var btn = obj.GetComponent<Button>();
        var txtObj = new GameObject("Text", typeof(RectTransform));
        txtObj.transform.SetParent(obj.transform, false);
        var txt = txtObj.AddComponent<Text>();
        txt.font = BuiltinFont;
        txt.text = label;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        var rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(60,24);
        return btn;
    }

    // --- Global settings panel ----------------------------------------------
    private void CreateGlobalSettingsButton()
    {
        var btn = CreateIconButton(_canvas.transform, globalGearIconTexture, "⚙", new Vector2(1,1), TextAnchor.MiddleCenter);
        var rt = btn.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(-padding,-padding);
        btn.onClick.AddListener(ToggleSettingsPanel);
    }

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
        var panelObj = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image));
        panelObj.transform.SetParent(_canvas.transform, false);
        var img = panelObj.GetComponent<Image>();
        img.color = new Color(0,0,0,0.8f);
        var rt = panelObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0,0);
        rt.anchorMax = new Vector2(1,1);
        rt.pivot = new Vector2(0.5f,0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Scroll view container simplified as vertical layout
        var scroll = new GameObject("Scroll", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Mask), typeof(Image));
        scroll.transform.SetParent(panelObj.transform, false);
        var scrt = scroll.GetComponent<RectTransform>();
        scrt.anchorMin = new Vector2(0,0);
        scrt.anchorMax = new Vector2(1,1);
        scrt.pivot = new Vector2(0.5f,0.5f);
        scrt.offsetMin = new Vector2(20,20);
        scrt.offsetMax = new Vector2(-20,-20);
        var vlg = scroll.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 10f;
        vlg.padding = new RectOffset(20,20,20,20);
        vlg.childAlignment = TextAnchor.MiddleCenter;
        var scrollImg = scroll.GetComponent<Image>();
        scrollImg.color = new Color(1,1,1,0.1f);
        settingsListContainer = scroll.transform;

        // Close button (top-right inside panel)
        var closeBtn = CreateIconButton(panelObj.transform, closeIconTexture, "X", new Vector2(1,1), TextAnchor.MiddleCenter);
        var cRT = closeBtn.GetComponent<RectTransform>();
        cRT.anchoredPosition = new Vector2(-10, -10);
        closeBtn.onClick.AddListener(ToggleSettingsPanel);

        settingsPanel = panelObj.transform;

        RefreshGlobalSettingsList();
    }

    private void RefreshGlobalSettingsList()
    {
        if (settingsListContainer == null) return;

        foreach (Transform child in settingsListContainer)
            Destroy(child.gameObject);

        foreach (var cam in cameras)
        {
            var item = new GameObject("Item", typeof(RectTransform), typeof(Image));
            item.transform.SetParent(settingsListContainer, false);
            var img = item.GetComponent<Image>();
            img.color = new Color(1,1,1,0.1f);
            var rt = item.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0,28);

            var txt = new GameObject("Text", typeof(RectTransform));
            txt.transform.SetParent(item.transform, false);
            var t = txt.AddComponent<Text>();
            t.font = BuiltinFont;
            t.text = cam.niceName;
            t.alignment = TextAnchor.MiddleLeft;
            t.color = Color.white;
            var le = item.AddComponent<LayoutElement>();
            le.preferredHeight = 28;

            var drag = item.AddComponent<DraggableItem>();
            drag.OnDropCallback = () => { ApplyNewOrderFromSettings(); };
            drag.GetComponent<LayoutElement>().preferredWidth = 200;
            drag.OnDropCallback = () => { ApplyNewOrderFromSettings(); };
        }
    }

    private void ApplyNewOrderFromSettings()
    {
        // Build new order from children sequence
        var newList = new List<CameraInfo>();
        foreach (Transform child in settingsListContainer)
        {
            var txt = child.GetComponentInChildren<Text>();
            var cam = cameras.Find(c => c.niceName == txt.text);
            if (cam != null) newList.Add(cam);
        }
        cameras.Clear();
        cameras.AddRange(newList);

        // Reorder tiles in grid
        for (int i = 0; i < cameras.Count; i++)
        {
            cameras[i].tileObject.transform.SetSiblingIndex(i);
        }
    }

    // --- Layout computation --------------------------------------------------
    private void ConfigureGridLayout()
    {
        int total = cameras.Count;
        if (total == 0) return;
        int columns = Mathf.Min(3, total);
        int rows = Mathf.CeilToInt(total / (float)columns);
        rows = Mathf.Min(rows, 2);

        float gameWidth = Screen.width;
        float usableWidth = gameWidth - 2 * padding;
        float totalSpacing = (columns - 1) * padding;
        float cellWidth = (usableWidth - totalSpacing) / columns;
        float maxAllowed = (gameWidth - 2 * padding) / 2f;
        cellWidth = Mathf.Min(cellWidth, maxAllowed);
        float cellHeight = cellWidth / aspectRatio;

        _gridGroup.cellSize = new Vector2(cellWidth, cellHeight);
        _gridGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _gridGroup.constraintCount = columns;
    }

    private void Update()
    {
        // Update textures
        foreach (var cam in cameras)
        {
            var tex = cam.receiver.texture;
            if (tex != null && cam.rawImage.texture != tex)
            {
                cam.rawImage.texture = tex;
            }
        }
    }

    private System.Collections.IEnumerator SourceRefreshLoop()
    {
        while (true)
        {
            RefreshSources();
            yield return new WaitForSeconds(3f);
        }
    }

    private void RefreshSources()
    {
        var existing = new HashSet<string>();
        foreach (var c in cameras) existing.Add(c.sourceName);

        bool addedAny = false;
        foreach (var src in NdiFinder.sourceNames)
        {
            if (!existing.Contains(src))
            {
                AddCameraInternal(src, src);
                addedAny = true;
            }
        }

        if (addedAny) ConfigureGridLayout();
    }
} 