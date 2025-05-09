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
    private InputField ipAddressInputField;
    private InputField portInputField;
    private Button saveButton;
    private Text ndiNameLabel;
    
    // Data
    private CameraInfo cameraInfo;
    
    // Event fired when changes are saved
    public event Action<string> OnSaveChanges;
    public event Action<string, int> OnConnectionChanged;
    
    // Reference to the overlay object
    private GameObject overlayObject;
    
    // We'll add fields for VISCA IP and port settings
    private InputField viscaIpInput;
    private InputField viscaPortInput;
    
    // Reference to the registry for updating connection settings
    private CameraRegistry cameraRegistry;
    
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
        ipAddressInputField.text = cameraInfo.viscaIp;
        portInputField.text = cameraInfo.viscaPort.ToString();
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
        
        // VISCA IP section - container
        var viscaIpContainer = new GameObject("ViscaIpContainer", typeof(RectTransform));
        viscaIpContainer.transform.SetParent(parent, false);
        var viscaIpVLayout = viscaIpContainer.AddComponent<VerticalLayoutGroup>();
        viscaIpVLayout.childAlignment = TextAnchor.UpperCenter;
        viscaIpVLayout.spacing = 5f;
        viscaIpVLayout.padding = new RectOffset(0, 0, 5, 5);
        
        // VISCA IP label
        var viscaIpTitleObj = new GameObject("ViscaIpTitle", typeof(RectTransform));
        viscaIpTitleObj.transform.SetParent(viscaIpContainer.transform, false);
        var viscaIpTitle = viscaIpTitleObj.AddComponent<Text>();
        viscaIpTitle.text = "VISCA IP Address:";
        viscaIpTitle.font = UIFactory.BuiltinFont;
        viscaIpTitle.fontSize = 12;
        viscaIpTitle.alignment = TextAnchor.UpperCenter;
        viscaIpTitle.color = new Color(0.7f, 0.7f, 0.7f);
        
        // Add layout element
        var viscaIpTitleLayout = viscaIpTitleObj.AddComponent<LayoutElement>();
        viscaIpTitleLayout.minHeight = 20;
        viscaIpTitleLayout.preferredHeight = 20;
        
        // Create IP input field
        CreateViscaIpField(viscaIpContainer.transform);
        
        // VISCA Port section - container
        var viscaPortContainer = new GameObject("ViscaPortContainer", typeof(RectTransform));
        viscaPortContainer.transform.SetParent(parent, false);
        var viscaPortVLayout = viscaPortContainer.AddComponent<VerticalLayoutGroup>();
        viscaPortVLayout.childAlignment = TextAnchor.UpperCenter;
        viscaPortVLayout.spacing = 5f;
        viscaPortVLayout.padding = new RectOffset(0, 0, 5, 5);
        
        // VISCA Port label
        var viscaPortTitleObj = new GameObject("ViscaPortTitle", typeof(RectTransform));
        viscaPortTitleObj.transform.SetParent(viscaPortContainer.transform, false);
        var viscaPortTitle = viscaPortTitleObj.AddComponent<Text>();
        viscaPortTitle.text = "VISCA Port:";
        viscaPortTitle.font = UIFactory.BuiltinFont;
        viscaPortTitle.fontSize = 12;
        viscaPortTitle.alignment = TextAnchor.UpperCenter;
        viscaPortTitle.color = new Color(0.7f, 0.7f, 0.7f);
        
        // Add layout element
        var viscaPortTitleLayout = viscaPortTitleObj.AddComponent<LayoutElement>();
        viscaPortTitleLayout.minHeight = 20;
        viscaPortTitleLayout.preferredHeight = 20;
        
        // Create Port input field
        CreateViscaPortField(viscaPortContainer.transform);
        
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
    
    private void CreateViscaIpField(Transform parent)
    {
        // Create label
        var labelObj = new GameObject("IPLabel", typeof(RectTransform));
        labelObj.transform.SetParent(parent, false);
        var labelText = labelObj.AddComponent<Text>();
        labelText.text = "VISCA IP Address:";
        labelText.font = UIFactory.BuiltinFont;
        labelText.fontSize = 16;
        labelText.color = Color.white;
        var labelRT = labelObj.GetComponent<RectTransform>();
        labelRT.sizeDelta = new Vector2(150, 24);
        
        // Create input field
        var inputObj = new GameObject("IPInput", typeof(RectTransform), typeof(Image));
        inputObj.transform.SetParent(parent, false);
        var inputImage = inputObj.GetComponent<Image>();
        inputImage.color = new Color(0.2f, 0.2f, 0.2f);
        
        // viscaIpInput = inputObj.AddComponent<InputField>();
        // viscaIpInput.textComponent = UIFactory.CreateTextComponent(inputObj.transform);
        // viscaIpInput.textComponent.color = Color.white;
        // viscaIpInput.textComponent.font = UIFactory.BuiltinFont;
        // viscaIpInput.textComponent.fontSize = 16;
        // viscaIpInput.textComponent.alignment = TextAnchor.MiddleLeft;
        
        // Add placeholder
        var placeholder = new GameObject("Placeholder", typeof(RectTransform));
        placeholder.transform.SetParent(inputObj.transform, false);
        var placeholderText = placeholder.AddComponent<Text>();
        placeholderText.text = "192.168.1.100";
        placeholderText.font = UIFactory.BuiltinFont;
        placeholderText.fontSize = 16;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f);
        placeholderText.alignment = TextAnchor.MiddleLeft;
        var placeholderRT = placeholder.GetComponent<RectTransform>();
        placeholderRT.anchorMin = Vector2.zero;
        placeholderRT.anchorMax = Vector2.one;
        placeholderRT.offsetMin = new Vector2(8, 0);
        placeholderRT.offsetMax = Vector2.zero;
        
        viscaIpInput.placeholder = placeholderText;
        
        var inputRT = inputObj.GetComponent<RectTransform>();
        inputRT.sizeDelta = new Vector2(180, 26);
    }
    
    private void CreateViscaPortField(Transform parent)
    {
        // Create label
        var labelObj = new GameObject("PortLabel", typeof(RectTransform));
        labelObj.transform.SetParent(parent, false);
        var labelText = labelObj.AddComponent<Text>();
        labelText.text = "VISCA Port:";
        labelText.font = UIFactory.BuiltinFont;
        labelText.fontSize = 16;
        labelText.color = Color.white;
        var labelRT = labelObj.GetComponent<RectTransform>();
        labelRT.sizeDelta = new Vector2(150, 24);
        
        // Create input field
        var inputObj = new GameObject("PortInput", typeof(RectTransform), typeof(Image));
        inputObj.transform.SetParent(parent, false);
        var inputImage = inputObj.GetComponent<Image>();
        inputImage.color = new Color(0.2f, 0.2f, 0.2f);
        
        // viscaPortInput = inputObj.AddComponent<InputField>();
        // viscaPortInput.textComponent = UIFactory.CreateTextComponent(inputObj.transform);
        // viscaPortInput.textComponent.color = Color.white;
        // viscaPortInput.textComponent.font = UIFactory.BuiltinFont;
        // viscaPortInput.textComponent.fontSize = 16;
        // viscaPortInput.textComponent.alignment = TextAnchor.MiddleLeft;
        
        // Add placeholder
        var placeholder = new GameObject("Placeholder", typeof(RectTransform));
        placeholder.transform.SetParent(inputObj.transform, false);
        var placeholderText = placeholder.AddComponent<Text>();
        placeholderText.text = "52381";
        placeholderText.font = UIFactory.BuiltinFont;
        placeholderText.fontSize = 16;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f);
        placeholderText.alignment = TextAnchor.MiddleLeft;
        var placeholderRT = placeholder.GetComponent<RectTransform>();
        placeholderRT.anchorMin = Vector2.zero;
        placeholderRT.anchorMax = Vector2.one;
        placeholderRT.offsetMin = new Vector2(8, 0);
        placeholderRT.offsetMax = Vector2.zero;
        
        viscaPortInput.placeholder = placeholderText;
        viscaPortInput.contentType = InputField.ContentType.IntegerNumber;
        
        var inputRT = inputObj.GetComponent<RectTransform>();
        inputRT.sizeDelta = new Vector2(180, 26);
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
        
        // Get new connection settings
        string newIp = viscaIpInput.text.Trim();
        string portStr = viscaPortInput.text.Trim();
        int newPort = 52381;  // default port
        
        bool validPort = int.TryParse(portStr, out newPort);
        if (!validPort)
        {
            Debug.LogWarning("Invalid port number entered, using default 52381");
            newPort = 52381;
        }
        
        // Check if IP is valid format
        if (string.IsNullOrEmpty(newIp))
        {
            Debug.LogWarning("Empty IP address, using default");
            newIp = "192.168.1.104";
        }
        
        // Trigger the save event for the name
        OnSaveChanges?.Invoke(newName);
        
        // Trigger connection changed event if IP or port changed
        if (newIp != cameraInfo.viscaIp || newPort != cameraInfo.viscaPort)
        {
            Debug.Log($"Connection settings changed for {cameraInfo.niceName}: IP {cameraInfo.viscaIp} -> {newIp}, Port {cameraInfo.viscaPort} -> {newPort}");
            OnConnectionChanged?.Invoke(newIp, newPort);
            
            // Update the camera registry directly
            var registry = UnityEngine.Object.FindObjectOfType<NDIViewerApp>()?.GetCameraRegistry();
            if (registry != null)
            {
                registry.UpdateCameraConnection(cameraInfo, newIp, newPort);
            }
            
            // Update the controller if this is the active camera
            if (cameraInfo.isActive)
            {
                var controller = UnityEngine.Object.FindObjectOfType<ViscaControlPanelController>();
                if (controller != null)
                {
                    controller.SetIPAddress(newIp);
                    controller.SetPort(newPort);
                }
            }
        }
        
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