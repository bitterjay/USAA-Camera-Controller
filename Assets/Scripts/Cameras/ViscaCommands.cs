using UnityEngine;

public static class ViscaCommands
{
    // Pan/Tilt speed values (0x01 to 0x18)
    public const byte MIN_PAN_SPEED = 0x01;
    public const byte MAX_PAN_SPEED = 0x18;
    public const byte DEFAULT_PAN_SPEED = 0x0C;
    
    // Zoom speed values (0x00 to 0x07)
    public const byte MIN_ZOOM_SPEED = 0x00;
    public const byte MAX_ZOOM_SPEED = 0x07;
    public const byte DEFAULT_ZOOM_SPEED = 0x04;

    // Command Headers
    private const byte COMMAND_HEADER = 0x81;
    private const byte COMMAND_TERMINATOR = 0xFF;
    
    public static byte[] PanTiltCommand(bool isLeft, bool isRight, bool isUp, bool isDown, byte panSpeed = DEFAULT_PAN_SPEED, byte tiltSpeed = DEFAULT_PAN_SPEED)
    {
        byte[] command = new byte[9];
        command[0] = COMMAND_HEADER;
        command[1] = 0x01;
        command[2] = 0x06;
        command[3] = 0x01;
        command[4] = panSpeed;
        command[5] = tiltSpeed;
        
        // Pan direction
        command[6] = 0x03; // Stop
        if (isLeft) command[6] = 0x01;
        if (isRight) command[6] = 0x02;
        
        // Tilt direction
        command[7] = 0x03; // Stop
        if (isUp) command[7] = 0x01;
        if (isDown) command[7] = 0x02;
        
        command[8] = COMMAND_TERMINATOR;
        return command;
    }

    public static byte[] ZoomCommand(bool isIn, bool isOut, byte speed = DEFAULT_ZOOM_SPEED)
    {
        return new byte[] {
            COMMAND_HEADER,
            0x01,
            0x04,
            0x07,
            (byte)(isIn ? (0x20 | speed) : (isOut ? (0x30 | speed) : 0x00)),
            COMMAND_TERMINATOR
        };
    }

    public static byte[] StopCommand()
    {
        return PanTiltCommand(false, false, false, false);
    }

    public static byte[] HomeCommand()
    {
        return new byte[] {
            COMMAND_HEADER,
            0x01,
            0x06,
            0x04,
            COMMAND_TERMINATOR
        };
    }

    public static byte[] ZoomStopCommand()
    {
        // 0x81 0x01 0x04 0x07 0x00 0xFF (Zoom Stop)
        return new byte[] { 0x81, 0x01, 0x04, 0x07, 0x00, 0xFF };
    }
} 