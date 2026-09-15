using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LaserHit2D
{
    public class InteractablePuzzle : MonoBehaviour
    {
        [Header("要載入的 2D 謎題場景名稱")]
        [SerializeField] private string m_PuzzleSceneName = "Scene_LaserPuzzle";

        [Header("3D 互動提示 UI")]
        [SerializeField] private GameObject m_InteractPrompt;
        [SerializeField] private TextMeshProUGUI m_PromptText;

        [Header("提示文字設定")]
        [SerializeField] private string m_NormalPrompt = "按 F 鍵進入解謎";
        [SerializeField] private string m_ClearedPrompt = "此關卡已通關";

        public static bool s_IsPuzzleCompleted = false;
        private bool m_IsPlayerInRange = false;

        private void Start()
        {
            // 如果有儲存的座標，載入場景時將玩家移回原位
            if (PuzzlePlayerData.Instance != null && PuzzlePlayerData.Instance.HasSavedPosition)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player == null) player = GameObject.Find("FPSController");

                if (player != null)
                {
                    // 若有 CharacterController 組件，需要先停用才能修改 Transform 座標
                    CharacterController cc = player.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;

                    player.transform.position = PuzzlePlayerData.Instance.SavedPosition;
                    player.transform.rotation = PuzzlePlayerData.Instance.SavedRotation;

                    if (cc != null) cc.enabled = true;
                }

                // 讀取完後重置標記
                PuzzlePlayerData.Instance.HasSavedPosition = false;
            }
        }

        private void Update()
        {
            if (m_IsPlayerInRange && !s_IsPuzzleCompleted && Input.GetKeyDown(KeyCode.E))
            {
                // 1. 儲存玩家當前座標與旋轉角度
                GameObject player = GameObject.FindWithTag("Player");
                if (player == null) player = GameObject.Find("FPSController");

                if (player != null)
                {
                    if (PuzzlePlayerData.Instance == null)
                    {
                        new GameObject("PuzzlePlayerData").AddComponent<PuzzlePlayerData>();
                    }

                    PuzzlePlayerData.Instance.SavedPosition = player.transform.position;
                    PuzzlePlayerData.Instance.SavedRotation = player.transform.rotation;
                    PuzzlePlayerData.Instance.HasSavedPosition = true;
                }

                // 2. 解鎖游標並載入 2D 場景
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                SceneManager.LoadScene(m_PuzzleSceneName);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.gameObject.name.Contains("FPSController"))
            {
                m_IsPlayerInRange = true;

                if (m_InteractPrompt != null)
                {
                    if (m_PromptText != null)
                    {
                        m_PromptText.text = s_IsPuzzleCompleted ? m_ClearedPrompt : m_NormalPrompt;
                    }
                    m_InteractPrompt.SetActive(true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") || other.gameObject.name.Contains("FPSController"))
            {
                m_IsPlayerInRange = false;
                if (m_InteractPrompt != null) m_InteractPrompt.SetActive(false);
            }
        }
    }
}