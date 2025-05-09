using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class ViscaOverIpSender : System.IDisposable
{
    private readonly string cameraIp;
    private readonly int cameraPort;
    private UdpClient udpClient;
    private IPEndPoint endPoint;

    public ViscaOverIpSender(string ip = "192.168.1.104", int port = 52381)
    {
        cameraIp = ip;
        cameraPort = port;
        endPoint = new IPEndPoint(IPAddress.Parse(cameraIp), cameraPort);
        udpClient = new UdpClient();
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

    private async Task SendPacketAsync(byte[] packet)
    {
        try
        {
            await udpClient.SendAsync(packet, packet.Length, endPoint);
            Debug.Log($"VISCA packet sent: {System.BitConverter.ToString(packet)}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"VISCA send error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        udpClient?.Dispose();
    }
} 