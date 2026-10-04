using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    void Start()
    {
        // 顯示滑鼠
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 避免之前有暫停遊戲
        Time.timeScale = 1f;
    }


    // ============================================
    // Retry 按鈕
    // 回到玩家剛才死亡的關卡
    // ============================================
    public void RetryLevel()
    {
        string lastLevel =
            PlayerPrefs.GetString(
                "LastLevel",
                "Level1"
            );

        SceneManager.LoadScene(
            lastLevel
        );
    }


    // ============================================
    // 回主選單
    // ============================================
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(
            "StartMenu"
        );
    }
}