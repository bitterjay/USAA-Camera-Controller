using System.Collections.Generic;
using UnityEngine;

public static class PresetSnapshotManager
{
    // Each camera gets an array of 4 snapshots (slots 0-3)
    private static Dictionary<CameraInfo, Texture2D[]> snapshots = new Dictionary<CameraInfo, Texture2D[]>();

    public static void SetSnapshot(CameraInfo camera, int slot, Texture2D tex)
    {
        if (!snapshots.ContainsKey(camera))
            snapshots[camera] = new Texture2D[4];
        snapshots[camera][slot] = tex;
    }

    public static Texture2D GetSnapshot(CameraInfo camera, int slot)
    {
        if (snapshots.TryGetValue(camera, out var arr) && arr[slot] != null)
            return arr[slot];
        return null;
    }
} 