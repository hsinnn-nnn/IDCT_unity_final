using System;
using System.IO.Ports;
using UnityEngine;
using System.Collections;

public class SerialManager : MonoBehaviour
{
    [Header("------- Port Settings -------")]
    public string Port = "COM3";
    public int BaudRate = 115200;

    public SerialPort serialPort;

    public bool IsReady => serialPort != null && serialPort.IsOpen;

    public event Action<byte> OnByteReceived;

    [Header("Read Settings")]
    public int readTimeoutMs = 50;
    public float pollInterval = 0.01f;

    private Coroutine readRoutine;

    private void Awake()
    {
        SerialInitialize();
    }

    private void OnDestroy()
    {
        Close();
    }

    private void SerialInitialize()
    {
        try
        {
            serialPort = new SerialPort(Port, BaudRate);
            serialPort.ReadTimeout = readTimeoutMs;
            serialPort.Open();

            Debug.Log($"[SerialManagerV2] Opened: {Port} @ {BaudRate}");

            readRoutine = StartCoroutine(ReadLoop());
        }
        catch (Exception e)
        {
            Debug.LogError($"[SerialManagerV2] Open failed: {Port}, Error: {e.Message}");
        }
    }

    private IEnumerator ReadLoop()
    {
        while (IsReady)
        {
            if (serialPort.BytesToRead > 0)
            {
                try
                {
                    int v = serialPort.ReadByte();
                    if (v >= 0) OnByteReceived?.Invoke((byte)v);
                }
                catch (TimeoutException) { }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SerialManagerV2] Read error: {e.Message}");
                }
            }

            yield return new WaitForSeconds(pollInterval);
        }
    }

    public void WriteByte(byte b)
    {
        if (!IsReady) return;

        try
        {
            serialPort.Write(new byte[] { b }, 0, 1);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SerialManagerV2] Write error: {e.Message}");
        }
    }

    public void WriteString(string s)
    {
        if (!IsReady) return;

        try
        {
            serialPort.Write(s);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SerialManagerV2] Write error: {e.Message}");
        }
    }

    public void Close()
    {
        try
        {
            if (readRoutine != null) StopCoroutine(readRoutine);
            readRoutine = null;

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                Debug.Log("[SerialManagerV2] Port closed.");
            }
        }
        catch { }
    }
}
