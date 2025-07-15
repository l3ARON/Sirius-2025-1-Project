using System.Collections;
using UnityEngine;

public class MiddleEnemyMove : MonoBehaviour
{
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    public float patrolSpeed = 1f;
    public float runSpeed = 3f;
    private float xSpeed = 0f;

    public Transform player;
    public string targetTag = "Player";

    public float detectRangeX = 10f;
    public float detectRangeY = 1f;
    public float closeRangeX = 4f;
    public float closeRangeY = 1f;

    public AudioSource barkClip;
    private bool hasBarked = false;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        StartCoroutine(PatrolRoutine());
    }

    void FixedUpdate()
    {
        bool inBigRange = IsTargetInRange();
        bool inSmallRange = IsPlayerVeryClose();
        AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo(0);

        if (inSmallRange)
        {
            xSpeed = 0;

            // 공격 애니메이션 중복 방지
            if (!currentState.IsName("wolf_attack"))
            {
                SetAnimState(idle: false, walk: false, run: false, attack: true);
                if (!hasBarked)
                {
                    barkClip.Play();
                    hasBarked = true;
                }
            }
        }
        else
        {
            anim.SetBool("isAttack", false); // 공격 상태 해제
            hasBarked = false;

            if (inBigRange && player != null)
            {
                float direction = Mathf.Sign(player.position.x - transform.position.x);
                xSpeed = runSpeed * direction;
                SetAnimState(idle: false, walk: false, run: true, attack: false);
            }
            else
            {
                SetAnimState(
                    idle: Mathf.Abs(xSpeed) < 0.1f,
                    walk: Mathf.Abs(xSpeed) >= 0.1f,
                    run: false,
                    attack: false
                );
            }
        }

        // 이동 적용
        rigid.velocity = new Vector2(xSpeed, rigid.velocity.y);
        FlipSprite();

        // 낭떠러지/벽 감지 후 방향 반전
        Vector2 frontVec = new Vector2(rigid.position.x + Mathf.Sign(xSpeed) * 0.5f, rigid.position.y);
        Vector2 checkDir = (xSpeed > 0) ? Vector2.right : Vector2.left;

        if (!IsGroundAhead(frontVec) || IsWallAhead(frontVec, checkDir))
        {
            xSpeed *= -1;
        }
    }

    IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (!IsTargetInRange() && !IsPlayerVeryClose())
            {
                xSpeed = patrolSpeed * (Random.value > 0.5f ? 1 : -1);
                yield return new WaitForSeconds(Random.Range(2f, 4f));

                xSpeed = 0;
                yield return new WaitForSeconds(Random.Range(1f, 2f));
            }
            else
            {
                yield return null;
            }
        }
    }

    void SetAnimState(bool idle, bool walk, bool run, bool attack)
    {
        anim.SetBool("isIdle", idle);
        anim.SetBool("isWalk", walk);
        anim.SetBool("isRun", run);
        anim.SetBool("isAttack", attack);
    }

    bool IsTargetInRange()
    {
        Vector2 center = transform.position;
        Vector2 size = new Vector2(detectRangeX * 2, detectRangeY * 2);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag(targetTag))
            {
                player = hit.transform;
                return true;
            }
        }

        player = null;
        return false;
    }

    bool IsPlayerVeryClose()
    {
        Vector2 center = transform.position;
        Vector2 size = new Vector2(closeRangeX * 2, closeRangeY * 2);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag(targetTag))
                return true;
        }

        return false;
    }

    void FlipSprite()
    {
        if (xSpeed != 0)
            spriteRenderer.flipX = xSpeed < 0;
    }

    bool IsGroundAhead(Vector2 origin)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 2f);
        Debug.DrawRay(origin, Vector2.down * 2f, Color.green);
        return hit.collider != null;
    }

    bool IsWallAhead(Vector2 origin, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, 1f);
        Debug.DrawRay(origin, direction * 1f, Color.red);
        return hit.collider != null && hit.collider.gameObject != gameObject;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector2(detectRangeX * 2, detectRangeY * 2));

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, new Vector2(closeRangeX * 2, closeRangeY * 2));
    }
}
