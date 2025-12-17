using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CaneAttachToRightHand : MonoBehaviour
{
    [Header("Game Flow Control")]
    public GameFlowController gameFlow;

    [Header("VR Right Hand (RightHandAnchor)")]
    public Transform rightHandAnchor;

    [Header("Grip offset in hand local space")]
    public Vector3 localPositionOffset = new Vector3(0f, -0.1f, 0.5f);
    public Vector3 localRotationOffsetEuler = Vector3.zero;

    [Header("Follow strength (higher = more tightly follows hand)")]
    public float followSpeed = 20f;

    [Header("Maximum allowed distance from the hand (meters)")]
    public float maxDistance = 0.3f;

    [Header("Seconds to stop following when hitting a Trigger")]
    public float stopFollowDuration = 5f;

    // ========= Optional SerialReceiver =========
    [Header("(Optional) SerialReceiver for receiving Arduino signals")]
    public SerialReceiver serialReceiver;

    [Header("Resume follow ACKs (interrupt stopFollow)")]
    [Tooltip("只要收到這些 ACK 其中之一，就會立刻解除停止跟隨狀態（預設 1~4）")]
    public string resumeFollowAcks = "1234";
    // ===========================================

    private Rigidbody rb;
    private bool canFollow = true; // 這是碰撞後的暫停旗標
    private float stopFollowTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        // 最優先檢查「遊戲是否正在進行」
        if (gameFlow != null && !gameFlow.isGameRunning)
        {
            return;
        }

        // If following is currently stopped → first check whether Arduino wants to resume early
        if (!canFollow)
        {
            // ✅ 收到 1~4 立即恢復跟隨（中斷 stopFollow 狀態）
            if (serialReceiver != null && serialReceiver.ConsumeAny(resumeFollowAcks))
            {
                canFollow = true;
                stopFollowTimer = 0f;
                Debug.Log($"[Cane] Received Arduino ACK in '{resumeFollowAcks}'. Follow movement resumed immediately.");
            }
            else
            {
                stopFollowTimer -= Time.fixedDeltaTime;
                if (stopFollowTimer <= 0f)
                {
                    canFollow = true;   // Auto resume
                }
            }

            if (!canFollow)
                return;
        }

        if (rightHandAnchor == null) return;

        // Calculate ideal position based on hand
        Vector3 targetPos = rightHandAnchor.TransformPoint(localPositionOffset);
        Quaternion targetRot = rightHandAnchor.rotation * Quaternion.Euler(localRotationOffsetEuler);

        // Clamp distance
        Vector3 dir = targetPos - rb.position;
        float dist = dir.magnitude;
        if (dist > maxDistance)
        {
            targetPos = rb.position + dir.normalized * maxDistance;
        }

        // Move using physics
        Vector3 newPos = Vector3.Lerp(rb.position, targetPos, followSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, followSpeed * Time.fixedDeltaTime));
    }

    // Triggered when the cane enters a trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // 如果遊戲沒開始，也不要觸發碰撞暫停邏輯
        if (gameFlow != null && !gameFlow.isGameRunning) return;

        if (!other.CompareTag("Obstacle") && !other.CompareTag("ObstacleDown")) return;

        canFollow = false;
        stopFollowTimer = stopFollowDuration;

        Debug.Log($"[Cane] Trigger detected. Stop following for {stopFollowDuration} seconds (or resume by ACK {resumeFollowAcks}).");
    }
}

