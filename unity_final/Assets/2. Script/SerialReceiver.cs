using UnityEngine;

[DisallowMultipleComponent]
public class SerialReceiver : MonoBehaviour
{
    [Header("Reference (可不填，會自動找)")]
    [SerializeField] private SerialManager serialManager;

    [Header("Auto Find")]
    public bool autoFindOnAwake = true;
    public bool searchInParentIfMissing = true;

    [Header("Accepted ACK chars")]
    [Tooltip("只接受這些字元作為 ACK（Arduino 目前用 '1'~'6'）")]
    public string acceptedAcks = "123456";

    [Header("Resume Signal（舊 API 相容）")]
    public char resumeAckChar = '5';

    [Header("Debug (建議先全部關掉)")]
    public bool debugLogAck = false;
    public bool debugLogNoise = false;
    public bool debugLogSubscribe = false;

    public byte lastByte;
    public char lastChar;

    // ✅ 只保存最後一個 ACK，避免 queue 爆掉
    private char lastAck = '\0';

    private void Awake()
    {
        if (autoFindOnAwake) ResolveSerialManager();
    }

    private void OnEnable()
    {
        ResolveSerialManager();

        if (serialManager != null)
        {
            serialManager.OnByteReceived += HandleByte;
            if (debugLogSubscribe)
                Debug.Log($"[SerialReceiver] Subscribed OnByteReceived on '{serialManager.gameObject.name}'.");
        }
        else
        {
            Debug.LogWarning("[SerialReceiver] SerialManager not found. (請確認 SerialManager 與 SerialReceiver 在同物件或父物件，或手動指定 reference)");
        }
    }

    private void OnDisable()
    {
        if (serialManager != null)
        {
            serialManager.OnByteReceived -= HandleByte;
            if (debugLogSubscribe)
                Debug.Log($"[SerialReceiver] Unsubscribed OnByteReceived on '{serialManager.gameObject.name}'.");
        }
    }

    private void ResolveSerialManager()
    {
        if (serialManager != null) return;

        serialManager = GetComponent<SerialManager>();
        if (serialManager == null && searchInParentIfMissing)
            serialManager = GetComponentInParent<SerialManager>();
    }

    private void HandleByte(byte b)
    {
        lastByte = b;
        lastChar = (char)b;

        char c = (char)b;

        // 只收可預期 ACK
        if (!string.IsNullOrEmpty(acceptedAcks) && acceptedAcks.IndexOf(c) >= 0)
        {
            lastAck = c;

            if (debugLogAck)
                Debug.Log($"[SerialReceiver] ACK: '{c}' ({b})");
        }
        else
        {
            if (debugLogNoise)
                Debug.LogWarning($"[SerialReceiver] Noise: '{c}' ({b})");
        }
    }

    /// <summary>清空 ACK（例如校正前呼叫一次）</summary>
    public void ClearAcks()
    {
        lastAck = '\0';
        if (debugLogAck) Debug.Log("[SerialReceiver] ACK cleared.");
    }

    /// <summary>消耗指定 ACK（例如 ConsumeAck('5')）</summary>
    public bool ConsumeAck(char target)
    {
        if (lastAck == target)
        {
            lastAck = '\0';
            return true;
        }
        return false;
    }

    /// <summary>
    /// ✅ 新增：消耗任一個 ACK（例如 "1234"）
    /// </summary>
    public bool ConsumeAny(string targets)
    {
        if (string.IsNullOrEmpty(targets)) return false;

        if (lastAck != '\0' && targets.IndexOf(lastAck) >= 0)
        {
            lastAck = '\0';
            return true;
        }
        return false;
    }

    /// <summary>舊 API 相容：ConsumeResumeSignal()</summary>
    public bool ConsumeResumeSignal()
    {
        return ConsumeAck(resumeAckChar);
    }
}

