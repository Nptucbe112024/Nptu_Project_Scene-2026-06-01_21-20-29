using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LaserHit2D
{
    public class WinUIManager : MonoBehaviour
    {
        [Header("UI 元件引用")]
        [SerializeField] private Image m_StatusLight;
        [SerializeField] private TMP_Text m_StatusText;

        [Header("狀態燈號圖片設定 (Corner Lights)")]
        [SerializeField] private List<SpriteRenderer> m_CornerStatusLights; // 2D 物件的 SpriteRenderer
        [SerializeField] private Sprite m_CloseSprite;                      // 未通關圖片 (Button (2) - 紅色)
        [SerializeField] private Sprite m_OpenSprite;                       // 通關圖片 (Button (1) - 綠色)

        [Header("燈號顏色設定")]
        [SerializeField] private Color m_CloseColor = Color.red;
        [SerializeField] private Color m_OpenColor = Color.green;

        [Header("狀態文字設定")]
        [SerializeField] private string m_CloseText = "CLOSE";
        [SerializeField] private string m_OpenText = "OPEN";

        [Header("通關時雷射顏色變更")]
        [SerializeField] private Color m_ClearLaserColor = Color.green;
        [SerializeField] private LaserShooter m_PlayerShooter;
        [SerializeField] private LaserShooter m_TargetShooter;

        [Header("選單控制器（用於停止 CD）")]
        [SerializeField] private PuzzleMenuManager m_PuzzleMenuManager; // 引用 PuzzleMenuManager

        [Header("通關時需要禁用的 UI 控制項")]
        [SerializeField] private Button m_ResetButton;
        [SerializeField] private Button m_RotateLeftButton;
        [SerializeField] private Button m_RotateRightButton;
        [SerializeField] private TMP_InputField m_AngleInputField;

        private void Awake()
        {
            // 預設為未通關狀態 (紅燈 + CLOSE)
            SetStatus(false);
        }

        public void SetStatus(bool isCleared)
        {
            // 同步靜態變數，鎖定鏡子
            MirrorRotator.s_IsGameCleared = isCleared;

            if (isCleared)
            {
                // 【關鍵修復】：將通關狀態記錄至 3D 互動組件，切回 SampleScene 後依然保留
                InteractablePuzzle.s_IsPuzzleCompleted = true;

                // 1. 中央 Status 卡片：綠燈 + OPEN
                if (m_StatusLight != null) m_StatusLight.color = m_OpenColor;
                if (m_StatusText != null) m_StatusText.text = m_OpenText;

                // 2. 切換四角燈號 Sprite 為通關綠燈 (Button (1))
                UpdateCornerLightSprites(m_OpenSprite);

                // 3. 變更雷射顏色
                ChangeLaserColor(m_ClearLaserColor);

                // 4. 立刻停止 Reset 按鈕的冷卻 CD 並重置 UI 文字
                if (m_PuzzleMenuManager != null)
                {
                    m_PuzzleMenuManager.StopCooldown();
                }

                // 5. 鎖定控制項
                SetUIInteractable(false);
            }
            else
            {
                // 1. 中央 Status 卡片：紅燈 + CLOSE
                if (m_StatusLight != null) m_StatusLight.color = m_CloseColor;
                if (m_StatusText != null) m_StatusText.text = m_CloseText;

                // 2. 切換四角燈號 Sprite 為未通關紅燈 (Button (2))
                UpdateCornerLightSprites(m_CloseSprite);

                // 3. 解除控制項鎖定
                SetUIInteractable(true);
            }
        }

        private void UpdateCornerLightSprites(Sprite targetSprite)
        {
            if (targetSprite == null || m_CornerStatusLights == null) return;

            foreach (SpriteRenderer sr in m_CornerStatusLights)
            {
                if (sr != null)
                {
                    sr.sprite = targetSprite;
                }
            }
        }

        private void ChangeLaserColor(Color newColor)
        {
            if (m_PlayerShooter != null) m_PlayerShooter.SetLaserColor(newColor);
            if (m_TargetShooter != null) m_TargetShooter.SetLaserColor(newColor);
        }

        private void SetUIInteractable(bool state)
        {
            if (m_ResetButton != null) m_ResetButton.interactable = state;
            if (m_RotateLeftButton != null) m_RotateLeftButton.interactable = state;
            if (m_RotateRightButton != null) m_RotateRightButton.interactable = state;
            if (m_AngleInputField != null) m_AngleInputField.interactable = state;
        }
    }
}