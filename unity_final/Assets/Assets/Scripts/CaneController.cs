using UnityEngine;

public class CaneFreeAim : MonoBehaviour
{
    [Header("設定")]
    public float maxDistance     = 20f;
    public LayerMask aimLayerMask = ~0;
    public float smoothSpeed      = 8f;
    public bool lockVertical      = true;

    private Quaternion initialWorldRotation;

    void Start()
    {
        // 儲存世界空間的起始旋轉
        initialWorldRotation = transform.rotation;
    }

    void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, aimLayerMask))
        {
            Vector3 dir = hit.point - transform.position;
            if (lockVertical)
            {
                dir.y = 0f;
            }
            if (dir.sqrMagnitude < 0.001f)
                return;

            Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);

            // 平滑旋轉：從目前角度逐漸向「initialRotation * targetRot」鏡像方向靠攏。
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                initialWorldRotation * targetRot,
                Time.deltaTime * smoothSpeed
            );
        }
    }
}
