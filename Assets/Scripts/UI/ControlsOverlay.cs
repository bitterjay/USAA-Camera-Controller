using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ControlsOverlay : MonoBehaviour
{
    public static ControlsOverlay Instance { get; private set; }
    private Canvas overlayCanvas;
    private GameObject panelObj;
    private bool isShown = false;
    private bool justShown = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateOverlay();
        Hide();
    }

    private void CreateOverlay()
    {
        Debug.Log("ControlsOverlay: Creating overlay");
        // Canvas
        overlayCanvas = gameObject.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 2000;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        // Use LegacyRuntime.ttf font directly
        var legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (legacyFont == null)
            Debug.LogWarning("ControlsOverlay: LegacyRuntime.ttf not found! Text will not render.");

        // Background
        var bgObj = new GameObject("OverlayBackground", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(overlayCanvas.transform, false);
        var bg = bgObj.GetComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.85f);
        var bgRT = bgObj.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // Panel
        panelObj = new GameObject("ControlsPanel", typeof(RectTransform), typeof(Image));
        panelObj.transform.SetParent(bgObj.transform, false);
        var panelImg = panelObj.GetComponent<Image>();
        panelImg.color = new Color(0.18f, 0.18f, 0.18f, 0.98f);
        var panelRT = panelObj.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(600, 600);
        panelRT.anchoredPosition = Vector2.zero;

        // Title
        var titleObj = new GameObject("Title", typeof(RectTransform), typeof(Text));
        titleObj.transform.SetParent(panelObj.transform, false);
        var titleRT = titleObj.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.sizeDelta = new Vector2(0, 60);
        titleRT.anchoredPosition = new Vector2(0, -20);
        var titleText = titleObj.GetComponent<Text>();
        titleText.text = "Gamepad Controls";
        titleText.font = legacyFont;
        titleText.fontSize = 32;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;

        // Controls list
        string[] controls = new string[] {
            "Left Stick: Pan/Tilt Camera",
            "Right Stick: Zoom In/Out",
            "Right Trigger: Fast Movement/Zoom",
            "LB/RB: Cycle Cameras",
            "Triangle (Y): Fullscreen Camera Preview",
            "R3 (Right Stick Press): Focus One Push",
            "L3 (Left Stick Press): White Balance One Push",
            "Start: Show/Hide Controls",
            "B/Circle: Close Controls Overlay"
        };
        for (int i = 0; i < controls.Length; i++)
        {
            var ctrlObj = new GameObject($"Control{i}", typeof(RectTransform), typeof(Text));
            ctrlObj.transform.SetParent(panelObj.transform, false);
            var ctrlRT = ctrlObj.GetComponent<RectTransform>();
            ctrlRT.anchorMin = new Vector2(0, 1);
            ctrlRT.anchorMax = new Vector2(1, 1);
            ctrlRT.pivot = new Vector2(0.5f, 1);
            ctrlRT.sizeDelta = new Vector2(0, 40);
            ctrlRT.anchoredPosition = new Vector2(0, -80 - i * 50);
            var ctrlText = ctrlObj.GetComponent<Text>();
            ctrlText.text = controls[i];
            ctrlText.font = legacyFont;
            ctrlText.fontSize = 22;
            ctrlText.color = Color.white;
            ctrlText.alignment = TextAnchor.MiddleLeft;
        }
    }

    public void Show()
    {
        Debug.Log("ControlsOverlay: Show() called");
        overlayCanvas.enabled = true;
        isShown = true;
        justShown = true;
    }

    public void Hide()
    {
        Debug.Log("ControlsOverlay: Hide() called");
        overlayCanvas.enabled = false;
        isShown = false;
    }

    public void Toggle()
    {
        if (isShown) Hide();
        else Show();
    }

    private void Update()
    {
        if (!isShown) return;
        if (justShown) { justShown = false; return; }
        // Dismiss with Start or B/Circle
        if (Gamepad.current != null && (Gamepad.current.startButton.wasPressedThisFrame || Gamepad.current.buttonEast.wasPressedThisFrame))
        {
            Hide();
        }
        // Also allow Escape key for keyboard
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Hide();
        }
    }
} 