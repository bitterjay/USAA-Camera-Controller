using UnityEngine;
using UnityEngine.UI;
using Klak.Ndi;

/// <summary>
/// Information about an NDI camera source
/// </summary>
[System.Serializable]
public class CameraInfo
{
    /// <summary>
    /// The original NDI source name (fixed)
    /// </summary>
    public string sourceName;
    
    /// <summary>
    /// User-defined display name for the camera
    /// </summary>
    public string niceName;
    
    /// <summary>
    /// NDI receiver component for this camera
    /// </summary>
    public NdiReceiver receiver;
    
    /// <summary>
    /// UI element displaying the camera
    /// </summary>
    public RawImage rawImage;
    
    /// <summary>
    /// Reference to the GameObject representing this camera in the UI
    /// </summary>
    public GameObject tileObject;
    
    /// <summary>
    /// Whether this camera is currently active
    /// </summary>
    public bool isActive;
    
    /// <summary>
    /// Whether the NDI feed is currently available
    /// </summary>
    public bool isFeedAvailable;
} 