using UnityEngine;

[DisallowMultipleComponent]
public class SerialSender : MonoBehaviour
{
    private SerialManager serialManager;

    [Header("Send Throttle")]
    [Tooltip("同一個指令重複送出最小間隔（秒）。例如 0.05 = 50ms")]
    public float minInterval = 0.05f;

    [Header("Debug")]
    [Tooltip("是否輸出送出指令的 Debug.Log（建議先關掉避免洗版）")]
    public bool debugLog = false;

    private float lastSendTime = -999f;
    private char lastCmd = '\0';

    private void Awake()
    {
        serialManager = GetComponent<SerialManager>();
        if (serialManager == null)
        {
            Debug.LogWarning("[SerialSender] SerialManager not found on same GameObject.");
        }
    }

    /// <summary>
    /// 核心送出：force=true 會忽略節流與重複指令限制（請只在「按鍵/事件當下」使用）
    /// </summary>
    public void SendCmd(char c, bool force = false)
    {
        if (serialManager == null || !serialManager.IsReady)
        {
            if (debugLog) Debug.LogWarning("[SerialSender] Serial not ready.");
            return;
        }

        float now = Time.unscaledTime;

        // 非強制：同指令 + 在節流時間內 -> 不送
        if (!force)
        {
            if (c == lastCmd && (now - lastSendTime) < minInterval)
                return;
        }

        lastCmd = c;
        lastSendTime = now;

        serialManager.WriteByte((byte)c);

        if (debugLog)
            Debug.Log($"[SerialSender] Sent '{c}' (force={force})");
    }

    // 舊 API 相容
    public void SendByte(char c) => SendCmd(c, false);
    public void SendByte(char c, bool force) => SendCmd(c, force);
}
