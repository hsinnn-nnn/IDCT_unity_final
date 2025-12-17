using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class CalibrationUIController : MonoBehaviour
{
    [Header("References（建議都掛在同一個 Cane 物件上）")]
    public SerialSender serialSender;
    public SerialReceiver serialReceiver;
    public GameFlowController gameFlow; // ✅ 記得拖曳場景中的 GameFlowController
    public CaneCollisionDetector caneCollisionDetector; 

    [Header("Key Bindings")]
    public KeyCode calibrateKey = KeyCode.C;
    public KeyCode startKey = KeyCode.Space;

    [Header("Calibration Settings")]
    public float calibrateTimeout = 8f;
    public float calibrateCooldown = 1.0f;
    public bool sendStopBeforeCalibrate = true;
    public bool sendStopAfterCalibrate = true;

    [Header("校正期間暫停碰撞送指令")]
    public bool disableCollisionSendDuringCalibration = true;

    [Header("Send Burst Settings")]
    public int calibrateBurstCount = 5;
    public float calibrateBurstInterval = 0.05f;

    [Header("ACK Chars（Arduino）")]
    public char ackCalibrateStart = '6'; 
    public char ackCalibrateDone = '5';  

    [Header("Debug")]
    public bool debugLog = true;

    // 狀態屬性
    public bool IsCalibrated { get; private set; } = false;
    public bool IsCalibrating => isCalibrating;
    public bool CanStart => IsCalibrated && !isCalibrating;

    private bool isCalibrating = false;
    private float lastCalibrateTime = -999f;

    private void Awake()
    {
        if (serialSender == null) serialSender = GetComponent<SerialSender>();
        if (serialReceiver == null) serialReceiver = GetComponent<SerialReceiver>();
        if (gameFlow == null) gameFlow = FindObjectOfType<GameFlowController>();
        if (caneCollisionDetector == null) caneCollisionDetector = GetComponent<CaneCollisionDetector>();
    }

    private void Start()
    {
        if (debugLog) Debug.Log("[CalibrationUIController] 等待校正 (按 C)。校正完才會顯示 Opening。");
    }

    private void Update()
    {
        // 1. 偵測校正 (C)
        if (Input.GetKeyDown(calibrateKey))
        {
            TryStartCalibration();
        }

        // 2. 偵測開始 (Space / VR Button)
        bool keyboardStart = Input.GetKeyDown(startKey);
        bool vrStart = false;
#if UNITY_ANDROID || UNITY_EDITOR
        vrStart = OVRInput.GetDown(OVRInput.Button.One);
#endif

        if (keyboardStart || vrStart)
        {
            TryStartGame();
        }
    }

    public void TryStartCalibration()
    {
        if (isCalibrating) return;

        float now = Time.unscaledTime;
        if (now - lastCalibrateTime < calibrateCooldown) return;

        if (serialSender == null || serialReceiver == null)
        {
            Debug.LogError("[CalibrationUIController] 缺少 Serial 元件！");
            return;
        }

        lastCalibrateTime = now;
        StartCoroutine(CalibrationRoutine());
    }

    private IEnumerator CalibrationRoutine()
    {
        isCalibrating = true;
        IsCalibrated = false;

        // 暫停碰撞發送
        if (disableCollisionSendDuringCalibration && caneCollisionDetector != null)
            caneCollisionDetector.allowSend = false;

        if (debugLog) Debug.Log("[CalibrationUIController] 校正中...");

        // 送停止 & 送 C
        if (sendStopBeforeCalibrate)
        {
            serialSender.SendCmd('F', true);
            yield return new WaitForSecondsRealtime(0.08f);
        }

        DrainAck(ackCalibrateDone, 8); // 清除舊 ACK
        DrainAck(ackCalibrateStart, 8);

        for (int i = 0; i < calibrateBurstCount; i++)
        {
            serialSender.SendCmd('C', true);
            yield return new WaitForSecondsRealtime(calibrateBurstInterval);
        }

        bool gotStart = false;
        float t = 0f;

        // 等待回應
        while (t < calibrateTimeout)
        {
            if (!gotStart && serialReceiver.ConsumeAck(ackCalibrateStart))
            {
                gotStart = true;
            }

            // ✅ 收到 '5' (校正完成)
            if (serialReceiver.ConsumeAck(ackCalibrateDone))
            {
                IsCalibrated = true;
                isCalibrating = false;

                if (sendStopAfterCalibrate) serialSender.SendCmd('F', true);

                if (disableCollisionSendDuringCalibration && caneCollisionDetector != null)
                    caneCollisionDetector.allowSend = true;

                if (debugLog) Debug.Log("[CalibrationUIController] ✅ 校正成功！呼叫 Opening 畫面。");

                // ✨ 關鍵修改：校正成功後，才叫出 Opening 畫面
                if (gameFlow != null)
                {
                    gameFlow.ShowOpening();
                }

                yield break;
            }

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // 逾時
        isCalibrating = false;
        if (disableCollisionSendDuringCalibration && caneCollisionDetector != null)
            caneCollisionDetector.allowSend = true;
        
        Debug.LogWarning("[CalibrationUIController] ⚠ 校正逾時。");
    }

    private void DrainAck(char target, int maxTry)
    {
        for (int i = 0; i < maxTry; i++)
        {
            if (!serialReceiver.ConsumeAck(target)) break;
        }
    }

    public void TryStartGame()
    {
        // 必須已校正 且 非校正中 才能開始
        if (!CanStart)
        {
            if (debugLog) Debug.Log("[CalibrationUIController] 請先按 C 完成校正才能開始。");
            return;
        }

        // 呼叫 GameFlow 正式開始
        if (gameFlow != null)
        {
            gameFlow.StartGameByButton();
        }
    }
}
