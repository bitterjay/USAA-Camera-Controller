using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class ViscaOverIpSender : System.IDisposable
{
    // Camera connection details
    public string cameraIp = "192.168.1.100"; // Default IP address
    public int cameraPort = 52381; // Default VISCA port
    
    // Network objects
    private UdpClient udpClient;
    private IPEndPoint endPoint;

    // Constructor with explicit IP and port
    public ViscaOverIpSender(string ip, int port)
    {
        cameraIp = ip;
        cameraPort = port;
        
        try {
            CreateEndPoint();
            Debug.Log($"VISCA sender created for {cameraIp}:{cameraPort}");
        }
        catch (System.Exception ex) {
            Debug.LogError($"Error creating ViscaOverIpSender: {ex.Message} (IP: {cameraIp}, Port: {cameraPort})");
        }
    }

    // Constructor with camera info
    public ViscaOverIpSender(CameraInfo cameraInfo)
    {
        // Get IP either from the camera info or from the global setting
        if (!string.IsNullOrEmpty(cameraInfo.viscaIp))
        {
            cameraIp = cameraInfo.viscaIp;
            Debug.Log($"Using IP from camera info: {cameraIp}");
        }   
        
        CreateEndPoint();
        Debug.Log($"VISCA sender created for {cameraInfo.niceName} ({cameraIp}:{cameraPort})");
    }
    
    private void CreateEndPoint()
    {
        if (udpClient != null)
        {
            udpClient.Dispose();
        }
        
        endPoint = new IPEndPoint(IPAddress.Parse(cameraIp), cameraPort);
        udpClient = new UdpClient();
    }
    
    public void UpdateConnection(string newIp, int newPort)
    {
        if (cameraIp == newIp && cameraPort == newPort)
        {
            return; // No change needed
        }
        
        Debug.Log($"Updating VISCA connection: {cameraIp}:{cameraPort} → {newIp}:{newPort}");
        cameraIp = newIp;
        cameraPort = newPort;
        
        try {
            CreateEndPoint();
        }
        catch (System.Exception ex) {
            Debug.LogError($"Error updating ViscaOverIpSender: {ex.Message} (IP: {cameraIp}, Port: {cameraPort})");
        }
    }

    public async Task PanLeft(byte speed = ViscaCommands.DEFAULT_PAN_SPEED)
    {
        await SendPacketAsync(ViscaCommands.PanTiltCommand(true, false, false, false, speed, 0x00));
    }

    public async Task PanRight(byte speed = ViscaCommands.DEFAULT_PAN_SPEED)
    {
        await SendPacketAsync(ViscaCommands.PanTiltCommand(false, true, false, false, speed, 0x00));
    }

    public async Task TiltUp(byte speed = ViscaCommands.DEFAULT_PAN_SPEED)
    {
        await SendPacketAsync(ViscaCommands.PanTiltCommand(false, false, true, false, 0x00, speed));
    }

    public async Task TiltDown(byte speed = ViscaCommands.DEFAULT_PAN_SPEED)
    {
        await SendPacketAsync(ViscaCommands.PanTiltCommand(false, false, false, true, 0x00, speed));
    }

    public async Task PanTiltUpLeft(byte panSpeed = ViscaCommands.DEFAULT_PAN_SPEED, byte tiltSpeed = ViscaCommands.DEFAULT_PAN_SPEED)
    {
        await SendPacketAsync(ViscaCommands.PanTiltUpLeftCommand(panSpeed, tiltSpeed));
    }

    public async Task PanTiltUpRight(byte panSpeed = ViscaCommands.DEFAULT_PAN_SPEED, byte tiltSpeed = ViscaCommands.DEFAULT_PAN_SPEED)
    {
        await SendPacketAsync(ViscaCommands.PanTiltUpRightCommand(panSpeed, tiltSpeed));
    }

    public async Task PanTiltDownLeft(byte panSpeed = ViscaCommands.DEFAULT_PAN_SPEED, byte tiltSpeed = ViscaCommands.DEFAULT_PAN_SPEED)
    {
        await SendPacketAsync(ViscaCommands.PanTiltDownLeftCommand(panSpeed, tiltSpeed));
    }

    public async Task PanTiltDownRight(byte panSpeed = ViscaCommands.DEFAULT_PAN_SPEED, byte tiltSpeed = ViscaCommands.DEFAULT_PAN_SPEED)
    {
        await SendPacketAsync(ViscaCommands.PanTiltDownRightCommand(panSpeed, tiltSpeed));
    }

    public async Task ZoomIn(byte speed = ViscaCommands.DEFAULT_ZOOM_SPEED)
    {
        await SendPacketAsync(ViscaCommands.ZoomCommand(true, false, speed));
    }

    public async Task ZoomOut(byte speed = ViscaCommands.DEFAULT_ZOOM_SPEED)
    {
        await SendPacketAsync(ViscaCommands.ZoomCommand(false, true, speed));
    }

    public async Task Stop()
    {
        await SendPacketAsync(ViscaCommands.StopCommand());
    }

    public async Task Home()
    {
        await SendPacketAsync(ViscaCommands.HomeCommand());
    }

    public async Task ZoomStop()
    {
        await SendPacketAsync(ViscaCommands.ZoomStopCommand());
    }

    // Focus Control Methods
    public async Task FocusNear()
    {
        await SendPacketAsync(ViscaCommands.FocusNearCommand());
    }

    public async Task FocusFar()
    {
        await SendPacketAsync(ViscaCommands.FocusFarCommand());
    }

    public async Task FocusStop()
    {
        await SendPacketAsync(ViscaCommands.FocusStopCommand());
    }

    public async Task FocusAuto()
    {
        await SendPacketAsync(ViscaCommands.FocusAutoCommand());
    }

    public async Task FocusManual()
    {
        await SendPacketAsync(ViscaCommands.FocusManualCommand());
    }

    public async Task FocusOnePush()
    {
        await SendPacketAsync(ViscaCommands.FocusOnePushCommand());
    }

    public async Task ZoomFocusDirect(int zoomPosition, int focusPosition)
    {
        await SendPacketAsync(ViscaCommands.ZoomFocusDirectCommand(zoomPosition, focusPosition));
    }

    // White Balance Methods
    public async Task WhiteBalanceAuto()
    {
        await SendPacketAsync(ViscaCommands.WhiteBalanceAutoCommand());
    }

    public async Task WhiteBalanceIndoor()
    {
        await SendPacketAsync(ViscaCommands.WhiteBalanceIndoorCommand());
    }

    public async Task WhiteBalanceOutdoor()
    {
        await SendPacketAsync(ViscaCommands.WhiteBalanceOutdoorCommand());
    }

    public async Task WhiteBalanceOnePush()
    {
        await SendPacketAsync(ViscaCommands.WhiteBalanceOnePushCommand());
    }

    public async Task WhiteBalanceATW()
    {
        await SendPacketAsync(ViscaCommands.WhiteBalanceATWCommand());
    }

    public async Task WhiteBalanceOnePushTrigger()
    {
        await SendPacketAsync(ViscaCommands.WhiteBalanceOnePushTriggerCommand());
    }

    // Exposure Methods
    public async Task ExposureFullAuto()
    {
        await SendPacketAsync(ViscaCommands.ExposureFullAutoCommand());
    }

    public async Task ExposureManual()
    {
        await SendPacketAsync(ViscaCommands.ExposureManualCommand());
    }

    public async Task ExposureShutterPriority()
    {
        await SendPacketAsync(ViscaCommands.ExposureShutterPriorityCommand());
    }

    public async Task ExposureIrisPriority()
    {
        await SendPacketAsync(ViscaCommands.ExposureIrisPriorityCommand());
    }

    // Preset Methods
    public async Task PresetRecall(byte presetNumber)
    {
        await SendPacketAsync(ViscaCommands.PresetRecallCommand(presetNumber));
    }

    public async Task PresetSet(byte presetNumber)
    {
        await SendPacketAsync(ViscaCommands.PresetSetCommand(presetNumber));
    }

    public async Task PresetReset(byte presetNumber)
    {
        await SendPacketAsync(ViscaCommands.PresetResetCommand(presetNumber));
    }

    private async Task SendPacketAsync(byte[] packet)
    {
        // Validate client and endpoint
        if (udpClient == null)
        {
            Debug.LogError("Cannot send VISCA packet: udpClient is null");
            return;
        }

        if (endPoint == null)
        {
            Debug.LogError($"Cannot send VISCA packet: endPoint is null. IP: {cameraIp}, Port: {cameraPort}");
            return;
        }
        
        // Check if endpoint needs updating
        if (endPoint.Address.ToString() != cameraIp)
        {
            Debug.LogError($"Endpoint mismatch: Endpoint {endPoint.Address}:{endPoint.Port} doesn't match {cameraIp}:{cameraPort}");
            CreateEndPoint();
        }

        try
        {
            await udpClient.SendAsync(packet, packet.Length, endPoint);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"VISCA send error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (udpClient != null)
        {
            udpClient.Dispose();
            udpClient = null;
        }
    }
} 