using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PTZ.UI {

public class PresetSnapshotGrid : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject panelObj;
    private RawImage[] images = new RawImage[4];
    private Text[] labels = new Text[4];
    private CameraInfo currentCamera;
    private RectTransform panelRT;
    private Vector2 dragOffset;
    private bool isDragging = false;
    private Image panelImg;
    private Color normalColor = new Color(0, 0, 0, 0.7f);
    private Color hoverColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
    private GameObject borderObj;
    private int selectedPresetIndex = 0;
    private Image[] cellBorders = new Image[4];
    private Color selectedColor = new Color(0.2f, 1f, 0.2f, 0.8f); // Green
    private Color unselectedColor = new Color(1, 1, 1, 0.0f);
    private bool isActiveGrid = false;

    private void Awake()
    {
        CreateGridUI();
        Hide();
    }

    private void CreateGridUI()
    {
        // Panel
        panelObj = new GameObject("PresetSnapshotGridPanel", typeof(RectTransform), typeof(Image));
        panelObj.transform.SetParent(transform, false);
        panelRT = panelObj.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(1, 0.5f);
        panelRT.anchorMax = new Vector2(1, 0.5f);
        panelRT.pivot = new Vector2(1, 0.5f);
        panelRT.sizeDelta = new Vector2(400, 230); // 2x2 grid, 16:9 aspect
        panelRT.anchoredPosition = new Vector2(-20, 0);
        panelImg = panelObj.GetComponent<Image>();
        panelImg.color = normalColor;
        panelImg.raycastTarget = true;
        // Add EventTrigger for drag
        var trigger = panelObj.AddComponent<EventTrigger>();
        AddDragEvents(trigger);

        float cellW = 180, cellH = 101.25f; // 16:9
        float spacing = 10f;
        for (int i = 0; i < 4; i++)
        {
            var cell = new GameObject($"PresetCell{i+1}", typeof(RectTransform));
            cell.transform.SetParent(panelObj.transform, false);
            var cellRT = cell.GetComponent<RectTransform>();
            int row = i / 2, col = i % 2;
            cellRT.sizeDelta = new Vector2(cellW, cellH);
            cellRT.anchorMin = cellRT.anchorMax = cellRT.pivot = new Vector2(0, 1);
            cellRT.anchoredPosition = new Vector2(col * (cellW + spacing) + 20, -row * (cellH + spacing) - 20);
            // Border for selection (unique per cell)
            var cellBorderObj = new GameObject($"CellBorder{i+1}", typeof(RectTransform), typeof(Image));
            cellBorderObj.transform.SetParent(cell.transform, false);
            var cellBorderRT = cellBorderObj.GetComponent<RectTransform>();
            cellBorderRT.anchorMin = Vector2.zero;
            cellBorderRT.anchorMax = Vector2.one;
            cellBorderRT.offsetMin = new Vector2(-3, -3);
            cellBorderRT.offsetMax = new Vector2(3, 3);
            var cellBorderImg = cellBorderObj.GetComponent<Image>();
            cellBorderImg.color = i == selectedPresetIndex ? selectedColor : unselectedColor;
            cellBorderImg.raycastTarget = false;
            cellBorders[i] = cellBorderImg;
            // RawImage
            var imgObj = new GameObject("Image", typeof(RectTransform), typeof(RawImage));
            imgObj.transform.SetParent(cell.transform, false);
            var imgRT = imgObj.GetComponent<RectTransform>();
            imgRT.anchorMin = Vector2.zero;
            imgRT.anchorMax = Vector2.one;
            imgRT.offsetMin = Vector2.zero;
            imgRT.offsetMax = Vector2.zero;
            var rawImg = imgObj.GetComponent<RawImage>();
            rawImg.color = Color.white;
            images[i] = rawImg;
            var labelObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObj.transform.SetParent(cell.transform, false);
            var labelRT = labelObj.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 1);
            labelRT.anchorMax = new Vector2(0, 1);
            labelRT.pivot = new Vector2(0, 1);
            labelRT.sizeDelta = new Vector2(24, 24);
            labelRT.anchoredPosition = new Vector2(4, -4);
            var label = labelObj.GetComponent<Text>();
            label.text = (i + 1).ToString();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 18;
            label.color = Color.white;
            label.alignment = TextAnchor.UpperLeft;
            labels[i] = label;
        }
        UpdateSelectedHighlight();
    }

    private void AddDragEvents(EventTrigger trigger)
    {
        var beginDrag = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
        beginDrag.callback.AddListener((data) => { OnBeginDrag((PointerEventData)data); });
        trigger.triggers.Add(beginDrag);
        var drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        drag.callback.AddListener((data) => { OnDrag((PointerEventData)data); });
        trigger.triggers.Add(drag);
        var endDrag = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
        endDrag.callback.AddListener((data) => { OnEndDrag((PointerEventData)data); });
        trigger.triggers.Add(endDrag);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        Vector2 pointerLocalInParent;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRT.parent as RectTransform, eventData.position, eventData.pressEventCamera, out pointerLocalInParent);
        dragOffset = pointerLocalInParent - panelRT.anchoredPosition;
        // Bring to front
        panelObj.transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        Vector2 pointerLocalInParent;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRT.parent as RectTransform, eventData.position, eventData.pressEventCamera, out pointerLocalInParent))
        {
            panelRT.anchoredPosition = pointerLocalInParent - dragOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        panelImg.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        panelImg.color = normalColor;
    }

    public void ShowForCamera(CameraInfo camera)
    {
        if (camera == null)
        {
            Hide();
            return;
        }
        currentCamera = camera;
        var arr = new Texture2D[4];
        for (int i = 0; i < 4; i++)
        {
            arr[i] = PresetSnapshotManager.GetSnapshot(camera, i);
            images[i].texture = arr[i];
            images[i].color = arr[i] ? Color.white : new Color(0.2f,0.2f,0.2f,1f);
        }
        panelObj.SetActive(true);
    }

    public void Hide()
    {
        if (panelObj != null)
            panelObj.SetActive(false);
    }

    public void SetSelectedPreset(int index)
    {
        selectedPresetIndex = Mathf.Clamp(index, 0, 3);
        UpdateSelectedHighlight();
    }

    public void SetGridActive(bool active)
    {
        isActiveGrid = active;
        UpdateSelectedHighlight();
    }

    private void UpdateSelectedHighlight()
    {
        for (int i = 0; i < 4; i++)
        {
            if (cellBorders[i] != null)
            {
                if (isActiveGrid && i == selectedPresetIndex)
                    cellBorders[i].color = selectedColor;
                else
                    cellBorders[i].color = unselectedColor;
            }
        }
    }

    public void UpdateGridSize(Vector2 newSize)
    {
        if (panelRT == null) return;
        panelRT.sizeDelta = newSize;
        // Calculate cell size and spacing for 2x2 grid, 16:9 aspect
        float spacing = 10f;
        float gridW = newSize.x;
        float gridH = newSize.y;
        float cellW = (gridW - 3 * spacing) / 2f;
        float cellH = (gridH - 3 * spacing) / 2f;
        int cellIdx = 0;
        foreach (Transform cell in panelObj.transform)
        {
            if (cellIdx >= 4) break;
            var cellRT = cell as RectTransform;
            int row = cellIdx / 2, col = cellIdx % 2;
            cellRT.sizeDelta = new Vector2(cellW, cellH);
            cellRT.anchoredPosition = new Vector2(col * (cellW + spacing) + spacing, -row * (cellH + spacing) - spacing);
            cellIdx++;
        }
    }
}

} 