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

    // Focus Commands
    public static byte[] FocusNearCommand()
    {
        // 0x81 0x01 0x04 0x08 0x02 0xFF (Focus Near Standard)
        return new byte[] { 0x81, 0x01, 0x04, 0x08, 0x03, 0xFF };
    }

    public static byte[] FocusFarCommand()
    {
        // 0x81 0x01 0x04 0x08 0x03 0xFF (Focus Far Standard)
        return new byte[] { 0x81, 0x01, 0x04, 0x08, 0x02, 0xFF };
    }

    public static byte[] FocusStopCommand()
    {
        // 0x81 0x01 0x04 0x08 0x00 0xFF (Focus Stop)
        return new byte[] { 0x81, 0x01, 0x04, 0x08, 0x00, 0xFF };
    }

    public static byte[] FocusAutoCommand()
    {
        // 0x81 0x01 0x04 0x38 0x02 0xFF (Auto Focus On)
        return new byte[] { 0x81, 0x01, 0x04, 0x38, 0x03, 0xFF };
    }

    public static byte[] FocusManualCommand()
    {
        // 0x81 0x01 0x04 0x38 0x03 0xFF (Auto Focus Off - Manual Focus)
        return new byte[] { 0x81, 0x01, 0x04, 0x38, 0x03, 0xFF };
    }

    public static byte[] FocusOnePushCommand()
    {
        // 0x81 0x01 0x04 0x18 0x01 0xFF (One Push Auto Focus)
        return new byte[] { 0x81, 0x01, 0x04, 0x18, 0x01, 0xFF };
    }

    public static byte[] ZoomFocusDirectCommand(int zoomPosition, int focusPosition)
    {
        // The VISCA command format for zoom/focus direct is:
        // 8x 01 04 47 0p 0q 0r 0s 0t 0u 0v 0w FF
        // where p,q,r,s are zoom position values (0-0xFFFF) and t,u,v,w are focus values (0-0xFFFF)
        
        byte p = (byte)((zoomPosition >> 12) & 0x0F);
        byte q = (byte)((zoomPosition >> 8) & 0x0F);
        byte r = (byte)((zoomPosition >> 4) & 0x0F);
        byte s = (byte)(zoomPosition & 0x0F);
        
        byte t = (byte)((focusPosition >> 12) & 0x0F);
        byte u = (byte)((focusPosition >> 8) & 0x0F);
        byte v = (byte)((focusPosition >> 4) & 0x0F);
        byte w = (byte)(focusPosition & 0x0F);
        
        return new byte[] { 
            0x81, 0x01, 0x04, 0x47, 
            p, q, r, s, t, u, v, w, 
            0xFF 
        };
    }

    // White Balance Commands
    public static byte[] WhiteBalanceAutoCommand()
    {
        // 0x81 0x01 0x04 0x35 0x00 0xFF (Auto White Balance)
        return new byte[] { 0x81, 0x01, 0x04, 0x35, 0x00, 0xFF };
    }

    public static byte[] WhiteBalanceIndoorCommand()
    {
        // 0x81 0x01 0x04 0x35 0x01 0xFF (Indoor White Balance)
        return new byte[] { 0x81, 0x01, 0x04, 0x35, 0x01, 0xFF };
    }

    public static byte[] WhiteBalanceOutdoorCommand()
    {
        // 0x81 0x01 0x04 0x35 0x02 0xFF (Outdoor White Balance)
        return new byte[] { 0x81, 0x01, 0x04, 0x35, 0x02, 0xFF };
    }

    public static byte[] WhiteBalanceOnePushCommand()
    {
        // 0x81 0x01 0x04 0x35 0x03 0xFF (One Push White Balance)
        return new byte[] { 0x81, 0x01, 0x04, 0x18, 0x01, 0xFF };
    }

    public static byte[] WhiteBalanceATWCommand()
    {
        // 0x81 0x01 0x04 0x35 0x04 0xFF (Auto Tracing White Balance)
        return new byte[] { 0x81, 0x01, 0x04, 0x35, 0x04, 0xFF };
    }

    public static byte[] WhiteBalanceOnePushTriggerCommand()
    {
        // 0x81 0x01 0x04 0x10 0x05 0xFF (One Push White Balance Trigger)
        return new byte[] { 0x81, 0x01, 0x04, 0x10, 0x05, 0xFF };
    }

    // Automatic Exposure Commands
    public static byte[] ExposureFullAutoCommand()
    {
        // 0x81 0x01 0x04 0x39 0x00 0xFF (Full Auto Exposure)
        return new byte[] { 0x81, 0x01, 0x04, 0x39, 0x00, 0xFF };
    }

    public static byte[] ExposureManualCommand()
    {
        // 0x81 0x01 0x04 0x39 0x03 0xFF (Manual Exposure)
        return new byte[] { 0x81, 0x01, 0x04, 0x39, 0x03, 0xFF };
    }

    public static byte[] ExposureShutterPriorityCommand()
    {
        // 0x81 0x01 0x04 0x39 0x0A 0xFF (Shutter Priority)
        return new byte[] { 0x81, 0x01, 0x04, 0x39, 0x0A, 0xFF };
    }

    public static byte[] ExposureIrisPriorityCommand()
    {
        // 0x81 0x01 0x04 0x39 0x0B 0xFF (Iris Priority)
        return new byte[] { 0x81, 0x01, 0x04, 0x39, 0x0B, 0xFF };
    }

    // Camera Memory (Presets) Commands
    public static byte[] PresetRecallCommand(byte presetNumber)
    {
        // 0x81 0x01 0x04 0x3F 0x02 0p FF (Recall Preset p)
        // Presets are typically 0-127 for most PTZ cameras
        if (presetNumber > 7) presetNumber = 7; // Limit to 8 presets (0-7)
        return new byte[] { 0x81, 0x01, 0x04, 0x3F, 0x02, presetNumber, 0xFF };
    }

    public static byte[] PresetSetCommand(byte presetNumber)
    {
        // 0x81 0x01 0x04 0x3F 0x01 0p FF (Set Preset p)
        if (presetNumber > 7) presetNumber = 7; // Limit to 8 presets (0-7)
        return new byte[] { 0x81, 0x01, 0x04, 0x3F, 0x01, presetNumber, 0xFF };
    }

    public static byte[] PresetResetCommand(byte presetNumber)
    {
        // 0x81 0x01 0x04 0x3F 0x00 0p FF (Reset Preset p)
        if (presetNumber > 7) presetNumber = 7; // Limit to 8 presets (0-7)
        return new byte[] { 0x81, 0x01, 0x04, 0x3F, 0x00, presetNumber, 0xFF };
    }

    // Diagonal Pan/Tilt Commands
    /// <summary>
    /// Pan and tilt up-left (diagonal).
    /// </summary>
    public static byte[] PanTiltUpLeftCommand(byte panSpeed = DEFAULT_PAN_SPEED, byte tiltSpeed = DEFAULT_PAN_SPEED)
    {
        return PanTiltCommand(isLeft: true, isRight: false, isUp: true, isDown: false, panSpeed, tiltSpeed);
    }

    /// <summary>
    /// Pan and tilt up-right (diagonal).
    /// </summary>
    public static byte[] PanTiltUpRightCommand(byte panSpeed = DEFAULT_PAN_SPEED, byte tiltSpeed = DEFAULT_PAN_SPEED)
    {
        return PanTiltCommand(isLeft: false, isRight: true, isUp: true, isDown: false, panSpeed, tiltSpeed);
    }

    /// <summary>
    /// Pan and tilt down-left (diagonal).
    /// </summary>
    public static byte[] PanTiltDownLeftCommand(byte panSpeed = DEFAULT_PAN_SPEED, byte tiltSpeed = DEFAULT_PAN_SPEED)
    {
        return PanTiltCommand(isLeft: true, isRight: false, isUp: false, isDown: true, panSpeed, tiltSpeed);
    }

    /// <summary>
    /// Pan and tilt down-right (diagonal).
    /// </summary>
    public static byte[] PanTiltDownRightCommand(byte panSpeed = DEFAULT_PAN_SPEED, byte tiltSpeed = DEFAULT_PAN_SPEED)
    {
        return PanTiltCommand(isLeft: false, isRight: true, isUp: false, isDown: true, panSpeed, tiltSpeed);
    }
} 