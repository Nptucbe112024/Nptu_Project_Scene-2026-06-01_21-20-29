using UnityEngine;

public class PrismPuzzleManager : MonoBehaviour
{
    [Header("Number Targets")]
    public WallNumberTarget[] numberTargets;

    [Header("Puzzle Result")]
    public bool isSolved = false;
    public string revealedCode = "";

    private int[] digits = new int[3];
    private bool[] beamHit = new bool[3];

    void Start()
    {
        HideAllNumbers();
    }

    // 每次 RGB 光束重新判斷前，先隱藏全部數字
    public void BeginBeamCheck()
    {
        for (int i = 0; i < beamHit.Length; i++)
        {
            beamHit[i] = false;
        }

        HideAllNumbers();
    }

    // 某條 RGB 光照到某個數字時呼叫
    public void RecordBeamHit(int beamIndex, WallNumberTarget target)
    {
        if (target == null)
        {
            return;
        }

        if (beamIndex < 0 || beamIndex >= digits.Length)
        {
            return;
        }

        // 被光照到才顯示
        target.SetLit(true);

        digits[beamIndex] = target.digit;
        beamHit[beamIndex] = true;
    }

    public void EndBeamCheck()
    {
        // 已經解開後，數字還是可以繼續顯示 / 隱藏
        // 只是不要重新覆蓋密碼
        if (isSolved)
        {
            return;
        }

        for (int i = 0; i < beamHit.Length; i++)
        {
            if (!beamHit[i])
            {
                return;
            }
        }

        revealedCode =
            digits[0].ToString() +
            digits[1].ToString() +
            digits[2].ToString();

        isSolved = true;

        Debug.Log("Prism password revealed: " + revealedCode);
    }

    public void HideAllNumbers()
    {
        foreach (WallNumberTarget target in numberTargets)
        {
            if (target != null)
            {
                target.SetLit(false);
            }
        }
    }
}