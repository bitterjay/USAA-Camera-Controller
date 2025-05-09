using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Handles calculation and configuration of the camera grid layout
/// </summary>
public class GridLayoutController
{
    private readonly GridLayoutGroup gridGroup;
    private readonly LayoutSettings settings;
    
    /// <summary>
    /// Creates a new grid layout controller
    /// </summary>
    /// <param name="gridLayoutGroup">The Unity GridLayoutGroup component to configure</param>
    /// <param name="layoutSettings">Layout settings to use</param>
    public GridLayoutController(GridLayoutGroup gridLayoutGroup, LayoutSettings layoutSettings)
    {
        gridGroup = gridLayoutGroup;
        settings = layoutSettings;
        
        // Initial setup
        ConfigureGridGroup();
    }
    
    /// <summary>
    /// Configures the grid layout based on settings and camera count
    /// </summary>
    /// <param name="cameraCount">Number of cameras in the grid</param>
    public void ConfigureLayout(int cameraCount)
    {
        if (cameraCount == 0) return;
        
        // Determine columns and rows based on settings
        int columns, rows;
        
        if (settings.AutoLayoutGrid)
        {
            // Auto layout mode - calculate based on number of cameras
            columns = Mathf.Min(settings.MaxAutoColumns, cameraCount);
            rows = Mathf.CeilToInt(cameraCount / (float)columns);
            rows = Mathf.Min(rows, settings.MaxAutoRows);
        }
        else
        {
            // Manual layout mode - use user-defined values
            columns = Mathf.Max(1, settings.ManualColumns);
            rows = Mathf.Max(1, settings.ManualRows);
        }
        
        // Determine cell size based on settings
        float cellWidth, cellHeight;
        
        if (settings.AutoSizeCells)
        {
            // Auto size mode - calculate based on screen size
            float gameWidth = Screen.width;
            float usableWidth = gameWidth - 2 * settings.Padding;
            float totalSpacing = (columns - 1) * settings.Padding;
            cellWidth = (usableWidth - totalSpacing) / columns;
            float maxAllowed = (gameWidth - 2 * settings.Padding);
            cellWidth = Mathf.Min(cellWidth, maxAllowed);
            cellHeight = cellWidth / settings.AspectRatio;
        }
        else
        {
            // Manual size mode - use user-defined values
            cellWidth = settings.ManualCellWidth;
            
            // Determine height based on aspect ratio setting
            if (settings.MaintainAspectRatio)
            {
                cellHeight = cellWidth / settings.AspectRatio;
            }
            else
            {
                cellHeight = settings.ManualCellHeight;
            }
        }

        // Apply cell size and column constraint to the grid layout
        gridGroup.cellSize = new Vector2(cellWidth, cellHeight);
        gridGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridGroup.constraintCount = columns;
    }
    
    /// <summary>
    /// Sets up the initial grid layout parameters
    /// </summary>
    private void ConfigureGridGroup()
    {
        gridGroup.padding = new RectOffset(0, 0, (int)settings.Padding, (int)settings.Padding);
        gridGroup.spacing = new Vector2(settings.Padding, settings.Padding);
        gridGroup.childAlignment = TextAnchor.UpperLeft;
    }
} 