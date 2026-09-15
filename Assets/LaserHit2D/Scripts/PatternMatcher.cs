using System.Collections.Generic;
using UnityEngine;

namespace LaserHit2D
{
    public class PatternMatcher : MonoBehaviour
    {
        [Header("比對對象")]
        [SerializeField] private LaserShooter m_PlayerShooter;
        [SerializeField] private LaserShooter m_TargetShooter;

        [Header("判定設定")]
        [SerializeField] private float m_Tolerance = 0.5f;

        [Header("時鐘角度特例放行清單 (僅允許 30, 60, 90, 120, 150, 180 反向通關)")]
        [SerializeField] private List<float> m_AllowedOppositeAngles = new List<float> 
        { 
            30f, 60f, 90f, 120f, 150f, 180f 
        };

        [Header("UI 控制器 (通關UI與紅綠燈)")]
        [SerializeField] private GameObject m_WinUIPanel;
        [SerializeField] private WinUIManager m_WinUIManager;

        private bool m_IsCleared = false;

        private void Start()
        {
            if (m_WinUIManager != null)
            {
                m_WinUIManager.SetStatus(false);
            }
        }

        private void Update()
        {
            if (!m_IsCleared)
            {
                CheckPatternMatch();
            }
        }

        public bool CheckPatternMatch()
        {
            if (m_PlayerShooter == null || m_TargetShooter == null) return false;

            List<Vector3> playerPoints = m_PlayerShooter.GetLaserPathPoints();
            List<Vector3> targetPoints = m_TargetShooter.GetLaserPathPoints();

            if (playerPoints.Count == 0 || targetPoints.Count == 0) return false;
            if (playerPoints.Count != targetPoints.Count) return false;

            Vector3 playerOrigin = m_PlayerShooter.transform.position;
            Vector3 targetOrigin = m_TargetShooter.transform.position;

            // 1. 取得兩邊發射器的角度
            float playerAngle = Mathf.Repeat(m_PlayerShooter.transform.eulerAngles.z, 360f);
            float targetAngle = Mathf.Repeat(m_TargetShooter.transform.eulerAngles.z, 360f);

            // 2. 計算角度差並判斷是否相差 180 度 (方向相反)
            float angleDiff = Mathf.Abs(playerAngle - targetAngle) % 360f;
            bool isOppositeDirection = Mathf.Abs(angleDiff - 180f) <= 1.0f; 

            // 3. 【特例檢查】僅允許列表中的「時鐘角度」進行反向通關
            bool allowOppositeMatch = false;

            if (isOppositeDirection)
            {
                // 將題目角度化簡為 0~180 範圍比較
                float normalizedTarget = Mathf.Repeat(targetAngle, 180f);
                if (Mathf.Approximately(normalizedTarget, 0f)) normalizedTarget = 180f;

                foreach (float allowedAngle in m_AllowedOppositeAngles)
                {
                    if (Mathf.Abs(normalizedTarget - allowedAngle) <= 1.0f)
                    {
                        allowOppositeMatch = true;
                        break;
                    }
                }

                // 若相差 180 度但不符合時鐘特例角度，直接判定不通關
                if (!allowOppositeMatch)
                {
                    return false;
                }
            }

            // 4. 比對各個折射路徑點位
            for (int i = 1; i < playerPoints.Count; i++)
            {
                Vector3 localPlayerPt = playerPoints[i] - playerOrigin;
                Vector3 localTargetPt = targetPoints[i] - targetOrigin;

                // 若符合特例，將玩家路徑相對座標做 180 度翻轉 (-x, -y) 後比對
                if (isOppositeDirection && allowOppositeMatch)
                {
                    localPlayerPt = new Vector3(-localPlayerPt.x, -localPlayerPt.y, localPlayerPt.z);
                }

                float dist = Vector3.Distance(localPlayerPt, localTargetPt);

                if (dist > m_Tolerance)
                {
                    return false;
                }
            }

            // 比對成功
            m_IsCleared = true;
            Debug.Log($"【恭喜通關】圖形吻合！(玩家輸入: {playerAngle}° / 題目: {targetAngle}°)");

            if (m_WinUIManager != null)
            {
                m_WinUIManager.SetStatus(true);
            }

            if (m_WinUIPanel != null)
            {
                m_WinUIPanel.SetActive(true);
            }

            return true;
        }

        // --- Exit 按鈕邏輯 ---
        public void OnExitButtonPressed()
        {
            Debug.Log("【離開謎題】關閉視窗");
            gameObject.SetActive(false); 

            // 如果是第一人稱 3D 遊戲，關閉謎題後重新隱藏並鎖定滑鼠游標
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // (可選) 恢復 3D 玩家移動
            // PlayerController.Instance.SetCanMove(true);
        }
    }
}