using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        // 檢查 2D 場景是否已經載入，若尚未載入則以 Additive 模式附加載入
        if (!SceneManager.GetSceneByName("Scene_LaserPuzzle").isLoaded)
        {
            SceneManager.LoadSceneAsync("Scene_LaserPuzzle", LoadSceneMode.Additive);
        }
    }
}