using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Factory class for creating UI elements programmatically
/// </summary>
public static class UIFactory
{
    private static Font _builtinFont;
    
    /// <summary>
    /// Gets a reference to the default built-in font
    /// </summary>
    public static Font BuiltinFont
    {
        get
        {
            if (_builtinFont == null)
            {
                try { _builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
                catch { _builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
            }
            return _builtinFont;
        }
    }
    
    /// <summary>
    /// Creates a button with text label
    /// </summary>
    public static Button CreateButton(Transform parent, string label, Vector2 size, UnityAction onClick = null)
    {
        var obj = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        
        var img = obj.GetComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 0.2f, 1);
        
        var btn = obj.GetComponent<Button>();
        if (onClick != null)
            btn.onClick.AddListener(onClick);
        
        var rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        
        var txtObj = new GameObject("Text", typeof(RectTransform));
        txtObj.transform.SetParent(obj.transform, false);
        var txt = txtObj.AddComponent<Text>();
        txt.font = BuiltinFont;
        txt.text = label;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        
        var textRT = txtObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        
        return btn;
    }
    
    /// <summary>
    /// Creates an icon button with optional text
    /// </summary>
    public static Button CreateIconButton(Transform parent, Texture2D iconTexture, string label, Vector2 anchorPivot, TextAnchor textAnchor, Vector2 size)
    {
        var obj = new GameObject("IconButton", typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorPivot;
        rt.anchorMax = anchorPivot;
        rt.pivot = anchorPivot;
        rt.sizeDelta = size;
        
        var img = obj.GetComponent<Image>();
        img.color = Color.white;

        if (iconTexture != null)
        {
            var sprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
        }
        
        var btn = obj.GetComponent<Button>();

        if (iconTexture == null && !string.IsNullOrEmpty(label))
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
            
            var textRT = txtObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
        }
        
        return btn;
    }
    
    /// <summary>
    /// Creates a text label
    /// </summary>
    public static Text CreateLabel(Transform parent, string text, TextAnchor alignment = TextAnchor.UpperLeft, Color color = default)
    {
        if (color == default)
            color = Color.white;
            
        var obj = new GameObject("Label", typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        
        var t = obj.AddComponent<Text>();
        t.font = BuiltinFont;
        t.text = text;
        t.alignment = alignment;
        t.color = color;
        
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0, 1);
        rt.offsetMin = new Vector2(4, -20);
        rt.offsetMax = new Vector2(-4, 0);
        
        return t;
    }
    
    /// <summary>
    /// Creates an input field
    /// </summary>
    public static InputField CreateInputField(Transform parent, string value, string placeholder = "")
    {
        var obj = new GameObject("Input", typeof(RectTransform), typeof(Image), typeof(InputField));
        obj.transform.SetParent(parent, false);
        
        var img = obj.GetComponent<Image>();
        img.color = new Color(1, 1, 1, 0.1f);
        
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
        ph.text = string.IsNullOrEmpty(placeholder) ? "Enter text..." : placeholder;
        ph.fontStyle = FontStyle.Italic;
        ph.color = new Color(1, 1, 1, 0.4f);
        input.placeholder = ph;

        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(0, 24);

        // Set up text and placeholder RectTransforms to fill the input field
        var textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0, 0);
        textRT.anchorMax = new Vector2(1, 1);
        textRT.offsetMin = new Vector2(5, 0);
        textRT.offsetMax = new Vector2(-5, 0);

        var phRT = placeholderObj.GetComponent<RectTransform>();
        phRT.anchorMin = new Vector2(0, 0);
        phRT.anchorMax = new Vector2(1, 1);
        phRT.offsetMin = new Vector2(5, 0);
        phRT.offsetMax = new Vector2(-5, 0);

        input.text = value;
        return input;
    }
    
    /// <summary>
    /// Creates a panel with a background
    /// </summary>
    public static RectTransform CreatePanel(Transform parent, Color backgroundColor, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        var panelObj = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panelObj.transform.SetParent(parent, false);
        
        var img = panelObj.GetComponent<Image>();
        img.color = backgroundColor;
        
        var rt = panelObj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        
        return rt;
    }
    
    /// <summary>
    /// Creates a raw image for displaying textures
    /// </summary>
    public static RawImage CreateRawImage(Transform parent, Texture texture = null)
    {
        var imageObj = new GameObject("RawImage", typeof(RectTransform), typeof(RawImage));
        imageObj.transform.SetParent(parent, false);
        
        var rawImage = imageObj.GetComponent<RawImage>();
        rawImage.texture = texture;
        rawImage.raycastTarget = false;
        
        var rt = imageObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        return rawImage;
    }
} 