using UnityEngine;

public class SerialCommandRouter : MonoBehaviour
{
    public SerialSender sender;

    [Header("Protocol Chars (match Arduino)")]
    public char CMD_LEFT = 'L';
    public char CMD_RIGHT = 'R';
    public char CMD_STOP = 'F';
    public char CMD_CALIB = 'C';

    private void Reset()
    {
        if (sender == null) sender = GetComponent<SerialSender>();
    }

    public void SendLeft(bool force = true)
    {
        if (sender == null) return;
        sender.SendByte(CMD_LEFT, force);
    }

    public void SendRight(bool force = true)
    {
        if (sender == null) return;
        sender.SendByte(CMD_RIGHT, force);
    }

    public void SendStop(bool force = true)
    {
        if (sender == null) return;
        sender.SendByte(CMD_STOP, force);
    }

    public void SendCalibrate(bool force = true)
    {
        if (sender == null) return;
        sender.SendByte(CMD_CALIB, force);
    }
}
