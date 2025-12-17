using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class CaneCollisionSound : MonoBehaviour
{
    [Header("一般障礙物（Obstacle）撞擊音效")]
    public AudioClip hitSound;
    public string obstacleTag = "Obstacle";

    [Header("下方障礙物（ObstacleDown）撞擊音效")]
    public AudioClip hitSoundDown;
    public string obstacleDownTag = "ObstacleDown";

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D 聲音效果

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Cane Trigger 到：{other.name}, Tag = {other.tag}");

        // 撞到一般障礙物
        if (other.CompareTag(obstacleTag) && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // 撞到下方障礙物
        if (other.CompareTag(obstacleDownTag) && hitSoundDown != null)
        {
            audioSource.PlayOneShot(hitSoundDown);
        }
    }
}
