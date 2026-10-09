using UnityEngine;
using TMPro;
using System.Collections;

public class StoryManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject storyPanel;
    public TMP_Text storyText;

    [Header("打字機效果")]
    [Tooltip("每個字出現的間隔，越小越快")]
    public float typingSpeed = 0.05f;

    [Header("跳過劇情設定")]
    [Tooltip("是否允許滑鼠左鍵跳到下一段")]
    public bool allowMouseSkip = true;

    private Coroutine storyCoroutine;

    // 是否要求跳到下一段
    private bool skipCurrentMessage = false;

    // 是否正在播放劇情
    private bool isPlayingStory = false;


    void Start()
    {
        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
        }
    }


    void Update()
    {
        // 沒有播放劇情就不處理滑鼠
        if (!isPlayingStory)
        {
            return;
        }

        // 是否允許滑鼠跳過
        if (!allowMouseSkip)
        {
            return;
        }

        // 滑鼠左鍵
        if (Input.GetMouseButtonDown(0))
        {
            skipCurrentMessage = true;
        }
    }


    // =====================================================
    // 顯示單段劇情
    // =====================================================
    public void ShowStory(string message, float duration)
    {
        ShowStories(
            new string[] { message },
            duration
        );
    }


    // =====================================================
    // 顯示多段劇情
    // =====================================================
    public void ShowStories(string[] messages, float duration)
    {
        if (storyPanel == null || storyText == null)
        {
            Debug.LogError("StoryManager UI 尚未設定！");
            return;
        }

        if (messages == null || messages.Length == 0)
        {
            return;
        }

        // 停止上一段劇情流程
        if (storyCoroutine != null)
        {
            StopCoroutine(storyCoroutine);
            storyCoroutine = null;
        }

        skipCurrentMessage = false;

        isPlayingStory = true;

        storyCoroutine = StartCoroutine(
            ShowStoriesRoutine(messages, duration)
        );
    }


    // =====================================================
    // 劇情播放流程
    // =====================================================
    IEnumerator ShowStoriesRoutine(
        string[] messages,
        float duration)
    {
        storyPanel.SetActive(true);

        // 依序播放每一段
        foreach (string message in messages)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                continue;
            }

            // 每段開始時重設跳過狀態
            skipCurrentMessage = false;

            // 設定完整文字
            storyText.text = message;

            // 更新 TMP 文字資訊
            storyText.ForceMeshUpdate();

            // 取得文字總字元數
            int totalCharacters =
                storyText.textInfo.characterCount;

            // 一開始隱藏全部文字
            storyText.maxVisibleCharacters = 0;


            // =============================================
            // 逐字顯示
            // =============================================
            for (int i = 0; i < totalCharacters; i++)
            {
                // 玩家按下滑鼠左鍵
                // 直接跳過目前這段
                if (skipCurrentMessage)
                {
                    break;
                }

                // 顯示下一個字
                storyText.maxVisibleCharacters = i + 1;

                // 根據標點符號調整停頓
                char letter =
                    storyText.textInfo.characterInfo[i].character;

                float delay = typingSpeed;

                if (letter == '。' ||
                    letter == '！' ||
                    letter == '？' ||
                    letter == '!' ||
                    letter == '?')
                {
                    delay *= 6f;
                }
                else if (letter == '，' ||
                         letter == ',' ||
                         letter == '…')
                {
                    delay *= 3f;
                }

                if (delay > 0f)
                {
                    yield return new WaitForSeconds(delay);
                }
                else
                {
                    yield return null;
                }
            }


            // =============================================
            // 全部文字出現後等待
            // =============================================
            float timer = 0f;

            while (!skipCurrentMessage &&
                   timer < duration)
            {
                timer += Time.deltaTime;

                yield return null;
            }

            // 如果按下滑鼠左鍵
            // 就不等待，直接進入下一段
        }


        // =================================================
        // 所有劇情播放完成
        // =================================================
        storyPanel.SetActive(false);

        skipCurrentMessage = false;

        isPlayingStory = false;

        storyCoroutine = null;
    }
}