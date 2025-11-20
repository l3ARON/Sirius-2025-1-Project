using UnityEngine;

public class MiddleEnemyMovement : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    MiddleEnemyAttack attack;

    [Header("이동 속도")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 4.0f; // 공격하러 갈 땐 더 빠르게
    
    [Header("아군 모드 AI")]
    public float followDistance = 2.5f;      // 플레이어와 유지할 거리
    public float enemyDetectRadius = 8.0f;   // 적 감지 범위
    public LayerMask enemyLayer;             // 감지할 적 레이어 (Enemy)

    [Header("적군 모드 AI")]
    public float detectWidth = 8f;
    public float detectHeight = 3f;
    public LayerMask playerLayer;            // 감지할 플레이어 레이어

    [Header("상태")]
    public bool isMovementPaused = false;
    public bool isFriendlyMode = false;

    Transform player;

    void Awake()
    {
        rigid  = GetComponent<Rigidbody2D>();
        anim   = GetComponent<Animator>();
        attack = GetComponent<MiddleEnemyAttack>();
    }

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (isMovementPaused) 
        {
            UpdateAnim(false);
            return;
        }

        if (isFriendlyMode)
        {
            FriendlyBehavior();
        }
        else
        {
            EnemyBehavior();
        }
    }

    // 🛡️ 아군일 때 행동 패턴
    void FriendlyBehavior()
    {
        if (player == null)
        {
            UpdateAnim(false);
            return;
        }

        // 1순위: 주변 적 탐색
        Transform targetEnemy = DetectNearestEnemy();

        if (targetEnemy != null)
        {
            // 🚨 적 발견! -> 적에게 돌진 & 공격 타겟 설정
            if (attack != null) attack.SetTarget(targetEnemy);
            MoveToTarget(targetEnemy.position, 0.8f); // 적 앞까지 바짝 붙음
            UpdateAnim(true);
            return;
        }

        // 🕊️ 적 없음 -> 플레이어 따라다니기 (보디가드)
        if (attack != null) attack.SetTarget(null);

        // ▶ 플레이어와의 거리로만 판단
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // followDistance 보다 멀면 따라가기
        if (distToPlayer > followDistance)
        {
            MoveToTarget(player.position, followDistance);
            UpdateAnim(true);          // 이동 애니메이션
        }
        else
        {
            // 일정 범위 안에 있으면 그 자리에서 대기 (Idle)
            rigid.velocity = new Vector2(0f, rigid.velocity.y);
            UpdateAnim(false);         // Idle 애니메이션
        }
    }


    // 😈 적일 때 행동 패턴
    void EnemyBehavior()
    {
        if (player == null) return;
        if (attack != null) attack.SetTarget(player); // 무조건 플레이어만 노림

        bool detected = DetectPlayerBox();
        // Debug.Log($"[EnemyBehavior] detected={detected}, velX={rigid.velocity.x}");

        if (detected)
        {
            // 플레이어를 발견하면 추격
            MoveToTarget(player.position, 0.5f);
            UpdateAnim(true);
        }
        else
        {
            // ❗ 플레이어가 감지되지 않으면 제자리에서 Idle
            rigid.velocity = new Vector2(0f, rigid.velocity.y);  // x속도 0
            // Debug.Log("[EnemyBehavior] Player not detected → stop & Idle");
            UpdateAnim(false);                                  // isWalk/isRun 끄고 Idle
        }
    }



    // 공통 이동 함수
    void MoveToTarget(Vector3 targetPos, float stopDist)
    {
        float dist = Vector2.Distance(transform.position, targetPos);
        // Debug.Log($"[MoveToTarget] dist={dist:F2}, stopDist={stopDist}");

        if (dist > stopDist)
        {
            float dirX = Mathf.Sign(targetPos.x - transform.position.x);
            rigid.velocity = new Vector2(chaseSpeed * dirX, rigid.velocity.y);
            Flip(dirX);
            
            // Debug.Log($"[MoveToTarget] MOVE → dirX={dirX}, velX={rigid.velocity.x}");
        }
        else
        {
            // 도착했으면 멈춤 (Idle)
            rigid.velocity = Vector2.zero;
            Debug.Log("[MoveToTarget] REACHED target → stop, Idle");
            UpdateAnim(false);
        }
    }


    // 가장 가까운 적 찾기 (OverlapCircle)
    Transform DetectNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, enemyDetectRadius, enemyLayer);
        Transform nearest = null;
        float minDst = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue; // 나 자신 제외

            float dst = Vector2.Distance(transform.position, hit.transform.position);
            if (dst < minDst)
            {
                minDst = dst;
                nearest = hit.transform;
            }
        }
        return nearest;
    }

    bool DetectPlayerBox()
    {
        Vector2 center = transform.position;
        Vector2 size = new Vector2(detectWidth, detectHeight);
        Collider2D hit = Physics2D.OverlapBox(center, size, 0f, playerLayer);

        bool result = (hit != null);
        Debug.Log($"[DetectPlayerBox] hit={(hit != null ? hit.name : "null")}, result={result}, layerMask={playerLayer.value}");
        return result;
    }


    public void SetFriendlyMode()
    {
        isFriendlyMode = true;
        isMovementPaused = false;
    }

    public void PauseMovement()
    {
        isMovementPaused = true;
        rigid.velocity = Vector2.zero;
        UpdateAnim(false);
    }

    public void ResumeMovement() => isMovementPaused = false;

    void Flip(float dir)
    {
        if (dir == 0) return;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dir > 0 ? 1 : -1);
        transform.localScale = scale;
    }

    void UpdateAnim(bool isMoving)
    {
        if (anim == null) return;
        
        // 아군이거나 플레이어 감지되면 Run 상태
        bool runCondition = isFriendlyMode ? isMoving : DetectPlayerBox();
        checkMove(isMoving, runCondition);
    }

    void checkMove(bool isMoving, bool runCondition)
    {
        if (!isMoving)
        {
            anim.SetBool("isRun",  false);
            anim.SetBool("isWalk", false);
            Debug.Log("[Anim] Idle (isMoving=false) → isRun=false, isWalk=false");
            return;
        }

        bool isRun  = runCondition;
        bool isWalk = !runCondition;

        anim.SetBool("isRun",  isRun);
        anim.SetBool("isWalk", isWalk);

        Debug.Log($"[Anim] isMoving={isMoving}, runCondition={runCondition} → isRun={isRun}, isWalk={isWalk}");
    }



    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; // 적군 감지 범위
        Gizmos.DrawWireCube(transform.position, new Vector2(detectWidth, detectHeight));
        
        Gizmos.color = Color.green; // 아군일 때 적 감지 범위
        Gizmos.DrawWireSphere(transform.position, enemyDetectRadius);
    }
}