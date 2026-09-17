using UnityEngine;
using System.Collections;

public class UltimateFlashlightController : MonoBehaviour
{
    [Header("基本設定")]
    public KeyCode toggleKey = KeyCode.F;
    public bool isOn = false;

    // 玩家目前是否可以操作 F 鍵
    private bool canToggle = true;


    [Header("指定光源 (請拖入底下的 WhiteLight)")]
    public Light _lightSource;


    [Header("攻擊後關燈設定")]
    public float delayTime = 0.5f;

    private Coroutine delayTurnOffCoroutine;


    [Header("音效設定")]
    public AudioSource audioSource;
    public AudioClip turnOnSound;
    public AudioClip turnOffSound;


    void Start()
    {
        // 初始狀態同步
        if (_lightSource != null)
        {
            _lightSource.enabled = isOn;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }


    void Update()
    {
        // =====================================================
        // 如果已經被怪物攻擊，F 鍵完全失效
        // =====================================================
        if (!canToggle)
        {
            return;
        }


        // F 開關手電筒
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }
    }


    // =========================================================
    // 玩家正常切換手電筒
    // =========================================================
    void ToggleFlashlight()
    {
        // 如果目前不允許操作，就直接忽略
        if (!canToggle)
        {
            return;
        }


        // 如果之前有一般的延遲關燈倒數
        // 玩家正常操作時可以取消
        if (delayTurnOffCoroutine != null)
        {
            StopCoroutine(delayTurnOffCoroutine);
            delayTurnOffCoroutine = null;
        }


        isOn = !isOn;


        if (_lightSource != null)
        {
            _lightSource.enabled = isOn;
        }


        // 播放音效
        if (audioSource != null)
        {
            if (isOn && turnOnSound != null)
            {
                audioSource.PlayOneShot(turnOnSound);
            }
            else if (!isOn && turnOffSound != null)
            {
                audioSource.PlayOneShot(turnOffSound);
            }
        }
    }


    // =========================================================
    // 原本接口
    // 要求手電筒延遲關閉
    // =========================================================
    public void RequestTurnOff()
    {
        if (!isOn)
        {
            return;
        }


        if (delayTurnOffCoroutine != null)
        {
            StopCoroutine(delayTurnOffCoroutine);
        }


        delayTurnOffCoroutine =
            StartCoroutine(DelayTurnOffRoutine());
    }


    // =========================================================
    // ★ 怪物開始攻擊時呼叫
    //
    // 1. 禁止 F 鍵
    // 2. 手電筒延遲關閉
    // 3. 玩家無法取消關燈
    // =========================================================
    public void DisableByMonsterAttack()
    {
        // 先鎖住 F 鍵
        canToggle = false;


        // 清除之前可能存在的倒數
        if (delayTurnOffCoroutine != null)
        {
            StopCoroutine(delayTurnOffCoroutine);
            delayTurnOffCoroutine = null;
        }


        // 如果手電筒現在是開著的
        // 開始怪物攻擊後的延遲關燈
        if (isOn)
        {
            delayTurnOffCoroutine =
                StartCoroutine(DelayTurnOffRoutine());
        }


        Debug.Log(
            "怪物攻擊：手電筒 F 鍵已停用"
        );
    }


    // =========================================================
    // 延遲關燈
    // =========================================================
    private IEnumerator DelayTurnOffRoutine()
    {
        yield return new WaitForSeconds(delayTime);


        if (isOn)
        {
            isOn = false;


            if (_lightSource != null)
            {
                _lightSource.enabled = false;
            }


            if (audioSource != null &&
                turnOffSound != null)
            {
                audioSource.PlayOneShot(
                    turnOffSound
                );
            }
        }


        delayTurnOffCoroutine = null;
    }
}