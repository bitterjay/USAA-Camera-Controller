using UnityEngine;
using System.Linq;

public class GamepadCameraCycler : MonoBehaviour
{
    private InputActions inputActions;
    private CameraRegistry cameraRegistry;

    private void Awake()
    {
        inputActions = new InputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
      
        inputActions.GameController.selectLeftCamera.performed += OnSelectLeftCamera;
        inputActions.GameController.selectRightCamera.performed += OnSelectRightCamera;

        // Get the CameraRegistry instance from the NDIViewerApp
        cameraRegistry = FindObjectOfType<NDIViewerApp>()?.GetCameraRegistry();
    }

    private void OnDisable()
    {
        inputActions.GameController.selectLeftCamera.performed -= OnSelectLeftCamera;
        inputActions.GameController.selectRightCamera.performed -= OnSelectRightCamera;
        inputActions.Disable();
    }

    private void OnSelectLeftCamera(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        CycleCamera(-1);
    }

    private void OnSelectRightCamera(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        CycleCamera(1);
    }

    private void CycleCamera(int direction)
    {
        // Always get the latest registry and app
        var ndiApp = FindObjectOfType<NDIViewerApp>();
        if (ndiApp != null)
            cameraRegistry = ndiApp.GetCameraRegistry();

        if (cameraRegistry == null || cameraRegistry.Cameras.Count == 0)
            return;

        var cameras = cameraRegistry.Cameras.ToList();
        int count = cameras.Count;
        int currentIndex = cameras.IndexOf(cameraRegistry.ActiveCamera);

        int newIndex;
        if (currentIndex < 0)
        {
            newIndex = 0;
            Debug.Log("CameraRegistry: " + cameraRegistry);
            ndiApp?.SetActiveCamera(cameras[newIndex]);
            Debug.Log($"No camera was active. Selected first camera: {cameras[newIndex].niceName}");
        }
        else
        {
            newIndex = (currentIndex + direction + count) % count;
            Debug.Log("CameraRegistry: " + cameraRegistry);
            ndiApp?.SetActiveCamera(cameras[newIndex]);
            Debug.Log($"Switched to camera: {cameras[newIndex].viscaIp}");
        }

        // Update the currentIP in NDIViewerApp to match the selected camera's IP
        if (ndiApp != null && cameras[newIndex] != null)
        {
            // Extract IP from camera name if available
            string extractedIp = null;
            if (!string.IsNullOrEmpty(cameras[newIndex].niceName))
            {
                var ipPattern = @"\b(?:\d{1,3}\.){3}\d{1,3}\b";
                var match = System.Text.RegularExpressions.Regex.Match(cameras[newIndex].niceName, ipPattern);
                if (match.Success)
                {
                    extractedIp = match.Value;
                    Debug.Log($"Extracted IP {extractedIp} from camera name: {cameras[newIndex].niceName}");
                    ndiApp.currentIP = extractedIp;
                }
            }
        }

        // Trigger VISCA logic
        var viscaController = FindObjectOfType<ViscaControlPanelController>();
        if (viscaController != null)
        {
            viscaController.OnCameraSelected(cameras[newIndex]);
            Debug.Log("ViscaControlPanelController.OnCameraSelected invoked from gamepad selection.");
        }
    }
} 