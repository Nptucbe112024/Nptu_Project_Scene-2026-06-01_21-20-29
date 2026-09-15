using UnityEngine;

namespace LaserHit2D
{
    public class PuzzlePlayerData : MonoBehaviour
    {
        public static PuzzlePlayerData Instance { get; private set; }

        public Vector3 SavedPosition;
        public Quaternion SavedRotation;
        public bool HasSavedPosition = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // 確保切換場景時資料不被銷毀
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}