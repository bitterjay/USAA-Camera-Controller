using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Klak.Ndi;
using System;

/// <summary>
/// Registry that manages NDI camera discovery and status
/// </summary>
public class CameraRegistry
{
    // List of available cameras
    public List<CameraInfo> cameras = new List<CameraInfo>();
    
    // NDI Resources
    private NdiResources ndiResources;
    
    // Events
    public event Action<CameraInfo> OnCameraAdded;
    public event Action<CameraInfo> OnCameraRemoved;
    public event Action OnCamerasReordered;
    public event Action<CameraInfo, bool> OnFeedStatusChanged; // bool: true=available, false=lost
    public event Action<CameraInfo> OnCameraSelected; // Added event for camera selection
    
    // Properties
    public IReadOnlyList<CameraInfo> Cameras => cameras;
    public CameraInfo ActiveCamera => cameras.FirstOrDefault(c => c.isActive);
    
    private readonly Dictionary<CameraInfo, float> lastReconnectAttempt = new Dictionary<CameraInfo, float>();
    private const float RECONNECT_INTERVAL = 3f; // seconds
    
    /// <summary>
    /// Initialize the camera registry
    /// </summary>
    /// <param name="resources">Optional NDI resources</param>
    public CameraRegistry(NdiResources resources = null)
    {
        ndiResources = resources;
        
        // If no resources provided, try to find them
        if (ndiResources == null)
        {
            // Try to find an existing instance
            var found = Resources.FindObjectsOfTypeAll<NdiResources>();
            if (found.Length > 0) ndiResources = found[0];
            
#if UNITY_EDITOR
            if (ndiResources == null)
            {
                var guids = UnityEditor.AssetDatabase.FindAssets("t:Klak.Ndi.NdiResources");
                if (guids.Length > 0)
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    ndiResources = UnityEditor.AssetDatabase.LoadAssetAtPath<NdiResources>(path);
                }
            }
#endif
        }
    }
    
    /// <summary>
    /// Automatically discover and add all available NDI sources
    /// </summary>
    public void AutoDiscoverCameras()
    {
        // Clear existing cameras
        cameras.Clear();
        
        // Add all available sources
        foreach (var sourceName in NdiFinder.sourceNames)
        {
            AddCamera(sourceName, sourceName);
            Debug.Log($"Added camera: {sourceName}");
        }
        
        // Set the first camera as active if we have any
        if (cameras.Count > 0)
        {
            SetActiveCamera(cameras[0]);
            Debug.Log($"First camera: {cameras[0].niceName} (source: {cameras[0].sourceName})");
        }
    }
    
    /// <summary>
    /// Adds a new camera to the registry
    /// </summary>
    /// <param name="sourceName">NDI source name</param>
    /// <param name="niceName">Display name (defaults to source name)</param>
    /// <returns>The created camera info</returns>
    public CameraInfo AddCamera(string sourceName, string niceName = null)
    {
        if (string.IsNullOrEmpty(niceName))
            niceName = sourceName;
            
        // Check if this source already exists
        if (cameras.Any(c => c.sourceName == sourceName))
            return cameras.First(c => c.sourceName == sourceName);
            
        // Create the camera info
        var camera = new CameraInfo
        {
            sourceName = sourceName,
            niceName = niceName
        };
        
        // Create the NDI receiver
        var receiverObj = new GameObject("Receiver_" + sourceName);
        receiverObj.hideFlags = HideFlags.DontSave; // Don't show in hierarchy
        camera.receiver = receiverObj.AddComponent<NdiReceiver>();
        camera.receiver.ndiName = sourceName;
        
        // Set resources if available
        if (ndiResources != null)
            camera.receiver.SetResources(ndiResources);
            
        // Add to list
        cameras.Add(camera);
        
        // Trigger event
        OnCameraAdded?.Invoke(camera);
        
        return camera;
    }
    
    /// <summary>
    /// Removes a camera from the registry
    /// </summary>
    /// <param name="camera">The camera to remove</param>
    public void RemoveCamera(CameraInfo camera)
    {
        // Clean up receiver
        if (camera.receiver != null)
        {
            UnityEngine.Object.Destroy(camera.receiver.gameObject);
            camera.receiver = null;
        }
        
        // Remove from list
        cameras.Remove(camera);
        
        // Trigger event
        OnCameraRemoved?.Invoke(camera);
    }
    
    /// <summary>
    /// Checks for and adds any new NDI sources
    /// </summary>
    /// <returns>True if any new cameras were added</returns>
    public bool RefreshSources()
    {
        // Get existing sources
        var existing = new HashSet<string>();
        foreach (var cam in cameras)
            existing.Add(cam.sourceName);
            
        bool addedAny = false;
        CameraInfo firstNewCamera = null;
        
        // Check for new sources
        foreach (var src in NdiFinder.sourceNames)
        {
            if (!existing.Contains(src))
            {
                var newCam = AddCamera(src, src);
                if (firstNewCamera == null)
                    firstNewCamera = newCam;
                    
                addedAny = true;
            }
        }
        
        // If we've added cameras and have no active camera, set the first new one
        if (addedAny && ActiveCamera == null && firstNewCamera != null)
        {
            SetActiveCamera(firstNewCamera);
        }
        
        return addedAny;
    }
    
    /// <summary>
    /// Sets a camera as the active camera
    /// </summary>
    /// <param name="camera">The camera to activate</param>
    public void SetActiveCamera(CameraInfo camera)
    {
        Debug.Log($"Setting active camera: {(camera != null ? camera.niceName : "null")}");

        var viscaController = UnityEngine.Object.FindObjectOfType<ViscaControlPanelController>();
        if (viscaController != null) {
            viscaController.InitializePTZControls(camera);
        } else {
            Debug.LogWarning("ViscaControlPanelController not found in scene");
        }
    }
   
    public void UpdateCameraName(CameraInfo camera, string newName)
    {
        if (camera != null)
            camera.niceName = newName;
    }
    
    /// <summary>
    /// Reorders a camera to a new position in the list
    /// </summary>
    /// <param name="camera">The camera to move</param>
    /// <param name="newIndex">The target position</param>
    public void ReorderCamera(CameraInfo camera, int newIndex)
    {
        int currentIndex = cameras.IndexOf(camera);
        if (currentIndex < 0 || currentIndex == newIndex)
            return;
            
        // Remove and insert at new position
        cameras.RemoveAt(currentIndex);
        cameras.Insert(newIndex, camera);
        
        // Trigger event
        OnCamerasReordered?.Invoke();
    }
    
    /// <summary>
    /// Checks the feed status for all cameras and attempts to reconnect if the feed is lost
    /// </summary>
    public void CheckFeeds()
    {
        float now = Time.time;
        foreach (var cam in cameras)
        {
            bool available = cam.receiver != null && cam.receiver.texture != null;
            if (!lastReconnectAttempt.ContainsKey(cam))
                lastReconnectAttempt[cam] = 0f;

            if (!available)
            {
                if (lastReconnectAttempt[cam] + RECONNECT_INTERVAL < now)
                {
                    Debug.LogWarning($"NDI feed lost for {cam.sourceName}, attempting to reconnect...");
                    lastReconnectAttempt[cam] = now;

                    // Destroy the old receiver
                    if (cam.receiver != null)
                    {
                        UnityEngine.Object.Destroy(cam.receiver.gameObject);
                        cam.receiver = null;
                    }

                    // Recreate the receiver
                    var receiverObj = new GameObject("Receiver_" + cam.sourceName);
                    receiverObj.hideFlags = HideFlags.DontSave;
                    cam.receiver = receiverObj.AddComponent<NdiReceiver>();
                    cam.receiver.ndiName = cam.sourceName;
                    if (ndiResources != null)
                        cam.receiver.SetResources(ndiResources);
                }
            }
            // Fire event if feed status changed
            if (cam.isFeedAvailable != available)
            {
                cam.isFeedAvailable = available;
                OnFeedStatusChanged?.Invoke(cam, available);
            }
        }
    }
    
    /// <summary>
    /// Updates the VISCA connection settings for a camera
    /// </summary>
    /// <param name="camera">The camera to update</param>
    /// <param name="viscaIp">New VISCA IP address</param>
    /// <param name="viscaPort">New VISCA port</param>
    public void UpdateCameraConnection(CameraInfo camera, string viscaIp)
    {
        if (camera == null)
            return;
            
        camera.viscaIp = viscaIp;
        
        // If this is the active camera, make sure to refresh the controller
        if (camera.isActive)
        {
            OnCameraSelected?.Invoke(camera);
        }
    }
} 