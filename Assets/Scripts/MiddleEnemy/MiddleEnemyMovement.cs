using UnityEngine;

public class MiddleEnemyMovement : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    MiddleEnemyAttack attack;

    [Header("이동 속도")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 4.0f; // 공격하러 갈 땐 더 빠르게
    
    [Header("아군 모드 AI - 박스 감지")]
    public float friendlyDetectWidth = 6f;
    public float friendlyDetectHeight = 3f;
    public LayerMask enemyLayer;  // 적 레이어

    [Header("플레이어 따라가기")]
    public float followDistance = 2.5f;

    [Header("적군 모드 AI")]
    public float detectWidth = 8f;
    public float detectHeight = 3f;
    public LayerMask playerLayer;

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
            FriendlyBehavior();
        else
            EnemyBehavior();
    }

    // ============================
    // 🛡️ 아군 모드 행동
    // ============================
    void FriendlyBehavior()
    {
        if (player == null)
        {
            UpdateAnim(false);
            return;
        }

        // 🔥 직사각형 범위에서 적 감지
        Transform targetEnemy = DetectNearestEnemyBox();

        if (targetEnemy != null)
        {
            if (attack != null) attack.SetTarget(targetEnemy);
            MoveToTarget(targetEnemy.position, 0.8f);
            UpdateAnim(true);
            return;
        }

        // 적이 없으면 플레이어에게 따라가기
        if (attack != null) attack.SetTarget(null);

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer > followDistance)
        {
            MoveToTarget(player.position, followDistance);
            UpdateAnim(true);
        }
        else
        {
            rigid.velocity = new Vector2(0f, rigid.velocity.y);
            UpdateAnim(false);
        }
    }

    // ============================
    // 😈 적군 모드 행동
    // ============================
    void EnemyBehavior()
    {
        if (player == null) return;
        if (attack != null) attack.SetTarget(player);

        bool detected = DetectPlayerBox();

        if (detected)
        {
            MoveToTarget(player.position, 0.5f);
            UpdateAnim(true);
        }
        else
        {
            rigid.velocity = new Vector2(0f, rigid.velocity.y);
            UpdateAnim(false);
        }
    }

    // ============================
    // 📌 아군 모드 적 감지 (직사각형 박스)
    // ============================
    Transform DetectNearestEnemyBox()
    {
        Vector2 center = transform.position;
        Vector2 size = new Vector2(friendlyDetectWidth, friendlyDetectHeight);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, enemyLayer);

        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            float dst = Vector2.Distance(transform.position, hit.transform.position);
            if (dst < minDist)
            {
                nearest = hit.transform;
                minDist = dst;
            }
        }

        return nearest;
    }

    // ============================
    // 📌 적군 모드 플레이어 감지
    // ============================
    bool DetectPlayerBox()
    {
        Vector2 size = new Vector2(detectWidth, detectHeight);
        Collider2D hit = Physics2D.OverlapBox(transform.position, size, 0f, playerLayer);

        return hit != null;
    }

    // ============================
    // 이동 공통 함수
    // ============================
    void MoveToTarget(Vector3 targetPos, float stopDist)
    {
        float dist = Vector2.Distance(transform.position, targetPos);

        if (dist > stopDist)
        {
            float dirX = Mathf.Sign(targetPos.x - transform.position.x);
            rigid.velocity = new Vector2(chaseSpeed * dirX, rigid.velocity.y);
            Flip(dirX);
        }
        else
        {
            rigid.velocity = Vector2.zero;
            UpdateAnim(false);
        }
    }

    void Flip(float dir)
    {
        if (dir == 0) return;
        Vector3 sc = transform.localScale;
        sc.x = Mathf.Abs(sc.x) * (dir > 0 ? 1 : -1);
        transform.localScale = sc;
    }

    // ============================
    // 애니메이션 갱신
    // ============================
    void UpdateAnim(bool isMoving)
    {
        if (anim == null) return;

        if (!isMoving)
        {
            anim.SetBool("isRun",  false);
            anim.SetBool("isWalk", false);
            return;
        }

        bool run = isFriendlyMode ? isMoving : DetectPlayerBox();

        anim.SetBool("isRun",  run);
        anim.SetBool("isWalk", !run);
    }

    // ============================
    // 기타 기능
    // ============================
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

    // ============================
    // Gizmos (디버그)
    // ============================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector2(friendlyDetectWidth, friendlyDetectHeight));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector2(detectWidth, detectHeight));
    }
}
