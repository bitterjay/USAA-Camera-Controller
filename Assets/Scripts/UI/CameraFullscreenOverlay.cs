using UnityEngine;
using UnityEngine.UI;

public class CameraFullscreenOverlay : MonoBehaviour
{
    private Canvas overlayCanvas;
    private RawImage videoImage;
    private Image background;
    private CameraInfo currentCamera;
    private bool isShown = false;

    public static CameraFullscreenOverlay Instance { get; private set; }

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
        // Canvas
        overlayCanvas = gameObject.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 1000;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        // Background
        var bgObj = new GameObject("OverlayBackground", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(overlayCanvas.transform, false);
        background = bgObj.GetComponent<Image>();
        background.color = new Color(0, 0, 0, 0.7f);
        var bgRT = bgObj.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0.025f, 0.025f);
        bgRT.anchorMax = new Vector2(0.975f, 0.975f);
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // Video RawImage
        var videoObj = new GameObject("CameraVideo", typeof(RectTransform), typeof(RawImage));
        videoObj.transform.SetParent(bgObj.transform, false);
        videoImage = videoObj.GetComponent<RawImage>();
        var videoRT = videoObj.GetComponent<RectTransform>();
        videoRT.anchorMin = Vector2.zero;
        videoRT.anchorMax = Vector2.one;
        videoRT.offsetMin = Vector2.zero;
        videoRT.offsetMax = Vector2.zero;
    }

    public void Show(CameraInfo camera)
    {
        currentCamera = camera;
        UpdateVideoTexture();
        overlayCanvas.enabled = true;
        isShown = true;
    }

    public void Hide()
    {
        overlayCanvas.enabled = false;
        isShown = false;
    }

    public void Toggle(CameraInfo camera)
    {
        if (isShown)
            Hide();
        else
            Show(camera);
    }

    private void Update()
    {
        if (isShown)
            UpdateVideoTexture();
    }

    private void UpdateVideoTexture()
    {
        if (currentCamera != null && currentCamera.receiver != null)
        {
            var tex = currentCamera.receiver.texture;
            if (tex != null && videoImage.texture != tex)
                videoImage.texture = tex;
        }
    }
} 