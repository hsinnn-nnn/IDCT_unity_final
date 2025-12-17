using UnityEngine;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour
{
    public GameObject startUI;          // 開場畫面
    public Button playButton;           // Play 按鈕
    public MonoBehaviour playerController;  // 玩家移動腳本
    public GameTimer gameTimer;         // ★ 新增：計時器
    public AudioManager audioManager;   // ★ 新增：音樂管理

    void Start()
    {
        startUI.SetActive(true);

        if (playerController != null)
            playerController.enabled = false;

        playButton.onClick.AddListener(StartGame);
    }

    void StartGame()
    {
        // 關掉開場畫面
        startUI.SetActive(false);

        // 啟動玩家
        if (playerController != null)
            playerController.enabled = true;

        // ★ 開始倒數計時
        gameTimer.StartTimer();

        // ★ 播放 BGM
        audioManager.PlayBGM();
    }
}
