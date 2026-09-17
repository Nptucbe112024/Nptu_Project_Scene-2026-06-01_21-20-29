using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class MonsterAI : MonoBehaviour
{
    // =========================================================
    // Target
    // =========================================================
    [Header("Target")]
    public Transform player;


    // =========================================================
    // Detection
    // =========================================================
    [Header("Detection")]
    public float detectRange = 10f;
    public float attackRange = 1.5f;
    public LayerMask obstacleLayer;


    // =========================================================
    // Flashlight
    // =========================================================
    [Header("Flashlight")]
    public Transform flashlight;
    public Light flashlightLight;
    public float flashlightStopAngle = 20f;


    // =========================================================
    // Movement
    // =========================================================
    [Header("Movement")]
    public NavMeshAgent agent;
    public float rotateSpeed = 8f;

    [Tooltip("實際速度低於這個值時，不播放走路動畫")]
    public float minimumWalkVelocity = 0.08f;

    [Tooltip("走路動畫原本對應的移動速度")]
    public float animationReferenceSpeed = 2f;

    [Tooltip("走路動畫最慢播放倍率")]
    public float minimumWalkAnimationSpeed = 0.75f;

    [Tooltip("走路動畫最快播放倍率")]
    public float maximumWalkAnimationSpeed = 1.5f;


    // =========================================================
    // Attack
    // =========================================================
    [Header("Attack")]
    public float attackCooldown = 1.5f;

    private float attackTimer = 0f;

    // 是否已經開始最終攻擊流程
    private bool isGameOverSequence = false;


    // =========================================================
    // Game Over
    // =========================================================
    [Header("Game Over")]
    [Tooltip("GameOver 場景名稱")]
    public string gameOverSceneName = "GameOver";


    // =========================================================
    // Animation
    // =========================================================
    [Header("Animation")]
    public Animator animator;


    // =========================================================
    // Sound
    // =========================================================
    [Header("Sound")]
    public AudioSource walkAudioSource;
    public AudioSource sfxAudioSource;

    public AudioClip walkSound;
    public AudioClip attackSound;


    // =========================================================
    // Internal State
    // =========================================================
    private bool isStoppedByLight = false;

    private bool currentIsLit = false;
    private bool currentIsWalking = false;
    private bool currentIsAttacking = false;


    // =========================================================
    // 玩家控制器
    // =========================================================
    private FirstPersonController playerController;

    private UltimateFlashlightController flashlightController;



    // =========================================================
    // Start
    // =========================================================
    void Start()
    {
        InitComponents();

        InitAudio();

        InitPlayerController();

        InitFlashlightController();


        if (agent != null)
        {
            agent.updateRotation = true;
        }
    }



    // =========================================================
    // Update
    // =========================================================
    void Update()
    {
        attackTimer -= Time.deltaTime;


        // -----------------------------------------------------
        // 已經進入攻擊死亡流程
        // 不再執行追逐、手電筒、Idle 等 AI 邏輯
        // -----------------------------------------------------
        if (isGameOverSequence)
        {
            FacePlayer();

            return;
        }


        // -----------------------------------------------------
        // 判斷是否被手電筒照到
        // -----------------------------------------------------
        isStoppedByLight = IsHitByFlashlight();


        if (isStoppedByLight)
        {
            StopMonsterByLight();

            UpdateAnimationFromMovement();

            return;
        }


        // -----------------------------------------------------
        // 看不到玩家
        // -----------------------------------------------------
        if (!CanSeePlayer())
        {
            Idle();

            UpdateAnimationFromMovement();

            return;
        }


        // -----------------------------------------------------
        // 計算與玩家距離
        // -----------------------------------------------------
        float distance = Vector3.Distance(
            transform.position,
            player.position
        );


        // -----------------------------------------------------
        // 攻擊 or 追逐
        // -----------------------------------------------------
        if (distance <= attackRange)
        {
            AttackPlayer();
        }
        else
        {
            ChasePlayer();
        }


        UpdateAnimationFromMovement();
    }



    // =========================================================
    // 初始化怪物元件
    // =========================================================
    void InitComponents()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }


        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }


        if (animator != null)
        {
            animator.cullingMode =
                AnimatorCullingMode.AlwaysAnimate;
        }
    }



    // =========================================================
    // 初始化玩家 FirstPersonController
    // =========================================================
    void InitPlayerController()
    {
        if (player == null)
        {
            return;
        }


        // 玩家本體
        playerController =
            player.GetComponent<FirstPersonController>();


        // 父物件
        if (playerController == null)
        {
            playerController =
                player.GetComponentInParent<FirstPersonController>();
        }


        // 子物件
        if (playerController == null)
        {
            playerController =
                player.GetComponentInChildren<FirstPersonController>();
        }


        if (playerController == null)
        {
            Debug.LogWarning(
                "MonsterAI 找不到 FirstPersonController！"
            );
        }
    }



    // =========================================================
    // 初始化玩家手電筒 Controller
    // =========================================================
    void InitFlashlightController()
    {
        if (player == null)
        {
            return;
        }


        // 玩家本體
        flashlightController =
            player.GetComponent<UltimateFlashlightController>();


        // 子物件
        if (flashlightController == null)
        {
            flashlightController =
                player.GetComponentInChildren<UltimateFlashlightController>();
        }


        // 父物件
        if (flashlightController == null)
        {
            flashlightController =
                player.GetComponentInParent<UltimateFlashlightController>();
        }


        if (flashlightController == null)
        {
            Debug.LogWarning(
                "MonsterAI 找不到 UltimateFlashlightController！"
            );
        }
    }



    // =========================================================
    // 初始化音效
    // =========================================================
    void InitAudio()
    {
        if (walkAudioSource != null)
        {
            walkAudioSource.clip = walkSound;

            walkAudioSource.loop = true;

            walkAudioSource.playOnAwake = false;
        }


        if (sfxAudioSource != null)
        {
            sfxAudioSource.playOnAwake = false;
        }
    }



    // =========================================================
    // 是否看得到玩家
    // =========================================================
    bool CanSeePlayer()
    {
        if (player == null)
        {
            return false;
        }


        float distance = Vector3.Distance(
            transform.position,
            player.position
        );


        if (distance > detectRange)
        {
            return false;
        }


        Vector3 origin =
            transform.position +
            Vector3.up * 1.5f;


        Vector3 target =
            player.position +
            Vector3.up * 1.0f;


        Vector3 direction =
            target - origin;


        // -----------------------------------------------------
        // 中間有障礙物
        // -----------------------------------------------------
        if (Physics.Raycast(
            origin,
            direction.normalized,
            distance,
            obstacleLayer))
        {
            return false;
        }


        return true;
    }



    // =========================================================
    // 是否被手電筒照到
    // =========================================================
    bool IsHitByFlashlight()
    {
        if (flashlight == null ||
            flashlightLight == null)
        {
            return false;
        }


        if (!flashlightLight.enabled)
        {
            return false;
        }


        Vector3 monsterPoint =
            transform.position +
            Vector3.up * 1.2f;


        Vector3 directionToMonster =
            monsterPoint -
            flashlight.position;


        float distanceToMonster =
            directionToMonster.magnitude;


        // -----------------------------------------------------
        // 超出手電筒距離
        // -----------------------------------------------------
        if (distanceToMonster >
            flashlightLight.range)
        {
            return false;
        }


        // -----------------------------------------------------
        // 判斷角度
        // -----------------------------------------------------
        float angle = Vector3.Angle(
            flashlight.forward,
            directionToMonster
        );


        if (angle > flashlightStopAngle)
        {
            return false;
        }


        // -----------------------------------------------------
        // 判斷光線中間是否被東西擋住
        // -----------------------------------------------------
        if (Physics.Raycast(
            flashlight.position,
            directionToMonster.normalized,
            out RaycastHit hit,
            distanceToMonster))
        {
            if (hit.transform != transform &&
                !hit.transform.IsChildOf(transform))
            {
                return false;
            }
        }


        return true;
    }



    // =========================================================
    // 追逐玩家
    // =========================================================
    void ChasePlayer()
    {
        if (agent == null ||
            player == null)
        {
            return;
        }


        agent.isStopped = false;


        // -----------------------------------------------------
        // 找玩家附近最近的 NavMesh 點
        // 避免玩家站在 NavMesh 邊緣時怪物卡住
        // -----------------------------------------------------
        NavMeshHit hit;


        if (NavMesh.SamplePosition(
            player.position,
            out hit,
            2f,
            NavMesh.AllAreas))
        {
            agent.SetDestination(
                hit.position
            );
        }
        else
        {
            agent.SetDestination(
                player.position
            );
        }


        currentIsLit = false;

        currentIsAttacking = false;
    }



    // =========================================================
    // 攻擊玩家
    // =========================================================
    void AttackPlayer()
    {
        // -----------------------------------------------------
        // 已經開始死亡攻擊流程
        // 不允許再次觸發
        // -----------------------------------------------------
        if (isGameOverSequence)
        {
            return;
        }


        // -----------------------------------------------------
        // 1. 進入 GameOver 流程
        // -----------------------------------------------------
        isGameOverSequence = true;


        // -----------------------------------------------------
        // 2. 停止玩家所有操作
        // -----------------------------------------------------
        DisablePlayerControl();


        // -----------------------------------------------------
        // 3. 怪物停止移動
        // -----------------------------------------------------
        StopAgentImmediately();


        StopWalkSound();


        // -----------------------------------------------------
        // 4. 面向玩家
        // -----------------------------------------------------
        FacePlayer();


        // -----------------------------------------------------
        // 5. 設定動畫狀態
        // -----------------------------------------------------
        currentIsLit = false;

        currentIsWalking = false;

        currentIsAttacking = true;


        // -----------------------------------------------------
        // 6. 播放攻擊動畫
        // -----------------------------------------------------
        if (animator != null)
        {
            // 攻擊動畫使用正常速度
            animator.speed = 1f;


            animator.SetBool(
                "IsLit",
                false
            );


            animator.SetBool(
                "IsWalking",
                false
            );


            animator.SetBool(
                "IsAttacking",
                true
            );


            animator.SetTrigger(
                "AttackTrigger"
            );
        }


        // -----------------------------------------------------
        // 7. 播放攻擊音效
        // -----------------------------------------------------
        PlayAttackSound();


        Debug.Log(
            "怪物開始攻擊玩家"
        );
    }



    // =========================================================
    // 停止玩家操作
    // =========================================================
    void DisablePlayerControl()
    {
        // -----------------------------------------------------
        // 玩家移動 / 跑步 / 跳躍 / Mouse Look
        // -----------------------------------------------------
        if (playerController == null)
        {
            InitPlayerController();
        }


        if (playerController != null)
        {
            playerController.enabled = false;


            Debug.Log(
                "FirstPersonController 已停用"
            );
        }



        // -----------------------------------------------------
        // 玩家手電筒 F 鍵
        // -----------------------------------------------------
        if (flashlightController == null)
        {
            InitFlashlightController();
        }


        if (flashlightController != null)
        {
            // F 鍵立即失效
            // 手電筒依 UltimateFlashlightController
            // 裡面的 delayTime 延遲熄滅
            flashlightController.DisableByMonsterAttack();


            Debug.Log(
                "手電筒控制已停用"
            );
        }
    }



    // =========================================================
    // ★ 攻擊動畫播放完成
    //
    // ZombieAttack_GameOver 動畫最後
    // 使用 Animation Event 呼叫這個 Function
    // =========================================================
    public void OnAttackAnimationFinished()
    {
        if (!isGameOverSequence)
        {
            return;
        }


        Debug.Log(
            "攻擊動畫完成，切換 GameOver"
        );


        SceneManager.LoadScene(
            gameOverSceneName
        );
    }



    // =========================================================
    // 被手電筒照到
    // =========================================================
    void StopMonsterByLight()
    {
        StopAgentImmediately();


        StopWalkSound();


        currentIsLit = true;

        currentIsWalking = false;

        currentIsAttacking = false;
    }



    // =========================================================
    // Idle
    // =========================================================
    void Idle()
    {
        StopAgentImmediately();


        StopWalkSound();


        currentIsLit = false;

        currentIsWalking = false;

        currentIsAttacking = false;
    }



    // =========================================================
    // 立即停止 NavMeshAgent
    // =========================================================
    void StopAgentImmediately()
    {
        if (agent == null)
        {
            return;
        }


        agent.isStopped = true;

        agent.velocity = Vector3.zero;


        if (agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }



    // =========================================================
    // 根據實際移動速度控制動畫
    // =========================================================
    void UpdateAnimationFromMovement()
    {
        if (animator == null)
        {
            return;
        }


        bool isActuallyMoving = false;

        float actualSpeed = 0f;


        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh &&
            !agent.isStopped)
        {
            actualSpeed =
                agent.velocity.magnitude;


            bool hasPath =
                agent.hasPath &&
                !agent.pathPending;


            bool hasDistanceToTravel =
                agent.remainingDistance >
                agent.stoppingDistance + 0.05f;


            isActuallyMoving =
                hasPath &&
                hasDistanceToTravel &&
                actualSpeed >= minimumWalkVelocity;
        }


        // -----------------------------------------------------
        // 被照到 or 攻擊中
        // 都不能播放走路動畫
        // -----------------------------------------------------
        if (currentIsLit ||
            currentIsAttacking)
        {
            isActuallyMoving = false;
        }


        currentIsWalking =
            isActuallyMoving;


        animator.SetBool(
            "IsLit",
            currentIsLit
        );


        animator.SetBool(
            "IsWalking",
            currentIsWalking
        );


        animator.SetBool(
            "IsAttacking",
            currentIsAttacking
        );


        UpdateAnimatorPlaybackSpeed(
            actualSpeed
        );


        // -----------------------------------------------------
        // 走路音效
        // -----------------------------------------------------
        if (currentIsWalking)
        {
            PlayWalkSound();
        }
        else
        {
            StopWalkSound();
        }
    }



    // =========================================================
    // 根據 NavMeshAgent 真實速度
    // 調整走路動畫播放速度
    // =========================================================
    void UpdateAnimatorPlaybackSpeed(
        float actualSpeed)
    {
        if (animator == null)
        {
            return;
        }


        if (!currentIsWalking)
        {
            animator.speed = 1f;

            return;
        }


        float referenceSpeed =
            Mathf.Max(
                animationReferenceSpeed,
                0.01f
            );


        float animationSpeedMultiplier =
            actualSpeed /
            referenceSpeed;


        animationSpeedMultiplier =
            Mathf.Clamp(
                animationSpeedMultiplier,
                minimumWalkAnimationSpeed,
                maximumWalkAnimationSpeed
            );


        animator.speed =
            animationSpeedMultiplier;
    }



    // =========================================================
    // 面向玩家
    // =========================================================
    void FacePlayer()
    {
        if (player == null)
        {
            return;
        }


        Vector3 direction =
            player.position -
            transform.position;


        direction.y = 0f;


        if (direction.sqrMagnitude <
            0.001f)
        {
            return;
        }


        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction
            );


        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotateSpeed *
                Time.deltaTime
            );
    }



    // =========================================================
    // 播放走路聲音
    // =========================================================
    void PlayWalkSound()
    {
        if (walkAudioSource == null ||
            walkSound == null)
        {
            return;
        }


        if (!walkAudioSource.isPlaying)
        {
            walkAudioSource.Play();
        }
    }



    // =========================================================
    // 停止走路聲音
    // =========================================================
    void StopWalkSound()
    {
        if (walkAudioSource == null)
        {
            return;
        }


        if (walkAudioSource.isPlaying)
        {
            walkAudioSource.Stop();
        }
    }



    // =========================================================
    // 播放攻擊聲音
    // =========================================================
    void PlayAttackSound()
    {
        if (sfxAudioSource == null ||
            attackSound == null)
        {
            return;
        }


        sfxAudioSource.PlayOneShot(
            attackSound
        );
    }



    // =========================================================
    // Scene Gizmos
    // =========================================================
    void OnDrawGizmosSelected()
    {
        // 偵測距離
        Gizmos.color =
            Color.yellow;


        Gizmos.DrawWireSphere(
            transform.position,
            detectRange
        );


        // 攻擊距離
        Gizmos.color =
            Color.red;


        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}