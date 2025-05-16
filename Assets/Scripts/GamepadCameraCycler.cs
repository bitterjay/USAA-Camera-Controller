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
    private Vector2 lastZoomDirection = Vector2.zero;
    private bool isZooming = false;
    private const float ZOOM_DEADZONE = 0.3f;

    private void Awake()
    {
        inputActions = new InputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
      
        inputActions.GameController.selectLeftCamera.performed += OnSelectLeftCamera;
        inputActions.GameController.selectRightCamera.performed += OnSelectRightCamera;
        inputActions.GameController.SavePreset.performed += OnSavePreset;
        inputActions.GameController.focusOnePush.performed += OnFocusOnePush;
        inputActions.GameController.whiteBalanceOnePush.performed += OnWhiteBalanceOnePush;
        inputActions.GameController.showControls.performed += OnShowControls;

        // Get the CameraRegistry instance from the NDIViewerApp
        cameraRegistry = FindObjectOfType<NDIViewerApp>()?.GetCameraRegistry();
        viscaController = FindObjectOfType<ViscaControlPanelController>();
    }

    private void OnDisable()
    {
        inputActions.GameController.selectLeftCamera.performed -= OnSelectLeftCamera;
        inputActions.GameController.selectRightCamera.performed -= OnSelectRightCamera;
        inputActions.GameController.SavePreset.performed -= OnSavePreset;
        inputActions.GameController.focusOnePush.performed -= OnFocusOnePush;
        inputActions.GameController.whiteBalanceOnePush.performed -= OnWhiteBalanceOnePush;
        inputActions.GameController.showControls.performed -= OnShowControls;
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

    private void OnSavePreset(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (cameraRegistry == null || cameraRegistry.ActiveCamera == null)
            return;
        // Ensure overlay exists
        if (CameraFullscreenOverlay.Instance == null)
        {
            var go = new GameObject("CameraFullscreenOverlay");
            go.AddComponent<CameraFullscreenOverlay>();
        }
        CameraFullscreenOverlay.Instance.Toggle(cameraRegistry.ActiveCamera);
    }

    private void OnFocusOnePush(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        viscaController?.FocusOnePush();
    }

    private void OnWhiteBalanceOnePush(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        viscaController?.WhiteBalanceOnePush();
    }

    private void OnShowControls(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        Debug.Log("ShowControls triggered");
        if (ControlsOverlay.Instance == null)
        {
            var go = new GameObject("ControlsOverlay");
            go.AddComponent<ControlsOverlay>();
        }
        ControlsOverlay.Instance.Toggle();
    }

    private void Update()
    {
        if (viscaController == null)
            viscaController = FindObjectOfType<ViscaControlPanelController>();

        // Determine speed based on right trigger
        float augment = inputActions.GameController.augmentSpeed.ReadValue<float>();
        bool fast = augment > 0.5f;
        byte panSpeed = fast ? ViscaCommands.MAX_PAN_SPEED : ViscaCommands.DEFAULT_PAN_SPEED;
        byte tiltSpeed = fast ? ViscaCommands.MAX_PAN_SPEED : ViscaCommands.DEFAULT_PAN_SPEED;
        byte zoomSpeed = fast ? ViscaCommands.MAX_ZOOM_SPEED : ViscaCommands.DEFAULT_ZOOM_SPEED;

        // Left stick: PTZ
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
                    viscaController?.PanTiltUpLeft(panSpeed, tiltSpeed);
                else if (direction.x > 0 && direction.y > 0)
                    viscaController?.PanTiltUpRight(panSpeed, tiltSpeed);
                else if (direction.x < 0 && direction.y < 0)
                    viscaController?.PanTiltDownLeft(panSpeed, tiltSpeed);
                else if (direction.x > 0 && direction.y < 0)
                    viscaController?.PanTiltDownRight(panSpeed, tiltSpeed);
                // Cardinal
                else if (direction.x < 0)
                    viscaController?.PanLeft(panSpeed);
                else if (direction.x > 0)
                    viscaController?.PanRight(panSpeed);
                else if (direction.y > 0)
                    viscaController?.TiltUp(tiltSpeed);
                else if (direction.y < 0)
                    viscaController?.TiltDown(tiltSpeed);
            }
        }
        else if (isMoving)
        {
            isMoving = false;
            lastMoveDirection = Vector2.zero;
            viscaController?.Stop();
        }

        // Right stick: Zoom
        Vector2 zoom = inputActions.GameController.zoomCamera.ReadValue<Vector2>();
        float zoomY = zoom.y;
        if (Mathf.Abs(zoomY) > ZOOM_DEADZONE)
        {
            Vector2 zoomDirection = new Vector2(0, Mathf.Sign(zoomY));
            if (zoomDirection != lastZoomDirection)
            {
                lastZoomDirection = zoomDirection;
                isZooming = true;
                if (zoomY > 0)
                    viscaController?.ZoomIn(zoomSpeed);
                else if (zoomY < 0)
                    viscaController?.ZoomOut(zoomSpeed);
            }
        }
        else if (isZooming)
        {
            isZooming = false;
            lastZoomDirection = Vector2.zero;
            viscaController?.ZoomStop();
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