using UnityEngine;
using UnityEngine.UI;
using System;

public class GridSettingsPopup : MonoBehaviour
{
    public Action<int, int, float> OnSave;

    private GameObject overlayObj;
    private InputField columnsField;
    private InputField rowsField;
    private InputField cellWidthField;
    private RectTransform previewContainer;
    private Button saveButton;
    private Button closeButton;

    private int columns = 3;
    private int rows = 2;
    private float cellWidth = 200f;

    public static GridSettingsPopup Show(Transform parent, int currentCols, int currentRows, float currentCellWidth, Action<int, int, float> onSave)
    {
        var go = new GameObject("GridSettingsPopup");
        var popup = go.AddComponent<GridSettingsPopup>();
        popup.OnSave = onSave;
        popup.columns = currentCols;
        popup.rows = currentRows;
        popup.cellWidth = currentCellWidth;
        popup.CreateUI(parent);
        return popup;
    }

    private void CreateUI(Transform parent)
    {
        // Overlay
        overlayObj = new GameObject("GridSettingsOverlay", typeof(RectTransform), typeof(Image), typeof(Button));
        overlayObj.transform.SetParent(parent, false);
        var overlayRT = overlayObj.GetComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;
        var overlayImg = overlayObj.GetComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.5f);
        var overlayBtn = overlayObj.GetComponent<Button>();
        overlayBtn.onClick.AddListener(Hide);

        // Popup panel
        var panelObj = new GameObject("GridSettingsPanel", typeof(RectTransform), typeof(Image));
        panelObj.transform.SetParent(overlayObj.transform, false);
        var panelRT = panelObj.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(400, 350);
        var panelImg = panelObj.GetComponent<Image>();
        panelImg.color = new Color(0.12f, 0.12f, 0.12f, 0.98f);

        // Title
        var titleObj = new GameObject("Title", typeof(RectTransform), typeof(Text));
        titleObj.transform.SetParent(panelObj.transform, false);
        var titleRT = titleObj.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.sizeDelta = new Vector2(0, 40);
        titleRT.anchoredPosition = new Vector2(0, -10);
        var titleText = titleObj.GetComponent<Text>();
        titleText.text = "Grid Settings";
        titleText.font = UIFactory.BuiltinFont;
        titleText.fontSize = 20;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;

        // Columns field
        columnsField = CreateLabeledInput(panelObj.transform, "Columns", columns.ToString(), 60);
        columnsField.contentType = InputField.ContentType.IntegerNumber;
        columnsField.onValueChanged.AddListener(val => { if (int.TryParse(val, out int v)) { columns = Mathf.Clamp(v, 1, 6); UpdatePreview(); } });

        // Rows field
        rowsField = CreateLabeledInput(panelObj.transform, "Rows", rows.ToString(), 110);
        rowsField.contentType = InputField.ContentType.IntegerNumber;
        rowsField.onValueChanged.AddListener(val => { if (int.TryParse(val, out int v)) { rows = Mathf.Clamp(v, 1, 6); UpdatePreview(); } });

        // Cell width field
        cellWidthField = CreateLabeledInput(panelObj.transform, "Cell Width", cellWidth.ToString(), 160);
        cellWidthField.contentType = InputField.ContentType.DecimalNumber;
        cellWidthField.onValueChanged.AddListener(val => { if (float.TryParse(val, out float v)) { cellWidth = Mathf.Max(50, v); UpdatePreview(); } });

        // Preview label
        var previewLabelObj = new GameObject("PreviewLabel", typeof(RectTransform), typeof(Text));
        previewLabelObj.transform.SetParent(panelObj.transform, false);
        var previewLabelRT = previewLabelObj.GetComponent<RectTransform>();
        previewLabelRT.anchorMin = new Vector2(0, 1);
        previewLabelRT.anchorMax = new Vector2(1, 1);
        previewLabelRT.pivot = new Vector2(0.5f, 1);
        previewLabelRT.sizeDelta = new Vector2(0, 20);
        previewLabelRT.anchoredPosition = new Vector2(0, -210);
        var previewLabelText = previewLabelObj.GetComponent<Text>();
        previewLabelText.text = "Preview (up to 6)";
        previewLabelText.font = UIFactory.BuiltinFont;
        previewLabelText.fontSize = 14;
        previewLabelText.color = Color.white;
        previewLabelText.alignment = TextAnchor.MiddleCenter;

        // Preview container
        var previewObj = new GameObject("PreviewContainer", typeof(RectTransform));
        previewObj.transform.SetParent(panelObj.transform, false);
        previewContainer = previewObj.GetComponent<RectTransform>();
        previewContainer.anchorMin = new Vector2(0.5f, 1);
        previewContainer.anchorMax = new Vector2(0.5f, 1);
        previewContainer.pivot = new Vector2(0.5f, 1);
        previewContainer.sizeDelta = new Vector2(350, 80);
        previewContainer.anchoredPosition = new Vector2(0, -240);

        // Save button
        var saveBtnObj = new GameObject("SaveButton", typeof(RectTransform), typeof(Image), typeof(Button));
        saveBtnObj.transform.SetParent(panelObj.transform, false);
        var saveBtnRT = saveBtnObj.GetComponent<RectTransform>();
        saveBtnRT.anchorMin = new Vector2(0.5f, 0);
        saveBtnRT.anchorMax = new Vector2(0.5f, 0);
        saveBtnRT.pivot = new Vector2(0.5f, 0);
        saveBtnRT.sizeDelta = new Vector2(120, 36);
        saveBtnRT.anchoredPosition = new Vector2(0, 20);
        var saveBtnImg = saveBtnObj.GetComponent<Image>();
        saveBtnImg.color = new Color(0.2f, 0.6f, 0.2f, 1);
        saveButton = saveBtnObj.GetComponent<Button>();
        saveButton.onClick.AddListener(OnSaveClicked);
        var saveBtnTextObj = new GameObject("Text", typeof(RectTransform));
        saveBtnTextObj.transform.SetParent(saveBtnObj.transform, false);
        var saveBtnText = saveBtnTextObj.AddComponent<Text>();
        saveBtnText.text = "Save";
        saveBtnText.font = UIFactory.BuiltinFont;
        saveBtnText.fontSize = 16;
        saveBtnText.color = Color.white;
        saveBtnText.alignment = TextAnchor.MiddleCenter;
        var saveBtnTextRT = saveBtnTextObj.GetComponent<RectTransform>();
        saveBtnTextRT.anchorMin = Vector2.zero;
        saveBtnTextRT.anchorMax = Vector2.one;
        saveBtnTextRT.offsetMin = Vector2.zero;
        saveBtnTextRT.offsetMax = Vector2.zero;

        // Close button (hamburger style, top right)
        var closeBtnObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(panelObj.transform, false);
        var closeBtnRT = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRT.anchorMin = new Vector2(1, 1);
        closeBtnRT.anchorMax = new Vector2(1, 1);
        closeBtnRT.pivot = new Vector2(1, 1);
        closeBtnRT.sizeDelta = new Vector2(32, 32);
        closeBtnRT.anchoredPosition = new Vector2(-8, -8);
        var closeBtnImg = closeBtnObj.GetComponent<Image>();
        closeBtnImg.color = new Color(0.2f, 0.2f, 0.2f, 1);
        closeButton = closeBtnObj.GetComponent<Button>();
        closeButton.onClick.AddListener(Hide);
        var closeBtnTextObj = new GameObject("Text", typeof(RectTransform));
        closeBtnTextObj.transform.SetParent(closeBtnObj.transform, false);
        var closeBtnText = closeBtnTextObj.AddComponent<Text>();
        closeBtnText.text = "✕";
        closeBtnText.font = UIFactory.BuiltinFont;
        closeBtnText.fontSize = 18;
        closeBtnText.color = Color.white;
        closeBtnText.alignment = TextAnchor.MiddleCenter;
        var closeBtnTextRT = closeBtnTextObj.GetComponent<RectTransform>();
        closeBtnTextRT.anchorMin = Vector2.zero;
        closeBtnTextRT.anchorMax = Vector2.one;
        closeBtnTextRT.offsetMin = Vector2.zero;
        closeBtnTextRT.offsetMax = Vector2.zero;

        UpdatePreview();
    }

    private InputField CreateLabeledInput(Transform parent, string label, string value, float y)
    {
        var labelObj = new GameObject(label + "Label", typeof(RectTransform), typeof(Text));
        labelObj.transform.SetParent(parent, false);
        var labelRT = labelObj.GetComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0, 1);
        labelRT.anchorMax = new Vector2(0, 1);
        labelRT.pivot = new Vector2(0, 1);
        labelRT.sizeDelta = new Vector2(120, 30);
        labelRT.anchoredPosition = new Vector2(20, -y);
        var labelText = labelObj.GetComponent<Text>();
        labelText.text = label;
        labelText.font = UIFactory.BuiltinFont;
        labelText.fontSize = 14;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleLeft;

        var inputObj = new GameObject(label + "Input", typeof(RectTransform), typeof(Image), typeof(InputField));
        inputObj.transform.SetParent(parent, false);
        var inputRT = inputObj.GetComponent<RectTransform>();
        inputRT.anchorMin = new Vector2(0, 1);
        inputRT.anchorMax = new Vector2(0, 1);
        inputRT.pivot = new Vector2(0, 1);
        inputRT.sizeDelta = new Vector2(80, 30);
        inputRT.anchoredPosition = new Vector2(150, -y);
        var inputImg = inputObj.GetComponent<Image>();
        inputImg.color = new Color(0.18f, 0.18f, 0.18f, 1);
        var input = inputObj.GetComponent<InputField>();
        input.text = value;
        // Create a child Text component for the InputField
        var textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(inputObj.transform, false);
        var textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        var text = textObj.GetComponent<Text>();
        text.font = UIFactory.BuiltinFont;
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        input.textComponent = text;
        return input;
    }

    private void UpdatePreview()
    {
        foreach (Transform child in previewContainer)
            Destroy(child.gameObject);
        int count = Mathf.Min(6, columns * rows);
        float spacing = 8f;
        float rectW = Mathf.Min(60, cellWidth);
        float rectH = 40f;
        for (int i = 0; i < count; i++)
        {
            var rectObj = new GameObject($"GridRect{i+1}", typeof(RectTransform), typeof(Image));
            rectObj.transform.SetParent(previewContainer, false);
            var rectRT = rectObj.GetComponent<RectTransform>();
            rectRT.sizeDelta = new Vector2(rectW, rectH);
            rectRT.anchorMin = rectRT.anchorMax = rectRT.pivot = new Vector2(0, 1);
            float x = (i % columns) * (rectW + spacing);
            float y = -(i / columns) * (rectH + spacing);
            rectRT.anchoredPosition = new Vector2(x, y);
            var img = rectObj.GetComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        }
    }

    private void OnSaveClicked()
    {
        OnSave?.Invoke(columns, rows, cellWidth);
        Hide();
    }

    public void Hide()
    {
        if (overlayObj != null)
            Destroy(overlayObj);
        Destroy(gameObject);
    }
} 