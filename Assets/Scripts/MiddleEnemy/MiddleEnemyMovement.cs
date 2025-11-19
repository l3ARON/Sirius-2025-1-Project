using UnityEngine;

public class MiddleEnemyMovement : MonoBehaviour
{
    Rigidbody2D rigid;
    SpriteRenderer sprite;
    Animator anim;

    [Header("순찰 이동 속도")]
    public float patrolSpeed = 1.5f;

    [Header("추적 이동 속도 (1차 범위)")]
    public float chaseSpeed = 3.5f;

    [Header("감지 범위")]
    public float detectRange = 8f;     // 1차 범위 (추적)
    public LayerMask playerMask;

    [Header("상태")]
    public bool isPatrolling = true;
    public bool isChasing = false;
    public bool isMovementPaused = false;

    Transform player;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim   = GetComponent<Animator>();
    }

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (isMovementPaused)
        {
            rigid.velocity = Vector2.zero;
            UpdateAnim();
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= detectRange)
            StartChasing();
        else
            StopChasing();

        Move();
        UpdateAnim();
    }

    void StartChasing()
    {
        isPatrolling = false;
        isChasing = true;
    }

    void StopChasing()
    {
        isPatrolling = true;
        isChasing = false;
    }

    void Move()
    {
        float dir = 0f;

        if (isChasing)
        {
            dir = Mathf.Sign(player.position.x - transform.position.x);
            rigid.velocity = new Vector2(chaseSpeed * dir, rigid.velocity.y);
        }
        else if (isPatrolling)
        {
            // 기본적으로 오른쪽으로 걷기 (원하면 방향 반전 로직 추가 가능)
            dir = Mathf.Sign(rigid.velocity.x == 0 ? 1 : rigid.velocity.x);
            rigid.velocity = new Vector2(patrolSpeed * dir, rigid.velocity.y);
        }

        if (sprite != null && dir != 0)
            sprite.flipX = dir < 0;
    }

    void UpdateAnim()
    {
        if (anim == null) return;

        bool walk = isPatrolling && !isChasing && !isMovementPaused;
        bool run  = isChasing && !isMovementPaused;

        anim.SetBool("isWalk", walk);
        anim.SetBool("isRun", run);
        // isAttack은 공격 스크립트(MiddleEnemyAttack)에서만 제어
    }

    public void PauseMovement()
    {
        isMovementPaused = true;
        rigid.velocity = Vector2.zero;
        UpdateAnim();
    }

    public void ResumeMovement()
    {
        isMovementPaused = false;
        UpdateAnim();
    }

    // 📏 감지 범위 시각화 (Scene 뷰에서만 보임)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
