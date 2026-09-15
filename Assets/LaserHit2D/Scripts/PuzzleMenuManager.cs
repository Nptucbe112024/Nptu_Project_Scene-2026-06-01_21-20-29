using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LaserHit2D
{
    public class PuzzleMenuManager : MonoBehaviour
    {
        [Header("退回的 3D 主場景名稱")]
        [SerializeField] private string m_Main3DSceneName = "SampleScene";

        [Header("UI 元素")]
        [SerializeField] private Button m_ResetButton;
        [SerializeField] private TextMeshProUGUI m_ResetButtonText;

        [Header("Reset 冷卻時間設定")]
        [SerializeField] private float m_CooldownTime = 10f;
        [SerializeField] private LaserShooter m_TargetShooter;

        private static float s_CooldownEndTime = 0f;
        private int m_ResetCount = 0;

        private void Awake()
        {
            // 【關鍵修復 1】：每次載入 2D 關卡時，強制重置通關狀態
            MirrorRotator.s_IsGameCleared = false;

            // 【關鍵修復 2】：確保滑鼠解鎖並顯示，允許 UI 點擊
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Start()
        {
            if (m_ResetButton != null)
            {
                m_ResetButton.onClick.AddListener(OnResetButtonPressed);
            }
        }

        private void Update()
        {
            if (Time.time < s_CooldownEndTime)
            {
                UpdateCooldownUI();
            }
            else if (m_ResetButtonText != null && m_ResetButtonText.text != "Reset")
            {
                ResetCooldownUI();
            }
        }

        public void OnResetButtonPressed()
        {
            if (Time.time < s_CooldownEndTime || MirrorRotator.s_IsGameCleared) return;

            m_ResetCount++;
            ResetTargetLaser();

            s_CooldownEndTime = Time.time + m_CooldownTime;
            UpdateCooldownUI();
        }

        private void UpdateCooldownUI()
        {
            float remainingTime = s_CooldownEndTime - Time.time;

            if (remainingTime > 0f)
            {
                if (m_ResetButton != null) m_ResetButton.interactable = false;
                if (m_ResetButtonText != null)
                {
                    m_ResetButtonText.text = $"Reset ({Mathf.CeilToInt(remainingTime)}s)";
                }
            }
            else
            {
                ResetCooldownUI();
            }
        }

        private void ResetCooldownUI()
        {
            if (m_ResetButtonText != null)
            {
                m_ResetButtonText.text = "Reset";
            }

            if (m_ResetButton != null)
            {
                m_ResetButton.interactable = !MirrorRotator.s_IsGameCleared;
            }
        }

        public void StopCooldown()
        {
            s_CooldownEndTime = 0f;
            ResetCooldownUI();
        }

        private void ResetTargetLaser()
        {
            if (m_TargetShooter != null)
            {
                int randomAngle = 0;

                if (m_ResetCount >= 2)
                {
                    int[] valid10StepAngles = new int[] { 
                        -90, -80, -70, -60, -50, -40, -30, -20, -10, 
                        10,  20,  30,  40,  50,  60,  70,  80,  90 
                    };

                    int randomIndex = Random.Range(0, valid10StepAngles.Length);
                    randomAngle = valid10StepAngles[randomIndex];
                }
                else
                {
                    do
                    {
                        randomAngle = Random.Range(-90, 91);
                    } 
                    while (randomAngle == 0);
                }

                m_TargetShooter.transform.rotation = Quaternion.Euler(0, 0, randomAngle);
            }
        }

        // Exit 按鈕邏輯
        public void OnExitButtonPressed()
        {
            Debug.Log("【Exit】離開 2D 謎題，返回 3D 主場景");

            // 切換回 3D 世界時鎖定滑鼠
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // 切換回 3D 主場景
            SceneManager.LoadScene(m_Main3DSceneName);
        }
    }
}