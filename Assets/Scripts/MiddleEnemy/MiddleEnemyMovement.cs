using UnityEngine;

public class MiddleEnemyMovement : MonoBehaviour
{
    Rigidbody2D rigid;
    SpriteRenderer sprite;

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
    }

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (isMovementPaused) return;
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= detectRange)
            StartChasing();
        else
            StopChasing();

        Move();
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
        float dir = 0;

        if (isChasing)
        {
            dir = Mathf.Sign(player.position.x - transform.position.x);
            rigid.velocity = new Vector2(chaseSpeed * dir, rigid.velocity.y);
        }
        else if (isPatrolling)
        {
            rigid.velocity = new Vector2(patrolSpeed, rigid.velocity.y);
        }

        if (sprite != null && dir != 0)
            sprite.flipX = dir > 0;
    }

    public void PauseMovement()
    {
        isMovementPaused = true;
        rigid.velocity = Vector2.zero;
    }

    public void ResumeMovement()
    {
        isMovementPaused = false;
    }
}
