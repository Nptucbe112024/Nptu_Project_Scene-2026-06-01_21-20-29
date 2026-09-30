using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTeleporter : MonoBehaviour
{
    [Header("目標場景名稱（需加入 Build Settings）")]
    public string targetScene = "NextScene";

    [Header("扭曲效果")]
    public float duration = 2f;          // 扭曲持續時間（秒）
    public float maxFovAdd = 60f;        // FOV 最多增加多少
    public float maxRoll = 40f;          // 鏡頭最大傾斜角度
    public CanvasGroup fadeGroup;        // 選填：全螢幕黑色 Image 的 CanvasGroup，用來淡出

    bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(TeleportRoutine());
    }

    IEnumerator TeleportRoutine()
    {
        Camera cam = Camera.main;
        float startFov = cam.fieldOfView;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            float ease = p * p; // 越到後面越劇烈

            // 拉伸 FOV 造成扭曲感
            cam.fieldOfView = startFov + maxFovAdd * ease
                              + Mathf.Sin(t * 20f) * 3f * ease; // 加上抖動

            // 鏡頭旋轉扭曲
            cam.transform.localRotation *= Quaternion.Euler(0, 0, maxRoll * ease * Time.deltaTime * 5f);

            if (fadeGroup) fadeGroup.alpha = Mathf.Clamp01((p - 0.6f) / 0.4f);
            yield return null;
        }

        SceneManager.LoadScene(targetScene);
    }
}