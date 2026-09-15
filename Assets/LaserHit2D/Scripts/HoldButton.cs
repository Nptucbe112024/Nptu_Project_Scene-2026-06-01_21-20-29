using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("按住時要持續執行的事件")]
    public UnityEvent OnHold;

    private bool m_IsPressed = false;

    private void Update()
    {
        // 只要按鈕處於被按住狀態，每幀都發送事件
        if (m_IsPressed)
        {
            OnHold?.Invoke();
        }
    }

    // 當滑鼠/手指按下按鈕瞬間
    public void OnPointerDown(PointerEventData eventData)
    {
        m_IsPressed = true;
    }

    // 當滑鼠/手指放開按鈕瞬間
    public void OnPointerUp(PointerEventData eventData)
    {
        m_IsPressed = false;
    }
}