using UnityEngine;

namespace LaserHit2D
{
    public class ClockMirrorSpawner : MonoBehaviour
    {
        [Header("設定參數")]
        [SerializeField] private GameObject m_MirrorPrefab;
        [SerializeField] private int m_Count = 12;
        [SerializeField] private float m_Radius = 4f;

        [ContextMenu("Generate Clock Mirrors")]
        private void SpawnMirrors()
        {
            if (m_MirrorPrefab == null) return;

            // 為了方便重新生成，我們先清空之前的克隆體
            int childCount = transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < m_Count; i++)
            {
                // 從 12 點鐘方向開始計算位置角度
                float posAngleDeg = 90f - (i * (360f / m_Count));
                float angleRad = posAngleDeg * Mathf.Deg2Rad;

                // 計算圓周座標
                Vector3 spawnPos = transform.position + new Vector3(
                    Mathf.Cos(angleRad) * m_Radius,
                    Mathf.Sin(angleRad) * m_Radius,
                    0f
                );

                // --- 修改處開始 ---
                // 我們要讓原本面向外的面朝向圓心。
                //原本 `posAngleDeg` 是讓物件頭朝外，
                //加上 `180f` 就能讓它轉 180 度，變成頭朝內。
                float targetRotationZ = posAngleDeg + 180f; 
                // --- 修改處結束 ---

                // 生成鏡子並使鏡面朝向圓心
                Quaternion spawnRot = Quaternion.Euler(0, 0, targetRotationZ);
                Instantiate(m_MirrorPrefab, spawnPos, spawnRot, transform);
            }
        }
    }
}