using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class ViscaOverIpSender : System.IDisposable
{
    private string _cameraIp;
    private int _cameraPort;
    private UdpClient udpClient;
    private IPEndPoint endPoint;
    
    public string cameraIp => _cameraIp;
    public int cameraPort => _cameraPort;

    public ViscaOverIpSender(string ip, int port)
    {
        Debug.Log($"ViscaOverIpSender constructor called with IP: {ip}, Port: {port}");
        _cameraIp = ip;
        _cameraPort = port;
        Debug.Log($"ViscaOverIpSender setting up with IP: {_cameraIp}, Port: {_cameraPort}");
        
        try {
            CreateEndPoint();
            Debug.Log($"ViscaOverIpSender successfully created for {_cameraIp}:{_cameraPort}");
        }
        catch (System.Exception ex) {
            Debug.LogError($"Error creating ViscaOverIpSender: {ex.Message}");
            Debug.LogError($"Attempted IP: {_cameraIp}, Port: {_cameraPort}");
        }
    }

    public ViscaOverIpSender(CameraInfo cameraInfo)
    {
        _cameraIp = cameraInfo.viscaIp;
        _cameraPort = cameraInfo.viscaPort;
        CreateEndPoint();
        Debug.Log($"ViscaOverIpSender created with camera: {cameraInfo.niceName}, IP: {_cameraIp}, Port: {_cameraPort}");
    }
    
    private void CreateEndPoint()
    {
        if (udpClient != null)
        {
            udpClient.Dispose();
        }
        
        endPoint = new IPEndPoint(IPAddress.Parse(_cameraIp), _cameraPort);
        udpClient = new UdpClient();
    }
    
    public void UpdateConnection(string newIp, int newPort)
    {
        // Log full stack trace for every call to help track usage
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
        Debug.Log($"UpdateConnection called from:\n{stackTrace}");
        
        if (_cameraIp == newIp && _cameraPort == newPort)
        {
            Debug.Log($"UpdateConnection called with the same IP/port, no changes needed: {newIp}:{newPort}");
            return;
        }
        
        Debug.Log($"⚠️ UPDATING VISCA SENDER CONNECTION: {_cameraIp}:{_cameraPort} → {newIp}:{newPort} ⚠️");
        _cameraIp = newIp;
        _cameraPort = newPort;
        
        try {
            CreateEndPoint();
            Debug.Log($"✅ ViscaOverIpSender successfully updated to {_cameraIp}:{_cameraPort}");
        }
        catch (System.Exception ex) {
            Debug.LogError($"Error updating ViscaOverIpSender: {ex.Message}");
            Debug.LogError($"Attempted IP: {_cameraIp}, Port: {_cameraPort}");
        }
    }

    public async Task PanLeft(byte speed = ViscaCommands.DEFAULT_PAN_SPEED)
    {
        Debug.Log($"PanLeft command called with speed: {speed} -> Camera: {_cameraIp}:{_cameraPort}");
        await SendPacketAsync(ViscaCommands.PanTiltCommand(true, false, false, false, speed, 0x00));
    }

    public async Task PanRight(byte speed = ViscaCommands.DEFAULT_PAN_SPEED)
    {
        Debug.Log($"PanRight command called with speed: {speed} -> Camera: {_cameraIp}:{_cameraPort}");
        await SendPacketAsync(ViscaCommands.PanTiltCommand(false, true, false, false, speed, 0x00));
    }

    public async Task TiltUp(byte speed = ViscaCommands.DEFAULT_PAN_SPEED)
    {
        Debug.Log($"TiltUp command called with speed: {speed} -> Camera: {_cameraIp}:{_cameraPort}");
        await SendPacketAsync(ViscaCommands.PanTiltCommand(false, false, true, false, 0x00, speed));
    }

    public async Task TiltDown(byte speed = ViscaCommands.DEFAULT_PAN_SPEED)
    {
        Debug.Log($"TiltDown command called with speed: {speed} -> Camera: {_cameraIp}:{_cameraPort}");
        await SendPacketAsync(ViscaCommands.PanTiltCommand(false, false, false, true, 0x00, speed));
    }

    public async Task ZoomIn(byte speed = ViscaCommands.DEFAULT_ZOOM_SPEED)
    {
        Debug.Log($"ZoomIn command called with speed: {speed} -> Camera: {_cameraIp}:{_cameraPort}");
        await SendPacketAsync(ViscaCommands.ZoomCommand(true, false, speed));
    }

    public async Task ZoomOut(byte speed = ViscaCommands.DEFAULT_ZOOM_SPEED)
    {
        Debug.Log($"ZoomOut command called with speed: {speed} -> Camera: {_cameraIp}:{_cameraPort}");
        await SendPacketAsync(ViscaCommands.ZoomCommand(false, true, speed));
    }

    public async Task Stop()
    {
        Debug.Log($"Stop command called -> Camera: {_cameraIp}:{_cameraPort}");
        await SendPacketAsync(ViscaCommands.StopCommand());
    }

    public async Task Home()
    {
        Debug.Log($"Home command called -> Camera: {_cameraIp}:{_cameraPort}");
        await SendPacketAsync(ViscaCommands.HomeCommand());
    }

    public async Task ZoomStop()
    {
        Debug.Log($"ZoomStop command called -> Camera: {_cameraIp}:{_cameraPort}");
        await SendPacketAsync(ViscaCommands.ZoomStopCommand());
    }

    private async Task SendPacketAsync(byte[] packet)
    {
        Debug.Log("--------------------------------");
        Debug.Log($"VISCA SEND - Starting send process");
        Debug.Log($"Current IP/Port: {_cameraIp}:{_cameraPort}");
        Debug.Log($"GLOBAL TRACKER: Active camera is {NDIViewerApp.ActiveCameraName} with IP {NDIViewerApp.ActiveCameraIP}:{NDIViewerApp.ActiveCameraPort}");
        
        // SAFETY CHECK: Verify IP address matches the global tracker
        bool ipMismatch = _cameraIp != NDIViewerApp.ActiveCameraIP && !string.IsNullOrEmpty(NDIViewerApp.ActiveCameraIP) && NDIViewerApp.ActiveCameraIP != "Not Set";
        bool portMismatch = _cameraPort != NDIViewerApp.ActiveCameraPort && NDIViewerApp.ActiveCameraPort != 0;
        
        if (ipMismatch || portMismatch)
        {
            Debug.LogError($"⚠️ CRITICAL SAFETY CHECK: Attempted to send VISCA command to {_cameraIp}:{_cameraPort} but active camera is {NDIViewerApp.ActiveCameraName} with IP {NDIViewerApp.ActiveCameraIP}:{NDIViewerApp.ActiveCameraPort}");
            
            // Auto-correct the connection details
            Debug.Log($"🛡️ AUTO-CORRECTING CONNECTION: Changing from {_cameraIp}:{_cameraPort} to {NDIViewerApp.ActiveCameraIP}:{NDIViewerApp.ActiveCameraPort}");
            UpdateConnection(NDIViewerApp.ActiveCameraIP, NDIViewerApp.ActiveCameraPort);
            
            // Log call stack to help debug
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
            Debug.Log($"Call stack for safety correction:\n{stackTrace}");
        }
        
        // Validate client and endpoint
        if (udpClient == null)
        {
            Debug.LogError("Cannot send VISCA packet: udpClient is null");
            return;
        }

        if (endPoint == null)
        {
            Debug.LogError($"Cannot send VISCA packet: endPoint is null. IP: {_cameraIp}, Port: {_cameraPort}");
            return;
        }
        
        // CRITICAL: Make sure the endpoint has the correct IP & port
        Debug.Log($"Current endpoint is: {endPoint.Address}:{endPoint.Port}");
        
        if (endPoint.Address.ToString() != _cameraIp || endPoint.Port != _cameraPort)
        {
            Debug.LogError($"ENDPOINT MISMATCH: Endpoint {endPoint.Address}:{endPoint.Port} doesn't match current IP {_cameraIp}:{_cameraPort}");
            Debug.LogError($"GLOBAL TRACKER says active camera should be: {NDIViewerApp.ActiveCameraIP}:{NDIViewerApp.ActiveCameraPort}");
            Debug.Log("Recreating endpoint with correct IP and port");
            try {
                CreateEndPoint();
                Debug.Log($"Endpoint recreated - new endpoint: {endPoint.Address}:{endPoint.Port}");
            }
            catch (System.Exception ex) {
                Debug.LogError($"Failed to recreate endpoint: {ex.Message}");
                return;
            }
        }

        try
        {
            string packetHex = System.BitConverter.ToString(packet);
            Debug.Log($"📡 Sending VISCA packet to {_cameraIp}:{_cameraPort} - Data: {packetHex}");
            Debug.Log($"📡 GLOBAL TRACKER says active camera is: {NDIViewerApp.ActiveCameraName} ({NDIViewerApp.ActiveCameraIP}:{NDIViewerApp.ActiveCameraPort})");
            
            // Always log the call path for sends
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
            Debug.Log($"VISCA packet call path: {stackTrace.ToString()}");
            
            await udpClient.SendAsync(packet, packet.Length, endPoint);
            Debug.Log($"VISCA packet sent successfully: {packetHex}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"VISCA send error: {ex.Message}\nStack trace: {ex.StackTrace}");
        }
        Debug.Log("--------------------------------");
    }

    public void Dispose()
    {
        Debug.Log($"Disposing ViscaOverIpSender for {_cameraIp}:{_cameraPort}");
        if (udpClient != null)
        {
            udpClient.Dispose();
            udpClient = null;
        }
    }
} 