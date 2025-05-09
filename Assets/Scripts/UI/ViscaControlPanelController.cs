using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;

public class ViscaControlPanelController : MonoBehaviour
{
    private ViscaOverIpSender viscaSender;

    private void OnEnable()
    {
        viscaSender = new ViscaOverIpSender("192.168.1.104", 52381);
        
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("UIDocument component not found!");
            return;
        }

        var root = uiDoc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("rootVisualElement is null!");
            return;
        }

        // Pan Controls (custom VisualElements from UXML)
        var panLeftVE = root.Q<VisualElement>("panLeftVE");
        if (panLeftVE != null)
        {
            panLeftVE.RegisterCallback<PointerDownEvent>(async evt => {
                Debug.Log("Pan Left");
                await viscaSender.PanLeft();
            });
            panLeftVE.RegisterCallback<PointerUpEvent>(async evt => {
                Debug.Log("Pan Left Stop");
                await viscaSender.Stop();
            });
            panLeftVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                Debug.Log("Pan Left Stop (Leave)");
                await viscaSender.Stop();
            });
        }

        var panRightVE = root.Q<VisualElement>("panRightVE");
        if (panRightVE != null)
        {
            panRightVE.RegisterCallback<PointerDownEvent>(async evt => {
                Debug.Log("Pan Right");
                await viscaSender.PanRight();
            });
            panRightVE.RegisterCallback<PointerUpEvent>(async evt => {
                Debug.Log("Pan Right Stop");
                await viscaSender.Stop();
            });
            panRightVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                Debug.Log("Pan Right Stop (Leave)");
                await viscaSender.Stop();
            });
        }

        // Tilt Controls (custom VisualElements from UXML)
        var tiltUpVE = root.Q<VisualElement>("tiltUpVE");
        if (tiltUpVE != null)
        {
            tiltUpVE.RegisterCallback<PointerDownEvent>(async evt => {
                Debug.Log("Tilt Up");
                await viscaSender.TiltUp();
            });
            tiltUpVE.RegisterCallback<PointerUpEvent>(async evt => {
                Debug.Log("Tilt Up Stop");
                await viscaSender.Stop();
            });
            tiltUpVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                Debug.Log("Tilt Up Stop (Leave)");
                await viscaSender.Stop();
            });
        }

        var tiltDownVE = root.Q<VisualElement>("tiltDownVE");
        if (tiltDownVE != null)
        {
            tiltDownVE.RegisterCallback<PointerDownEvent>(async evt => {
                Debug.Log("Tilt Down");
                await viscaSender.TiltDown();
            });
            tiltDownVE.RegisterCallback<PointerUpEvent>(async evt => {
                Debug.Log("Tilt Down Stop");
                await viscaSender.Stop();
            });
            tiltDownVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                Debug.Log("Tilt Down Stop (Leave)");
                await viscaSender.Stop();
            });
        }

        // Zoom Controls (custom VisualElements from UXML)
        var zoomInVE = root.Q<VisualElement>("zoomInVE");
        if (zoomInVE != null)
        {
            zoomInVE.RegisterCallback<PointerDownEvent>(async evt => {
                Debug.Log("Zoom In");
                await viscaSender.ZoomIn();
            });
            zoomInVE.RegisterCallback<PointerUpEvent>(async evt => {
                Debug.Log("Zoom In Stop");
                await viscaSender.ZoomStop();
            });
            zoomInVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                Debug.Log("Zoom In Stop (Leave)");
                await viscaSender.ZoomStop();
            });
        }

        var zoomOutVE = root.Q<VisualElement>("zoomOutVE");
        if (zoomOutVE != null)
        {
            zoomOutVE.RegisterCallback<PointerDownEvent>(async evt => {
                Debug.Log("Zoom Out");
                await viscaSender.ZoomOut();
            });
            zoomOutVE.RegisterCallback<PointerUpEvent>(async evt => {
                Debug.Log("Zoom Out Stop");
                await viscaSender.ZoomStop();
            });
            zoomOutVE.RegisterCallback<PointerLeaveEvent>(async evt => {
                Debug.Log("Zoom Out Stop (Leave)");
                await viscaSender.ZoomStop();
            });
        }

        // Stop and Reset Controls
        var stop = root.Q<Button>("stopButton");
        if (stop != null)
        {
            stop.clicked += async () => {
                Debug.Log("Stop");
                await viscaSender.Stop();
            };
        }

        var reset = root.Q<Button>("resetButton");
        if (reset != null)
        {
            reset.clicked += async () => {
                Debug.Log("Reset to Home");
                await viscaSender.Home();
            };
        }
    }

    private void OnDisable()
    {
        viscaSender?.Dispose();
    }
}

public static class TaskExtensions
{
    public static System.Collections.IEnumerator AsCoroutine(this Task task)
    {
        while (!task.IsCompleted)
            yield return null;
        if (task.IsFaulted)
            throw task.Exception;
    }
}