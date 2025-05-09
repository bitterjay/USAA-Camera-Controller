using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Popup window for editing a camera's display name
/// </summary>
public class CameraSettingsPopup : MonoBehaviour
{
    // References to UI elements
    private InputField nameInputField;
    private Button saveButton;
    private Text ndiNameLabel;
    
    // Data
    private CameraInfo cameraInfo;
    
    // Event fired when changes are saved
    public event Action<string> OnSaveChanges;
    
    // Reference to the overlay object
    private GameObject overlayObject;
    
    /// <summary>
    /// Initialize the popup with camera data
    /// </summary>
    public void Initialize(CameraInfo cameraInfo, LayoutSettings settings)
    {
        this.cameraInfo = cameraInfo;
        
        // Setup the panel background
        var image = GetComponent<Image>();
        if (image == null)
            image = gameObject.AddComponent<Image>();
        image.color = new Color(49, 49, 49, 0.8f);
        
        // Create content with padding
        var contentContainer = new GameObject("ContentContainer", typeof(RectTransform));
        contentContainer.transform.SetParent(transform, false);
        var containerRT = contentContainer.GetComponent<RectTransform>();
        containerRT.anchorMin = Vector2.zero;
        containerRT.anchorMax = Vector2.one;
        containerRT.offsetMin = new Vector2(20, 20);
        containerRT.offsetMax = new Vector2(-20, -20);
        
        // Add close button in top-right corner
        AddCloseButton();
        
        // Create the UI elements
        CreateUIElements(contentContainer.transform);
        
        // Set initial values
        nameInputField.text = cameraInfo.niceName;
        ndiNameLabel.text = cameraInfo.sourceName;
    }
    
    /// <summary>
    /// Adds a close button to the top-right corner of the popup
    /// </summary>
    private void AddCloseButton()
    {
        var btnObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(transform, false);
        
        // Configure appearance
        var btnImg = btnObj.GetComponent<Image>();
        btnImg.color = new Color(0.8f, 0.2f, 0.2f, 1);
        
        // Set up rect transform in top-right corner
        var btnRT = btnObj.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(1, 1);
        btnRT.anchorMax = new Vector2(1, 1);
        btnRT.pivot = new Vector2(1, 1);
        btnRT.sizeDelta = new Vector2(30, 30);
        btnRT.anchoredPosition = new Vector2(-10, -10);
        
        // Configure the button
        var btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(CloseWithoutSaving);
        
        // Create X text
        var textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        var text = textObj.AddComponent<Text>();
        text.font = UIFactory.BuiltinFont;
        text.fontSize = 18;
        text.text = "✕";
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        
        // Set up text rect transform
        var textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
    }
    
    private void CreateUIElements(Transform parent)
    {
        // Add a vertical layout group to the parent
        var verticalLayout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        verticalLayout.childAlignment = TextAnchor.UpperCenter;
        verticalLayout.spacing = 10f;
        verticalLayout.padding = new RectOffset(20, 20, 10, 10);
        
        // Title
        var titleObj = new GameObject("TitleLabel", typeof(RectTransform));
        titleObj.transform.SetParent(parent, false);
        var titleText = titleObj.AddComponent<Text>();
        titleText.text = "Edit Camera Settings";
        titleText.font = UIFactory.BuiltinFont;
        titleText.fontSize = 18;
        titleText.alignment = TextAnchor.UpperCenter;
        titleText.color = Color.white;
        
        // Add layout element to control height
        var titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.minHeight = 30;
        titleLayout.preferredHeight = 30;
        
        // NDI Source section - container
        var ndiSourceContainer = new GameObject("NDISourceContainer", typeof(RectTransform));
        ndiSourceContainer.transform.SetParent(parent, false);
        var ndiSourceVLayout = ndiSourceContainer.AddComponent<VerticalLayoutGroup>();
        ndiSourceVLayout.childAlignment = TextAnchor.UpperCenter;
        ndiSourceVLayout.spacing = 5f;
        ndiSourceVLayout.padding = new RectOffset(0, 0, 5, 5);
        
        // NDI source name label (title)
        var ndiSourceTitleObj = new GameObject("NDISourceTitle", typeof(RectTransform));
        ndiSourceTitleObj.transform.SetParent(ndiSourceContainer.transform, false);
        var ndiSourceTitle = ndiSourceTitleObj.AddComponent<Text>();
        ndiSourceTitle.text = "NDI Source Name (fixed):";
        ndiSourceTitle.font = UIFactory.BuiltinFont;
        ndiSourceTitle.fontSize = 12;
        ndiSourceTitle.alignment = TextAnchor.UpperCenter;
        ndiSourceTitle.color = new Color(0.7f, 0.7f, 0.7f);
        
        // Add layout element
        var ndiSourceTitleLayout = ndiSourceTitleObj.AddComponent<LayoutElement>();
        ndiSourceTitleLayout.minHeight = 20;
        ndiSourceTitleLayout.preferredHeight = 20;
        
        // NDI source name value
        var ndiNameObj = new GameObject("NDINameValue", typeof(RectTransform));
        ndiNameObj.transform.SetParent(ndiSourceContainer.transform, false);
        ndiNameLabel = ndiNameObj.AddComponent<Text>();
        ndiNameLabel.text = "";
        ndiNameLabel.font = UIFactory.BuiltinFont;
        ndiNameLabel.fontSize = 14;
        ndiNameLabel.alignment = TextAnchor.UpperCenter;
        ndiNameLabel.color = Color.white;
        
        // Add layout element
        var ndiNameLayout = ndiNameObj.AddComponent<LayoutElement>();
        ndiNameLayout.minHeight = 25;
        ndiNameLayout.preferredHeight = 25;
        
        // Display name section - container
        var displayNameContainer = new GameObject("DisplayNameContainer", typeof(RectTransform));
        displayNameContainer.transform.SetParent(parent, false);
        var displayNameVLayout = displayNameContainer.AddComponent<VerticalLayoutGroup>();
        displayNameVLayout.childAlignment = TextAnchor.UpperCenter;
        displayNameVLayout.spacing = 5f;
        displayNameVLayout.padding = new RectOffset(0, 0, 5, 5);
        
        // Display name label
        var displayNameTitleObj = new GameObject("DisplayNameTitle", typeof(RectTransform));
        displayNameTitleObj.transform.SetParent(displayNameContainer.transform, false);
        var displayNameTitle = displayNameTitleObj.AddComponent<Text>();
        displayNameTitle.text = "Display Name (editable):";
        displayNameTitle.font = UIFactory.BuiltinFont;
        displayNameTitle.fontSize = 12;
        displayNameTitle.alignment = TextAnchor.UpperCenter;
        displayNameTitle.color = new Color(0.7f, 0.7f, 0.7f);
        
        // Add layout element
        var displayNameTitleLayout = displayNameTitleObj.AddComponent<LayoutElement>();
        displayNameTitleLayout.minHeight = 20;
        displayNameTitleLayout.preferredHeight = 20;
        
        // Create input field
        CreateInputField(displayNameContainer.transform);
        
        // Add a spacer that will push the save button to the bottom
        var spacerObj = new GameObject("Spacer", typeof(RectTransform));
        spacerObj.transform.SetParent(parent, false);
        var spacerLayout = spacerObj.AddComponent<LayoutElement>();
        spacerLayout.flexibleHeight = 1;
        
        // Create save button at the bottom
        CreateSaveButton(parent);
    }
    
    private void CreateInputField(Transform parent)
    {
        // Create the input field object
        var inputObj = new GameObject("NameInput", typeof(RectTransform), typeof(Image), typeof(InputField));
        inputObj.transform.SetParent(parent, false);
        
        // Configure the appearance
        var inputImg = inputObj.GetComponent<Image>();
        inputImg.color = new Color(1, 1, 1, 0.1f);
        
        // Add layout element
        var inputLayout = inputObj.AddComponent<LayoutElement>();
        inputLayout.minHeight = 30;
        inputLayout.preferredHeight = 30;
        
        // Configure the input field
        nameInputField = inputObj.GetComponent<InputField>();
        
        // Create text component
        var textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(inputObj.transform, false);
        var text = textObj.AddComponent<Text>();
        text.font = UIFactory.BuiltinFont;
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = Color.white;
        nameInputField.textComponent = text;
        
        // Set up text rect transform
        var textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(5, 0);
        textRT.offsetMax = new Vector2(-5, 0);
        
        // Create placeholder
        var phObj = new GameObject("Placeholder", typeof(RectTransform));
        phObj.transform.SetParent(inputObj.transform, false);
        var ph = phObj.AddComponent<Text>();
        ph.font = UIFactory.BuiltinFont;
        ph.fontSize = 14;
        ph.text = "Enter display name...";
        ph.fontStyle = FontStyle.Italic;
        ph.color = new Color(1, 1, 1, 0.4f);
        nameInputField.placeholder = ph;
        
        // Set up placeholder rect transform
        var phRT = phObj.GetComponent<RectTransform>();
        phRT.anchorMin = Vector2.zero;
        phRT.anchorMax = Vector2.one;
        phRT.offsetMin = new Vector2(5, 0);
        phRT.offsetMax = new Vector2(-5, 0);
    }
    
    private void CreateSaveButton(Transform parent)
    {
        var btnObj = new GameObject("SaveButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        
        // Configure appearance
        var btnImg = btnObj.GetComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 0.2f, 1);
        
        // Add layout element
        var btnLayout = btnObj.AddComponent<LayoutElement>();
        btnLayout.minHeight = 40;
        btnLayout.preferredHeight = 40;
        btnLayout.minWidth = 100;
        btnLayout.preferredWidth = 100;
        
        // Configure the button
        saveButton = btnObj.GetComponent<Button>();
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        
        // Create button text
        var textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        var text = textObj.AddComponent<Text>();
        text.font = UIFactory.BuiltinFont;
        text.fontSize = 14;
        text.text = "Save";
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        
        // Set up text rect transform
        var textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
    }
    
    private void OnSaveButtonClicked()
    {
        // Get the new name from the input field
        string newName = nameInputField.text.Trim();
        
        // Trigger the save event
        OnSaveChanges?.Invoke(newName);
        
        // Close the popup
        if (overlayObject != null)
            Destroy(overlayObject);
        else
            Destroy(gameObject);
    }
    
    /// <summary>
    /// Creates a settings popup for a camera
    /// </summary>
    public static CameraSettingsPopup Create(Transform parent, CameraInfo camera, LayoutSettings settings)
    {
        // Find the root canvas to parent the fullscreen popup to it
        Canvas rootCanvas = null;
        Transform currentTransform = parent;
        while (currentTransform != null)
        {
            rootCanvas = currentTransform.GetComponentInParent<Canvas>();
            if (rootCanvas != null && rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                break;
            
            if (currentTransform.parent == null)
                break;
            
            currentTransform = currentTransform.parent;
        }
        
        // If no canvas found, use the provided parent
        Transform popupParent = rootCanvas != null ? rootCanvas.transform : parent;
        
        // Create overlay to darken the background
        var overlayObj = new GameObject("DarkOverlay", typeof(RectTransform), typeof(Image), typeof(Button));
        overlayObj.transform.SetParent(popupParent, false);
        
        // Make the overlay cover the entire screen
        var overlayRT = overlayObj.GetComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;
        
        // Set the overlay color to semi-transparent black
        var overlayImage = overlayObj.GetComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0.7f);
        
        // Create the popup window
        var popupObj = new GameObject("CameraSettings", typeof(RectTransform), typeof(Image));
        popupObj.transform.SetParent(overlayObj.transform, false);
        
        // Set up rect transform to center the popup
        var rt = popupObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(settings.CameraSettingsPopupWidth, settings.CameraSettingsPopupHeight);
        
        // Add a simple shadow/outline effect
        var outline = new GameObject("Outline", typeof(RectTransform), typeof(Image));
        outline.transform.SetParent(popupObj.transform, false);
        outline.transform.SetAsFirstSibling(); // Put it behind the popup
        
        var outlineRT = outline.GetComponent<RectTransform>();
        outlineRT.anchorMin = Vector2.zero;
        outlineRT.anchorMax = Vector2.one;
        outlineRT.offsetMin = new Vector2(-4, -4);
        outlineRT.offsetMax = new Vector2(4, 4);
        
        var outlineImage = outline.GetComponent<Image>();
        outlineImage.color = new Color(0, 0, 0, 0.5f);
        
        // Add component and initialize
        var popup = popupObj.AddComponent<CameraSettingsPopup>();
        popup.Initialize(camera, settings);
        
        // Store reference to overlay for cleanup
        popup.overlayObject = overlayObj;
        
        // Add click handler to the overlay background for closing
        var overlayButton = overlayObj.GetComponent<Button>();
        var colorBlock = overlayButton.colors;
        colorBlock.pressedColor = overlayButton.colors.normalColor; // No visual change when pressed
        colorBlock.highlightedColor = overlayButton.colors.normalColor;
        overlayButton.colors = colorBlock;
        overlayButton.onClick.AddListener(popup.OnBackgroundClicked);
        
        return popup;
    }

    /// <summary>
    /// Called when clicking the background overlay
    /// </summary>
    private void OnBackgroundClicked()
    {
        // If the click was directly on the background (not on the popup window)
        if (!IsPointerOverPopup())
        {
            CloseWithoutSaving();
        }
    }

    /// <summary>
    /// Checks if the pointer is over the popup window itself
    /// </summary>
    private bool IsPointerOverPopup()
    {
        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (var result in results)
        {
            // Check if the hit object is the popup or one of its children
            if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
            {
                return true;
            }
        }
        
        return false;
    }

    private void Update()
    {
        // Close popup when Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseWithoutSaving();
        }
    }

    /// <summary>
    /// Closes the popup without saving changes
    /// </summary>
    private void CloseWithoutSaving()
    {
        // Destroy the overlay which contains the popup
        if (overlayObject != null)
            Destroy(overlayObject);
        else
            Destroy(gameObject);
    }
} 