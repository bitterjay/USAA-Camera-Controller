using UnityEngine;

public class GamepadDebug : MonoBehaviour
{
    private InputActions inputActions;

    private void Awake()
    {
        inputActions = new InputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        Debug.Log("Controller Test Enabled");
        inputActions.GameController.selectLeftCamera.performed += OnSelectLeftCamera;
        inputActions.GameController.selectRightCamera.performed += OnSelectRightCamera;
    }

    private void OnDisable()
    {
        inputActions.GameController.selectLeftCamera.performed -= OnSelectLeftCamera;
        inputActions.GameController.selectRightCamera.performed -= OnSelectRightCamera;
        inputActions.Disable();
    }

    private void OnSelectLeftCamera(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        Debug.Log("selectLeftCamera pressed");
    }

    private void OnSelectRightCamera(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        Debug.Log("selectRightCamera pressed");
    }

    private void Update()
    {
        Vector2 move = inputActions.GameController.moveCamera.ReadValue<Vector2>();
        if (Mathf.Abs(move.x) > 0.3f || Mathf.Abs(move.y) > 0.3f)
        {
            Debug.Log($"Left Stick: {move}");
        }
    }
} 