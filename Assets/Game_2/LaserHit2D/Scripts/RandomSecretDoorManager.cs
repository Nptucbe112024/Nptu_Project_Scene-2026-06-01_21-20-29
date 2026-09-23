using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSecretDoorManager : MonoBehaviour
{
    [System.Serializable]
    public class SecretDoorPair
    {
        public string pairName = "暗門組";
        [Tooltip("正面暗門")]
        public Transform frontDoor;
        [Tooltip("對應的背面暗門")]
        public Transform backDoor;
    }

    [Header("4 組對應的暗門設定")]
    [SerializeField] private List<SecretDoorPair> m_DoorPairs = new List<SecretDoorPair>();

    [Header("正面暗門高度設定")]
    [SerializeField] private float m_FrontClosedY = -5.0f;
    [SerializeField] private float m_FrontOpenY = 5.0f;

    [Header("背面暗門高度設定")]
    [SerializeField] private float m_BackClosedY = 21.87798f;
    [SerializeField] private float m_BackOpenY = 26.0f;

    [Header("開啟動畫設定")]
    [SerializeField] private bool m_AnimateOpen = true;
    [SerializeField] private float m_OpenSpeed = 3.0f;

    private void Start()
    {
        // 1. 開局先確保所有暗門回到初始關閉高度
        ResetAllDoorsToClosed();

        // 2. 隨機抽出一組進行開啟
        SelectAndOpenRandomPair();
    }

    private void ResetAllDoorsToClosed()
    {
        foreach (var pair in m_DoorPairs)
        {
            if (pair.frontDoor != null)
            {
                Vector3 pos = pair.frontDoor.localPosition;
                pos.y = m_FrontClosedY;
                pair.frontDoor.localPosition = pos;
            }

            if (pair.backDoor != null)
            {
                Vector3 pos = pair.backDoor.localPosition;
                pos.y = m_BackClosedY;
                pair.backDoor.localPosition = pos;
            }
        }
    }

    private void SelectAndOpenRandomPair()
    {
        if (m_DoorPairs == null || m_DoorPairs.Count == 0)
        {
            Debug.LogWarning("[SecretDoorManager] 未指派任何暗門組！");
            return;
        }

        // 隨機抽取 0 ~ Count-1
        int randomIndex = Random.Range(0, m_DoorPairs.Count);
        SecretDoorPair chosenPair = m_DoorPairs[randomIndex];

        if (chosenPair != null)
        {
            // 正面門升到 5，背面門升到 26
            if (chosenPair.frontDoor != null) 
                OpenDoorToY(chosenPair.frontDoor, m_FrontOpenY);

            if (chosenPair.backDoor != null) 
                OpenDoorToY(chosenPair.backDoor, m_BackOpenY);
        }
    }

    private void OpenDoorToY(Transform door, float targetY)
    {
        Vector3 targetPos = door.localPosition;
        targetPos.y = targetY;

        if (m_AnimateOpen)
        {
            StartCoroutine(AnimateDoor(door, targetPos));
        }
        else
        {
            door.localPosition = targetPos;
        }
    }

    private IEnumerator AnimateDoor(Transform door, Vector3 targetPos)
    {
        while (Vector3.Distance(door.localPosition, targetPos) > 0.001f)
        {
            door.localPosition = Vector3.MoveTowards(
                door.localPosition,
                targetPos,
                m_OpenSpeed * Time.deltaTime
            );
            yield return null;
        }
        door.localPosition = targetPos;
    }
}