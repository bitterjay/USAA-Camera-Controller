using UnityEngine;
using System.Linq;

public class GamepadCameraCycler : MonoBehaviour
{
    private InputActions inputActions;
    private CameraRegistry cameraRegistry;
    private ViscaControlPanelController viscaController;
    private Vector2 lastMoveDirection = Vector2.zero;
    private bool isMoving = false;
    private const float DEADZONE = 0.3f;

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
        viscaController = FindObjectOfType<ViscaControlPanelController>();
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

    private void Update()
    {
        if (viscaController == null)
            viscaController = FindObjectOfType<ViscaControlPanelController>();

        Vector2 move = inputActions.GameController.moveCamera.ReadValue<Vector2>();
        if (move.magnitude > DEADZONE)
        {
            Vector2 direction = new Vector2(Mathf.Round(move.x), Mathf.Round(move.y));
            if (direction != lastMoveDirection)
            {
                lastMoveDirection = direction;
                isMoving = true;
                // Diagonal
                if (direction.x < 0 && direction.y > 0)
                    viscaController?.PanTiltUpLeft();
                else if (direction.x > 0 && direction.y > 0)
                    viscaController?.PanTiltUpRight();
                else if (direction.x < 0 && direction.y < 0)
                    viscaController?.PanTiltDownLeft();
                else if (direction.x > 0 && direction.y < 0)
                    viscaController?.PanTiltDownRight();
                // Cardinal
                else if (direction.x < 0)
                    viscaController?.PanLeft();
                else if (direction.x > 0)
                    viscaController?.PanRight();
                else if (direction.y > 0)
                    viscaController?.TiltUp();
                else if (direction.y < 0)
                    viscaController?.TiltDown();
            }
        }
        else if (isMoving)
        {
            isMoving = false;
            lastMoveDirection = Vector2.zero;
            viscaController?.Stop();
        }
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
        viscaController = FindObjectOfType<ViscaControlPanelController>();
        if (viscaController != null)
        {
            viscaController.OnCameraSelected(cameras[newIndex]);
            Debug.Log("ViscaControlPanelController.OnCameraSelected invoked from gamepad selection.");
        }
    }
} 