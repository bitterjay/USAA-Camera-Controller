using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Status bar that displays information about the active camera
/// </summary>
public class StatusBarView : MonoBehaviour
{
    private Text statusText;
    
    /// <summary>
    /// Initialize the status bar
    /// </summary>
    public void Initialize()
    {
        // Set up the background
        var image = GetComponent<Image>();
        if (image == null)
            image = gameObject.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.8f);
        
        // Create the status text
        var textObj = new GameObject("StatusText", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(transform, false);
        
        var textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(10, 5);
        textRT.offsetMax = new Vector2(-10, -5);
        
        statusText = textObj.GetComponent<Text>();
        statusText.font = UIFactory.BuiltinFont;
        statusText.fontSize = 14;
        statusText.alignment = TextAnchor.MiddleLeft;
        statusText.color = Color.white;
        statusText.text = "No active camera";
    }
    
    /// <summary>
    /// Updates the status text to show the currently active camera
    /// </summary>
    /// <param name="camera">The active camera</param>
    public void SetActiveCamera(CameraInfo camera)
    {
        if (camera == null)
        {
            statusText.text = "No active camera";
            return;
        }
        
        statusText.text = $"Active Camera: {camera.niceName} (NDI: {camera.sourceName})";
    }
} 