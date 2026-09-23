using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LaserHit2D
{
    public class WinUIManager : MonoBehaviour
    {
        [Header("頂部提示文字 UI (共用組件)")]
        [SerializeField] private GameObject m_GateInstructionGO;   
        [SerializeField] private TMP_Text m_GateInstructionText;   
        
        [Header("各狀態提示文字內容")]
        [SerializeField] private string m_CloseInstruction = "Press [E] to open the gate.";
        [SerializeField] private string m_GameplayInstruction = "Rotate the laser to make the patterns identical";
        [SerializeField] private string m_OpenInstruction = "The gate is open";

        [Header("UI 元件引用")]
        [SerializeField] private Image m_StatusLight;
        [SerializeField] private TMP_Text m_StatusText;

        [Header("狀態燈號圖片設定 (Corner Lights)")]
        [SerializeField] private List<Image> m_CornerStatusLights; 
        [SerializeField] private Sprite m_CloseSprite;                       
        [SerializeField] private Sprite m_OpenSprite;                        

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
        [SerializeField] private PuzzleMenuManager m_PuzzleMenuManager;

        [Header("通關時需要禁用的 UI 控制項")]
        [SerializeField] private Button m_ResetButton;
        [SerializeField] private Button m_RotateLeftButton;
        [SerializeField] private Button m_RotateRightButton;
        [SerializeField] private TMP_InputField m_AngleInputField;

        private TabletPuzzleManager m_OwnerTabletManager;
        private bool m_IsCleared = false;

        public void SetOwnerTabletManager(TabletPuzzleManager manager)
        {
            m_OwnerTabletManager = manager;
        }

        private void Awake()
        {
            SetStatus(false);
        }

        public void SetStatus(bool isCleared)
        {
            m_IsCleared = isCleared;
            MirrorRotator.s_IsGameCleared = isCleared;

            if (isCleared)
            {
                // 通關：切換為門開啟提示
                if (m_GateInstructionText != null) 
                    m_GateInstructionText.text = m_OpenInstruction;

                if (m_OwnerTabletManager != null)
                {
                    m_OwnerTabletManager.OnPuzzleCleared();
                }

                if (m_StatusLight != null) m_StatusLight.color = m_OpenColor;
                if (m_StatusText != null) m_StatusText.text = m_OpenText;

                UpdateCornerLightSprites(m_OpenSprite);
                ChangeLaserColor(m_ClearLaserColor);

                if (m_PuzzleMenuManager != null)
                {
                    m_PuzzleMenuManager.StopCooldown();
                }

                SetUIInteractable(false);
            }
            else
            {
                // 未通關預設 (3D 世界狀態)
                if (m_GateInstructionText != null) 
                    m_GateInstructionText.text = m_CloseInstruction;

                if (m_StatusLight != null) m_StatusLight.color = m_CloseColor;
                if (m_StatusText != null) m_StatusText.text = m_CloseText;
                UpdateCornerLightSprites(m_CloseSprite);
                SetUIInteractable(true);
            }
        }

        /// <summary>
        /// 根據目前是「3D 觀察模式」還是「2D 全螢幕解謎模式」切換頂部共用文字
        /// </summary>
        public void UpdateInstructionText(bool isFullPuzzleView)
        {
            if (m_GateInstructionText == null) return;

            if (m_IsCleared)
            {
                m_GateInstructionText.text = m_OpenInstruction;
            }
            else
            {
                // 進入 2D 全螢幕解謎時顯示玩法提示；留在 3D 世界時顯示按 E 提示
                m_GateInstructionText.text = isFullPuzzleView ? m_GameplayInstruction : m_CloseInstruction;
            }
        }

        private void UpdateCornerLightSprites(Sprite targetSprite)
        {
            if (targetSprite == null || m_CornerStatusLights == null) return;

            foreach (Image img in m_CornerStatusLights)
            {
                if (img != null)
                {
                    img.sprite = targetSprite;
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