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
        
        // Always use manual layout mode
        int columns = Mathf.Max(1, settings.ManualColumns);
        int rows = Mathf.Max(1, settings.ManualRows);
        
        // Always use manual size mode
        float cellWidth = settings.ManualCellWidth;
        float cellHeight;
        if (settings.MaintainAspectRatio)
        {
            cellHeight = cellWidth / settings.AspectRatio;
        }
        else
        {
            cellHeight = settings.ManualCellHeight;
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