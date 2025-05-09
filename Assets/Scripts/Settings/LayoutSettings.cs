using UnityEngine;

[CreateAssetMenu(fileName = "LayoutSettings", menuName = "NDI Viewer/Layout Settings")]
public class LayoutSettings : ScriptableObject
{
    [Header("Layout")]
    [SerializeField] private float padding = 10f;
    [SerializeField] private float aspectRatio = 16f / 9f;

    [Header("Grid Layout")]
    [Tooltip("When enabled, rows and columns are automatically calculated")]
    [SerializeField] private bool autoLayoutGrid = true;
    [SerializeField] private int manualColumns = 3;
    [SerializeField] private int manualRows = 2;
    [SerializeField] private int maxAutoColumns = 4;
    [SerializeField] private int maxAutoRows = 2;

    [Header("Cell Size")]
    [Tooltip("When enabled, cell size is calculated automatically based on screen size")]
    [SerializeField] private bool autoSizeCells = true;
    [SerializeField] private float manualCellWidth = 320f;
    [SerializeField] private bool maintainAspectRatio = true;
    [SerializeField] private float manualCellHeight = 180f;

    [Header("Tile Appearance")]
    [SerializeField] private Color tileBorderColor = Color.white;
    [SerializeField] private Color activeTileBorderColor = Color.green;
    [SerializeField] private float tileBorderWidth = 1f;

    [Header("Settings Panel Item Appearance")]
    [SerializeField] private float settingsItemWidth = 200f;
    [SerializeField] private float settingsItemHeight = 24f;
    [SerializeField] private Color settingsItemBackground = Color.white;
    [SerializeField] private Color settingsItemTextColor = Color.black;

    [Header("Settings Panel Appearance")]
    [SerializeField] private Color settingsPanelBackground = new Color(0,0,0,1f);
    [SerializeField] private Color settingsScrollBackground = Color.white;
    [SerializeField] private float settingsScrollVerticalPadding = 40f;
    [SerializeField] private float settingsPanelWidth = 300f;
    [SerializeField] private float settingsPanelHeight = 400f;
    [SerializeField] private float cameraSettingsPopupWidth = 400f;
    [SerializeField] private float cameraSettingsPopupHeight = 300f;

    // Properties
    public float Padding => padding;
    public float AspectRatio => aspectRatio;
    
    // Grid Layout Properties
    // public bool AutoLayoutGrid => autoLayoutGrid;
    public int ManualColumns => manualColumns;
    public int ManualRows => manualRows;
    // public int MaxAutoColumns => maxAutoColumns;
    // public int MaxAutoRows => maxAutoRows;
    
    // Cell Size Properties
    public bool AutoSizeCells => autoSizeCells;
    public float ManualCellWidth => manualCellWidth;
    public bool MaintainAspectRatio => maintainAspectRatio;
    public float ManualCellHeight => manualCellHeight;
    
    // Tile Appearance Properties
    public Color TileBorderColor => tileBorderColor;
    public Color ActiveTileBorderColor => activeTileBorderColor;
    public float TileBorderWidth => tileBorderWidth;
    
    // Settings Panel Properties
    public float SettingsItemWidth => settingsItemWidth;
    public float SettingsItemHeight => settingsItemHeight;
    public Color SettingsItemBackground => settingsItemBackground;
    public Color SettingsItemTextColor => settingsItemTextColor;
    public Color SettingsPanelBackground => settingsPanelBackground;
    public Color SettingsScrollBackground => settingsScrollBackground;
    public float SettingsScrollVerticalPadding => settingsScrollVerticalPadding;
    public float SettingsPanelWidth => settingsPanelWidth;
    public float SettingsPanelHeight => settingsPanelHeight;
    public float CameraSettingsPopupWidth => cameraSettingsPopupWidth;
    public float CameraSettingsPopupHeight => cameraSettingsPopupHeight;
} 