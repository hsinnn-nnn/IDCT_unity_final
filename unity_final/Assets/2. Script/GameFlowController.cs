using UnityEngine;
using System.Collections;

public class GameFlowController : MonoBehaviour
{
    [Header("UI 圖片設定")]
    public GameObject opening;   // 開場畫面（P1）
    public GameObject ending;    // 結束畫面（P2）

    [Header("遊戲時間設定")]
    public float gameDuration = 60.0f;
    public float warningTime = 30.0f;

    [Header("音效設定")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioClip warningClip;
    public AudioSource endSoruce;

    [Header("狀態")]
    public bool isGameRunning = false;
    public bool isGameFinished = false;

    void Start()
    {
        // ✅ 關鍵修改：預設隱藏 Opening，等待校正完成
        if (opening != null) opening.SetActive(false);
        if (ending != null)  ending.SetActive(false);

        isGameRunning = false;
        isGameFinished = false;

        // BGM 照常播放 (待機音樂)
        if (bgmSource != null)
        {
            bgmSource.loop = true;
            bgmSource.Play();
        }
        
        Debug.Log("[GameFlow] 就緒。Opening 目前隱藏，等待校正指令...");
    }

    // ❌ Update 已清空，完全被動等待 CalibrationUIController 指令
    void Update() { }

    /// <summary>
    /// ✨ 新增：被 CalibrationUIController 呼叫，顯示 Opening
    /// </summary>
    public void ShowOpening()
    {
        if (opening != null)
        {
            opening.SetActive(true);
            Debug.Log("[GameFlow] 接獲校正完成指令 -> 顯示 Opening");
        }
    }

    /// <summary>
    /// 被 CalibrationUIController 呼叫，開始遊戲倒數
    /// </summary>
    public void StartGameByButton()
    {
        StartGame();
    }

    private void StartGame()
    {
        if (isGameRunning || isGameFinished) return;
        StartCoroutine(GameFlowCoroutine());
    }

    private IEnumerator GameFlowCoroutine()
    {
        isGameRunning = true;

        // 遊戲開始，隱藏 Opening
        if (opening != null) opening.SetActive(false);
        Debug.Log("[GameFlow] 遊戲計時開始！");

        // --- 計時邏輯開始 ---

        if (warningTime > 0f && warningTime < gameDuration)
        {
            // 1. 等待直到提示時間
            yield return new WaitForSeconds(warningTime);

            // 2. 播放提示音
            if (warningClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(warningClip);
                Debug.Log("[GameFlow] 播放時間提示音");
            }

            // 3. 等待剩餘時間
            float remaining = gameDuration - warningTime;
            if (remaining > 0f) yield return new WaitForSeconds(remaining);
        }
        else
        {
            yield return new WaitForSeconds(gameDuration);
        }

        // --- 遊戲結束 ---
        
        Debug.Log("[GameFlow] 時間到，顯示 Ending");

        if (ending != null)
        {
            ending.SetActive(true);
            if (endSoruce != null)
            {
                endSoruce.loop = true;
                endSoruce.Play();
            }
        }

        isGameRunning = false;
        isGameFinished = true;

        if (bgmSource != null) bgmSource.Stop();
        if (sfxSource != null) sfxSource.Stop();
    }
}

