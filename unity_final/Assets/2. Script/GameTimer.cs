using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public float gameDuration = 120f;    
    public GameObject gameOverUI;       
    public MonoBehaviour playerController;  
    public AudioManager audioManager;    // ★ 新增：音樂管理

    private float timer;
    private bool isRunning = false;
    private bool isGameOver = false;

    void Start()
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(false);
    }

    void Update()
    {
        if (!isRunning || isGameOver) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            EndGame();
        }
    }

    // ★ 按下 Play 時會呼叫這個
    public void StartTimer()
    {
        timer = gameDuration;
        isRunning = true;
    }

    void EndGame()
    {
        isGameOver = true;
        isRunning = false;

        // 顯示結束畫面
        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        // 停止玩家
        if (playerController != null)
            playerController.enabled = false;

        // ★ 停止 BGM
        audioManager.StopBGM();
    }
}
