using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    void Start()
    {
        // 解鎖滑鼠
        Cursor.lockState = CursorLockMode.None;

        // 顯示滑鼠游標
        Cursor.visible = true;

        // 保險：如果之前有暫停遊戲，恢復時間
        Time.timeScale = 1f;
    }
    public void Retry()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("StartMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}