using UnityEngine;
using UnityEngine.EventSystems;

public class AutoCleanEventSystem : MonoBehaviour
{
    private void Awake()
    {
        // 尋找場景中所有的 EventSystem
        EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

        // 如果數量超過 1 個，且自己不是第一個，就把自己刪除
        if (eventSystems.Length > 1)
        {
            Destroy(gameObject);
        }
    }
}