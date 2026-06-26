using UnityEngine;

public class WallNumberTarget : MonoBehaviour
{
    [Header("Number")]
    [Range(0, 9)]
    public int digit;

    [Header("Display")]
    public GameObject numberVisual;

    void Start()
    {
        SetLit(false);
    }

    public void SetLit(bool isLit)
    {
        if (numberVisual != null)
        {
            numberVisual.SetActive(isLit);
        }
    }
}