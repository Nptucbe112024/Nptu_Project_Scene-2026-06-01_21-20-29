using UnityEngine;
using System.Collections.Generic;

public class StoryTrigger : MonoBehaviour
{
    [Header("第一段劇情")]
    [TextArea(3, 6)]
    public string storyMessage;

    [Header("接續顯示的劇情")]
    [TextArea(3, 6)]
    public string[] followingMessages;

    [Header("顯示設定")]
    public float displayTime = 3f;

    public StoryManager storyManager;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (storyManager == null)
        {
            Debug.LogWarning("StoryManager 尚未指定！");
            return;
        }

        hasTriggered = true;

        // 整理所有劇情文字
        List<string> messages = new List<string>();

        messages.Add(storyMessage);

        if (followingMessages != null)
        {
            messages.AddRange(followingMessages);
        }

        // 依序播放
        storyManager.ShowStories(
            messages.ToArray(),
            displayTime
        );
    }
}