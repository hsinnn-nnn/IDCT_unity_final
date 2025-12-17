using UnityEngine;

public class CaneCollisionDetector : MonoBehaviour
{
    [Header("流程控制器")]
    public GameFlowController gameFlow;

    [Header("玩家的根節點（例如 XROrigin, PlayerRoot）")]
    public Transform playerRoot;

    [Header("Tags")]
    public string obstacleTag = "Obstacle";
    public string obstacleDownTag = "ObstacleDown";

    [Header("Trigger")]
    public float triggerCooldown = 0.15f;
    public bool debugLog = true;

    [Header("前方判斷角度")]
    [Range(0f, 89f)]
    public float frontAngleDegree = 30f;

    [Header("序列埠 Sender")]
    public SerialSender serialSender;

    [Header("送出是否強制（建議 true，避免節流擋掉事件）")]
    public bool forceSend = true;

    [Header("Gate（由校正流程暫停送出）")]
    public bool allowSend = true;

    private float lastTriggerTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        // 遊戲沒在跑就不要送 Arduino
        if (gameFlow != null && !gameFlow.isGameRunning) return;

        // 校正或其他流程暫停送出
        if (!allowSend) return;

        bool isDownObstacle = other.CompareTag(obstacleDownTag);
        bool isNormalObstacle = other.CompareTag(obstacleTag);
        if (!isDownObstacle && !isNormalObstacle) return;

        if (Time.time - lastTriggerTime < triggerCooldown) return;
        lastTriggerTime = Time.time;

        // 下方障礙物：送 D
        if (isDownObstacle)
        {
            SendToArduino('D');
            if (debugLog) Debug.Log($"[Cane] 下方障礙物 | {other.name}");
            return;
        }

        if (playerRoot == null)
        {
            Debug.LogWarning("[Cane] playerRoot 未指定！");
            return;
        }

        // 以「碰撞點」相對於 playerRoot（通常是頭/玩家根）來判斷左右/前
        Vector3 hitPointWorld = other.ClosestPoint(transform.position);
        Vector3 hitLocal = playerRoot.InverseTransformPoint(hitPointWorld);

        // 只看 XZ 平面方向
        Vector3 dir = new Vector3(hitLocal.x, 0f, hitLocal.z);
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        float angle = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
        float absAngle = Mathf.Abs(angle);

        string sideMsg;
        char codeToSend = '\0'; // '\0' = 不送

        // ✅ 正前方：不送指令（避免送 F 把馬達停掉）
        if (hitLocal.z > 0f && absAngle <= frontAngleDegree)
        {
            sideMsg = "正前方";
            codeToSend = '\0';
        }
        else
        {
            if (angle > 0f) { sideMsg = "右側"; codeToSend = 'R'; }
            else { sideMsg = "左側"; codeToSend = 'L'; }
        }

        if (codeToSend != '\0')
        {
            SendToArduino(codeToSend);
            if (debugLog) Debug.Log($"[Cane] {sideMsg} | {other.name}（送 '{codeToSend}'）");
        }
        else
        {
            if (debugLog) Debug.Log($"[Cane] {sideMsg} | {other.name}（不送指令，維持上一方向）");
        }
    }

    private void SendToArduino(char c)
    {
        if (serialSender == null)
        {
            if (debugLog) Debug.LogWarning("[Cane] serialSender 未指定");
            return;
        }

        serialSender.SendByte(c, forceSend);

        if (debugLog)
            Debug.Log($"[Cane] >>> Sent '{c}' to Arduino (force={forceSend})");
    }
}
