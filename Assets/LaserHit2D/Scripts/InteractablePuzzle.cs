using System.Collections;
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

        [Header("門（Bay Door）開門設定")]
        [SerializeField] private Transform m_BayDoorTransform; // 拖入 Bay Door 物件
        [SerializeField] private float m_OpenHeight = 4.0f;     // 門往上升的高度 (Y軸)
        [SerializeField] private float m_OpenSpeed = 2.0f;      // 開門平滑移動速度

        public static bool s_IsPuzzleCompleted = false;
        private bool m_IsPlayerInRange = false;

        private void Start()
        {
            // 1. 還原玩家位置（若有紀錄）
            if (PuzzlePlayerData.Instance != null && PuzzlePlayerData.Instance.HasSavedPosition)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player == null) player = GameObject.Find("FPSController");

                if (player != null)
                {
                    CharacterController cc = player.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;

                    player.transform.position = PuzzlePlayerData.Instance.SavedPosition;
                    player.transform.rotation = PuzzlePlayerData.Instance.SavedRotation;

                    if (cc != null) cc.enabled = true;
                }

                PuzzlePlayerData.Instance.HasSavedPosition = false;
            }

            // 2. 如果已經通關，啟動升降開門協程
            if (s_IsPuzzleCompleted && m_BayDoorTransform != null)
            {
                StartCoroutine(OpenDoorRoutine());
            }
        }

        private IEnumerator OpenDoorRoutine()
        {
            Vector3 startPos = m_BayDoorTransform.position;
            Vector3 targetPos = startPos + new Vector3(0, m_OpenHeight, 0);

            // 平滑往上移動門的座標
            while (Vector3.Distance(m_BayDoorTransform.position, targetPos) > 0.01f)
            {
                m_BayDoorTransform.position = Vector3.MoveTowards(
                    m_BayDoorTransform.position, 
                    targetPos, 
                    m_OpenSpeed * Time.deltaTime
                );
                yield return null;
            }

            m_BayDoorTransform.position = targetPos;
        }

        private void Update()
        {
            if (m_IsPlayerInRange && !s_IsPuzzleCompleted && Input.GetKeyDown(KeyCode.E))
            {
                // 紀錄玩家當前座標
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