using UnityEngine;

namespace LaserHit2D
{
    public class MirrorRotator : MonoBehaviour
    {
        [Header("互動開關（左邊勾選，右邊取消勾選）")]
        [SerializeField] private bool m_IsInteractable = true;

        [Header("旋轉設定")]
        [SerializeField] private float m_RotationAngle = 45f;
        [SerializeField] private string m_CameraName = "Main Camera";
        private Camera m_GameplayCamera;

        private void Awake()
        {
            m_GameplayCamera = GameObject.Find(m_CameraName)?.GetComponent<Camera>();

            if (m_GameplayCamera == null)
            {
                foreach (Camera cam in Camera.allCameras)
                {
                    if (cam.tag != "MainCamera")
                    {
                        m_GameplayCamera = cam;
                        break;
                    }
                }
            }
        }
        public static bool s_IsGameCleared = false; // 通關鎖定標記
        private void Update()
        {
            // 如果此鏡子被設定為不可互動（例如右邊區域），直接返回不處理點擊
            if (!m_IsInteractable || m_GameplayCamera == null) return;

            HandleMouseInput();
            HandleTouchInput();
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0)) // Left click
            {
                Vector2 mousePos = m_GameplayCamera.ScreenToWorldPoint(Input.mousePosition);
                Collider2D hit = Physics2D.OverlapPoint(mousePos);

                if (hit != null && hit.gameObject == gameObject)
                {
                    RotateMirror();
                }
            }
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Vector2 touchPos = m_GameplayCamera.ScreenToWorldPoint(Input.GetTouch(0).position);
                Collider2D hit = Physics2D.OverlapPoint(touchPos);

                if (hit != null && hit.gameObject == gameObject)
                {
                    RotateMirror();
                }
            }
        }

        private void RotateMirror()
        {
            transform.Rotate(0f, 0f, m_RotationAngle);
        }

        // 開放外部動態控制（例如生成鏡子時透過腳本設定）
        public void SetInteractable(bool interactable)
        {
            m_IsInteractable = interactable;
        }
    }
}